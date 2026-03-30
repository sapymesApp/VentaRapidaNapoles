using CommunityToolkit.Maui.Views;
using MauiApp6.Models;
using MauiApp6.Services;

namespace MauiApp6;

public partial class NuevaVentasPage : ContentPage
{


    private readonly ServicioImpresionTickets _servicioImpresion;
    private readonly ServicioImpresionTickets58mm _servicioImpresion58mm;
    private int _ventasIdActual = 0;
    private bool _hayCambiosSinGuardar = false;
    private int _ventaNumero = 1;
    public int VentaNumero 
    { 
        get => _ventaNumero; 
        set 
        { 
            _ventaNumero = value;
            ActualizarColoresVenta();
        } 
    }

    private Color ObtenerColorPrimario()
    {
        return _ventaNumero switch
        {
            1 => Color.FromArgb("#10B981"),
            2 => Color.FromArgb("#F59E0B"),
            3 => Color.FromArgb("#3B82F6"),
            _ => Color.FromArgb("#10B981")
        };
    }

    private void ActualizarColoresVenta()
    {
        if (BtnRegistrarVenta == null) return;

        var colorPrimario = ObtenerColorPrimario();
        BtnRegistrarVenta.PrimaryColor = colorPrimario;

        switch (_ventaNumero)
        {
            case 1:
                BtnRegistrarVenta.CardBackgroundColor = Color.FromArgb("#F0FFF4");
                break;
            case 2:
                BtnRegistrarVenta.CardBackgroundColor = Color.FromArgb("#FFFBEB");
                break;
            case 3:
                BtnRegistrarVenta.CardBackgroundColor = Color.FromArgb("#EFF6FF");
                break;
        }

        if (ContenedorCatalogo.IsVisible)
        {
            TabCatalogo.BackgroundColor = colorPrimario;
        }
        else
        {
            TabCarrito.BackgroundColor = colorPrimario;
        }
    }
    List<Producto> ProductosLista;
    List<Producto> CarritoLista = new List<Producto>();


    public NuevaVentasPage(ServicioImpresionTickets servicioImpresion, ServicioImpresionTickets58mm servicioImpresion58mm)
	{
		InitializeComponent();
        CargarCatalogo();
        _servicioImpresion = servicioImpresion;
        _servicioImpresion58mm = servicioImpresion58mm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        string vistaGuardada = Preferences.Default.Get("VistaCatalogo", "Grid");
        if (vistaGuardada == "Lista")
        {
            CvProductosGrid.IsVisible = false;
            CvProductosLista.IsVisible = true;
        }
        else
        {
            CvProductosGrid.IsVisible = true;
            CvProductosLista.IsVisible = false;
        }
    }




    private async void CargarCatalogo()
    {
        ProductosLista = await App.Database.GetProductosAsync();
        _hayCambiosSinGuardar = false;


        var borrador = await App.Database.ObtenerVentasBorradorAsync(VentaNumero);

        if (borrador != null)
        {
            // Guardamos el ID para saber que estamos sobreescribiendo este borrador
            // y no creando uno nuevo cuando le demos a Guardar otra vez.
            _ventasIdActual = borrador.Id;
            _hayCambiosSinGuardar = true;


            // 3. Traemos las partidas de este borrador
            var detallesBorrador = await App.Database.ObtenerDetallesPorVentasIdAsync(borrador.Id);

            // 4. MAPEO: Traemos los detalles al carrito
            foreach (var detalleGuardado in detallesBorrador)
            {
                var prodBase = ProductosLista.FirstOrDefault(p => p.IdInventario == detalleGuardado.ProductoId);
                if (prodBase != null)
                {
                    CarritoLista.Add(new Producto
                    {
                        IdInventario = prodBase.IdInventario,
                        Descripcion = prodBase.Descripcion,
                        ImagenBase64 = prodBase.ImagenBase64,
                        Precio1 = prodBase.Precio1,
                        Precio2 = prodBase.Precio2,
                        Precio3 = prodBase.Precio3,
                        Precio4 = prodBase.Precio4,
                        Costo = prodBase.Costo,
                        PrecioVenta = detalleGuardado.Precio,
                        CantidadCapturada = detalleGuardado.Cantidad
                    });
                }
            }

            //borramos el borrador
            await App.Database.EliminarVentasCompletaAsync(_ventasIdActual);


            // Opcional: Avisar al usuario que se recuperó información
            var toast = CommunityToolkit.Maui.Alerts.Toast.Make("Borrador recuperado");
            await toast.Show();

            
        }
        else
        {
            _ventasIdActual = 0; // Es una ventas limpia
        }


 
        CvProductosGrid.ItemsSource = ProductosLista;
        CvProductosLista.ItemsSource = ProductosLista;
        CvCarrito.ItemsSource = CarritoLista;
        CalcularTotal();
    }

    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        // Obtenemos lo que el usuario escribió (y lo pasamos a minúsculas para comparar fácil)
        string filtro = SbProductos.Text?.ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(filtro))
        {
            // Si borró todo, regresamos a la lista original completa
            CvProductosGrid.ItemsSource = ProductosLista;
            CvProductosLista.ItemsSource = ProductosLista;
        }
        else
        {
            // Filtramos la lista maestra buscando coincidencias en la Descripción
            // Usamos .ToList() para crear una nueva lista filtrada
            var resultadosFiltrados = ProductosLista
                .Where(p => p.Descripcion.ToLower().Contains(filtro))
                .ToList();

            CvProductosGrid.ItemsSource = resultadosFiltrados;
            CvProductosLista.ItemsSource = resultadosFiltrados;
        }
    }



    private void OnProductoTapped(object sender, EventArgs e)
    {
        var productCard = (MauiApp6.Controls.ProductCard)sender;
        if (productCard.BindingContext is Producto producto)
        {
            ProcesarSeleccionProducto(producto);
        }
    }

    private void OnProductoListaTapped(object sender, EventArgs e)
    {
        var border = (Microsoft.Maui.Controls.Border)sender;
        if (border.BindingContext is Producto producto)
        {
            ProcesarSeleccionProducto(producto);
        }
    }

    private async void ProcesarSeleccionProducto(Producto producto)
    {
        var popup = new CantidadPopup(producto.Descripcion, producto.esGranel, producto.Precio1, producto.Precio2, producto.Precio3, producto.Precio4);
        var popupResult = await this.ShowPopupAsync(popup);

        if (popupResult is CantidadPrecioResult result)
        {
            _hayCambiosSinGuardar = true;

            var nuevoItem = new Producto
            {
                IdInventario = producto.IdInventario,
                Descripcion = producto.Descripcion,
                ImagenBase64 = producto.ImagenBase64,
                Precio1 = producto.Precio1,
                Precio2 = producto.Precio2,
                Precio3 = producto.Precio3,
                Precio4 = producto.Precio4,
                Costo = producto.Costo,
                CantidadCapturada = result.Cantidad,
                PrecioVenta = result.Precio > 0 ? result.Precio : producto.PrecioVenta
            };

            CarritoLista.Add(nuevoItem);
            
            CvCarrito.ItemsSource = null;
            CvCarrito.ItemsSource = CarritoLista;

            CalcularTotal();
        }
    }

    private void OnTabCatalogoTapped(object sender, EventArgs e)
    {
        TabCatalogo.BackgroundColor = ObtenerColorPrimario();
        LblTabCatalogo.TextColor = Colors.White;
        
        TabCarrito.BackgroundColor = Color.FromArgb("#F3F4F6");
        LblTabCarrito.TextColor = Color.FromArgb("#4B5563");

        ContenedorCatalogo.IsVisible = true;
        ContenedorCarrito.IsVisible = false;
    }

    private void OnTabCarritoTapped(object sender, EventArgs e)
    {
        TabCarrito.BackgroundColor = ObtenerColorPrimario();
        LblTabCarrito.TextColor = Colors.White;
        
        TabCatalogo.BackgroundColor = Color.FromArgb("#F3F4F6");
        LblTabCatalogo.TextColor = Color.FromArgb("#4B5563");

        ContenedorCatalogo.IsVisible = false;
        ContenedorCarrito.IsVisible = true;
    }

    private void CalcularTotal()
    {
        decimal total = CarritoLista.Sum(p => p.Subtotal);
        LblTotal.Text = total.ToString("C");
    }

    //private async void OnGuardarClicked(object sender, EventArgs e)
    //{
     
    //}



    protected override bool OnBackButtonPressed()
    {

        if (CargaOverlay.IsVisible)
        {
            return true;
        }


        // Si NO hay cambios, deja que se salga normal
        if (!_hayCambiosSinGuardar) // o si tu validación es: si la lista está vacía
        {
            return base.OnBackButtonPressed();
        }

        // Si HAY cambios, detenemos la ventas y desplegamos el menú
        Dispatcher.Dispatch(async () =>
        {
            // DisplayActionSheet da múltiples opciones
            string accion = await DisplayActionSheet(
                "Tienes registros capturados", // Título
                "Cancelar",
                "Guardar y Salir",// Botón de cancelar (se queda en la página)
                "Descartar y Salir"           // Botón destructivo (en rojo en iOS)
                              // Nuestra nueva opción estrella 🌟
            );

            // Evaluamos qué eligió el usuario
            if (accion == "Guardar y Salir")
            {




                // 1. Mostrar que estamos trabajando
                var toast = CommunityToolkit.Maui.Alerts.Toast.Make("Guardando borrador...");
                await toast.Show();

                ////borramos el borrador
                //await App.Database.BorrarDetallesPorVentasIdAsync(_ventasIdActual);


                var itemsASalir = CarritoLista;


                // 1. Crear Cabecera
                var ventas = new Ventas
                {
                    Fecha = DateTime.Now,
                    Total = itemsASalir.Sum(p => p.Subtotal),
                    Status = "Borrador",
                    VentaNumero = VentaNumero
                };
                int ventasId = await App.Database.SaveVentasAsync(ventas);

                // 2. Crear Detalles
                foreach (var item in itemsASalir)
                {
                    var detalle = new VentasDetalle
                    {
                        VentasId = ventasId,
                        ProductoId = item.IdInventario,
                        Descripcion = item.Descripcion,
                        Cantidad = item.CantidadCapturada,
                        Precio = item.PrecioVenta
                    };
                    await App.Database.SaveDetalleAsync(detalle);
                }


                // 2. Aquí llamas a tu método que ya tienes para guardar la ventas
                // await GuardarVentasCompleta(); 

                // 3. Bajamos la bandera y salimos
                _hayCambiosSinGuardar = false;
                await Navigation.PopAsync();
            }
            else if (accion == "Descartar y Salir")
            {
                // Si elige perder los datos, bajamos la bandera y salimos
                _hayCambiosSinGuardar = false;
                await Navigation.PopAsync();
            }
            // Si elige "Cancelar" o toca fuera del menú, no hacemos nada y se queda en la pantalla
        });

        // Retornamos TRUE para bloquear la ventas automática
        return true;
    }

    private async void MenuButton_Tapped(object sender, EventArgs e)
    {
        var itemsASalir = CarritoLista;

        if (!itemsASalir.Any())
        {
            await DisplayAlert("Aviso", "No has capturado ninguna cantidad.", "OK");
            return;
        }

        decimal totalVenta = itemsASalir.Sum(p => p.Subtotal);

        // Mostrar popup de cobro para ingresar la cantidad pagada
        var cobroPopup = new CobroPopup(totalVenta);
        var resultCobro = await this.ShowPopupAsync(cobroPopup);

        if (resultCobro is not bool aceptado || !aceptado)
        {
            return; // El usuario canceló o cerró el popup de cobro
        }

        // Obtener automáticamente el cliente por defecto (Mostrador)
        var clientes = await App.Database.GetClientesAsync();
        var clienteSeleccionado = clientes.FirstOrDefault();

        if (clienteSeleccionado == null)
        {
            await DisplayAlert("Error", "No se encontró ningún cliente registrado.", "OK");
            return;
        }


        CargaOverlay.IsVisible = true;

        // 1. Crear Cabecera
        var ventas = new Ventas
        {
            idCliente = clienteSeleccionado.IdCliente,
            Fecha = DateTime.Now,
            Total = itemsASalir.Sum(p => p.Subtotal),
            Saldo = itemsASalir.Sum(p => p.Subtotal),
        };



        int ventasId = await App.Database.SaveVentasAsync(ventas);



        if (clienteSeleccionado.Credito == false)
        {
            ventas.Saldo = 0;
            ventas.Status = "Saldada";

            await App.Database.UpdateVentasAsync(ventas);


            int idEmpresa = Preferences.Get("IdEmpresa", 0);

            var nuevoAbono = new Abonos
            {
                idEmpresa=idEmpresa,
                idVenta = ventasId,
                idCliente = clienteSeleccionado.IdCliente,
                Fecha = DateTime.Now,
                Abono = ventas.Total,
                Sincronizado = false
            };
            await App.Database.SaveAbonoAsync(nuevoAbono);
        }



        // 1.5 Actualizar el saldo del cliente
        clienteSeleccionado.Saldo += ventas.Total;
        await App.Database.UpdateClienteAsync(clienteSeleccionado);

        // 2. Crear Detalles
        foreach (var item in itemsASalir)
        {
            var detalle = new VentasDetalle
            {
                VentasId = ventasId,
                ProductoId = item.IdInventario,
                Descripcion = item.Descripcion,
                Cantidad = item.CantidadCapturada,
                Precio = item.PrecioVenta,
                Costo = item.Costo,
            };
            await App.Database.SaveDetalleAsync(detalle);
        }



        //await DisplayAlert("Éxito", "Venta registrada correctamente.", "OK");

        // --- LÓGICA DE IMPRESIÓN ---
        try
        {
            string printerName = Preferences.Default.Get("ImpresoraGuardada", string.Empty);
            if (!string.IsNullOrEmpty(printerName))
            {
                // Le pasamos los objetos que acabas de crear/guardar
                // (ventas, clienteSeleccionado, y tu lista itemsASalir simulando los detalles)

                // Nota: Convertimos itemsASalir a VentasDetalle para que coincida con la firma del método
                var detallesImpresion = itemsASalir.Select(item => new VentasDetalle
                {
                    Descripcion = item.Descripcion,
                    Cantidad = item.CantidadCapturada,
                    Precio = item.PrecioVenta
                }).ToList();

                string tamanoPapel = Preferences.Default.Get("TamanoPapel", "80mm");

                if (tamanoPapel == "58mm")
                {
                    await _servicioImpresion58mm.ImprimirTicketAsync(
                        printerName,
                        ventas,
                        clienteSeleccionado,
                        detallesImpresion);
                }
                else
                {
                    await _servicioImpresion.ImprimirTicketAsync(
                        printerName,
                        ventas,
                        clienteSeleccionado,
                        detallesImpresion);
                }
            }
            else
            {
                await DisplayAlert("Aviso", "No hay impresora configurada.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error de Impresión", ex.Message, "OK");
        }

        finally
        {
            CargaOverlay.IsVisible = false;
        }

        await Navigation.PopAsync();


        //await DisplayAlert("Éxito", "Ventas registrada correctamente.", "OK");
        //await Navigation.PopAsync();
    }

    private void OnProductoTapped(object sender, TappedEventArgs e)
    {

    }

    private async void OnEliminarProductoInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is Producto producto)
        {
            bool respuesta = await DisplayAlert("Confirmar", $"¿Deseas eliminar '{producto.Descripcion}' del carrito?", "Sí, eliminar", "No");

            if (respuesta)
            {
                _hayCambiosSinGuardar = true;
                CarritoLista.Remove(producto);

                CvCarrito.ItemsSource = null;
                CvCarrito.ItemsSource = CarritoLista;

                CalcularTotal();
            }
        }
    }

    //protected override bool OnBackButtonPressed()
    //{
    //    // Si NO hay cambios, deja que se salga normal (retorna false)
    //    if (!_hayCambiosSinGuardar)
    //    {
    //        return base.OnBackButtonPressed();
    //    }

    //    // Si HAY cambios, detenemos la ventas y preguntamos
    //    Dispatcher.Dispatch(async () =>
    //    {
    //        bool respuesta = await DisplayAlert(
    //            "⚠️ ¿Salir sin guardar?",
    //            "Tienes cambios pendientes en la ventas. Si sales ahora, perderás lo que capturaste.",
    //            "Salir de todos modos", // Botón destructivo
    //            "Cancelar" // Botón para quedarse
    //        );

    //        if (respuesta)
    //        {
    //            // Si el usuario dice "Sí, quiero salir":
    //            _hayCambiosSinGuardar = false; // Bajamos la bandera para que no se cicle
    //            await Navigation.PopAsync();   // Nos vamos manualmente
    //        }
    //    });

    //    // Retornamos TRUE para decirle al sistema: "¡Espera! Yo manejo el botón atrás, no te salgas todavía"
    //    return true;
    //}


}