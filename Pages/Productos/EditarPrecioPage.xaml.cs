using MauiApp6.Models;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace MauiApp6;

public partial class EditarPrecioPage : ContentPage
{
    Producto _producto;

    public EditarPrecioPage(Producto producto)
	{
		InitializeComponent();

        _producto = producto;

        // Llenamos los campos con la información actual
        TxtDescripcion.Text = _producto.Descripcion;
        TxtCosto.Text = _producto.Costo.ToString();
        TxtPrecio.Text = _producto.Precio1.ToString();
        TxtPrecio2.Text = _producto.Precio2.ToString();
        TxtPrecio3.Text = _producto.Precio3.ToString();
        TxtPrecio4.Text = _producto.Precio4.ToString();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Delay(100);

        if (TxtPrecio.Text != null)
        {
            TxtPrecio.Focus();
            TxtPrecio.CursorPosition = 0;
            TxtPrecio.SelectionLength = TxtPrecio.Text.Length;
        }
    }

    private async void MenuButton_Tapped(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtDescripcion.Text))
        {
            await DisplayAlert("Falta información", "La descripción del producto no puede estar vacía.", "Entendido");
            TxtDescripcion.Focus();
            return;
        }

        bool p1Ok = decimal.TryParse(TxtPrecio.Text, out decimal p1);
        bool p2Ok = decimal.TryParse(TxtPrecio2.Text, out decimal p2);
        bool p3Ok = decimal.TryParse(TxtPrecio3.Text, out decimal p3);
        bool p4Ok = decimal.TryParse(TxtPrecio4.Text, out decimal p4);
        bool costoOk = decimal.TryParse(TxtCosto.Text, out decimal costo);

        if (p1Ok && p2Ok && p3Ok && p4Ok && costoOk)
        {
            _producto.Descripcion = TxtDescripcion.Text.ToUpper();
            _producto.Precio1 = p1;
            _producto.Precio2 = p2;
            _producto.Precio3 = p3;
            _producto.Precio4 = p4;
            _producto.Costo = costo;

            await App.Database.UpdateProductoAsync(_producto);

            var toast = Toast.Make("✅ Producto actualizado correctamente", ToastDuration.Short, 14);
            await toast.Show();

            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Por favor ingresa valores numéricos válidos en todos los campos de precio y costo.", "OK");
        }
    }
}
