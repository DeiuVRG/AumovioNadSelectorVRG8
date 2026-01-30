using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NadMatcher.Maui.Services;
using System.Threading.Tasks;

namespace NadMatcher.Maui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly UpdateService _updateService;

    [ObservableProperty]
    private NadSelectionViewModel _nadSelectionViewModel;

    [ObservableProperty]
    private CountrySelectionViewModel _countrySelectionViewModel;

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private bool _isDarkMode;

    [ObservableProperty]
    private bool _updateAvailable;

    [ObservableProperty]
    private string _updateVersion = string.Empty;

    [ObservableProperty]
    private bool _isUpdating;

    [ObservableProperty]
    private int _updateProgress;

    [ObservableProperty]
    private string _currentVersion = "1.0.0";

    public MainWindowViewModel(
        NadSelectionViewModel nadSelectionViewModel,
        CountrySelectionViewModel countrySelectionViewModel,
        UpdateService updateService)
    {
        _nadSelectionViewModel = nadSelectionViewModel;
        _countrySelectionViewModel = countrySelectionViewModel;
        _updateService = updateService;

        // Get version from Velopack if installed, otherwise use assembly version
        CurrentVersion = _updateService.CurrentVersion
            ?? System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)
            ?? "1.0.0";

        // Check for updates on startup
        _ = CheckForUpdatesAsync();
    }

    // Design-time constructor
    public MainWindowViewModel() : this(null!, null!, new UpdateService())
    {
    }

    private async Task CheckForUpdatesAsync()
    {
        var hasUpdate = await _updateService.CheckForUpdatesAsync();
        if (hasUpdate)
        {
            UpdateAvailable = true;
            UpdateVersion = _updateService.GetNewVersion() ?? "Unknown";
        }
    }

    [RelayCommand]
    private async Task InstallUpdateAsync()
    {
        if (!UpdateAvailable || IsUpdating)
            return;

        IsUpdating = true;
        UpdateProgress = 0;

        await _updateService.DownloadAndApplyUpdateAsync(progress =>
        {
            UpdateProgress = progress;
        });

        // If we get here, the update failed (app would have restarted)
        IsUpdating = false;
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        var app = Microsoft.Maui.Controls.Application.Current;
        if (app != null)
        {
            app.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
        }
    }
}
