using Microsoft.Maui.Controls;

namespace NadMatcher.Maui;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var mainPage = _serviceProvider.GetRequiredService<MainPage>();

        var window = new Window(mainPage)
        {
            Title = "AumovioNadSelectorVRG8 - Automotive Module Country Compatibility Tool",
            Width = 1200,
            Height = 800,
            MinimumWidth = 900,
            MinimumHeight = 600
        };

        return window;
    }
}