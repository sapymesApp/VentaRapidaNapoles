using System.Windows.Input;

namespace MauiApp6.Controls;


public partial class MenuButton : ContentView
{
    // 1. Definimos las propiedades que recibirá el control
    public static readonly BindableProperty ButtonTextProperty = BindableProperty.Create(nameof(ButtonText), typeof(string), typeof(MenuButton), string.Empty);
    public static readonly BindableProperty CardBackgroundColorProperty = BindableProperty.Create(nameof(CardBackgroundColor), typeof(Color), typeof(MenuButton), Colors.White);
    public static readonly BindableProperty PrimaryColorProperty = BindableProperty.Create(nameof(PrimaryColor), typeof(Color), typeof(MenuButton), Colors.Black);
    public static readonly BindableProperty IconGlyphProperty = BindableProperty.Create(nameof(IconGlyph), typeof(string), typeof(MenuButton), string.Empty);

    // 2. Creamos los "getters" y "setters" estándar
    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public Color CardBackgroundColor
    {
        get => (Color)GetValue(CardBackgroundColorProperty);
        set => SetValue(CardBackgroundColorProperty, value);
    }

    public Color PrimaryColor
    {
        get => (Color)GetValue(PrimaryColorProperty);
        set => SetValue(PrimaryColorProperty, value);
    }

    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }




    // 1. Agregamos la propiedad para el Comando
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(MenuButton),
        null);

    // 2. Agregamos la propiedad para el Parámetro (opcional, pero muy recomendada)
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(MenuButton),
        null);

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }





    // 3. Exponemos un evento para detectar el clic desde afuera
    public event EventHandler Tapped;

    public MenuButton()
    {
        InitializeComponent();
    }

    // Cuando el usuario toca el Border interno, disparamos el evento hacia afuera
    // 1. Agregamos 'async' al método
    private async void OnBorderTapped(object sender, TappedEventArgs e)
    {
        // 2. Ejecutamos la animación sobre todo el control ('this')
        // Esto le da el efecto de presión a todo el botón al mismo tiempo
        await this.ScaleTo(0.95, 100);
        await this.ScaleTo(1.0, 100);

        // 3. UNA VEZ TERMINADA la animación, disparamos el evento hacia afuera
        // para que MainPage haga la navegación
        // 2. Disparamos el evento tradicional (si es que alguien lo está usando)
        Tapped?.Invoke(this, EventArgs.Empty);

        // 3. EJECUTAMOS EL COMANDO MVVM (si es que alguien le asignó uno)
        if (Command != null && Command.CanExecute(CommandParameter))
        {
            Command.Execute(CommandParameter);
        }
    }

}
