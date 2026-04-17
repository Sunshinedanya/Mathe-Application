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
        if (isBusy)
        {
            return;
        }

        try
        {
            isBusy = true;
            statusMessage = "Подключение к базе данных...";

            await catalogDataService.InitializeDatabaseAsync();

            statusMessage = "Загрузка каталога...";

            await services.LoadAsync();
            await collections.LoadAsync();
            await directions.LoadAsync();

            statusMessage = "Данные загружены";
        }
        catch (Exception exception)
        {
            statusMessage = $"Ошибка инициализации: {exception.Message}";
        }
        finally
        {
            isBusy = false;
        }
    }

    [RelayCommand]
    private void ShowServices()
    {
        currentSectionViewModel = services;
        currentSectionTitle = "Услуги";
        statusMessage = "Открыт раздел услуг";
    }

    [RelayCommand]
    private void ShowCollections()
    {
        currentSectionViewModel = collections;
        currentSectionTitle = "Коллекции";
        statusMessage = "Открыт раздел коллекций";
    }

    [RelayCommand]
    private void ShowDirections()
    {
        currentSectionViewModel = directions;
        currentSectionTitle = "Направления";
        statusMessage = "Открыт раздел направлений";
    }

    [RelayCommand]
    private async Task RefreshCurrentSectionAsync()
    {
        if (isBusy)
        {
            return;
        }

        try
        {
            isBusy = true;
            statusMessage = "Обновление данных...";

            switch (currentSectionViewModel)
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

            statusMessage = "Данные обновлены";
        }
        catch (Exception exception)
        {
            statusMessage = $"Ошибка обновления: {exception.Message}";
        }
        finally
        {
            isBusy = false;
        }
    }
}
