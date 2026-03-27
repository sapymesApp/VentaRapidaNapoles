using MauiApp6.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace MauiApp6.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        const string BaseUrl = "https://sapymesapp.azurewebsites.net/api";
        public ApiService()
        {
            _httpClient = new HttpClient();

            // Reemplaza esta URL con la ruta real de tu API en la nube o servidor
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(15); // Buena práctica para evitar bloqueos largos
        }

        public async Task<bool> ValidarDispositivoAsync(string idDispositivo)
        {
            try
            {
                // Hacemos la petición GET al endpoint que creamos en ASP.NET Core
                var response = await _httpClient.GetAsync($"{BaseUrl}/dispositivos/validar/{idDispositivo}");

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
    }
}
