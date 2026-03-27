using MauiApp6.Services;
using MauiApp6.ViewModels;

namespace MauiApp6;

public partial class SincronizacionPage : ContentPage
{
    //private readonly ApiService _apiService;

    public SincronizacionPage()
	{
		InitializeComponent();

        BindingContext = new SincronizacionViewModel();
        //_apiService = new ApiService();
    }


  
    //private string ObtenerIdDispositivo()
    //{
    //    var idDispositivo = Preferences.Default.Get("MiApp_DeviceID", string.Empty);
    //    if (string.IsNullOrWhiteSpace(idDispositivo))
    //    {
    //        idDispositivo = System.Guid.NewGuid().ToString();
    //        Preferences.Default.Set("MiApp_DeviceID", idDispositivo);
    //    }
    //    return idDispositivo;
    //}



    //private async void MenuButton_Tapped(object sender, EventArgs e)
    //{
    //    string idDispositivo = ObtenerIdDispositivo();

    //    // 2. Consultamos a la API
    //    bool estaAutorizado = await _apiService.ValidarDispositivoAsync(idDispositivo);

    //    // 3. Tomamos una decisión de navegación
    //    if (estaAutorizado)
    //    {
    //        btnCatalogo.IsEnabled = true;
    //        btnVentas.IsEnabled = true;
    //        btnAbonos.IsEnabled = true;
    //        // El dispositivo está registrado y activo. Navegamos al menú principal.
    //        //Application.Current.MainPage = new NavigationPage(new MenuPrincipalPage());
    //    }
    //    else
    //    {
    //        // El dispositivo no está registrado o fue desactivado en la BD.
    //        await DisplayAlert("Acceso Denegado", "Este dispositivo no está autorizado para operar el sistema.", "Entendido");

    //        // Opcional: Redirigir a una pantalla de contacto o cerrar la app
    //        // Application.Current.MainPage = new AccesoDenegadoPage();
    //    }
    //}
}