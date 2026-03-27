using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MauiApp6.Models;
using MauiApp6.Services;

namespace MauiApp6.ViewModels
{
    public class SincronizacionViewModel : BindableObject
    {
        private SyncService _syncService;

        //private readonly ApiService _apiService;


        private string _Empresa;
        public string Empresa
        {
            get => _Empresa;
            set
            {
                _Empresa = value;
                OnPropertyChanged(nameof(Empresa));
            }
        }


        private string _AndroidID;
        public string AndroidID
        {
            get => _AndroidID;
            set
            {
                _AndroidID = value;
                OnPropertyChanged(nameof(AndroidID));
            }
        }




        // Propiedades para la UI
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        private string _mensajeEstado;
        public string MensajeEstado
        {
            get => _mensajeEstado;
            set { _mensajeEstado = value; OnPropertyChanged(); }
        }


        private bool _dispositivoAutorizado;



        public bool NotDispositivoAutorizado => !DispositivoAutorizado;
        // Esta es la propiedad pública a la que se conectará tu XAML
        public bool DispositivoAutorizado
        {
            get => _dispositivoAutorizado;
            set
            {
                if (_dispositivoAutorizado != value)
                {
                    _dispositivoAutorizado = value;
                    // Notificamos a la vista que el valor cambió
                    OnPropertyChanged(nameof(DispositivoAutorizado));

                    // ¡Clave! Notificamos que la propiedad inversa también cambió
                    OnPropertyChanged(nameof(NotDispositivoAutorizado));
                }
            }
        }


        //para desactivar los botones
        private bool _btnCatalogo;
        public bool btnCatalogo
        {
            get => _btnCatalogo;
            set
            {
                _btnCatalogo=value; 
                OnPropertyChanged(nameof(btnCatalogo));
            }
        }



        //para desactivar los botones
        private bool _btnVentas;
        public bool btnVentas
        {
            get => _btnVentas;
            set
            {
                _btnVentas = value;
                OnPropertyChanged(nameof(btnVentas));
            }
        }


        //para desactivar los botones
        private bool _btnAbonos;
        public bool btnAbonos
        {
            get => _btnAbonos;
            set
            {
                _btnAbonos = value;
                OnPropertyChanged(nameof(btnAbonos));
            }
        }




        public ICommand DescargarCommand { get; }
        public ICommand SubirVentasCommand { get; }
        public ICommand SubirAbonosCommand { get; }
        public ICommand DescargarEmpresaCommand {  get; }

        public ICommand ActivarCommand { get; }

        public ICommand ValidarCommand { get; }

        public ICommand ValidarDispositivoCommand { get; }


        public ICommand GuardarEmpresa { get; }


        public ICommand InicioCommand { get; }

        public SincronizacionViewModel()
        {
            _syncService = new SyncService();
            DescargarCommand = new Command(async () => await EjecutarDescarga());
            SubirVentasCommand = new Command(async () => await SincronizarVentas());
            SubirAbonosCommand = new Command(async () => await SincronizarAbonos());
            DescargarEmpresaCommand = new Command(async () => await DescargarEmpresa());

            ActivarCommand = new Command(async () => await ActivarEmpresa());

            ValidarCommand = new Command(async () => await Validar());

            ValidarDispositivoCommand = new Command(async () => await ValidarDispositivoGlobal());

            InicioCommand = new Command(async () => await ValidarInicio());

            GuardarEmpresa = new Command(async () => await GuardaEmpresa());


            //_apiService = new ApiService();
        }



        private async Task GuardaEmpresa()
        {
            Empresa=Empresa?.ToUpper() ?? string.Empty;
            Preferences.Default.Set("EMPRESA", Empresa);

            return;
        }





        private string ObtenerIdDispositivo()
        {
            var idDispositivo = Preferences.Default.Get("MiApp_DeviceID", string.Empty);
            if (string.IsNullOrWhiteSpace(idDispositivo))
            {
                idDispositivo = System.Guid.NewGuid().ToString();
                Preferences.Default.Set("MiApp_DeviceID", idDispositivo);
            }
            return idDispositivo;
        }



        private async Task ValidarInicio()
        {

            bool Act=Preferences.Default.Get("Activado", false);

            _dispositivoAutorizado = Act;

            _Empresa = Preferences.Default.Get("EMPRESA", "");


            OnPropertyChanged(nameof(Empresa));
            OnPropertyChanged(nameof(DispositivoAutorizado));
            OnPropertyChanged(nameof(NotDispositivoAutorizado));
            
            return;         

        }


        private async Task ActivarEmpresa()
        {

            if (Empresa.Length == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Mensaje", "Debes especificar la clave de la empresa.", "OK");
                return;
            }



            if (IsBusy) return;

            IsBusy = true;
            MensajeEstado = "Conectando con el servidor...";

            // Simulamos un pequeño delay visual
            await Task.Delay(500);

            Empresa = Empresa.Trim();
            Empresa= Empresa.ToUpper();

            string resultado = await _syncService.Activar(Empresa, AndroidID);

            if (resultado == "Ok")
            {
                MensajeEstado = "Datos de activación enviados correctamente.";
                await Application.Current.MainPage.DisplayAlert("Éxito", "Datos de activación enviados correctamente.", "OK");
            }
            else
            {
                MensajeEstado = "Error en el envío de datos ❌";
                await Application.Current.MainPage.DisplayAlert("Error", resultado, "OK");
            }

            IsBusy = false;

        }


        private async Task ValidarDispositivoGlobal()
        {
            if (IsBusy) return;

            IsBusy = true;
            MensajeEstado = "Conectando con el servidor...";


            string idDispositivo = ObtenerIdDispositivo();

            // 2. Consultamos a la API
            //bool estaAutorizado = await _apiService.ValidarDispositivoAsync(idDispositivo);
            bool estaAutorizado = await _syncService.Validar(idDispositivo);

            // Actualizamos nuestra propiedad observable
            _dispositivoAutorizado = estaAutorizado;


            OnPropertyChanged(nameof(DispositivoAutorizado));
            OnPropertyChanged(nameof(NotDispositivoAutorizado));


            Preferences.Default.Set("Activado", DispositivoAutorizado);


            if (estaAutorizado)
            {
                MensajeEstado = "Dispositivo autorizado. Listo para sincronizar.";
                await Application.Current.MainPage.DisplayAlert("Éxito", "Dispositivo activado correctamente.", "OK");
            }
            else
            {
                MensajeEstado = "Dispositivo no autorizado. Contacte al administrador.";
                await Application.Current.MainPage.DisplayAlert("Éxito", "Dispositivo no autorizado. Contacte al administrador.", "OK");
            }

            IsBusy = false;
        }










        private async Task Validar()
        {
            if (IsBusy) return;

            IsBusy = true;
            MensajeEstado = "Conectando con el servidor...";


            string idDispositivo = ObtenerIdDispositivo();

            // 2. Consultamos a la API
            bool estaAutorizado = await _syncService.ValidarDispositivoAsync(idDispositivo);

            // Actualizamos nuestra propiedad observable
            _dispositivoAutorizado = estaAutorizado;
            
            
            _btnCatalogo = estaAutorizado;
            OnPropertyChanged(nameof(btnCatalogo));


            _btnVentas = estaAutorizado;
            OnPropertyChanged(nameof(btnVentas));


            _btnAbonos = estaAutorizado;
            OnPropertyChanged(nameof(btnCatalogo));


            OnPropertyChanged(nameof(DispositivoAutorizado));
            OnPropertyChanged(nameof(NotDispositivoAutorizado));


            


            //Preferences.Default.Set("Activado", DispositivoAutorizado);


            if (estaAutorizado)
            {
                MensajeEstado = "Dispositivo autorizado. Listo para sincronizar.";
                // Aquí puedes ejecutar lógica adicional si es necesario
            }
            else
            {
                MensajeEstado = "Dispositivo no autorizado. Contacte al administrador.";
            }

            IsBusy =false;
        }



        private async Task DescargarEmpresa()
        {
            if (IsBusy) return;

            IsBusy = true;
            MensajeEstado = "Conectando con el servidor...";


            string ClaveEmpresa = Preferences.Default.Get("EMPRESA","");


            // Simulamos un pequeño delay visual
            await Task.Delay(500);

            string resultado = await _syncService.DescargarEmpresa(ClaveEmpresa);

            if (resultado == "OK")
            {
                Preferences.Default.Set("Activado", true);
                MensajeEstado = "¡Sincronización Completada! ✅";
                await Application.Current.MainPage.DisplayAlert("Éxito", "Datos de empresa descargados correctamente.", "OK");
            }
            else
            {
                Preferences.Default.Set("Activado", false);
                MensajeEstado = "Error en la sincronización ❌";
                await Application.Current.MainPage.DisplayAlert("Error", resultado, "OK");
            }

            IsBusy = false;
        }






        private async Task EjecutarDescarga()
        {
            if (IsBusy) return;

            IsBusy = true;
            MensajeEstado = "Conectando con el servidor...";

            // AQUÍ PONES EL ID DE LA EMPRESA QUE INICIÓ SESIÓN
            int idEmpresa = Preferences.Default.Get("IdEmpresa", 0);

            // Simulamos un pequeño delay visual
            await Task.Delay(500);

            string resultado = await _syncService.DescargarCatalogos(idEmpresa);

            if (resultado == "OK")
            {
                _btnCatalogo=false;
                OnPropertyChanged(nameof(btnCatalogo));

                MensajeEstado = "¡Sincronización Completada! ✅";
                await Application.Current.MainPage.DisplayAlert("Éxito", "Catálogo de productos y clientes actualizados correctamente.", "OK");
            }
            else
            {
                MensajeEstado = "Error en la sincronización ❌";
                await Application.Current.MainPage.DisplayAlert("Error", resultado, "OK");
            }

            IsBusy = false;
        }

        public async Task SincronizarVentas()
        {
            if (IsBusy) return;

            IsBusy = true;
            MensajeEstado = "Conectando con el servidor...";

            // AQUÍ PONES EL ID DE LA EMPRESA QUE INICIÓ SESIÓN
            int idEmpresa = Preferences.Default.Get("IdEmpresa",0);

            // Simulamos un pequeño delay visual
            await Task.Delay(500);

            string resultado = await _syncService.SincronizarVentasPendientes(idEmpresa);

            if (resultado == "OK")
            {
                _btnVentas = false;
                OnPropertyChanged(nameof(btnVentas));

                MensajeEstado = "¡Sincronización Completada! ✅";
                await Application.Current.MainPage.DisplayAlert("Éxito", "Ventas actualizadas correctamente.", "OK");
            }
            else
            {
                MensajeEstado = "Error en la sincronización ❌";
                await Application.Current.MainPage.DisplayAlert("Error", resultado, "OK");
            }

            IsBusy = false;
        }

        public async Task SincronizarAbonos()
        {
            if (IsBusy) return;

            IsBusy = true;
            MensajeEstado = "Subiendo abonos pendientes...";

            // Simulamos un pequeño delay visual
            await Task.Delay(500);

            string resultado = await _syncService.SincronizarAbonosPendientes();

            if (resultado == "OK")
            {

                _btnAbonos = false;
                OnPropertyChanged(nameof(btnAbonos));


                MensajeEstado = "¡Sincronización Completada! ✅";
                await Application.Current.MainPage.DisplayAlert("Éxito", "Abonos sincronizados correctamente.", "OK");
            }
            else
            {
                MensajeEstado = "Error en la sincronización ❌";
                await Application.Current.MainPage.DisplayAlert("Información", resultado, "OK");
            }

            IsBusy = false;
        }







    }













}
