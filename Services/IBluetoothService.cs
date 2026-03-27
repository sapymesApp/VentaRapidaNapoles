using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp6.Services
{
    public interface IBluetoothService
    {
        Task PrintBytesAsync(string deviceName, byte[] data); // Cambiado de string a byte[]
        IList<string> GetPairedDevices();



        // NUEVO: Método para procesar la imagen
        byte[] FormatImageForPrinter(byte[] imageFileBytes);
    }
}
