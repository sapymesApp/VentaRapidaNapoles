using MauiApp6.Models;

namespace MauiApp6;

public partial class ClientesPage : ContentPage
{
	public ClientesPage()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AppShell.SetNavBarIsVisible(this, false);
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsEnabled = false, IsVisible = false });
        await CargarClientes();
    }

    private async Task CargarClientes()
    {
        var lista = await App.Database.GetClientesAsync();
        CvClientes.ItemsSource = lista;
    }

    //private async void OnNuevoClienteClicked(object sender, EventArgs e)
    //{
    //    await Navigation.PushAsync(new NuevoClientePage());
    //}

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnEditarSwipeInvoked(object sender, EventArgs e)
    {
        var swipeItem = (SwipeItem)sender;
        var cliente = (Clientes)swipeItem.BindingContext;

        if (cliente != null)
        {
            await DisplayAlert("Info", "Editar cliente en desarrollo", "OK");
        }
    }

    private async void OnEliminarSwipeInvoked(object sender, EventArgs e)
    {
        var swipeItem = (SwipeItem)sender;
        var cliente = (Clientes)swipeItem.BindingContext;

        if (cliente != null)
        {
            await ProcesarEliminacion(cliente);
        }
    }

    private async Task ProcesarEliminacion(Clientes cliente)
    {
        bool confirmar = await DisplayAlert("Eliminar",
            $"¿Estás seguro de eliminar '{cliente.Nombre}'?",
            "Sí, eliminar", "Cancelar");

        if (confirmar)
        {
            await App.Database.DeleteClienteAsync(cliente);
            await CargarClientes();
            CvClientes.SelectedItem = null;
        }
    }

    private async void OnClienteSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Clientes clienteSeleccionado)
        {
            await Navigation.PushAsync(new VentasClientePage(clienteSeleccionado));
            CvClientes.SelectedItem = null; // Quitar la selección visual
        }
    }

    private async void MenuButton_Tapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NuevoClientePage());
    }
}
