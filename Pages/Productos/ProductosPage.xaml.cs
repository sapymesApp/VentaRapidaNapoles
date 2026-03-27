using MauiApp6.Models;

namespace MauiApp6;

public partial class ProductosPage : ContentPage
{
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
        CvProductos.ItemsSource = lista;
    }

    private async void OnNuevoProductoClicked(object sender, EventArgs e)
    {
        // Abrir ventana de alta
        await Navigation.PushAsync(new NuevoProductoPage());
    }

    //private async void OnEliminarClicked(object sender, EventArgs e)
    //{
    //    // 1. Verificar si hay algo seleccionado
    //    var productoSeleccionado = CvProductos.SelectedItem as Producto;

    //    if (productoSeleccionado == null)
    //    {
    //        await DisplayAlert("Atención", "Selecciona un producto de la lista primero.", "OK");
    //        return;
    //    }

    //    // 2. Preguntar al usuario
    //    bool confirmar = await DisplayAlert("Eliminar",
    //        $"¿Estás seguro de eliminar '{productoSeleccionado.Descripcion}'?",
    //        "Sí, eliminar", "Cancelar");

    //    if (confirmar)
    //    {
    //        // 3. Eliminar de BD
    //        await App.Database.DeleteProductoAsync(productoSeleccionado);

    //        // 4. Recargar la lista y limpiar selección
    //        await CargarProductos();
    //        CvProductos.SelectedItem = null;
    //    }
    //}




    //private async void OnProductoDoubleTapped(object sender, EventArgs e)
    //{
    //    // El 'sender' es el Border que recibió los toques
    //    var border = (Border)sender;

    //    // El BindingContext de ese Border es el objeto Producto de la lista
    //    var productoSeleccionado = (Producto)border.BindingContext;

    //    if (productoSeleccionado != null)
    //    {
    //        // Navegamos a la página de edición pasando el objeto
    //        await Navigation.PushAsync(new EditarPrecioPage(productoSeleccionado));
    //    }
    //}


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