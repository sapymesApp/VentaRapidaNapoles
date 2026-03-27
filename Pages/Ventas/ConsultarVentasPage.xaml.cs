using MauiApp6.Models;

namespace MauiApp6;

public partial class ConsultarVentasPage : ContentPage
{
	public ConsultarVentasPage()
	{
		InitializeComponent();
        DpFecha.Date = DateTime.Now; // Por defecto hoy
    }



    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarVentas();
    }

    private async void OnNuevaVentasClicked(object sender, EventArgs e)
    {
        // Le pedimos a MAUI que resuelva y construya la página con todas sus dependencias
        var paginaNuevaVenta = Handler.MauiContext.Services.GetService<NuevaVentasPage>();

        // Ahora sí, navegamos a la instancia ya construida
        await Navigation.PushAsync(paginaNuevaVenta);
    }

    //private async void OnVolverClicked(object sender, EventArgs e)
    //{
    //    await Navigation.PopAsync();
    //}

    private async void OnDateSelected(object sender, DateChangedEventArgs e) => await CargarVentas();

    bool _mostrarSoloConSaldo = false;

    private async Task CargarVentas()
    {
        var lista = await App.Database.GetVentasPorFechaAsync(DpFecha.Date);
        var clientes = await App.Database.GetClientesAsync();

        foreach (var ventas in lista)
        {
            var cliente = clientes.FirstOrDefault(c => c.IdCliente == ventas.idCliente);
            if (cliente != null)
            {
                ventas.ClienteNombre = cliente.Nombre;
            }
            else
            {
                ventas.ClienteNombre = "Público General"; // Valor por defecto si no hay cliente (o fue eliminado)
            }
        }

        var listaFiltrada = lista;
        if (_mostrarSoloConSaldo)
        {
            listaFiltrada = lista.Where(v => v.Saldo > 0).ToList();
        }

        CvVentas.ItemsSource = listaFiltrada;
    }

    private async void OnMostrarConSaldoToggled(object sender, ToggledEventArgs e)
    {
        _mostrarSoloConSaldo = e.Value;
        await CargarVentas();
    }

    private async void OnVentasSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Ventas seleccionada)
        {
            // Navegamos a la pantalla de detalle pasando el objeto de la ventas
            await Navigation.PushAsync(new DetalleVentasPage(seleccionada));

            // Limpiar selección para que pueda volver a tocar el mismo item
            CvVentas.SelectedItem = null;
        }
    }

    private async void OnVenta1Tapped(object sender, EventArgs e) => await AbrirNuevaVenta(1);
    private async void OnVenta2Tapped(object sender, EventArgs e) => await AbrirNuevaVenta(2);
    private async void OnVenta3Tapped(object sender, EventArgs e) => await AbrirNuevaVenta(3);

    private async Task AbrirNuevaVenta(int numeroVenta)
    {
        var paginaNuevaVenta = Handler.MauiContext.Services.GetService<NuevaVentasPage>();
        paginaNuevaVenta.VentaNumero = numeroVenta;
        await Navigation.PushAsync(paginaNuevaVenta);
    }
}