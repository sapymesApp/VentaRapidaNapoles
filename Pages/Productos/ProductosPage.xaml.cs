using MauiApp6.Models;

namespace MauiApp6;

public partial class ProductosPage : ContentPage
{
    private List<Producto> _allProductos = new List<Producto>();

	public ProductosPage()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AppShell.SetNavBarIsVisible(this, false);
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsEnabled = false, IsVisible = false });
        await CargarProductos();
    }

    private async Task CargarProductos()
    {
        // Llamada a la BD
        var lista = await App.Database.GetProductosAsync();
        _allProductos = lista.ToList();
        CvProductos.ItemsSource = _allProductos;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string textoBusqueda = e.NewTextValue?.ToUpper() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(textoBusqueda))
        {
            CvProductos.ItemsSource = _allProductos;
        }
        else
        {
            var filtrados = _allProductos
                .Where(p => p.Descripcion != null && p.Descripcion.ToUpper().Contains(textoBusqueda))
                .ToList();

            CvProductos.ItemsSource = filtrados;
        }
    }

    private async void OnNuevoProductoClicked(object sender, EventArgs e)
    {
        // Abrir ventana de alta
        await Navigation.PushAsync(new NuevoProductoPage());
    }

    // Se dispara al deslizar a la izquierda (Editar)
    private async void OnEditarSwipeInvoked(object sender, EventArgs e)
    {
        var swipeItem = (SwipeItem)sender;

        // El BindingContext del SwipeItem es el Producto de esa fila
        var producto = (Producto)swipeItem.BindingContext;

        if (producto != null)
        {
            await Navigation.PushAsync(new EditarPrecioPage(producto));
        }
    }

    // Se dispara al deslizar a la derecha (Eliminar)
    private async void OnEliminarSwipeInvoked(object sender, EventArgs e)
    {
        var swipeItem = (SwipeItem)sender;
        var producto = (Producto)swipeItem.BindingContext;

        if (producto != null)
        {
            await ProcesarEliminacion(producto);
        }
    }

    // Creamos este método auxiliar para no repetir código entre el botón y el swipe
    private async Task ProcesarEliminacion(Producto producto)
    {
        bool confirmar = await DisplayAlert("Eliminar",
            $"¿Estás seguro de eliminar '{producto.Descripcion}'?",
            "Sí, eliminar", "Cancelar");

        if (confirmar)
        {
            await App.Database.DeleteProductoAsync(producto);
            await CargarProductos();

            // Limpiamos la selección por si acaso
            CvProductos.SelectedItem = null;
        }
    }
}
