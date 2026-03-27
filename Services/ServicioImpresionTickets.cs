using MauiApp6.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp6.Services
{
    public class ServicioImpresionTickets
    {
        private readonly IBluetoothService _bluetoothService;

        // Inyectamos el servicio Bluetooth que ya construimos
        public ServicioImpresionTickets(IBluetoothService bluetoothService)
        {
            _bluetoothService = bluetoothService;
        }

        /// <summary>
        /// Imprime un ticket de venta estándar de 58mm.
        /// </summary>
        public async Task ImprimirTicketAsync(string nombreImpresora, Ventas venta, Clientes cliente, List<VentasDetalle> detalles,
            //string nombreEmpresa = "MI TIENDA APP",
            string ubicacion = "Mesones #221, Agropecuario, CP 20116\nAguacalientes, Ags.")
        {
            if (string.IsNullOrEmpty(nombreImpresora))
                throw new Exception("No se especificó una impresora válida.");

            if (venta == null || detalles == null || !detalles.Any())
                throw new Exception("Los datos de la venta o los detalles están vacíos.");

            List<byte> printData = new List<byte>();

            // 1. Inicializar
            printData.AddRange(EscPosCommands.Initialize);

            //// 2. Logo (Opcional: Si falla, no detiene la impresión)
            //try
            //{

            //    //string rutaLogo = Preferences.Default.Get("RutaLogoTicket", string.Empty);

            //    string rutaDirectorio = FileSystem.Current.AppDataDirectory;
            //    string rutaLogo = Path.Combine(rutaDirectorio, "logoTicket.png");


            //    using var stream = await FileSystem.OpenAppPackageFileAsync(rutaLogo);
            //    using var memoryStream = new MemoryStream();
            //    await stream.CopyToAsync(memoryStream);
            //    byte[] imageBytes = memoryStream.ToArray();

            //    byte[] printerLogo = _bluetoothService.FormatImageForPrinter(imageBytes);
            //    printData.AddRange(EscPosCommands.AlignCenter);
            //    printData.AddRange(printerLogo);
            //    printData.AddRange(EscPosCommands.FeedLine);
            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Logo no encontrado o error: {ex.Message}");
            //}

            try
            {
                string rutaLogo = Preferences.Default.Get("RutaLogoTicket", string.Empty);

                // Verificamos que el archivo físico exista en esa ruta
                if (File.Exists(rutaLogo))
                {
                    // Leemos TODOS los bytes del archivo directamente al arreglo en una sola línea
                    byte[] imageBytes = File.ReadAllBytes(rutaLogo);

                    // Pasamos los bytes a tu formateador de la impresora
                    byte[] printerLogo = _bluetoothService.FormatImageForPrinter(imageBytes);

                    printData.AddRange(EscPosCommands.AlignCenter);
                    printData.AddRange(printerLogo);
                    printData.AddRange(EscPosCommands.FeedLine);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"El archivo físico no se encontró en: {rutaLogo}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al preparar logo para imprimir: {ex.Message}");
            }



            // 3. Encabezado
            //printData.AddRange(EscPosCommands.AlignCenter);
            //printData.AddRange(EscPosCommands.SizeDouble);
            //printData.AddRange(EscPosCommands.BoldOn);
            //printData.AddRange(Encoding.ASCII.GetBytes($"{nombreEmpresa}\n"));

            printData.AddRange(EscPosCommands.BoldOff);
            printData.AddRange(EscPosCommands.SizeNormal);
            printData.AddRange(Encoding.ASCII.GetBytes($"{ubicacion}\n"));
            printData.AddRange(EscPosCommands.FeedLine);

            // 4. Datos del Cliente / Folio
            printData.AddRange(EscPosCommands.AlignLeft);
            printData.AddRange(Encoding.ASCII.GetBytes($"Folio: {venta.Id}\n"));
            printData.AddRange(Encoding.ASCII.GetBytes($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}\n"));

            if (cliente != null)
            {
                printData.AddRange(Encoding.ASCII.GetBytes($"Cliente: {cliente.Nombre}\n"));
            }
            printData.AddRange(EscPosCommands.FeedLine);

            // 5. Partidas (Detalle)
            // Extendemos la línea a 48 guiones para impresoras de 80mm
            printData.AddRange(Encoding.ASCII.GetBytes("----------------------------------------------\n"));

            // Acomodamos los encabezados (Cant, Descripcion, Precio, Importe)
            printData.AddRange(Encoding.ASCII.GetBytes("Cant    Descripcion          Precio    Importe\n"));
            printData.AddRange(Encoding.ASCII.GetBytes("----------------------------------------------\n"));

            foreach (var item in detalles)
            {
                decimal importe = (decimal)(item.Cantidad * item.Precio);

                // ¡Ojo aquí! Le pasamos también el 'item.Precio' a tu método
                string lineaFormateada = FormatearLineaTicket(item.Cantidad, item.Descripcion, item.Precio, importe);
                printData.AddRange(Encoding.ASCII.GetBytes(lineaFormateada));
            }
            printData.AddRange(Encoding.ASCII.GetBytes("----------------------------------------------\n"));

            // 6. Totales
            printData.AddRange(EscPosCommands.AlignRight);
            printData.AddRange(EscPosCommands.SizeDouble);
            printData.AddRange(EscPosCommands.BoldOn);
            printData.AddRange(Encoding.ASCII.GetBytes($"TOTAL: {venta.Total:C2}\n"));
            printData.AddRange(EscPosCommands.BoldOff);
            printData.AddRange(EscPosCommands.SizeNormal);

            // 7. Pie y Corte
            printData.AddRange(EscPosCommands.AlignCenter);
            printData.AddRange(EscPosCommands.FeedLine);
            printData.AddRange(Encoding.ASCII.GetBytes("¡Gracias por su compra!\n"));
            printData.AddRange(EscPosCommands.Feed3Lines);
            printData.AddRange(EscPosCommands.FeedLine);
            printData.AddRange(EscPosCommands.PartialCut);

            // 8. Enviar a la impresora
            await _bluetoothService.PrintBytesAsync(nombreImpresora, printData.ToArray());
        }



        public string FormatearLineaTicket(decimal cantidad, string descripcion, decimal precio, decimal importe)
        {
            int anchoTotal = 46; // Ancho estándar para 80mm

            string cantStr = cantidad.ToString("0.##").PadLeft(6);
            string precioStr = precio.ToString("C2").PadLeft(12);
            string importeStr = importe.ToString("C2").PadLeft(10);

            // Ajuste matemático: restamos 5 espacios huecos en total
            int anchoDesc = anchoTotal - cantStr.Length - precioStr.Length - importeStr.Length - 5;

            string descStr = descripcion;

            if (descStr.Length > anchoDesc)
            {
                descStr = descStr.Substring(0, anchoDesc);
            }
            else
            {
                descStr = descStr.PadRight(anchoDesc);
            }

            // Armamos la línea final con 3 espacios de separación iniciales
            return $"{cantStr}   {descStr} {precioStr} {importeStr}\n";
        }


        //private string FormatearLineaTicket(int cantidad, string descripcion, decimal importe)
        //{
        //    string cantStr = cantidad.ToString().PadRight(4);
        //    string importeStr = importe.ToString("C2").PadLeft(10);

        //    string descStr = descripcion ?? "";
        //    if (descStr.Length > 17)
        //        descStr = descStr.Substring(0, 17);
        //    else
        //        descStr = descStr.PadRight(17);

        //    return $"{cantStr}{descStr} {importeStr}\n";
        //}
    }
}
