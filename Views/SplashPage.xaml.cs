namespace MauiApp6.Views;

public partial class SplashPage : ContentPage
{
	public SplashPage()
	{
		InitializeComponent();
	}



    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. Aquí puedes cargar datos de tu SQLite, inicializar el Bluetooth, etc.
        // Por ahora, solo simularemos que carga durante 2.5 segundos
        await Task.Delay(2500);

        // 2. Transición al menú principal. 
        // Cambia "AppShell" por "new NavigationPage(new MainPage())" si no usas Shell.
        //Application.Current.MainPage = new AppShell();
        Application.Current.MainPage = new NavigationPage(new MainPage());
    }

}