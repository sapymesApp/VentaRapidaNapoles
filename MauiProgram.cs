using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using QuestPDF.Infrastructure;
using SkiaSharp.Views.Maui.Controls.Hosting;
using MauiApp6.Services;

namespace MauiApp6
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {

            
            //QuestPDF.Settings.License = LicenseType.Community;
            

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseMauiCommunityToolkit()                
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons.ttf", "MaterialIcons");
                });



            builder.Services.AddSingleton<ServicioImpresionTickets>();
            builder.Services.AddTransient<NuevaVentasPage>();
            builder.Services.AddTransient<ConfiguracionPage>();

#if ANDROID
            builder.Services.AddSingleton<IBluetoothService, MauiApp6.Platforms.Android.BluetoothService>();
#endif




            //try
            //{
            //    QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            //}
            //catch (Exception ex)
            //{
            //    var errorReal = ex.InnerException?.Message;
            //    System.Diagnostics.Debug.WriteLine($"EL ERROR REAL ES: {errorReal}");
            //}


            //QuestPDF.Settings.de = "sans-serif";


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
