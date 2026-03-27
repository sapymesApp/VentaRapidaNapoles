using MauiApp6.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;
using IContainer = QuestPDF.Infrastructure.IContainer;

namespace MauiApp6;

public partial class VentasClientePage : ContentPage
{
    private readonly Clientes _cliente;

    public VentasClientePage(Clientes cliente)
    {
        InitializeComponent();
        _cliente = cliente;

        // Configurar UI con datos del cliente
        LblNombreCliente.Text = _cliente.Nombre;

        // Establecer fechas por defecto: Desde inicio de mes hasta hoy
        DpDesde.Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        DpHasta.Date = DateTime.Now;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarVentas();
    }

    private async void OnFiltrosChanged(object sender, EventArgs e)
    {
        await CargarVentas();
    }

    private async Task CargarVentas()
    {
        if (_cliente == null) return;

        var lista = await App.Database.GetVentasPorClienteAsync(
            _cliente.IdCliente, 
            DpDesde.Date, 
            DpHasta.Date, 
            SwIncluirPagadas.IsToggled);

        // Agregamos el nombre del cliente para consistencia, aunque aquí todos son el mismo
        foreach(var ventas in lista)
        {
            ventas.ClienteNombre = _cliente.Nombre; 
        }

        CvVentas.ItemsSource = lista;
    }

    private async void OnVentasSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Ventas seleccionada)
        {
            await Navigation.PushAsync(new DetalleVentasPage(seleccionada));
            CvVentas.SelectedItem = null;
        }
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnExportarPDFClicked(object sender, EventArgs e)
    {
        try
        {
            var lista = CvVentas.ItemsSource as List<Ventas>;
            if (lista == null || !lista.Any())
            {
                await DisplayAlert("Atención", "No hay ventas para exportar con los filtros actuales.", "OK");
                return;
            }

            // 1. Obtener el logo
            byte[] logo = await ObtenerLogoBytes();

            // 2. Generar el PDF
            string rutaPdf = GenerarReportePdf(lista, logo);

            if (string.IsNullOrEmpty(rutaPdf)) return;

            // 3. Mostrar Toast de éxito
            var toast = CommunityToolkit.Maui.Alerts.Toast.Make("Reporte generado", 
                CommunityToolkit.Maui.Core.ToastDuration.Short, 14);
            await toast.Show();

            // 4. Compartir
            await Task.Delay(500); // Pequeña pausa
            await CompartirArchivo(rutaPdf);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo generar el reporte: {ex.Message}", "OK");
        }
    }

    private string GenerarReportePdf(List<Ventas> ventas, byte[] logoBytes)
    {
        try { QuestPDF.Settings.License = LicenseType.Community; } catch { }
        QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;

        string nombreArchivo = $"Reporte_Ventas_{_cliente.Nombre}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        string rutaArchivo = Path.Combine(FileSystem.CacheDirectory, nombreArchivo);

        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(QuestPDF.Helpers.PageSizes.Letter);
                page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                page.PageColor(QuestPDF.Helpers.Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(container => ComposeHeader(container, logoBytes));
                page.Content().Element(container => ComposeContent(container, ventas));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        })
        .GeneratePdf(rutaArchivo);

        return rutaArchivo;
    }

    private void ComposeHeader(IContainer container, byte[] logoBytes)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("REPORTE DE VENTAS").FontSize(20).SemiBold().FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);
                column.Item().Text($"Cliente: {_cliente.Nombre}").FontSize(14);
                column.Item().PaddingTop(5).Text($"Rango: {DpDesde.Date:dd/MM/yyyy} - {DpHasta.Date:dd/MM/yyyy}");
                column.Item().Text($"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}");
            });

            if (logoBytes != null && logoBytes.Length > 0)
            {
                row.ConstantItem(100).Image(logoBytes);
            }
        });
    }

    private void ComposeContent(IContainer container, List<Ventas> ventas)
    {
        container.PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre).Column(column =>
        {
            column.Item().Table(table =>
            {
                // Definir columnas
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Fecha / Folio
                    columns.RelativeColumn(2); // Total
                    columns.RelativeColumn(2); // Devolución
                    columns.RelativeColumn(2); // Abono
                    columns.RelativeColumn(2); // Saldo
                });

                // Cabecera
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Fecha / Folio").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Total").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Devolución").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Abono").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Saldo").SemiBold();

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Black);
                    }
                });

                // Filas
                foreach (var s in ventas)
                {
                    table.Cell().Element(CellStyle).Text($"{s.Fecha:dd/MM/yyyy HH:mm}\nFolio: {s.Id}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{s.Total:C}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{s.Devolucion:C}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{s.Abono:C}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{s.Saldo:C}").FontColor(QuestPDF.Helpers.Colors.Red.Medium);

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                }
            });

            var totalMonto = ventas.Sum(x => x.Total);
            var totalDevolucion = ventas.Sum(x => x.Devolucion);
            var totalAbonos = ventas.Sum(x => x.Abono);
            var totalSaldo = ventas.Sum(x => x.Saldo);

            column.Item().PaddingTop(15).AlignRight().Text($"Suma Total: {totalMonto:C}").FontSize(12).SemiBold();
            column.Item().AlignRight().Text($"Suma Devoluciones: {totalDevolucion:C}").FontSize(12).SemiBold();
            column.Item().AlignRight().Text($"Suma Abonos: {totalAbonos:C}").FontSize(12).SemiBold();
            column.Item().AlignRight().Text($"Suma Saldo: {totalSaldo:C}").FontSize(14).Bold().FontColor(QuestPDF.Helpers.Colors.Red.Medium);
        });
    }

    private async Task<byte[]> ObtenerLogoBytes()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("sapymes.png");
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        catch
        {
            return null;
        }
    }

    private async Task CompartirArchivo(string rutaArchivo)
    {
        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Compartir Reporte de Ventas",
            File = new ShareFile(rutaArchivo)
        });
    }
}
