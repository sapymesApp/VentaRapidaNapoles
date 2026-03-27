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
    }


    // Este método se dispara justo cuando la pantalla se vuelve visible
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Esperamos un brevísimo instante para asegurar que el control esté listo
        await Task.Delay(100);

        if (TxtPrecio.Text != null)
        {
            // 1. Mandamos el foco al Entry para que salga el teclado
            TxtPrecio.Focus();

            // 2. Colocamos el cursor al inicio
            TxtPrecio.CursorPosition = 0;

            // 3. Seleccionamos toda la longitud del texto
            TxtPrecio.SelectionLength = TxtPrecio.Text.Length;
        }
    }

    //private async void OnActualizarClicked(object sender, EventArgs e)
    //{

        
    //}

    private async void MenuButton_Tapped(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtDescripcion.Text))
        {
            await DisplayAlert("Falta información", "La descripción del producto no puede estar vacía.", "Entendido");

            // Truco de UX: Regresamos el cursor al campo para que escriba
            TxtDescripcion.Focus();
            return; // 🛑 DETENEMOS TODO AQUÍ
        }


        if (decimal.TryParse(TxtPrecio.Text, out decimal nuevoPrecio) && decimal.TryParse(TxtCosto.Text, out decimal nuevoCosto))
        {
            // Actualizamos solo el precio en nuestro objeto
            _producto.Descripcion = TxtDescripcion.Text.ToUpper();
            _producto.Precio1 = nuevoPrecio;
            _producto.Costo = nuevoCosto;

            // Guardamos el cambio en la base de datos
            await App.Database.UpdateProductoAsync(_producto);

            var toast = Toast.Make("✅ Producto actualizado correctamente", ToastDuration.Short, 14);

            // Mostramos el Toast y esperamos a que termine antes de cerrar la página
            // (Esto asegura que el usuario lea el mensaje antes de que la pantalla cambie)
            await toast.Show();

            // Regresamos a la lista
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Por favor ingresa un precio y costo válidos.", "OK");
        }
    }
}