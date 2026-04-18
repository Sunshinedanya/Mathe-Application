using System;
using System.Threading.Tasks;
using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication2.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly CatalogDataService catalogDataService;

    [ObservableProperty]
    private string applicationTitle;

    [ObservableProperty]
    private string statusMessage = "Готово к работе";

    [ObservableProperty]
    private object? currentSectionViewModel;

    [ObservableProperty]
    private string currentSectionTitle = "Услуги";

    [ObservableProperty]
    private bool isBusy;

    public MainWindowViewModel(
        CatalogDataService catalogDataService,
        int pageSize = 3,
        string defaultSort = "alphabetAsc",
        string applicationTitle = "Матье")
    {
        this.catalogDataService = catalogDataService;
        this.applicationTitle = applicationTitle;

        services = new ServicesViewModel(catalogDataService, pageSize, defaultSort);
        collections = new CollectionsViewModel(catalogDataService, pageSize, defaultSort);
        directions = new DirectionsViewModel(catalogDataService, pageSize, defaultSort);

        currentSectionViewModel = services;
    }

    public ServicesViewModel services { get; }

    public CollectionsViewModel collections { get; }

    public DirectionsViewModel directions { get; }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Подключение к базе данных...";

            await catalogDataService.InitializeDatabaseAsync();

            StatusMessage = "Загрузка каталога...";

            await services.LoadAsync();
            await collections.LoadAsync();
            await directions.LoadAsync();

            StatusMessage = "Данные загружены";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка инициализации: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ShowServices()
    {
        CurrentSectionViewModel = services;
        CurrentSectionTitle = "Услуги";
        StatusMessage = "Открыт раздел услуг";
    }

    [RelayCommand]
    private void ShowCollections()
    {
        CurrentSectionViewModel = collections;
        CurrentSectionTitle = "Коллекции";
        StatusMessage = "Открыт раздел коллекций";
    }

    [RelayCommand]
    private void ShowDirections()
    {
        CurrentSectionViewModel = directions;
        CurrentSectionTitle = "Направления";
        StatusMessage = "Открыт раздел направлений";
    }

    [RelayCommand]
    private async Task RefreshCurrentSectionAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Обновление данных...";

            switch (CurrentSectionViewModel)
            {
                case ServicesViewModel currentServices:
                    await currentServices.LoadAsync();
                    break;

                case CollectionsViewModel currentCollections:
                    await currentCollections.LoadAsync();
                    break;

                case DirectionsViewModel currentDirections:
                    await currentDirections.LoadAsync();
                    break;
            }

            StatusMessage = "Данные обновлены";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка обновления: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
