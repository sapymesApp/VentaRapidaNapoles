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

        // NUEVO: Método para leer de la báscula
        //Task<string> LeerPesoAsync(string deviceName);

        // Reemplaza o agrega este método en la interfaz
        Task EscucharBasculaAsync(string deviceName, Action<string> alRecibirPeso, CancellationToken cancellationToken);

    }
}
