using MauiApp6.Models;

namespace MauiApp6;

public partial class CortesPage : ContentPage
{
    public CortesPage()
    {
        InitializeComponent();

        // Defaults to today
        DpInicio.Date = DateTime.Today;
        DpFin.Date = DateTime.Today;
    }

    private async void OnCalcularCorteClicked(object sender, EventArgs e)
    {
        DateTime fechaInicio = DpInicio.Date;
        DateTime fechaFin = DpFin.Date;

        if (fechaFin < fechaInicio)
        {
            await DisplayAlert("Error", "La fecha de fin no puede ser menor a la fecha de inicio.", "OK");
            return;
        }

        // 1. Mostrar estado de carga (opcional)
        SlResultados.IsVisible = false;

        // 2. Traer datos
        var ventas = await App.Database.GetVentasPorRangoFechaAsync(fechaInicio, fechaFin);
        var abonos = await App.Database.GetAbonosPorRangoFechaAsync(fechaInicio, fechaFin);

        // 3. Calcular
        decimal totalVentas = ventas.Sum(v => v.Total);
        decimal totalAbonos = abonos.Sum(a => a.Abono);
        decimal granTotal = totalVentas + totalAbonos;

        // 4. Mostrar en UI
        LblTotalVentas.Text = totalVentas.ToString("C");
        LblTotalAbonos.Text = totalAbonos.ToString("C");
        //LblGranTotal.Text = granTotal.ToString("C");

        // 5. Hacer visible la tarjeta de resultados
        SlResultados.IsVisible = true;
    }
}
