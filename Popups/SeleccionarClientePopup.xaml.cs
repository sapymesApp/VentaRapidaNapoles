using CommunityToolkit.Maui.Views;
using MauiApp6.Models;

namespace MauiApp6;

public partial class SeleccionarClientePopup : CommunityToolkit.Maui.Views.Popup
{
    private List<Clientes> _clientesLista;

    public SeleccionarClientePopup()
    {
        InitializeComponent();
        CargarClientes();
    }

    private async void CargarClientes()
    {
        _clientesLista = await App.Database.GetClientesAsync();
        CvClientes.ItemsSource = _clientesLista;
    }

    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        string filtro = SbClientes.Text?.ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(filtro))
        {
            CvClientes.ItemsSource = _clientesLista;
        }
        else
        {
            var resultadosFiltrados = _clientesLista
                .Where(c => c.Nombre.ToLower().Contains(filtro))
                .ToList();

            CvClientes.ItemsSource = resultadosFiltrados;
        }
    }

    private void OnClienteTapped(object sender, EventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is Clientes cliente)
        {
            Close(cliente);
        }
    }

    private void OnCancelarClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}
