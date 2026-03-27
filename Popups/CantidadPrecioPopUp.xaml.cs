namespace MauiApp6;
using CommunityToolkit.Maui.Views;

public partial class CantidadPrecioPopUp : CommunityToolkit.Maui.Views.Popup
{
    public class CantidadPrecioResult
    {
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
    }

    public CantidadPrecioPopUp(string nombre, decimal precioActual)
    {
        InitializeComponent();
        LblProducto.Text = nombre;
        TxtPrecio.Text = precioActual.ToString("0.##");
        
        // Truco: Poner el foco automticamente en cantidad
        Task.Run(async () => {
            await Task.Delay(100);
            MainThread.BeginInvokeOnMainThread(() => TxtCantidad.Focus());
        });
    }

    private void OnTxtCantidadCompleted(object sender, EventArgs e)
    {
        TxtPrecio.Focus();
    }

    private void OnAceptarClicked(object sender, EventArgs e)
    {
        decimal.TryParse(TxtCantidad.Text, out decimal cantidad);
        decimal.TryParse(TxtPrecio.Text, out decimal precio);
        
        var result = new CantidadPrecioResult 
        { 
            Cantidad = cantidad, 
            Precio = precio 
        };
        
        Close(result); // Cierra el popup devolviendo el valor
    }

    private async void OnPopupOpened(object sender, CommunityToolkit.Maui.Core.PopupOpenedEventArgs e)
    {
        // TRUCO: Esperamos 300ms a que la animacin de apertura termine.
        // Sin este delay, el teclado a veces no sube en Android.
        await Task.Delay(200);

        // 1. Ponemos el foco en la caja de texto (Sube el teclado)
        TxtCantidad.Focus();

        // 2. (Opcional) Seleccionamos todo el texto por si hay un "0" o "1" por defecto,
        // as el usuario escribe encima sin tener que borrar.
        TxtCantidad.CursorPosition = 0;
        TxtCantidad.SelectionLength = TxtCantidad.Text?.Length ?? 0;
    }
}
