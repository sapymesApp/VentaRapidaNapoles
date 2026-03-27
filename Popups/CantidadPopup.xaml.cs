namespace MauiApp6;
using CommunityToolkit.Maui.Views;

public class CantidadPrecioResult
{
    public decimal Cantidad { get; set; }
    public decimal Precio { get; set; }
}

public partial class CantidadPopup : CommunityToolkit.Maui.Views.Popup
{
    private bool _usaPrecios = false;

    public CantidadPopup(string nombre, decimal? precio1 = null, decimal? precio2 = null, decimal? precio3 = null, decimal? precio4 = null)
    {
        InitializeComponent();
        LblProducto.Text = nombre;

        int preciosCount = 0;
        
        if (precio1.HasValue && precio1.Value > 0)
        {
            BtnPrecio1.Text = precio1.Value.ToString("0.##");
            BtnPrecio1.IsVisible = true;
            preciosCount++;
        }
        if (precio2.HasValue && precio2.Value > 0)
        {
            BtnPrecio2.Text = precio2.Value.ToString("0.##");
            BtnPrecio2.IsVisible = true;
            preciosCount++;
        }
        if (precio3.HasValue && precio3.Value > 0)
        {
            BtnPrecio3.Text = precio3.Value.ToString("0.##");
            BtnPrecio3.IsVisible = true;
            preciosCount++;
        }
        if (precio4.HasValue && precio4.Value > 0)
        {
            BtnPrecio4.Text = precio4.Value.ToString("0.##");
            BtnPrecio4.IsVisible = true;
            preciosCount++;
        }

        if (preciosCount > 0)
        {
            _usaPrecios = true;
            BotonesPrecios.IsVisible = true;
            BtnAceptar.IsVisible = false;
        }
        else
        {
            _usaPrecios = false;
            BotonesPrecios.IsVisible = false;
            BtnAceptar.IsVisible = true;
        }

        // No longer need to focus the entry as we have a custom keypad
    }

    private void OnTxtCantidadCompleted(object sender, EventArgs e)
    {
        if (!_usaPrecios)
        {
            OnAceptarClicked(sender, e);
        }
    }

    private void OnPrecioClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            decimal.TryParse(TxtCantidad.Text, out decimal cantidad);
            decimal.TryParse(btn.Text, out decimal precio);

            var result = new CantidadPrecioResult
            {
                Cantidad = cantidad,
                Precio = precio
            };

            Close(result);
        }
    }

    private void OnAceptarClicked(object sender, EventArgs e)
    {
        decimal.TryParse(TxtCantidad.Text, out decimal cantidad);
        
        var result = new CantidadPrecioResult
        {
            Cantidad = cantidad,
            Precio = 0
        };
            
        Close(result); // Cierra el popup devolviendo el valor
    }

    private void OnPopupOpened(object sender, CommunityToolkit.Maui.Core.PopupOpenedEventArgs e)
    {
        // No auto-focus needed for custom keypad
    }

    private void OnDigitClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            string digit = btn.Text;
            string currentText = TxtCantidad.Text ?? "";

            if (digit == "." && currentText.Contains("."))
                return;

            if (currentText == "0" && digit != ".")
                currentText = digit;
            else if (string.IsNullOrEmpty(currentText) && digit == ".")
                currentText = "0.";
            else
                currentText += digit;

            TxtCantidad.Text = currentText;
        }
    }

    private void OnBackspaceClicked(object sender, EventArgs e)
    {
        string currentText = TxtCantidad.Text ?? "";
        if (currentText.Length > 0)
        {
            currentText = currentText.Substring(0, currentText.Length - 1);
            if (string.IsNullOrEmpty(currentText))
                currentText = "";
            TxtCantidad.Text = currentText;
        }
    }
}
