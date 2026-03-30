using CommunityToolkit.Maui.Views;
using MauiApp6.Models;
using QuestPDF.Fluent;
//using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using System.IO;
using IContainer = QuestPDF.Infrastructure.IContainer;
using CommunityToolkit.Maui.Alerts; // Para crear el Toast
using CommunityToolkit.Maui.Core;
using MauiApp6.Services;   // Para definir la duraci�n (Corta/Larga)



namespace MauiApp6;

public partial class DetalleVentasPage : ContentPage
{
    Ventas _ventas;

    public DetalleVentasPage(Ventas ventas)
	{


        //QuestPDF.Settings.License = LicenseType.Community;

        InitializeComponent();



        _ventas = ventas;

        LblFolio.Text = $"Ventas #{_ventas.Id}";
        LblFecha.Text = _ventas.Fecha.ToString("f"); // Fecha larga
        LblStatus.Text = _ventas.Status.ToUpper();

        BtnAbonar.IsVisible = _ventas.Status != "Saldada" && _ventas.Status != "Finalizada";

        CargarDetalles();


        // L�GICA DE VISIBILIDAD DE BOTONES
        bool esFinalizada = _ventas.Status == "Saldada" || _ventas.Status == "Finalizada";

        BtnAbonar.IsVisible = !esFinalizada;
        //BtnImprimir.IsVisible = esFinalizada;
    }


    private async void CargarDetalles()
    {
        // Obtener datos del cliente para mostrar el nombre
        var clientes = await App.Database.GetClientesAsync();
        var cliente = clientes.FirstOrDefault(c => c.IdCliente == _ventas.idCliente);
        LblCliente.Text = cliente != null ? cliente.Nombre : "Público General";

        var detalles = await App.Database.GetDetallesByVentasIdAsync(_ventas.Id);
        CvDetalles.ItemsSource = detalles;

        decimal granTotal = detalles.Sum(d => d.ImporteTotal);

        LblGranTotal.Text = granTotal.ToString("C");

        // Format and show 'Saldo' properly
        LblSaldo.Text = $"Saldo: {_ventas.Saldo:C}";
    }



    private async Task ProcesarYEnviarPdf()
    {
        try
        {
            // 1. Mostrar que estamos trabajando (Opcional pero recomendado)
            // await DisplayAlert("Generando", "Creando documento...", "OK"); // O un ActivityIndicator

            // 2. Obtener datos frescos
            var detalles = await App.Database.GetDetallesByVentasIdAsync(_ventas.Id);

            // 3. Obtener el logo (Si implementaste el paso anterior)
            byte[] logo = await ObtenerLogoBytes();

            // 4. Generar el PDF
            string rutaPdf = GenerarArchivoPdf(_ventas, detalles, logo);

            if (string.IsNullOrEmpty(rutaPdf)) return;

            // 5. Mostrar Toast de �xito
            var toast = CommunityToolkit.Maui.Alerts.Toast.Make("Documento listo para enviar", CommunityToolkit.Maui.Core.ToastDuration.Short, 14);
            await toast.Show();

            // 6. Compartir
            await Task.Delay(500); // Peque�a pausa est�tica
            await CompartirArchivo(rutaPdf);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo generar el reporte: {ex.Message}", "OK");
        }
    }


    private async void OnImprimirClicked(object sender, EventArgs e)
    {
        // Simplemente llamamos a la funci�n compartida
        await ProcesarYEnviarPdf();
    }


    ////private async void OnFinalizarClicked(object sender, EventArgs e)
    //private async void OnAbonarClicked(object sender, EventArgs e)
    //{
    //    var popup = new CantidadPopup($"Abono para la ventas #{_ventas.Id}");
    //    var popupResult = await this.ShowPopupAsync(popup);
        
    //    if (popupResult is CantidadPrecioResult result && result.Cantidad > 0)
    //    {
    //        decimal cantAbono = result.Cantidad;
    //        if (cantAbono > _ventas.Saldo)
    //        {
    //            await DisplayAlert("Error", "El abono no puede ser mayor al saldo pendiente.", "OK");
    //            return;
    //        }

    //        _ventas.Abono += cantAbono;
    //        _ventas.Saldo -= cantAbono;

    //        if (_ventas.Saldo <= 0)
    //        {
    //            _ventas.Status = "Saldada";
    //            LblStatus.Text = "SALDADA";
    //            LblStatus.TextColor = Color.Parse("#4CAF50");
    //            BtnAbonar.IsVisible = false;
    //            //BtnImprimir.IsVisible = true;
    //        }
    //        else
    //        {
    //            // Update UI visually if still not 0
    //            CargarDetalles(); // it refreshes gran total, but maybe just reload ui manually
    //        }

    //        await App.Database.UpdateVentasAsync(_ventas);


    //        int idEmpresa = Preferences.Get("IdEmpresa", 0);
    //        int idDisposito = Preferences.Get("IdDispositivo", 0);

    //        var nuevoAbono = new MauiApp6.Models.Abonos { 
    //            idEmpresa= idEmpresa,
    //            idVenta = _ventas.Id, 
    //            idCliente = _ventas.idCliente, 
    //            idDispositivo= idDisposito,
    //            Fecha = DateTime.Now, 
    //            Abono = cantAbono 
    //        };
    //        await App.Database.SaveAbonoAsync(nuevoAbono);
    //        await DisplayAlert("�xito", $"Abono registrado correctamente. Saldo restante: {(_ventas.Saldo):C}", "OK");
            
    //        // Refrescar lista de detalles para que se vea reflejado (aunque el abono es a la ventas no al detalle, 
    //        // el header se puede actualizar as si es necesario, o podemos hacer PopAsync si se prefiere).
    //        CargarDetalles(); // refrescar cabecera/total visual
    //    }
    //}



    private async Task CompartirArchivo(string rutaArchivo)
    {
        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Compartir Reporte de Ventas",
            File = new ShareFile(rutaArchivo)
        });
    }



    private string GenerarArchivoPdf(Ventas ventas, List<VentasDetalle> detalles, byte[] logoBytes)
    {
        // Aceptamos la licencia de forma segura
        try { QuestPDF.Settings.License = LicenseType.Community; } catch { }


        // CONFIGURACI�N CR�TICA
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        // Le decimos: "No busques fuentes raras, usa la que tengas a la mano"
        // Esto evita el crash de inicializaci�n de fuentes
        QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;



        string nombreArchivo = $"Reporte_Ventas_{ventas.Id}.pdf";
        string rutaArchivo = Path.Combine(FileSystem.CacheDirectory, nombreArchivo);

        Document.Create(document =>
        {
            document.Page(page =>
            {
                // SUSTITUIMOS PageSizes.A4 por sus puntos exactos (595 x 842)
                page.Size(595, 842);

                // SUSTITUIMOS Unit.Centimetre por puntos (28 puntos es aprox 1cm)
                page.Margin(30);

                page.PageColor("#FFFFFF");

                // SOLUCI�N AL ERROR DE DefaultFontFamily: 
                // En lugar de global, lo aplicamos directo al estilo de la p�gina
                page.DefaultTextStyle(x => x.FontFamily("sans-serif").FontSize(10).FontColor("#000000"));

                // ENCABEZADO



                page.Header().Row(row =>
                {
                    // COLUMNA 1: EL LOGO (Si existe)
                    if (logoBytes != null)
                    {
                        // ConstantItem(80) define un ancho fijo de 80 puntos para la columna del logo
                        row.ConstantItem(80).Image(logoBytes).FitArea();
                    }

                    // COLUMNA 2: T�TULO Y DATOS (Usamos RelativeItem para que ocupe el resto)
                    row.RelativeItem().PaddingLeft(10).Column(col =>
                    {
                        col.Item().Text("TU EMPRESA S.A. de C.V.").FontSize(14).Bold().FontColor("#757575");
                        col.Item().Text("REPORTE DE SALIDA").FontSize(20).SemiBold().FontColor("#2196F3");

                        col.Item().Row(r =>
                        {
                            r.AutoItem().Text($"Folio: #{ventas.Id}  |  ").FontSize(12);
                            r.AutoItem().Text($"Fecha: {ventas.Fecha:dd/MM/yyyy}").FontSize(12);
                        });
                    });

                    // COLUMNA 3: ESTADO (A la derecha del todo)
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("ESTADO").FontSize(10).SemiBold().FontColor("#757575");
                        col.Item().Text(ventas.Status.ToUpper())
                                  .FontSize(14)
                                  .SemiBold()
                                  .FontColor(ventas.Status == "Finalizada" ? "#4CAF50" : "#2196F3");
                    });
                });




                // TABLA
                //page.Content().PaddingVertical(15).Table(table =>
                //{
                //    table.ColumnsDefinition(columns =>
                //    {
                //        columns.RelativeColumn(3);
                //        columns.RelativeColumn();
                //        columns.RelativeColumn();
                //        columns.RelativeColumn();
                //    });

                //    table.Header(header =>
                //    {
                //        EstiloCabecera(header.Cell()).Text("Producto");
                //        EstiloCabecera(header.Cell()).AlignCenter().Text("Cant.");
                //        EstiloCabecera(header.Cell()).AlignRight().Text("Precio");
                //        EstiloCabecera(header.Cell()).AlignRight().Text("Subtotal");
                //    });

                //    foreach (var item in detalles)
                //    {
                //        EstiloNormal(table.Cell()).Text(item.Descripcion);
                //        EstiloNormal(table.Cell()).AlignCenter().Text(item.Cantidad.ToString());
                //        EstiloNormal(table.Cell()).AlignRight().Text(item.Precio.ToString("C"));
                //        EstiloNormal(table.Cell()).AlignRight().Text(item.ImporteTotal.ToString("C"));
                //    }
                //});

                // CONTENIDO - TABLA DE LIQUIDACI�N
                page.Content().PaddingVertical(10).Table(table =>
                {
                    // 1. DEFINICI�N DE COLUMNAS (6 Columnas en total)
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);   // Cantidad (Fija, peque�a)
                        columns.RelativeColumn(3);    // Descripci�n (La m�s ancha)
                        columns.RelativeColumn(1);    // Precio
                        columns.ConstantColumn(40);   // Devoluci�n (Fija)
                        columns.ConstantColumn(40);   // Neto (Fija)
                        columns.RelativeColumn(1.2f); // Subtotal (Espacio para cifras)
                    });

                    // 2. CABECERA
                    table.Header(header =>
                    {
                        // Usamos EstiloCabecera (Alineaci�n centrada para n�meros, izquierda para texto)
                        EstiloCabecera(header.Cell()).AlignCenter().Text("Cant.");
                        EstiloCabecera(header.Cell()).Text("Descripci�n"); // Alineada a la izquierda
                        EstiloCabecera(header.Cell()).AlignRight().Text("Precio");
                        EstiloCabecera(header.Cell()).AlignCenter().Text("Dev.");
                        EstiloCabecera(header.Cell()).AlignCenter().Text("Neto");
                        EstiloCabecera(header.Cell()).AlignRight().Text("Total");
                    });

                    // 3. FILAS (C�lculos en tiempo real)
                    foreach (var item in detalles)
                    {
                        // L�gica de Negocio: Calcular el Neto y el Subtotal real
                        decimal cantidadNeta = item.Cantidad - item.Devolucion;
                        decimal subtotalReal = cantidadNeta * item.Precio;

                        // Visual: Si hubo devoluci�n, pintamos el n�mero en rojo para que resalte
                        string colorDev = item.Devolucion > 0 ? "#D32F2F" : "#000000"; // Rojo o Negro

                        // Columna 1: Cantidad Original
                        EstiloNormal(table.Cell()).AlignCenter().Text(item.Cantidad.ToString());

                        // Columna 2: Descripci�n
                        EstiloNormal(table.Cell()).Text(item.Descripcion);

                        // Columna 3: Precio Unitario
                        EstiloNormal(table.Cell()).AlignRight().Text(item.Precio.ToString("C"));

                        // Columna 4: Devoluci�n (Con color condicional)
                        EstiloNormal(table.Cell()).AlignCenter().Text(item.Devolucion.ToString()).FontColor(colorDev);

                        // Columna 5: Cantidad Neta (Lo que realmente se vendi�)
                        EstiloNormal(table.Cell()).AlignCenter().Text(cantidadNeta.ToString()).SemiBold();

                        // Columna 6: Subtotal Final (Dinero real)
                        EstiloNormal(table.Cell()).AlignRight().Text(subtotalReal.ToString("C")).SemiBold();
                    }
                });





                // PIE DE P�GINA
                page.Footer().PaddingTop(10).Column(col =>
                {
                    // Usamos un Row para poner elementos lado a lado (Logo Izquierda - Total Derecha)
                    col.Item().Row(row =>
                    {
                        // 1. EL LOGOTIPO (Alineado a la izquierda)
                        if (logoBytes != null)
                        {
                            // ConstantItem(40) define el ancho fijo (40 puntos = aprox 1.5 cm, bastante peque�o)
                            // AlignLeft() asegura que se quede pegado a la izquierda
                            row.ConstantItem(40).AlignLeft().Image(logoBytes).FitArea();
                        }

                        // 2. ESPACIADOR (Empuja el contenido siguiente hacia la derecha)
                        row.RelativeItem();

                        // 3. EL TOTAL (Alineado a la derecha)
                        row.AutoItem().AlignRight().Text(text =>
                        {
                            text.Span("TOTAL NETO: ").FontSize(12).SemiBold().FontColor("#757575");
                            text.Span(ventas.Total.ToString("C")).FontSize(16).Bold().FontColor("#2196F3");
                        });
                    });

                    // 4. TEXTO LEGAL (Debajo del logo y el total)
                    // Usamos una l�nea separadora tenue para que se vea elegante
                    col.Item().PaddingTop(5).LineHorizontal(0.5f).LineColor("#E0E0E0");

                    col.Item().PaddingTop(2).AlignLeft().Text("Este documento es un comprobante de ventas de almac�n.")
                        .FontSize(7).Italic().FontColor("#9E9E9E");
                });


            });
        })
        .GeneratePdf(rutaArchivo);

        return rutaArchivo;
    }


    private async Task<byte[]> ObtenerLogoBytes()
    {
        try
        {
            // "logo_empresa.png" debe coincidir con el nombre de tu archivo en Resources/Images
            using var stream = await FileSystem.OpenAppPackageFileAsync("sapymes.png");
            using var memoryStream = new MemoryStream();

            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        catch
        {
            // Si falla (no encuentra la imagen), retornamos null para que no truene la app
            return null;
        }
    }


    private IContainer EstiloCabecera(IContainer container)
    {
        return container
            .DefaultTextStyle(x => x.SemiBold())
            .PaddingVertical(5)
            .BorderBottom(1)
            .BorderColor("#000000");
    }

    private IContainer EstiloNormal(IContainer container)
    {
        return container.PaddingVertical(5)
                        .BorderBottom(0.5f)
                        .BorderColor("#00FF00");
    }

    private void OnActualizarClicked(object sender, EventArgs e)
    {

    }

    private async void btnReimprimir_Tapped(object sender, EventArgs e)
    {
        // --- LÓGICA DE IMPRESIÓN ---
        try
        {
            string printerName = Preferences.Default.Get("ImpresoraGuardada", string.Empty);
            if (!string.IsNullOrEmpty(printerName))
            {
                // Obtenemos el cliente correspondiente a la venta
                var clientes = await App.Database.GetClientesAsync();
                var clienteSeleccionado = clientes.FirstOrDefault(c => c.IdCliente == _ventas.idCliente);
                
                // Obtenemos los detalles de la venta
                var detalles = await App.Database.GetDetallesByVentasIdAsync(_ventas.Id);

                // Formateamos los detalles para que coincidan con la firma de impresión
                var detallesImpresion = detalles.Select(item => new VentasDetalle
                {
                    Descripcion = item.Descripcion,
                    Cantidad = item.Cantidad,
                    Precio = item.Precio
                }).ToList();

                string tamanoPapel = Preferences.Default.Get("TamanoPapel", "80mm");

                if (tamanoPapel == "58mm")
                {
                    var servicioImpresion58mm = this.Handler?.MauiContext?.Services.GetService<MauiApp6.Services.ServicioImpresionTickets58mm>();
                    if (servicioImpresion58mm == null)
                        servicioImpresion58mm = Application.Current?.MainPage?.Handler?.MauiContext?.Services.GetService<MauiApp6.Services.ServicioImpresionTickets58mm>();

                    if (servicioImpresion58mm != null)
                    {
                        await servicioImpresion58mm.ImprimirTicketAsync(
                            printerName,
                            _ventas,
                            clienteSeleccionado,
                            detallesImpresion);
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo iniciar el servicio de impresión de 58mm.", "OK");
                    }
                }
                else
                {
                    var servicioImpresion = this.Handler?.MauiContext?.Services.GetService<MauiApp6.Services.ServicioImpresionTickets>();
                    if (servicioImpresion == null)
                        servicioImpresion = Application.Current?.MainPage?.Handler?.MauiContext?.Services.GetService<MauiApp6.Services.ServicioImpresionTickets>();

                    if (servicioImpresion != null)
                    {
                        await servicioImpresion.ImprimirTicketAsync(
                            printerName,
                            _ventas,
                            clienteSeleccionado,
                            detallesImpresion);
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo iniciar el servicio de impresión de 80mm.", "OK");
                    }
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
    }
}
