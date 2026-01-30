using Microsoft.Extensions.Logging;
using NadMatcher.Application;
using NadMatcher.Infrastructure;
using NadMatcher.Maui.Services;
using NadMatcher.Maui.ViewModels;
using NadMatcher.Maui.Views;

namespace NadMatcher.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Add Infrastructure and Application layers
        builder.Services.AddInfrastructure();
        builder.Services.AddApplication();

        // Register MAUI-specific services
        builder.Services.AddSingleton<UpdateService>();

        // Register ViewModels
        builder.Services.AddTransient<NadSelectionViewModel>();
        builder.Services.AddTransient<CountrySelectionViewModel>();
        builder.Services.AddTransient<MainWindowViewModel>();

        // Register Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<NadSelectionPage>();
        builder.Services.AddTransient<CountrySelectionPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
