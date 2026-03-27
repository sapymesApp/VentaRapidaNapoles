using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Views;

namespace MauiApp6;

public partial class CobroPopup : Popup
{
    private decimal _total;

    public CobroPopup(decimal total)
    {
        InitializeComponent();
        _total = total;
        LblTotal.Text = _total.ToString("C");
    }

    private void OnPagoTextChanged(object sender, TextChangedEventArgs e)
    {
        if (decimal.TryParse(e.NewTextValue, out decimal pago))
        {
            decimal cambio = pago - _total;
            if (cambio >= 0)
            {
                LblCambio.Text = cambio.ToString("C");
                LblCambio.TextColor = Color.FromArgb("#10B981"); // Verde
                BtnAceptar.IsEnabled = true;
            }
            else
            {
                LblCambio.Text = "$0.00";
                LblCambio.TextColor = Color.FromArgb("#DC2626"); // Rojo
                BtnAceptar.IsEnabled = false;
            }
        }
        else
        {
            LblCambio.Text = "$0.00";
            LblCambio.TextColor = Color.FromArgb("#DC2626");
            BtnAceptar.IsEnabled = false;
        }
    }

    private void OnCancelarClicked(object sender, EventArgs e)
    {
        Close(false);
    }

    private void OnAceptarClicked(object sender, EventArgs e)
    {
        Close(true);
    }

    private async void OnPopupOpened(object sender, CommunityToolkit.Maui.Core.PopupOpenedEventArgs e)
    {
        await Task.Delay(200);
        TxtPago.Focus();
    }
}
