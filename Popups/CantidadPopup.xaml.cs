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

    // 1. Declara estas dos variables al inicio de tu clase CantidadPopup
    private CancellationTokenSource _cts;
    private bool _ingresoManual = false;

    private bool _esGranel = false;


    private decimal _precioBase = 0;
    private bool _modoMonto = false; // Nos dirá si estamos capturando Kilos o Dinero


    public CantidadPopup(string nombre, bool esGranel, decimal? precio1 = null, decimal? precio2 = null, decimal? precio3 = null, decimal? precio4 = null)
    {
        InitializeComponent();
        LblProducto.Text = nombre;

        // Guardamos si es a granel
        _esGranel = esGranel;

        // Guardamos el precio 1 (el principal) para poder hacer la división matemática luego
        _precioBase = precio1 ?? 0;

        // Solo mostramos el botón de "Vender por precio" si es a granel y tiene un precio asignado
        if (_esGranel && _precioBase > 0)
        {
            BtnModoMonto.IsVisible = true;
        }

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


    // Opcional: Si el producto tiene habilitado BtnPrecio1, BtnPrecio2, etc., asegúrate de aplicar la misma división ahí:
    private void OnPrecioClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            _cts?.Cancel();

            decimal.TryParse(TxtCantidad.Text, out decimal valorCapturado);
            decimal.TryParse(btn.Text, out decimal precioSeleccionado);

            decimal cantidadFinal = valorCapturado;

            // Regla de tres usando el botón de precio específico que tocaron
            if (_modoMonto && precioSeleccionado > 0)
            {
                cantidadFinal = Math.Round(valorCapturado / precioSeleccionado, 3);
            }

            var result = new CantidadPrecioResult
            {
                Cantidad = cantidadFinal,
                Precio = precioSeleccionado
            };

            Close(result);
        }
    }

    //private void OnPrecioClicked(object sender, EventArgs e)
    //{
    //    if (sender is Button btn)
    //    {
    //        _cts?.Cancel(); // Detener el ciclo Bluetooth

    //        decimal.TryParse(TxtCantidad.Text, out decimal cantidad);
    //        decimal.TryParse(btn.Text, out decimal precio);
    //        var result = new CantidadPrecioResult { Cantidad = cantidad, Precio = precio };
    //        Close(result);
    //    }
    //}


    private void OnAceptarClicked(object sender, EventArgs e)
    {
        _cts?.Cancel(); // Siempre detenemos la báscula al salir

        decimal.TryParse(TxtCantidad.Text, out decimal valorCapturado);
        decimal cantidadFinal = valorCapturado;

        // Si el cajero tecleó dinero ($50) en vez de peso, hacemos la regla de 3
        if (_modoMonto && _precioBase > 0)
        {
            // Peso = Dinero / Precio por Kilo. (Ej. 50 / 100 = 0.500 Kg)
            // Redondeamos a 3 decimales porque son gramos
            cantidadFinal = Math.Round(valorCapturado / _precioBase, 3);
        }

        var result = new CantidadPrecioResult
        {
            Cantidad = cantidadFinal,
            Precio = 0
        };

        Close(result);
    }

    //private void OnAceptarClicked(object sender, EventArgs e)
    //{
    //    _cts?.Cancel(); // Detener el ciclo Bluetooth

    //    decimal.TryParse(TxtCantidad.Text, out decimal cantidad);

    //    var result = new CantidadPrecioResult
    //    {
    //        Cantidad = cantidad,
    //        Precio = 0
    //    };

    //    Close(result); // Cierra el popup devolviendo el valor
    //}

    private void OnModoMontoClicked(object sender, EventArgs e)
    {
        _modoMonto = !_modoMonto; // Cambiamos el estado (Toggle)

        if (_modoMonto)
        {
            LblEquivalencia.IsVisible = true;
            ActualizarEquivalenciaVisual();
            // === MODO DINERO ===
            _cts?.Cancel(); // Apagamos la lectura de la báscula Bluetooth inmediatamente
            _ingresoManual = true; // Tomamos el control

            LblInstruccion.Text = "Ingresa el importe a cobrar ($):";
            LblInstruccion.TextColor = Colors.DarkOrange;

            BtnModoMonto.Text = "VOLVER A LEER BÁSCULA";
            BtnModoMonto.BackgroundColor = Colors.Gray;

            TxtCantidad.Text = "0"; // Limpiamos la pantalla para que digiten el dinero
        }
        else
        {
            LblEquivalencia.IsVisible = false;
            // === REGRESAR A MODO BÁSCULA ===
            LblInstruccion.Text = "Ingresa la cantidad (KG):";
            LblInstruccion.TextColor = Colors.Gray;

            BtnModoMonto.Text = "VENDER POR PRECIO ($)";
            BtnModoMonto.BackgroundColor = Color.FromArgb("#FF9800");

            TxtCantidad.Text = "..."; // Preparamos pantalla
            _ingresoManual = false;

            // Volvemos a encender el Bluetooth
            _cts = new CancellationTokenSource();
            var bluetoothService = Application.Current.MainPage.Handler.MauiContext.Services.GetService<Services.IBluetoothService>();
            if (bluetoothService != null)
            {
                _ = bluetoothService.EscucharBasculaAsync("Mary3W", OnPesoRecibido, _cts.Token);
            }
        }
    }

    // 2. Modifica OnPopupOpened
    private async void OnPopupOpened(object sender, CommunityToolkit.Maui.Core.PopupOpenedEventArgs e)
    {
        if (!_esGranel)
        {
            // Sugerencia de UX: Si se vende por pieza, lo más común es llevar 1. 
            // Ponemos "1" por defecto para que el cajero solo tenga que darle a "Aceptar"
            TxtCantidad.Text = "";
            return; // Salimos de la función, el código de abajo (Bluetooth) ya no se ejecuta
        }



        TxtCantidad.Text = "...";
        _cts = new CancellationTokenSource();
        _ingresoManual = false;

        // 1. EL RESPIRO: Le damos 400 milisegundos a Android para liberar conexiones previas
        await Task.Delay(400);

        var bluetoothService = Application.Current.MainPage.Handler.MauiContext.Services.GetService<Services.IBluetoothService>();

        if (bluetoothService != null)
        {
            // Iniciamos la lectura en el fondo
            _ = bluetoothService.EscucharBasculaAsync("Mary3W", OnPesoRecibido, _cts.Token);

            // 2. EL SEGURO DE VIDA (Timeout): Si después de 2.5 segundos no conectó, liberamos la pantalla
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(2500, _cts.Token);

                    // Si el usuario no ha escrito nada y el texto sigue atascado en "..."
                    if (!_ingresoManual && TxtCantidad.Text == "...")
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            // Doble validación por si justo en ese milisegundo entró el peso
                            if (TxtCantidad.Text == "...")
                            {
                                TxtCantidad.Text = "0";
                            }
                        });
                    }
                }
                catch (TaskCanceledException)
                {
                    // Se ignora, ocurre si el usuario cierra el popup antes de los 2.5 segundos
                }
            });
        }
    }
    // 3. Crea el método que recibe las actualizaciones constantes
    private void OnPesoRecibido(string resultadoBruto)
    {
        // Si el usuario decide teclear algo manualmente, ignoramos la báscula
        if (_ingresoManual) return;

        // Actualizar la interfaz gráfica DEBE hacerse en el hilo principal
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!string.IsNullOrEmpty(resultadoBruto))
            {
                string soloNumeros = System.Text.RegularExpressions.Regex.Match(resultadoBruto, @"\d+(\.\d+)?").Value;

                if (!string.IsNullOrEmpty(soloNumeros))
                {
                    // LA MAGIA: Solo actualizamos y hacemos "beep" si el peso es DIFERENTE al que ya está en pantalla
                    if (TxtCantidad.Text != soloNumeros)
                    {
                        TxtCantidad.Text = soloNumeros;

                        // Si el peso es un valor válido (mayor a 0), hacemos sonar el Beep
                        if (soloNumeros != "0" && soloNumeros != "0.00" && soloNumeros != "...")
                        {
#if ANDROID
                        // Genera un Beep nativo de notificación en Android al 100% de volumen
                        var toneGen = new Android.Media.ToneGenerator(Android.Media.Stream.Notification, 100);
                        // El 150 indica que el sonido durará 150 milisegundos
                        toneGen.StartTone(Android.Media.Tone.PropBeep, 150); 
#endif
                        }
                    }
                }
            }
        });
    }

    private void OnDigitClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            // === LA INTERRUPCIÓN MÁGICA ===
            // Si es el primer botón que tocan y la báscula estaba activa...
            if (!_ingresoManual)
            {
                _ingresoManual = true; // Tomamos el control manual
                _cts?.Cancel();        // Apagamos la lectura Bluetooth por completo

                // Si estamos pesando kilos (no en modo dinero), limpiamos la pantalla
                if (!_modoMonto)
                {
                    TxtCantidad.Text = "";
                }
            }

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

            // Si tenemos la equivalencia en dinero activada, la actualizamos
            ActualizarEquivalenciaVisual();
        }
    }

    private void OnBackspaceClicked(object sender, EventArgs e)
    {
        // Si intentan borrar algo mientras la báscula estaba leyendo automáticamente
        if (!_ingresoManual)
        {
            _ingresoManual = true;
            _cts?.Cancel(); // Apagamos la báscula
            TxtCantidad.Text = "0"; // Dejamos en cero para que tecleen de nuevo
            ActualizarEquivalenciaVisual();
            return;
        }

        string currentText = TxtCantidad.Text ?? "";
        if (currentText.Length > 0)
        {
            currentText = currentText.Substring(0, currentText.Length - 1);

            if (string.IsNullOrEmpty(currentText))
                currentText = "0"; // Es mejor dejar un 0 que la pantalla en blanco al borrar todo

            TxtCantidad.Text = currentText;
        }

        ActualizarEquivalenciaVisual();
    }
    //private void OnDigitClicked(object sender, EventArgs e)
    //{
    //    _ingresoManual = true; // El usuario tomó el control

    //    if (sender is Button btn)
    //    {
    //        string digit = btn.Text;
    //        string currentText = TxtCantidad.Text ?? "";

    //        if (digit == "." && currentText.Contains("."))
    //            return;

    //        if (currentText == "0" && digit != ".")
    //            currentText = digit;
    //        else if (string.IsNullOrEmpty(currentText) && digit == ".")
    //            currentText = "0.";
    //        else
    //            currentText += digit;

    //        TxtCantidad.Text = currentText;
    //        ActualizarEquivalenciaVisual();
    //    }
    //}

    //private void OnBackspaceClicked(object sender, EventArgs e)
    //{
    //    _ingresoManual = true; // El usuario tomó el control

    //    string currentText = TxtCantidad.Text ?? "";
    //    if (currentText.Length > 0)
    //    {
    //        currentText = currentText.Substring(0, currentText.Length - 1);
    //        if (string.IsNullOrEmpty(currentText))
    //            currentText = "";
    //        TxtCantidad.Text = currentText;
    //        ActualizarEquivalenciaVisual();
    //    }
    //}


    private void ActualizarEquivalenciaVisual()
    {
        if (_modoMonto && _precioBase > 0)
        {
            // Leemos el dinero que han tecleado hasta el momento
            decimal.TryParse(TxtCantidad.Text, out decimal valorCapturado);

            // Calculamos los kilos
            decimal equivalencia = Math.Round(valorCapturado / _precioBase, 3);

            // Actualizamos el texto en pantalla (formato 0.000 para que siempre muestre los gramos)
            LblEquivalencia.Text = $"Equivale a: {equivalencia:0.000} Kg";
        }
    }
    //// Añadir este evento en la clase CantidadPopup
    //private async void OnLeerBasculaClicked(object sender, EventArgs e)
    //{
    //    try
    //    {
    //        // Indicador visual de que está leyendo
    //        TxtCantidad.Text = "...";
    //        //BtnLeerBascula.IsEnabled = false;

    //        // Obtenemos el servicio usando Dependency Injection de MAUI
    //        var bluetoothService = Application.Current.MainPage.Handler.MauiContext.Services.GetService<Services.IBluetoothService>();

    //        if (bluetoothService != null)
    //        {
    //            string resultadoBruto = await bluetoothService.LeerPesoAsync("Mary3W");

    //            if (!string.IsNullOrEmpty(resultadoBruto))
    //            {
    //                // Limpiamos la cadena para extraer SOLO números y el punto decimal
    //                // NUEVO CÓDIGO (Captura solo el primer peso encontrado)
    //                string soloNumeros = System.Text.RegularExpressions.Regex.Match(resultadoBruto, @"\d+(\.\d+)?").Value;

    //                if (!string.IsNullOrEmpty(soloNumeros))
    //                {
    //                    TxtCantidad.Text = soloNumeros;
    //                }
    //                else
    //                {
    //                    TxtCantidad.Text = "0";
    //                }
    //            }
    //            else
    //            {
    //                TxtCantidad.Text = "0";
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        // En caso de error (báscula apagada, fuera de rango) restauramos el valor
    //        TxtCantidad.Text = "0";
    //        // Opcional: mostrar una alerta si necesitas depurar
    //        // await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
    //    }
    //    finally
    //    {
    //        //BtnLeerBascula.IsEnabled = true;
    //    }
    //}



}
