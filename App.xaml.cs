using MauiApp6.Services;

namespace MauiApp6
{

    public partial class App : Application
    {


        static DatabaseService database;

        // Singleton para acceder a la BD desde cualquier pantalla
        public static DatabaseService Database
        {
            get
            {
                if (database == null)
                {
                //    var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "productos.db3");
                    database = new DatabaseService();
                }
                return database;
            }
        }

        public App()
        {
            InitializeComponent();


            UserAppTheme=AppTheme.Light;

            //MainPage = new AppShell();

            //MainPage = new Views.SplashPage();

            MainPage = new NavigationPage(new MainPage());
        }
    }
}
