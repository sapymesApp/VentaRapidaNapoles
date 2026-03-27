using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiApp6.Models;
using SQLite;

namespace MauiApp6.Services
{
    public class DatabaseService
    {
        private const string DB_NAME = "SapyMes.db3";
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, DB_NAME);
            _database = new SQLiteAsyncConnection(dbPath);

            // Esto crea las tablas si no existen (es seguro llamarlo siempre)
            _database.CreateTableAsync<Clientes>().Wait();
            _database.CreateTableAsync<Producto>().Wait();
            _database.CreateTableAsync<Ventas>().Wait();
            _database.CreateTableAsync<VentasDetalle>().Wait();
            _database.CreateTableAsync<Abonos>().Wait();
            _database.CreateTableAsync<Empresa>().Wait();
        }



        // 1. Obtener todos los productos (Para el CollectionView)
        public Task<List<Empresa>> GetDatosEmpresaAsync()
        {
            //return _database.Table<Producto>().ToListAsync();
            return _database.Table<Empresa>()                    
                    .ToListAsync();
        }



        // 1. Obtener todos los productos (Para el CollectionView)
        public Task<List<Producto>> GetProductosAsync()
        {
            //return _database.Table<Producto>().ToListAsync();
            return _database.Table<Producto>()
                    .OrderBy(x => x.Descripcion) // <--- LA MAGIA OCURRE AQUÍ
                    .ToListAsync();
        }

        // 2. Guardar nuevo producto
        public Task<int> SaveProductoAsync(Producto producto)
        {
            return _database.InsertAsync(producto);
        }

        // 3. Eliminar producto
        public Task<int> DeleteProductoAsync(Producto producto)
        {
            return _database.DeleteAsync(producto);
        }

        // 4. Editar un producto
        public Task<int> UpdateProductoAsync(Producto producto)
        {
            return _database.UpdateAsync(producto);
        }




        public async Task<int> SaveVentasAsync(Ventas ventas)
        {
            // Insertamos la cabecera
            await _database.InsertAsync(ventas);

            // Al ser AutoIncrement, SQLite ya le asignó un ID al objeto 'ventas'
            return ventas.Id;
        }

        // 2. Guardar un renglón del detalle
        public Task<int> SaveDetalleAsync(VentasDetalle detalle)
        {
            return _database.InsertAsync(detalle);
        }

        // 3. (Opcional pero recomendado) Guardar todo en una sola transacción
        // Esto asegura que si algo falla al guardar los detalles, no se guarde la cabecera sola
        public async Task<bool> GuardarVentasCompletaAsync(Ventas ventas, List<VentasDetalle> detalles)
        {
            try
            {
                await _database.RunInTransactionAsync(tran =>
                {
                    // Insertar cabecera
                    tran.Insert(ventas);

                    // Asignar el ID de la cabecera a cada detalle e insertar
                    foreach (var d in detalles)
                    {
                        d.VentasId = ventas.Id;
                        tran.Insert(d);
                    }
                });
                return true;
            }
            catch (Exception ex)
            {
                // Aquí podrías loguear el error
                return false;
            }
        }





        //// Obtener lista de cabeceras
        //public Task<List<Ventas>> GetVentasAsync()
        //{
        //    return _database.Table<Ventas>().OrderByDescending(x => x.Fecha).ToListAsync();
        //}

        //// Obtener el detalle de una ventas específica
        //public Task<List<VentasDetalle>> GetDetallesByVentasIdAsync(int ventasId)
        //{
        //    return _database.Table<VentasDetalle>().Where(x => x.VentasId == ventasId).ToListAsync();
        //}



        // Obtener ventas por fecha
        public Task<List<Ventas>> GetVentasPorFechaAsync(DateTime fecha)
        {
            var inicioDia = fecha.Date;
            var finDia = fecha.Date.AddDays(1).AddTicks(-1);

            return _database.Table<Ventas>()
                            .Where(x => x.Fecha >= inicioDia && x.Fecha <= finDia && x.Status != "Borrador")
                            .OrderByDescending(x => x.Fecha)
                            .ToListAsync();
        }

        // Obtener ventas por cliente y filtros
        public Task<List<Ventas>> GetVentasPorClienteAsync(int idCliente, DateTime desde, DateTime hasta, bool incluirPagadas)
        {
            var inicioDia = desde.Date;
            var finDia = hasta.Date.AddDays(1).AddTicks(-1);

            var query = _database.Table<Ventas>()
                .Where(x => x.idCliente == idCliente && 
                            x.Fecha >= inicioDia && 
                            x.Fecha <= finDia && 
                            x.Status != "Borrador");

            if (!incluirPagadas)
            {
                query = query.Where(x => x.Saldo > 0);
            }

            return query.OrderByDescending(x => x.Fecha).ToListAsync();
        }

        // Obtener detalles de una ventas
        public Task<List<VentasDetalle>> GetDetallesByVentasIdAsync(int ventasId)
        {
            return _database.Table<VentasDetalle>()
                            .Where(x => x.VentasId == ventasId)
                            .ToListAsync();
        }




        public Task<int> UpdateDetalleAsync(VentasDetalle detalle)
        {
            return _database.UpdateAsync(detalle);
        }

        // Actualiza la cabecera (para que el Total de la ventas sea correcto en el historial)
        public Task<int> UpdateVentasAsync(Ventas ventas)
        {
            return _database.UpdateAsync(ventas);
        }



        // Buscar si existe una cabecera en Borrador
        public Task<Ventas> ObtenerVentasBorradorAsync(int ventaNumero)
        {
            return _database.Table<Ventas>().FirstOrDefaultAsync(s => s.Status == "Borrador" && s.VentaNumero == ventaNumero);
        }

        // Traer los detalles usando el ID de la ventas
        public Task<List<VentasDetalle>> ObtenerDetallesPorVentasIdAsync(int ventasId)
        {
            return _database.Table<VentasDetalle>().Where(d => d.VentasId == ventasId).ToListAsync();
        }


        //public async Task<int> BorrarDetallesPorVentasIdAsync(int ventasId)
        //{
        //    // Esto ejecuta un DELETE directo y muy rápido en la tabla
        //    return await _database.Table<VentasDetalle>()
        //                          .Where(d => d.VentasId == ventasId)
        //                          .DeleteAsync();
        //}

        public async Task<bool> EliminarVentasCompletaAsync(int ventasId)
        {
            try
            {
                // 1. Primero borramos los detalles (los "hijos")
                await _database.Table<VentasDetalle>()
                               .Where(d => d.VentasId == ventasId)
                               .DeleteAsync();

                // 2. Luego borramos la cabecera (el "padre")
                await _database.Table<Ventas>()
                               .Where(s => s.Id == ventasId)
                               .DeleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar ventas completa: {ex.Message}");
                return false;
            }
        }


        public Task<List<Clientes>> GetClientesAsync()
        {
            return _database.Table<Clientes>()
                    .OrderBy(x => x.Nombre)
                    .ToListAsync();
        }

        public Task<int> SaveClienteAsync(Clientes cliente)
        {
            return _database.InsertAsync(cliente);
        }

        public Task<int> UpdateClienteAsync(Clientes cliente)
        {
            return _database.UpdateAsync(cliente);
        }

        public Task<int> DeleteClienteAsync(Clientes cliente)
        {
            return _database.DeleteAsync(cliente);
        }

        public Task<int> SaveAbonoAsync(Abonos abono)
        {
            return _database.InsertAsync(abono);
        }

        public Task<List<Abonos>> GetAbonosPorFechaAsync(DateTime fecha)
        {
            var inicioDia = fecha.Date;
            var finDia = fecha.Date.AddDays(1).AddTicks(-1);

            return _database.Table<Abonos>()
                            .Where(x => x.Fecha >= inicioDia && x.Fecha <= finDia)
                            .OrderByDescending(x => x.Fecha)
                            .ToListAsync();
        }

        // Obtener ventas por rango de fechas (Cortes)
        public Task<List<Ventas>> GetVentasPorRangoFechaAsync(DateTime inicio, DateTime fin)
        {
            var inicioDia = inicio.Date;
            var finDia = fin.Date.AddDays(1).AddTicks(-1);

            return _database.Table<Ventas>()
                            .Where(x => x.Fecha >= inicioDia && x.Fecha <= finDia && x.Status != "Borrador")
                            .OrderByDescending(x => x.Fecha)
                            .ToListAsync();
        }

        // Obtener abonos por rango de fechas (Cortes)
        public Task<List<Abonos>> GetAbonosPorRangoFechaAsync(DateTime inicio, DateTime fin)
        {
            var inicioDia = inicio.Date;
            var finDia = fin.Date.AddDays(1).AddTicks(-1);

            return _database.Table<Abonos>()
                            .Where(x => x.Fecha >= inicioDia && x.Fecha <= finDia)
                            .OrderByDescending(x => x.Fecha)
                            .ToListAsync();
        }
    }
}

