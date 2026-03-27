using MauiApp6.Models;
using System.Collections.ObjectModel;

namespace MauiApp6;

public partial class ConsultarAbonosPage : ContentPage
{
    public ConsultarAbonosPage()
    {
        InitializeComponent();
        DpFecha.Date = DateTime.Now; // Por defecto hoy
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarAbonos();
    }

    private async void OnDateSelected(object sender, DateChangedEventArgs e) => await CargarAbonos();

    private async Task CargarAbonos()
    {
        var abonos = await App.Database.GetAbonosPorFechaAsync(DpFecha.Date);
        var clientes = await App.Database.GetClientesAsync();
        
        var abonosConDetalle = new List<AbonoConDetalle>();

        foreach (var abono in abonos)
        {
            var cliente = clientes.FirstOrDefault(c => c.IdCliente == abono.idCliente);
            
            abonosConDetalle.Add(new AbonoConDetalle
            {
                IdAbono = abono.idAbono,
                VentasId = abono.idVenta,
                ClienteNombre = cliente != null ? cliente.Nombre : "Público General",
                Fecha = abono.Fecha,
                AbonoMonto = abono.Abono
            });
        }

        CvAbonos.ItemsSource = abonosConDetalle;
    }
}
