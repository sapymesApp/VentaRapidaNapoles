using MauiApp6.Models;
using Microsoft.Maui.Controls.PlatformConfiguration;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiApp6.Services
{
    public class SyncService
    {
        // ⚠️ IMPORTANTE: Si usas Emulador Android, localhost es 10.0.2.2
        // Si usas celular físico, pon la IP de tu PC (ej. 192.168.1.50)
        const string BaseUrl = "https://sapymesapp.azurewebsites.net/api";

        private SQLiteAsyncConnection _db;
        private readonly HttpClient _client;

        public SyncService()
        {
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(30);
        }

        async Task Init()
        {
            //if (_db != null) return;

            // Ruta de la base de datos en el celular
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "SapyMes.db3");
            _db = new SQLiteAsyncConnection(databasePath);
            // Esto crea las tablas si no existen (es seguro llamarlo siempre)
            _db.CreateTableAsync<Clientes>().Wait();
            _db.CreateTableAsync<Producto>().Wait();
            _db.CreateTableAsync<Ventas>().Wait();
            _db.CreateTableAsync<VentasDetalle>().Wait();
            _db.CreateTableAsync<Abonos>().Wait();
            _db.CreateTableAsync<Empresa>().Wait();
        }



        public async Task<string> DescargarEmpresa(string ClaveEmpresa)
        {
            try
            {
                await Init();

                var responseProd = await _client.GetAsync($"{BaseUrl}/Empresas/Clave/{ClaveEmpresa}");
                if (responseProd.IsSuccessStatusCode)
                {
                    var empresa = await responseProd.Content.ReadFromJsonAsync<Empresa>();
                    if (empresa != null)
                    {
                        await _db.InsertOrReplaceAsync(empresa);




                        if (empresa.LogoPequeño != null)
                        {
                            string rutaLogoMain = await GuardarLogoFisicoAsync(empresa.LogoPequeño, "logoMain.png");
                            // Opcional: Puedes guardar esta ruta en las preferencias de la app para leerla rápido al imprimir
                            Preferences.Default.Set("RutaLogoMain", rutaLogoMain);
                        }

                        if (empresa.LogoTicket != null)
                        {
                            string rutalogoTicket = await GuardarLogoFisicoAsync(empresa.LogoTicket, "logoTicket.png");
                            // Opcional: Puedes guardar esta ruta en las preferencias de la app para leerla rápido al imprimir
                            Preferences.Default.Set("RutaLogoTicket", rutalogoTicket);
                        }




                    }
                }



                return "OK";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }




        public async Task<string> GuardarLogoFisicoAsync(byte[] logoBytes, string nombreArchivo)
        {
            try
            {
                if (logoBytes == null || logoBytes.Length == 0)
                    return string.Empty;

                // 1. Obtenemos la ruta privada y segura de la app en el dispositivo
                string rutaDirectorio = FileSystem.Current.AppDataDirectory;

                // 2. Combinamos la ruta con el nombre del archivo
                string rutaCompleta = Path.Combine(rutaDirectorio, nombreArchivo);

                // 3. Escribimos los bytes directamente en el almacenamiento
                await File.WriteAllBytesAsync(rutaCompleta, logoBytes);

                // Retornamos la ruta completa para que la puedas usar en tu ticket
                return rutaCompleta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar logo: {ex.Message}");
                return string.Empty;
            }
        }



        public async Task<string> Activar(string _Empresa, string _AndroidID)
        {
            // A. Llenamos la clase con los valores actuales de las propiedades (cajas de texto)
            var datosActivar = new Activar
            {
                
                Empresa = _Empresa,
                Fecha = DateTime.Now,
                AndroidId = _AndroidID,
            };


            var opcionesJson = new JsonSerializerOptions { WriteIndented = true };
            string jsonParaPostman = JsonSerializer.Serialize(datosActivar, opcionesJson);



            var response = await _client.PostAsJsonAsync($"{BaseUrl}/Activar", datosActivar);




            // C. Evaluamos la respuesta del servidor
            if (response.IsSuccessStatusCode)
            {
                //await Application.Current.MainPage.DisplayAlert("Éxito", "Activación enviada correctamente", "OK");
                return "Ok";
               
            }
            else
            {
                //await Application.Current.MainPage.DisplayAlert("Error", $"El servidor respondió con error: {response.StatusCode}", "OK");
                return "Error";
            }

        }



        public async Task<bool> Validar(string AndroidID)
        {

            try
            {
                await Init();

                string Aux = $"{BaseUrl}/validar/{AndroidID}";


                var responseProd = await _client.GetAsync($"{BaseUrl}/validar/{AndroidID}");
                if (responseProd.IsSuccessStatusCode)
                {
                    var validacion = await responseProd.Content.ReadFromJsonAsync<Validar>();
                    if (validacion != null)                    
                    {
                        Preferences.Set("IdEmpresa", validacion.IdEmpresa);
                        Preferences.Set("IdDispositivo", validacion.IdDispositivo);                        
                        return true;
                    }
                    
                }
                else
                {
                    //return "Error";
                }
            }
            catch (Exception ex)
            {
                //return $"Error: {ex.Message}";
            }

            return false;
        }


        public async Task<bool> ValidarDispositivoAsync(string idDispositivo)
        {
            try
            {
                // Hacemos la petición GET al endpoint que creamos en ASP.NET Core
                var response = await _client.GetAsync($"{BaseUrl}/dispositivos/validar/{idDispositivo}");

                // Si la API responde con un 200 OK
                if (response.IsSuccessStatusCode)
                {
                    // Deserializamos automáticamente el JSON a nuestro modelo
                    var resultado = await response.Content.ReadFromJsonAsync<ValidacionDispositivoResponse>();

                    // Retornamos el valor, o false si el resultado vino nulo
                    return resultado?.Activo ?? false;
                }

                // Si la API responde un 400, 404, 500, etc.
                return false;
            }
            catch (Exception ex)
            {
                // Aquí puedes registrar el error (ej. problemas de red, timeout)
                Console.WriteLine($"Error de conexión: {ex.Message}");
                return false;
            }
        }










        public async Task<string> DescargarCatalogos(int idEmpresa)
        {
            try
            {
                await Init();

                //// 1. DESCARGAR LÍNEAS
                //var responseLineas = await _client.GetAsync($"{BaseUrl}/Lineas/Sincronizar/{idEmpresa}");
                //if (responseLineas.IsSuccessStatusCode)
                //{
                //    var lineas = await responseLineas.Content.ReadFromJsonAsync<List<LineaLocal>>();
                //    if (lineas != null && lineas.Count > 0)
                //    {
                //        // InsertOrReplace actualiza si ya existe el ID, o inserta si es nuevo
                //        await _db.InsertAllAsync(lineas, "OR REPLACE");
                //    }
                //}

                // 2. DESCARGAR PRODUCTOS
                // (Aquí podrías añadir la lógica de ?fecha=... más adelante)
                var responseProd = await _client.GetAsync($"{BaseUrl}/Inventario/Sincronizar/{idEmpresa}");
                if (responseProd.IsSuccessStatusCode)
                {
                    var productos = await responseProd.Content.ReadFromJsonAsync<List<Producto>>();
                    if (productos != null && productos.Count > 0)
                    {
                        await _db.InsertAllAsync(productos, "OR REPLACE");
                    }
                }


                var responseClie = await _client.GetAsync($"{BaseUrl}/Clientes/Sincronizar/{idEmpresa}");
                if (responseClie.IsSuccessStatusCode)
                {
                    var clientes = await responseClie.Content.ReadFromJsonAsync<List<Clientes>>();
                    if (clientes != null && clientes.Count > 0)
                    {
                        await _db.InsertAllAsync(clientes, "OR REPLACE");
                    }
                }

                return "OK";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }



        public async Task<string> SincronizarVentasPendientes(int idEmpresa)
        {
            try
            {
                await Init();

                // 1. Obtener ventas no sincronizadas
                var ventasPendientes = await _db.Table<Ventas>()
                                                .Where(v => v.Sincronizado == false)
                                                .ToListAsync();

                // Ruta de acceso 1: Si no hay ventas, regresamos un mensaje inmediatamente
                if (ventasPendientes.Count == 0)
                {
                    return "No hay ventas pendientes por sincronizar.";
                }

                int exitosas = 0;
                int fallidas = 0;


                int idDispositivo = Preferences.Get("IdDispositivo", 0);


                foreach (var venta in ventasPendientes)
                {
                    // 2. Obtener los detalles de ESTA venta
                    venta.Folio=venta.Id;
                    venta.idEmpresa = idEmpresa;
                    venta.idDispositivo = idDispositivo;
                    venta.idUsuario = 2;
                    venta.Saldo = venta.Total;
                    

                    var detalles = await _db.Table<VentasDetalle>()
                                            .Where(d => d.VentasId == venta.Id)
                                            .ToListAsync();

                    // 3. Preparar el objeto para la API
                    var request = new VentaRequest
                    {
                        Venta = venta,
                        Detalles = detalles
                    };


                    var opcionesJson = new JsonSerializerOptions { WriteIndented = true };
                    string jsonParaPostman = JsonSerializer.Serialize(request, opcionesJson);


                    // 4. Enviar a la API
                    var response = await _client.PostAsJsonAsync($"{BaseUrl}/Ventas/Sincronizar", request);

                    if (response.IsSuccessStatusCode)
                    {
                        // 5. Actualizar localmente si la API lo recibió bien
                        venta.Sincronizado = true;
                        await _db.UpdateAsync(venta);
                        exitosas++;
                    }
                    else
                    {
                        // 6. Si falla una, la contamos, pero dejamos que el foreach continúe con la siguiente venta
                        var error = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error en venta {venta.Id}: {error}");
                        fallidas++;
                    }
                }

                // Ruta de acceso 2: El ciclo terminó, evaluamos el resultado global
                if (fallidas > 0)
                {
                    return $"Sincronización parcial: {exitosas} correctas, {fallidas} con error.";
                }

                return "OK";
            }
            catch (Exception ex)
            {
                // Ruta de acceso 3: Si explota la red o la BD
                return $"Error de excepción: {ex.Message}";
            }
        }

        public async Task<string> SincronizarAbonosPendientes()
        {
            try
            {
                await Init();

                // Obtener abonos no sincronizados
                var abonosPendientes = await _db.Table<Abonos>()
                                                .Where(a => a.Sincronizado == false)
                                                .ToListAsync();

                if (abonosPendientes.Count == 0)
                {
                    return "No hay abonos pendientes por sincronizar.";
                }

                int exitosas = 0;
                int fallidas = 0;

                foreach (var abono in abonosPendientes)
                {


                    // Serializamos el abono con formato (WriteIndented = true) para leerlo fácil
                    var opcionesJson = new JsonSerializerOptions { WriteIndented = true };
                    string jsonParaPostman = JsonSerializer.Serialize(abono, opcionesJson);


                    var response = await _client.PostAsJsonAsync($"{BaseUrl}/Abonos/Sincronizar", abono);

                    if (response.IsSuccessStatusCode)
                    {
                        abono.Sincronizado = true;
                        await _db.UpdateAsync(abono);
                        exitosas++;
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error en abono {abono.idAbono}: {error}");
                        fallidas++;
                    }
                }

                if (fallidas > 0)
                {
                    return $"Sincronización parcial: {exitosas} correctas, {fallidas} con error.";
                }

                return "OK";
            }
            catch (Exception ex)
            {
                return $"Error de excepción: {ex.Message}";
            }
        }

    }
}
