namespace MauiApp6
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }



        protected override async void OnAppearing()
        {
            base.OnAppearing();
            //CargarDatosEmpresa();
            CargarLogoEnPantalla();
        }



        private void CargarLogoEnPantalla()
        {
            // 1. Reconstruimos la ruta exacta donde guardamos el archivo
            string rutaDirectorio = FileSystem.Current.AppDataDirectory;
            string rutaLogo = Path.Combine(rutaDirectorio, "logoMain.png");

            // Alternativa: Si guardaste la ruta en las Preferencias como vimos en el paso anterior, 
            // puedes recuperarla así en lugar de las dos líneas de arriba:
            // string rutaLogo = Preferences.Default.Get("RutaLogoTicket", string.Empty);

            // 2. Siempre es una excelente práctica verificar que el archivo realmente exista
            // antes de intentar dibujarlo, para evitar que la app se cierre por error (Crash).
            if (File.Exists(rutaLogo))
            {
                // 3. Asignamos el archivo físico directamente al control Image
                imgLogo.Source = ImageSource.FromFile(rutaLogo);
            }
            else
            {
                // Opcional: Si por alguna razón el archivo se borró o no se descargó bien,
                // le pones una imagen por defecto desde tus recursos locales.
                // imgLogoTicket.Source = "imagen_default.png";
                Console.WriteLine("El archivo físico del logo no se encontró en el dispositivo.");
            }
        }


        private async Task CargarDatosEmpresa()
        {
            // Llamada a la BD
            var lista = await App.Database.GetDatosEmpresaAsync();


            var miEmpresa = lista.FirstOrDefault();

            if (miEmpresa != null)
            {
                // 2. Validamos que la propiedad del logo no venga nula o vacía
                if (miEmpresa.LogoPequeño != null && miEmpresa.LogoPequeño.Length > 0)
                {
                    // 3. Convertimos los bytes a Stream y lo asignamos directamente al control Image
                    imgLogo.Source = ImageSource.FromStream(() => new MemoryStream(miEmpresa.LogoPequeño));
                }
                else
                {
                    // Opcional: Ponemos una imagen por defecto de los recursos de la app si la BD no tiene logo
                    // imgLogo.Source = "placeholder_logo.png";
                }

                // Aquí también puedes aprovechar para llenar otros controles de tu interfaz:
                // lblNombreEmpresa.Text = miEmpresa.NombreEmpresa;
            }

        }


        private async void mnuVentas(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConsultarVentasPage());
        }


        private async void mnuCatalogo(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProductosPage());
        }


        private async void mnuCortes(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CortesPage());
        }

        private async void mnuSincronizar(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SincronizacionPage());
        }


        private async void mnuConfiguracion(object sender, EventArgs e)
        {
            var configuracionPage = Handler.MauiContext.Services.GetService<ConfiguracionPage>();

            await Navigation.PushAsync(configuracionPage);
        }




        //private async void OnConfiguracionTapped(object sender, TappedEventArgs e)
        //{
        //    var border = sender as Border;
        //    await border.ScaleTo(0.95, 100);
        //    await border.ScaleTo(1.0, 100);
        //    //await Navigation.PushAsync(new ConfiguracionPage());
        //    var configuracionPage = Handler.MauiContext.Services.GetService<ConfiguracionPage>();

        //    await Navigation.PushAsync(configuracionPage);
        //}

        //private async void OnVentasTapped(object sender, TappedEventArgs e)
        //{
        //    var border = sender as Border;
        //    await border.ScaleTo(0.95, 100);
        //    await border.ScaleTo(1.0, 100);
        //    //await DisplayAlert("Navegación", "Ir a Consultar Ventas", "OK");
        //    await Navigation.PushAsync(new ConsultarVentasPage());
        //}

        //private async void onSincronizarTapped(object sender, TappedEventArgs e)
        //{
        //    var border = sender as Border;
        //    await border.ScaleTo(0.95, 100);
        //    await border.ScaleTo(1.0, 100);
        //    //await DisplayAlert("Navegación", "Ir a Consultar Ventas", "OK");
        //    await Navigation.PushAsync(new SincronizacionPage());
        //}

        //private async void OnCortesTapped(object sender, TappedEventArgs e)
        //{
        //    var border = sender as Border;
        //    await border.ScaleTo(0.95, 100);
        //    await border.ScaleTo(1.0, 100);
        //    await Navigation.PushAsync(new CortesPage());
        //}

        
    }

}
