using NadMatcher.Maui.ViewModels;
using NadMatcher.Maui.Views;

namespace NadMatcher.Maui;

public partial class MainPage : ContentPage
{
    private readonly MainWindowViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;
    private NadSelectionPage? _nadSelectionPage;
    private CountrySelectionPage? _countrySelectionPage;
    private int _selectedTab = 0;

    public MainPage(MainWindowViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        BindingContext = viewModel;

        // Load NAD selection tab by default
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object? sender, EventArgs e)
    {
        await LoadNadSelectionTab();
    }

    private async Task LoadNadSelectionTab()
    {
        if (_nadSelectionPage == null)
        {
            _nadSelectionPage = _serviceProvider.GetRequiredService<NadSelectionPage>();
            _nadSelectionPage.BindingContext = _viewModel.NadSelectionViewModel;
            await _viewModel.NadSelectionViewModel.LoadDataAsync();
        }
        TabContentView.Content = _nadSelectionPage;
        UpdateTabStyles(0);
    }

    private async Task LoadCountrySelectionTab()
    {
        if (_countrySelectionPage == null)
        {
            _countrySelectionPage = _serviceProvider.GetRequiredService<CountrySelectionPage>();
            _countrySelectionPage.BindingContext = _viewModel.CountrySelectionViewModel;
            await _viewModel.CountrySelectionViewModel.LoadDataAsync();
        }
        TabContentView.Content = _countrySelectionPage;
        UpdateTabStyles(1);
    }

    private void UpdateTabStyles(int selectedIndex)
    {
        _selectedTab = selectedIndex;

        if (selectedIndex == 0)
        {
            NadTabButton.BackgroundColor = Color.FromArgb("#1976d2");
            NadTabButton.TextColor = Colors.White;
            CountryTabButton.BackgroundColor = Color.FromArgb("#e0e0e0");
            CountryTabButton.TextColor = Color.FromArgb("#333333");
        }
        else
        {
            NadTabButton.BackgroundColor = Color.FromArgb("#e0e0e0");
            NadTabButton.TextColor = Color.FromArgb("#333333");
            CountryTabButton.BackgroundColor = Color.FromArgb("#1976d2");
            CountryTabButton.TextColor = Colors.White;
        }
    }

    private async void OnNadTabClicked(object? sender, EventArgs e)
    {
        if (_selectedTab != 0)
        {
            await LoadNadSelectionTab();
        }
    }

    private async void OnCountryTabClicked(object? sender, EventArgs e)
    {
        if (_selectedTab != 1)
        {
            await LoadCountrySelectionTab();
        }
    }
}
