using MauiApp6.Models;

namespace MauiApp6;

public partial class NuevoProductoPage : ContentPage
{
	public NuevoProductoPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // ⏱️ Pequeño truco para Android
        // A veces Android necesita una fracción de segundo para dibujar la pantalla 
        // antes de poder lanzar el teclado. Dispatcher ayuda con eso.
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
        {
            TxtDescripcion.Focus();
        });
    }



    // Este método salta del nombre al precio
    private void OnDescripcionCompleted(object sender, EventArgs e)
    {
        // Le pasamos la estafeta al campo de precio
        TxtPrecio.Focus();
    }

    // 💡 TRUCO EXTRA: Si el usuario presiona "Listo" en el precio, podemos guardar automáticamente
    private void OnPrecioCompleted(object sender, EventArgs e)
    {
        // Llamamos directamente a tu método de guardar, ahorrándole el clic en el botón
        OnGuardarClicked(sender, e);
    }


    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        // 1. Validaciones básicas
        if (string.IsNullOrWhiteSpace(TxtDescripcion.Text))
        {
            await DisplayAlert("Error", "Escribe una descripción", "OK");
            return;
        }

        if (!decimal.TryParse(TxtPrecio.Text, out decimal precio))
        {
            await DisplayAlert("Error", "El precio no es válido", "OK");
            return;
        }

        // 2. Crear objeto
        var nuevoProd = new Producto
        {
            Descripcion = TxtDescripcion.Text.ToUpper(),
            Precio1 = precio
        };

        // 3. Guardar en BD
        await App.Database.SaveProductoAsync(nuevoProd);

        // 4. Cerrar ventana (volver atrás)
        await Navigation.PopAsync();
    }

}