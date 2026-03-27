using Microsoft.Maui.Controls;
using System;
using System.Windows.Input;

namespace MauiApp6.Controls;

public partial class ProductCard : ContentView
{
    public event EventHandler Tapped;

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(ProductCard),
        null);

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(ProductCard),
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

    public ProductCard()
    {
        InitializeComponent();
    }

    private async void OnCardTapped(object sender, TappedEventArgs e)
    {
        // Execute click animation
        await this.ScaleTo(0.95, 100);
        await this.ScaleTo(1.0, 100);

        // Standard event
        Tapped?.Invoke(this, EventArgs.Empty);

        // MVVM command
        if (Command != null && Command.CanExecute(CommandParameter))
        {
            Command.Execute(CommandParameter);
        }
    }
}
