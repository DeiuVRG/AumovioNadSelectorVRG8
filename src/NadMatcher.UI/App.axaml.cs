using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NadMatcher.Infrastructure;
using NadMatcher.UI.Services;
using NadMatcher.UI.ViewModels;
using NadMatcher.UI.Views;
using System;
using System.Linq;
using AppDI = NadMatcher.Application.DependencyInjection;

namespace NadMatcher.UI;

public partial class App : Avalonia.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configure dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel,
            };

            // Load data when window is opened
            desktop.MainWindow.Opened += async (_, _) =>
            {
                await mainViewModel.NadSelectionViewModel.LoadDataAsync();
                await mainViewModel.CountrySelectionViewModel.LoadDataAsync();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Add Infrastructure layer (repositories)
        services.AddInfrastructure();

        // Add Application layer (services, workflows)
        AppDI.AddApplication(services);

        // Add Services
        services.AddSingleton<UpdateService>();

        // Add ViewModels
        services.AddTransient<NadSelectionViewModel>();
        services.AddTransient<CountrySelectionViewModel>();
        services.AddTransient<MainWindowViewModel>();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
