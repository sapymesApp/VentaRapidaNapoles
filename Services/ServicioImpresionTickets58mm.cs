using MauiApp6.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace MauiApp6.Services
{
    public class ServicioImpresionTickets58mm
    {
        private readonly IBluetoothService _bluetoothService;

        // Inyectamos el servicio Bluetooth
        public ServicioImpresionTickets58mm(IBluetoothService bluetoothService)
        {
            _bluetoothService = bluetoothService;
        }

        public async Task ImprimirTicketAsync(string nombreImpresora, Ventas venta, Clientes cliente, List<VentasDetalle> detalles)
        {
            if (string.IsNullOrEmpty(nombreImpresora))
                throw new Exception("No se especificó una impresora válida.");

            if (venta == null || detalles == null || !detalles.Any())
                throw new Exception("Los datos de la venta o los detalles están vacíos.");

            List<byte> printData = new List<byte>();

            // 1. Inicializar
            printData.AddRange(EscPosCommands.Initialize);

            try
            {
                string rutaLogo = Preferences.Default.Get("RutaLogoTicket", string.Empty);

                // Verificamos que el archivo físico exista en esa ruta
                if (File.Exists(rutaLogo))
                {
                    byte[] imageBytes = File.ReadAllBytes(rutaLogo);
                    byte[] printerLogo = _bluetoothService.FormatImageForPrinter(imageBytes);

                    printData.AddRange(EscPosCommands.AlignCenter);
                    printData.AddRange(printerLogo);
                    printData.AddRange(EscPosCommands.FeedLine);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al preparar logo para imprimir: {ex.Message}");
            }


            string Titulo = Preferences.Get("TicketTitulo", "");

            if (!string.IsNullOrEmpty(Titulo))
            {
                printData.AddRange(EscPosCommands.AlignCenter);
                printData.AddRange(EscPosCommands.SizeDouble);
                printData.AddRange(EscPosCommands.BoldOn);
                printData.AddRange(Encoding.ASCII.GetBytes($"{Titulo}\n"));
                printData.AddRange(EscPosCommands.BoldOff);
                printData.AddRange(EscPosCommands.SizeNormal);
            }


            string ubicacion = Preferences.Get("TicketDireccion", "");

            if (!String.IsNullOrEmpty(ubicacion))
            {
                printData.AddRange(EscPosCommands.BoldOff);
                printData.AddRange(EscPosCommands.SizeNormal);
                printData.AddRange(Encoding.ASCII.GetBytes($"{ubicacion}\n"));
                printData.AddRange(EscPosCommands.FeedLine);
            }


            //// 3. Encabezado
            //printData.AddRange(EscPosCommands.BoldOff);
            //printData.AddRange(EscPosCommands.SizeNormal);
            //printData.AddRange(Encoding.ASCII.GetBytes($"{ubicacion}\n"));
            //printData.AddRange(EscPosCommands.FeedLine);

            // 4. Datos del Cliente / Folio
            printData.AddRange(EscPosCommands.AlignLeft);
            printData.AddRange(Encoding.ASCII.GetBytes($"Folio: {venta.Id}\n"));
            printData.AddRange(Encoding.ASCII.GetBytes($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}\n"));

            //if (cliente != null)
            //{
            //    printData.AddRange(Encoding.ASCII.GetBytes($"Cliente: {cliente.Nombre}\n"));
            //}
            printData.AddRange(EscPosCommands.FeedLine);

            // 5. Partidas (Detalle)
            // Para impresoras de 58mm el ancho es de 32 caracteres (aprox)
            printData.AddRange(Encoding.ASCII.GetBytes("--------------------------------\n"));
            // Encabezados adaptados: Cantidad, Descripción e Importe Total
            printData.AddRange(Encoding.ASCII.GetBytes("Cant Descripcion         Importe\n"));
            printData.AddRange(Encoding.ASCII.GetBytes("--------------------------------\n"));

            foreach (var item in detalles)
            {
                decimal importe = (decimal)(item.Cantidad * item.Precio);

                // Usamos el método optimizado para 58mm que recibe solo 3 parámetros
                string lineaFormateada = FormatearLineaTicket(item.Cantidad, item.Descripcion, importe);
                printData.AddRange(Encoding.ASCII.GetBytes(lineaFormateada));
            }
            printData.AddRange(Encoding.ASCII.GetBytes("--------------------------------\n"));

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

        public string FormatearLineaTicket(decimal cantidad, string descripcion, decimal importe)
        {
            int anchoTotal = 32; // Ancho estándar para 58mm

            string cantStr = cantidad.ToString("0.##").PadLeft(4);
            string importeStr = importe.ToString("C2").PadLeft(9); // Un poco más de espacio por si son números grandes

            // Calcula el espacio para descripción: total - cantidad - importe - 2 espacios separadores
            int anchoDesc = anchoTotal - cantStr.Length - importeStr.Length - 2;

            string descStr = descripcion;

            if (descStr.Length > anchoDesc)
            {
                // Cortar si es más largo que el espacio
                descStr = descStr.Substring(0, anchoDesc);
            }
            else
            {
                // Rellenar con espacios si es más corto
                descStr = descStr.PadRight(anchoDesc);
            }

            // Armamos la línea final con los 2 espacios de separación entre columnas
            return $"{cantStr} {descStr} {importeStr}\n";
        }
    }
}
