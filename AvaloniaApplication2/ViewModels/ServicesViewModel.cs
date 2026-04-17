using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaApplication2.Services;

namespace AvaloniaApplication2.ViewModels;

public partial class ServicesViewModel : ViewModelBase
{
    private readonly CatalogDataService catalogDataService;
    private readonly int pageSize;

    [ObservableProperty]
    private ObservableCollection<ServiceListItem> items = new();

    [ObservableProperty]
    private ObservableCollection<LookupItem> collectionOptions = new();

    [ObservableProperty]
    private ServiceListItem? selectedItem;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool sortAscending = true;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private int totalCount;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = "Готово";

    [ObservableProperty]
    private int editorId;

    [ObservableProperty]
    private int editorCollectionId;

    [ObservableProperty]
    private string editorName = string.Empty;

    [ObservableProperty]
    private string editorDescription = string.Empty;

    [ObservableProperty]
    private int editorDurationMinutes = 60;

    [ObservableProperty]
    private decimal editorBasePrice;

    [ObservableProperty]
    private bool editorIsActive = true;

    [ObservableProperty]
    private string editorImagePath = string.Empty;

    [ObservableProperty]
    private DateTime? editorLastModifiedAt;

    public ServicesViewModel()
        : this(new CatalogDataService(), 3, "alphabetAsc")
    {
    }

    public ServicesViewModel(CatalogDataService catalogDataService, int pageSize, string defaultSort = "alphabetAsc")
    {
        this.catalogDataService = catalogDataService;
        this.pageSize = pageSize <= 0 ? 3 : pageSize;
        sortAscending = !string.Equals(defaultSort, "alphabetDesc", StringComparison.OrdinalIgnoreCase);
    }

    public string pageInfo => $"{currentPage}/{Math.Max(1, totalPages)}";

    public bool canGoPrevious => currentPage > 1;

    public bool canGoNext => currentPage < totalPages;

    public bool isNewRecord => editorId == 0;

    partial void OnSelectedItemChanged(ServiceListItem? value)
    {
        if (value is null)
        {
            return;
        }

        _ = LoadEditorAsync(value.id);
    }

    partial void OnCurrentPageChanged(int value)
    {
        OnPropertyChanged(nameof(pageInfo));
        OnPropertyChanged(nameof(canGoPrevious));
        OnPropertyChanged(nameof(canGoNext));
    }

    partial void OnTotalPagesChanged(int value)
    {
        OnPropertyChanged(nameof(pageInfo));
        OnPropertyChanged(nameof(canGoPrevious));
        OnPropertyChanged(nameof(canGoNext));
    }

    partial void OnEditorIdChanged(int value)
    {
        OnPropertyChanged(nameof(isNewRecord));
    }

    public async Task LoadAsync()
    {
        await LoadLookupAsync();
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        currentPage = 1;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task ToggleSortAsync()
    {
        sortAscending = !sortAscending;
        currentPage = 1;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (currentPage <= 1)
        {
            return;
        }

        currentPage--;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (currentPage >= totalPages)
        {
            return;
        }

        currentPage++;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (isBusy)
        {
            return;
        }

        if (!ValidateEditor())
        {
            return;
        }

        try
        {
            isBusy = true;
            statusMessage = "Сохранение услуги...";

            var savedId = await catalogDataService.SaveServiceAsync(new ServiceEditorModel
            {
                id = editorId,
                collectionId = editorCollectionId,
                name = editorName,
                description = editorDescription,
                durationMinutes = editorDurationMinutes,
                basePrice = editorBasePrice,
                isActive = editorIsActive,
                imagePath = string.IsNullOrWhiteSpace(editorImagePath) ? null : editorImagePath
            });

            await LoadLookupAsync();
            await LoadPageAsync();
            await LoadEditorAsync(savedId);

            statusMessage = "Услуга сохранена";
        }
        catch (Exception exception)
        {
            statusMessage = $"Ошибка сохранения: {exception.Message}";
        }
        finally
        {
            isBusy = false;
        }
    }

    [RelayCommand]
    private void CreateNew()
    {
        ClearEditor();
        statusMessage = "Режим добавления новой услуги";
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (isBusy || editorId == 0)
        {
            return;
        }

        try
        {
            isBusy = true;
            statusMessage = "Удаление услуги...";

            await catalogDataService.DeleteServiceAsync(editorId);

            ClearEditor();

            if (currentPage > 1 && items.Count == 1)
            {
                currentPage--;
            }

            await LoadPageAsync();
            statusMessage = "Услуга удалена";
        }
        catch (Exception exception)
        {
            statusMessage = $"Ошибка удаления: {exception.Message}";
        }
        finally
        {
            isBusy = false;
        }
    }

    [RelayCommand]
    private async Task ReloadAsync()
    {
        await LoadAsync();
    }

    private async Task LoadLookupAsync()
    {
        var lookupItems = await catalogDataService.GetCollectionLookupAsync();

        collectionOptions = new ObservableCollection<LookupItem>(lookupItems);

        if (editorCollectionId == 0 && collectionOptions.Count > 0)
        {
            editorCollectionId = collectionOptions[0].id;
        }
    }

    private async Task LoadPageAsync()
    {
        try
        {
            isBusy = true;
            statusMessage = "Загрузка списка услуг...";

            var result = await catalogDataService.GetServicesAsync(
                searchText,
                sortAscending,
                currentPage,
                pageSize);

            items = new ObservableCollection<ServiceListItem>(result.items);
            totalCount = result.totalCount;
            totalPages = Math.Max(1, result.totalPages);

            if (currentPage > totalPages)
            {
                currentPage = totalPages;
                result = await catalogDataService.GetServicesAsync(
                    searchText,
                    sortAscending,
                    currentPage,
                    pageSize);

                items = new ObservableCollection<ServiceListItem>(result.items);
                totalCount = result.totalCount;
                totalPages = Math.Max(1, result.totalPages);
            }

            if (editorId != 0)
            {
                var currentItem = items.FirstOrDefault(item => item.id == editorId);
                if (currentItem is not null)
                {
                    selectedItem = currentItem;
                }
            }

            statusMessage = $"Загружено услуг: {totalCount}";
        }
        catch (Exception exception)
        {
            statusMessage = $"Ошибка загрузки услуг: {exception.Message}";
        }
        finally
        {
            isBusy = false;
        }
    }

    private async Task LoadEditorAsync(int id)
    {
        try
        {
            isBusy = true;
            statusMessage = "Загрузка выбранной услуги...";

            var model = await catalogDataService.GetServiceByIdAsync(id);
            if (model is null)
            {
                statusMessage = "Выбранная услуга не найдена";
                return;
            }

            editorId = model.id;
            editorCollectionId = model.collectionId;
            editorName = model.name;
            editorDescription = model.description;
            editorDurationMinutes = model.durationMinutes;
            editorBasePrice = model.basePrice;
            editorIsActive = model.isActive;
            editorImagePath = model.imagePath ?? string.Empty;
            editorLastModifiedAt = model.lastModifiedAt;

            statusMessage = $"Выбрана услуга: {editorName}";
        }
        catch (Exception exception)
        {
            statusMessage = $"Ошибка загрузки услуги: {exception.Message}";
        }
        finally
        {
            isBusy = false;
        }
    }

    private bool ValidateEditor()
    {
        if (editorCollectionId <= 0)
        {
            statusMessage = "Выберите коллекцию";
            return false;
        }

        if (string.IsNullOrWhiteSpace(editorName))
        {
            statusMessage = "Введите название услуги";
            return false;
        }

        if (string.IsNullOrWhiteSpace(editorDescription))
        {
            statusMessage = "Введите описание услуги";
            return false;
        }

        if (editorDurationMinutes <= 0)
        {
            statusMessage = "Длительность должна быть больше 0";
            return false;
        }

        if (editorBasePrice < 0)
        {
            statusMessage = "Стоимость не может быть отрицательной";
            return false;
        }

        return true;
    }

    private void ClearEditor()
    {
        editorId = 0;
        editorCollectionId = collectionOptions.FirstOrDefault()?.id ?? 0;
        editorName = string.Empty;
        editorDescription = string.Empty;
        editorDurationMinutes = 60;
        editorBasePrice = 0;
        editorIsActive = true;
        editorImagePath = string.Empty;
        editorLastModifiedAt = null;
        selectedItem = null;
    }
}
