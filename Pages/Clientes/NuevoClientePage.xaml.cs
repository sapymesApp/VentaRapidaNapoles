using MauiApp6.Models;

namespace MauiApp6;

public partial class NuevoClientePage : ContentPage
{
    public NuevoClientePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
        {
            TxtNombre.Focus();
        });
    }

    private void OnNombreCompleted(object sender, EventArgs e)
    {
        TxtDireccion.Focus();
    }

    private void OnDireccionCompleted(object sender, EventArgs e)
    {
        TxtTelefono.Focus();
    }

    private void OnTelefonoCompleted(object sender, EventArgs e)
    {
        TxtWhatsApp.Focus();
    }

    private void OnWhatsAppCompleted(object sender, EventArgs e)
    {
        OnGuardarClicked(sender, e);
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            await DisplayAlert("Error", "Escribe el nombre del cliente", "OK");
            return;
        }

        var nuevoCliente = new Clientes
        {
            Nombre = TxtNombre.Text.ToUpper(),
            Direccion = string.IsNullOrWhiteSpace(TxtDireccion.Text) ? "" : TxtDireccion.Text.ToUpper(),
            Telefono = string.IsNullOrWhiteSpace(TxtTelefono.Text) ? "" : TxtTelefono.Text,
            WhatsApp = string.IsNullOrWhiteSpace(TxtWhatsApp.Text) ? "" : TxtWhatsApp.Text,
            Saldo = 0,
            Sincronizado = false,
            Descargado = false
        };

        // For IdCliente, if it's not AutoIncrement we might need to query the max ID, 
        // but SQLite handles "INTEGER PRIMARY KEY" implicitly as alias for rowid (autoincrement) 
        // if not provided, assuming IdCliente is integer.
        // If it throws, we could manually assign IdCliente.

        await App.Database.SaveClienteAsync(nuevoCliente);
        await Navigation.PopAsync();
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
