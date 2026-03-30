using MauiApp6.Services;
using MauiApp6.ViewModels;
using System.Text;

namespace MauiApp6;

public partial class ConfiguracionPage : ContentPage
{


    private readonly IBluetoothService _bluetoothService;

    public ConfiguracionPage(IBluetoothService bluetoothService)
	{
		InitializeComponent();
        BindingContext = new SincronizacionViewModel();
        _bluetoothService = bluetoothService;
    }





    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 2. Pausa de medio segundo para que Android tenga lista la 'Activity'
        await Task.Delay(500);

        string tamanoGuardado = Preferences.Default.Get("TamanoPapel", "80mm");
        if (tamanoGuardado == "58mm")
            Rb58mm.IsChecked = true;
        else
            Rb80mm.IsChecked = true;

        string vistaGuardada = Preferences.Default.Get("VistaCatalogo", "Grid");
        if (vistaGuardada == "Lista")
            RbVistaLista.IsChecked = true;
        else
            RbVistaGrid.IsChecked = true;

        txtEmpresa.Text= Preferences.Default.Get("EMPRESA","");
        txtTituloTicket.Text = Preferences.Default.Get("TicketTitulo", "MI TIENDA APP");
        txtDireccionTicket.Text = Preferences.Default.Get("TicketDireccion", "Tonalá, Jalisco");

        txtID.Text = ObtenerIdDispositivo();
        // 3. Ahora sí, llamamos a la función que pide los permisos
        CargarDispositivos();

        bool Activo=Preferences.Default.Get("Activado", false);


        //btnValidar.IsVisible = !Activo;
        //btnActivar.IsVisible = !Activo;
        txtEmpresa.IsEnabled = !Activo;

        btnEmpresa.IsEnabled = Activo;


        // 1. Verificamos que el BindingContext ya esté asignado a tu ViewModel
        if (this.BindingContext is SincronizacionViewModel viewModel)
        {
            // 2. Ejecutamos el comando
            // Nota: Reemplaza "CargarDatosCommand" por el nombre real de tu comando
            if (viewModel.InicioCommand.CanExecute(null))
            {
                viewModel.InicioCommand.Execute(null);
            }
        }


    }



    private async void CargarDispositivos()
    {
        try
        {
            PermissionStatus status = PermissionStatus.Unknown;

            if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 12)
            {
                status = await Permissions.RequestAsync<BluetoothConnectPermission>();
            }
            else
            {
                status = PermissionStatus.Granted;
            }

            if (status == PermissionStatus.Granted)
            {
                var devices = _bluetoothService.GetPairedDevices();
                DevicePicker.ItemsSource = devices.ToList();

                // 1. Recuperamos el nombre guardado (si no hay nada, devuelve un texto vacío)
                string impresoraGuardada = Preferences.Default.Get("ImpresoraGuardada", string.Empty);

                // 2. Verificamos si tenemos una guardada Y si ese dispositivo sigue vinculado al teléfono
                if (!string.IsNullOrEmpty(impresoraGuardada) && devices.Contains(impresoraGuardada))
                {
                    DevicePicker.SelectedItem = impresoraGuardada;
                }
                else if (devices.Count > 0)
                {
                    // Comportamiento por defecto si es la primera vez que entra a la app
                    DevicePicker.SelectedIndex = 0;
                }
                else
                {
                    await DisplayAlert("Aviso", "No hay dispositivos Bluetooth vinculados.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Permiso Denegado", "Se requiere permiso para usar el Bluetooth.", "OK");
            }
        }
        catch (Exception ex)
        {
            // Agregamos este try-catch para que, si algo falla, no se cierre en silencio
            // sino que te muestre el error exacto en pantalla.
            await DisplayAlert("Error Crítico", ex.Message, "OK");
        }
    }


    private void OnDevicePickerSelectedIndexChanged(object sender, EventArgs e)
    {
        if (DevicePicker.SelectedItem != null)
        {
            string impresoraSeleccionada = DevicePicker.SelectedItem.ToString();

            // Guardamos el nombre de la impresora en la memoria del teléfono
            Preferences.Default.Set("ImpresoraGuardada", impresoraSeleccionada);
        }
    }

    private void OnPaperSizeChanged(object sender, CheckedChangedEventArgs e)
    {
        if (Rb58mm != null && Rb58mm.IsChecked)
        {
            Preferences.Default.Set("TamanoPapel", "58mm");
        }
        else if (Rb80mm != null && Rb80mm.IsChecked)
        {
            Preferences.Default.Set("TamanoPapel", "80mm");
        }
    }

    private void OnVistaCatalogoChanged(object sender, CheckedChangedEventArgs e)
    {
        if (RbVistaLista != null && RbVistaLista.IsChecked)
        {
            Preferences.Default.Set("VistaCatalogo", "Lista");
        }
        else if (RbVistaGrid != null && RbVistaGrid.IsChecked)
        {
            Preferences.Default.Set("VistaCatalogo", "Grid");
        }
    }

    private void OnDatosTicketChanged(object sender, TextChangedEventArgs e)
    {
        if (sender == txtTituloTicket)
        {
            Preferences.Default.Set("TicketTitulo", txtTituloTicket.Text ?? string.Empty);
        }
        else if (sender == txtDireccionTicket)
        {
            Preferences.Default.Set("TicketDireccion", txtDireccionTicket.Text ?? string.Empty);
        }
    }


    //private void OnBluetoothDeviceSelected(object sender, EventArgs e)
    //{
    //    var picker = (Picker)sender;
    //    int selectedIndex = picker.SelectedIndex;

    //    if (selectedIndex != -1)
    //    {
    //        var selectedItem = picker.ItemsSource[selectedIndex] as string;
    //        // Opcional: Hacer algo inmediatamente después de seleccionar
    //    }
    //}

    //private async void OnGuardarClicked(object sender, EventArgs e)
    //{

    //}


    private async void OnPrintClicked(object sender, EventArgs e)
    {
        if (DevicePicker.SelectedItem == null) return;

        string printerName = DevicePicker.SelectedItem.ToString();

        // Usamos una lista para ir armando el flujo de bytes
        List<byte> printData = new List<byte>();

        // 1. Inicializar
        printData.AddRange(EscPosCommands.Initialize);



        try
        {
            // 2. LEER LA IMAGEN DESDE LOS RECURSOS DE MAUI
            using var stream = await FileSystem.OpenAppPackageFileAsync("logo.png");
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            byte[] imageFileBytes = memoryStream.ToArray();

            // 3. CONVERTIRLA A COMANDOS ESC/POS USANDO NUESTRO SERVICIO
            byte[] printerLogo = _bluetoothService.FormatImageForPrinter(imageFileBytes);

            // 4. AGREGARLA AL TICKET
            printData.AddRange(printerLogo);
            printData.AddRange(EscPosCommands.FeedLine); // Un espacio después del logo
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar logo: {ex.Message}");
            // Si falla la imagen, el código continúa y al menos imprime el texto
        }



        // 2. Encabezado: Centrado, Doble Tamaño y Negritas
        printData.AddRange(EscPosCommands.AlignCenter);
        printData.AddRange(EscPosCommands.SizeDouble);
        printData.AddRange(EscPosCommands.BoldOn);
        string tituloTicket = Preferences.Default.Get("TicketTitulo", "MI TIENDA APP");
        printData.AddRange(Encoding.ASCII.GetBytes($"{tituloTicket}\n"));

        // Desactivar negritas y volver a tamaño normal para subtítulo
        printData.AddRange(EscPosCommands.BoldOff);
        printData.AddRange(EscPosCommands.SizeNormal);
        string direccionTicket = Preferences.Default.Get("TicketDireccion", "Tonalá, Jalisco");
        printData.AddRange(Encoding.ASCII.GetBytes($"{direccionTicket}\n"));
        printData.AddRange(Encoding.ASCII.GetBytes("Tel: 555-1234\n"));
        printData.AddRange(EscPosCommands.FeedLine); // Espacio

        // 3. Cuerpo del ticket: Alineado a la izquierda
        printData.AddRange(EscPosCommands.AlignLeft);
        printData.AddRange(Encoding.ASCII.GetBytes("--------------------------------\n"));
        printData.AddRange(Encoding.ASCII.GetBytes("Prod          Cant      Total\n"));
        printData.AddRange(Encoding.ASCII.GetBytes("--------------------------------\n"));

        // Items
        printData.AddRange(Encoding.ASCII.GetBytes("Coca Cola      2       $30.00\n"));
        printData.AddRange(Encoding.ASCII.GetBytes("Papas Fritas   1       $15.00\n"));
        printData.AddRange(Encoding.ASCII.GetBytes("Galletas       3       $25.00\n"));
        printData.AddRange(EscPosCommands.FeedLine);

        // 4. Totales: Alineado a la derecha
        printData.AddRange(EscPosCommands.AlignRight);
        printData.AddRange(EscPosCommands.BoldOn);
        printData.AddRange(Encoding.ASCII.GetBytes("TOTAL: $70.00\n"));
        printData.AddRange(EscPosCommands.BoldOff);

        // 5. Pie de página: Centrado y mensaje final
        printData.AddRange(EscPosCommands.AlignCenter);
        printData.AddRange(EscPosCommands.FeedLine);
        printData.AddRange(Encoding.ASCII.GetBytes("¡Gracias por su compra!\n"));

        // 6. Espacio final para cortar
        printData.AddRange(EscPosCommands.Feed3Lines);

        printData.AddRange(EscPosCommands.FeedLine); // Una línea extra por si acaso

        // 3. ¡ZAS! EL CORTE DE GUILLOTINA
        printData.AddRange(EscPosCommands.PartialCut); // O usa FullCut según prefieras








        // ENVIAR A LA IMPRESORA
        try
        {
            await _bluetoothService.PrintBytesAsync(printerName, printData.ToArray());

            var selectedItem = DevicePicker.SelectedItem as string;
            Preferences.Default.Set("BluetoothPrinter", selectedItem);
            LblStatus.Text = $"Dispositivo '{selectedItem}' guardado correctamente.";
            LblStatus.TextColor = Colors.Green;
            await DisplayAlert("Mensaje", "Dispositivo configurado correctamente", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }



    public string ObtenerIdDispositivo()
    {
        // Intentamos obtener el ID guardado previamente
        var idDispositivo = Preferences.Default.Get("MiApp_DeviceID", string.Empty);

        // Si no existe, es la primera vez que se abre la app
        if (string.IsNullOrWhiteSpace(idDispositivo))
        {
            // Generamos un nuevo ID único
            idDispositivo = System.Guid.NewGuid().ToString();

            // Lo guardamos para futuras sesiones
            Preferences.Default.Set("MiApp_DeviceID", idDispositivo);
        }

        return idDispositivo;
    }


    //private async void GuardarConfiguracion(object sender, EventArgs e)
    //{
    //    var selectedItem = DevicePicker.SelectedItem as string;

    //    if (!string.IsNullOrEmpty(selectedItem))
    //    {
    //        // Guardar el dispositivo seleccionado en las preferencias de la aplicación
    //        Preferences.Default.Set("BluetoothPrinter", selectedItem);

    //        LblStatus.Text = $"Dispositivo '{selectedItem}' guardado correctamente.";
    //        LblStatus.TextColor = Colors.Green;

    //        await Task.Delay(2000); // Mostrar el mensaje por 2 segundos
    //        await Navigation.PopAsync(); // Regresar a la pantalla anterior
    //    }
    //    else
    //    {
    //        LblStatus.Text = "Por favor, seleccione un dispositivo.";
    //        LblStatus.TextColor = Colors.Red;
    //    }
    //}

    private void txtEmpresa_Unfocused(object sender, FocusEventArgs e)
    {

    }
}




// Clase para definir los permisos de Bluetooth en Android 12+
public class BluetoothConnectPermission : Permissions.BasePlatformPermission
{
#if ANDROID
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new List<(string androidPermission, bool isRuntime)>
        {
            // Se requieren ambos para buscar y conectar en Android 12+
            (Android.Manifest.Permission.BluetoothScan, true),
            (Android.Manifest.Permission.BluetoothConnect, true)
        }.ToArray();
#endif
}

