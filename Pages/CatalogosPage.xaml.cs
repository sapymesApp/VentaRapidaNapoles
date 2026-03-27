namespace MauiApp6;

public partial class CatalogosPage : ContentPage
{
	public CatalogosPage()
	{
		InitializeComponent();
	}

    private async void OnAbonosTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ConsultarAbonosPage());
    }

    private async void OnInventariosTapped(object sender, EventArgs e)
    {
        // As you already have a ProductosPage, we'll navigate there for 'Inventarios'
        await Navigation.PushAsync(new ProductosPage());
    }

    private async void OnClientesTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ClientesPage());
    }

   
}

