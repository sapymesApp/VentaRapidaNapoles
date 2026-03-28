using Android.Bluetooth;
using Android.Graphics;
using Java.Util;
using MauiApp6.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Java.Util;
using Color = Android.Graphics.Color;

namespace MauiApp6.Platforms.Android
{
    public class BluetoothService : IBluetoothService
    {
        public IList<string> GetPairedDevices()
        {
            var devices = new List<string>();
            var adapter = BluetoothAdapter.DefaultAdapter;

            if (adapter != null && adapter.IsEnabled)
            {
                foreach (var device in adapter.BondedDevices)
                {
                    devices.Add(device.Name);
                }
            }
            return devices;
        }

        public async Task PrintTextAsync(string deviceName, string text)
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null) throw new Exception("No se encontró adaptador Bluetooth");

            BluetoothDevice device = adapter.BondedDevices.FirstOrDefault(d => d.Name == deviceName);
            if (device == null) throw new Exception("Impresora no encontrada entre dispositivos vinculados");

            BluetoothSocket socket = null;
            try
            {
                // UUID estándar para SPP (Serial Port Profile) usado por casi todas las impresoras
                UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

                socket = device.CreateRfcommSocketToServiceRecord(uuid);
                await socket.ConnectAsync();

                if (socket.IsConnected)
                {
                    // Convertir texto a bytes (ASCII suele funcionar mejor para impresoras básicas)
                    byte[] buffer = Encoding.ASCII.GetBytes(text + "\n"); // \n para salto de línea
                    await socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);

                    // Forzar el vaciado del buffer si es necesario, aunque WriteAsync suele bastar
                    socket.OutputStream.Flush();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al imprimir: {ex.Message}");
                throw;
            }
            finally
            {
                socket?.Close();
            }
        }




        public async Task PrintBytesAsync(string deviceName, byte[] data)
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null) throw new Exception("No se encontró adaptador Bluetooth");

            BluetoothDevice device = adapter.BondedDevices.FirstOrDefault(d => d.Name == deviceName);
            if (device == null) throw new Exception("Impresora no encontrada");

            BluetoothSocket socket = null;
            try
            {
                Java.Util.UUID uuid = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
                socket = device.CreateRfcommSocketToServiceRecord(uuid);
                await socket.ConnectAsync();

                if (socket.IsConnected)
                {
                    // === SOLUCIÓN AL BUFFER: Enviar en fragmentos (Chunks) ===
                    int chunkSize = 1024; // 1 KB por paquete
                    int offset = 0;

                    while (offset < data.Length)
                    {
                        // Calculamos cuánto enviar en este bloque
                        int sendLength = Math.Min(chunkSize, data.Length - offset);

                        // Enviamos el bloque
                        await socket.OutputStream.WriteAsync(data, offset, sendLength);
                        socket.OutputStream.Flush();

                        offset += sendLength;

                        // Pausa de 50 milisegundos para que la impresora procese el buffer
                        await Task.Delay(50);
                    }

                    // === SOLUCIÓN AL CIERRE: Esperar antes de desconectar ===
                    // Le damos tiempo a la impresora de terminar de recibir el último paquete por el aire
                    // antes de que el bloque 'finally' cierre el socket bruscamente.
                    await Task.Delay(300);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al imprimir: {ex.Message}");
                throw;
            }
            finally
            {
                socket?.Close();
            }
        }



        public async Task<string> LeerPesoAsync(string deviceName)
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null) throw new Exception("No se encontró adaptador Bluetooth");

            BluetoothDevice device = adapter.BondedDevices.FirstOrDefault(d => d.Name == deviceName);
            if (device == null) throw new Exception("Báscula no encontrada entre dispositivos vinculados");

            BluetoothSocket socket = null;
            try
            {
                // UUID estándar para SPP (Serial Port Profile)
                UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
                socket = device.CreateRfcommSocketToServiceRecord(uuid);
                await socket.ConnectAsync();

                if (socket.IsConnected)
                {
                    byte[] buffer = new byte[1024];

                    // Damos medio segundo para que la báscula envíe su ráfaga de datos
                    await Task.Delay(500);

                    if (socket.InputStream.IsDataAvailable())
                    {
                        int bytesRead = await socket.InputStream.ReadAsync(buffer, 0, buffer.Length);
                        string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        return data;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al leer báscula: {ex.Message}");
                throw;
            }
            finally
            {
                socket?.Close();
            }
        }



        public async Task EscucharBasculaAsync(string deviceName, Action<string> alRecibirPeso, CancellationToken cancellationToken)
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null) return;

            BluetoothDevice device = adapter.BondedDevices.FirstOrDefault(d => d.Name == deviceName);
            if (device == null) return;

            BluetoothSocket socket = null;
            try
            {
                Java.Util.UUID uuid = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
                socket = device.CreateRfcommSocketToServiceRecord(uuid);
                await socket.ConnectAsync();

                if (socket.IsConnected)
                {
                    byte[] buffer = new byte[1024];

                    // Este ciclo se repite infinitamente MIENTRAS no cancelemos la tarea
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        if (socket.InputStream.IsDataAvailable())
                        {
                            int bytesRead = await socket.InputStream.ReadAsync(buffer, 0, buffer.Length);
                            string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                            // Enviamos los datos crudos de vuelta a la pantalla
                            alRecibirPeso?.Invoke(data);
                        }

                        // Una pequeñísima pausa para no saturar el procesador del teléfono
                        await Task.Delay(150, cancellationToken);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Esto es normal, ocurre cuando cerramos el popup y cancelamos el ciclo. Se ignora.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en lectura continua: {ex.Message}");
            }
            finally
            {
                // Cuando el ciclo se rompe (porque se cerró el popup), cerramos la conexión educadamente
                socket?.Close();
            }
        }




        //public async Task PrintBytesAsync(string deviceName, byte[] data)
        //{
        //    BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
        //    if (adapter == null) throw new Exception("No se encontró adaptador Bluetooth");

        //    BluetoothDevice device = adapter.BondedDevices.FirstOrDefault(d => d.Name == deviceName);
        //    if (device == null) throw new Exception("Impresora no encontrada entre dispositivos vinculados");

        //    BluetoothSocket socket = null;
        //    try
        //    {
        //        // UUID estándar para SPP (Serial Port Profile) usado por casi todas las impresoras
        //        UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

        //        socket = device.CreateRfcommSocketToServiceRecord(uuid);
        //        await socket.ConnectAsync();

        //        if (socket.IsConnected)
        //        {
        //            // En lugar de convertir texto, enviamos los bytes directos
        //            await socket.OutputStream.WriteAsync(data, 0, data.Length);
        //            socket.OutputStream.Flush(); // Asegura el envío
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error al imprimir: {ex.Message}");
        //        throw;
        //    }
        //    finally
        //    {
        //        socket?.Close();
        //    }
        //}



        public byte[] FormatImageForPrinter(byte[] imageFileBytes)
        {
            // 1. Decodificar la imagen PNG/JPG a un Bitmap de Android
            Bitmap originalBitmap = BitmapFactory.DecodeByteArray(imageFileBytes, 0, imageFileBytes.Length);
            if (originalBitmap == null) return new byte[0];

            // 2. Redimensionar para la impresora (384px es el ancho estándar para papel de 58mm)
            int targetWidth = 384;
            // Calculamos el alto proporcional
            int targetHeight = (int)Math.Round((double)originalBitmap.Height * (targetWidth / (double)originalBitmap.Width));
            Bitmap bmp = Bitmap.CreateScaledBitmap(originalBitmap, targetWidth, targetHeight, false);

            // 3. Convertir a comandos ESC/POS (GS v 0)
            int widthPixels = bmp.Width;
            int heightPixels = bmp.Height;
            int widthBytes = (widthPixels + 7) / 8; // 8 píxeles equivalen a 1 byte

            byte[] command = new byte[8 + (widthBytes * heightPixels)];
            command[0] = 0x1D; // GS
            command[1] = 0x76; // v
            command[2] = 0x30; // 0
            command[3] = 0x00; // m (Modo normal)
            command[4] = (byte)(widthBytes % 256); // xL
            command[5] = (byte)(widthBytes / 256); // xH
            command[6] = (byte)(heightPixels % 256); // yL
            command[7] = (byte)(heightPixels / 256); // yH

            int index = 8;
            for (int y = 0; y < heightPixels; y++)
            {
                for (int x = 0; x < widthBytes; x++)
                {
                    byte b = 0;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        int px = (x * 8) + bit;
                        if (px < widthPixels)
                        {
                            int pixelColor = bmp.GetPixel(px, y);
                            int r = Color.GetRedComponent(pixelColor);
                            int g = Color.GetGreenComponent(pixelColor);
                            int blue = Color.GetBlueComponent(pixelColor);
                            int alpha = Color.GetAlphaComponent(pixelColor);

                            // Calcular luminancia (escala de grises)
                            int luminance = (int)(r * 0.299 + g * 0.587 + blue * 0.114);

                            // Si el píxel NO es transparente y es oscuro, encendemos el bit (imprimir punto)
                            if (alpha > 128 && luminance < 128)
                            {
                                b |= (byte)(1 << (7 - bit));
                            }
                        }
                    }
                    command[index++] = b;
                }
            }
            return command;
        }



    }
}
