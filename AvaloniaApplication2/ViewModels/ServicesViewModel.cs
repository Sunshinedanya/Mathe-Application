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

    public string pageInfo => $"{CurrentPage}/{Math.Max(1, TotalPages)}";

    public bool canGoPrevious => CurrentPage > 1;

    public bool canGoNext => CurrentPage < TotalPages;

    public bool isNewRecord => EditorId == 0;

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
        CurrentPage = 1;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task ToggleSortAsync()
    {
        SortAscending = !SortAscending;
        CurrentPage = 1;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage <= 1)
        {
            return;
        }

        CurrentPage--;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage >= TotalPages)
        {
            return;
        }

        CurrentPage++;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!ValidateEditor())
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Сохранение услуги...";

            var savedId = await catalogDataService.SaveServiceAsync(new ServiceEditorModel
            {
                id = EditorId,
                collectionId = EditorCollectionId,
                name = EditorName,
                description = EditorDescription,
                durationMinutes = EditorDurationMinutes,
                basePrice = EditorBasePrice,
                isActive = EditorIsActive,
                imagePath = string.IsNullOrWhiteSpace(EditorImagePath) ? null : EditorImagePath
            });

            await LoadLookupAsync();
            await LoadPageAsync();
            await LoadEditorAsync(savedId);

            StatusMessage = "Услуга сохранена";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка сохранения: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CreateNew()
    {
        ClearEditor();
        StatusMessage = "Режим добавления новой услуги";
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (IsBusy || EditorId == 0)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Удаление услуги...";

            await catalogDataService.DeleteServiceAsync(EditorId);

            ClearEditor();

            if (CurrentPage > 1 && Items.Count == 1)
            {
                CurrentPage--;
            }

            await LoadPageAsync();
            StatusMessage = "Услуга удалена";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка удаления: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
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

        CollectionOptions = new ObservableCollection<LookupItem>(lookupItems);

        if (EditorCollectionId == 0 && CollectionOptions.Count > 0)
        {
            EditorCollectionId = CollectionOptions[0].id;
        }
    }

    private async Task LoadPageAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Загрузка списка услуг...";

            var result = await catalogDataService.GetServicesAsync(
                SearchText,
                SortAscending,
                CurrentPage,
                pageSize);

            Items = new ObservableCollection<ServiceListItem>(result.items);
            TotalCount = result.totalCount;
            TotalPages = Math.Max(1, result.totalPages);

            if (CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
                result = await catalogDataService.GetServicesAsync(
                    SearchText,
                    SortAscending,
                    CurrentPage,
                    pageSize);

                Items = new ObservableCollection<ServiceListItem>(result.items);
                TotalCount = result.totalCount;
                TotalPages = Math.Max(1, result.totalPages);
            }

            if (EditorId != 0)
            {
                var currentItem = Items.FirstOrDefault(item => item.id == EditorId);
                if (currentItem is not null)
                {
                    SelectedItem = currentItem;
                }
            }

            StatusMessage = $"Загружено услуг: {TotalCount}";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка загрузки услуг: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadEditorAsync(int id)
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Загрузка выбранной услуги...";

            var model = await catalogDataService.GetServiceByIdAsync(id);
            if (model is null)
            {
                StatusMessage = "Выбранная услуга не найдена";
                return;
            }

            EditorId = model.id;
            EditorCollectionId = model.collectionId;
            EditorName = model.name;
            EditorDescription = model.description;
            EditorDurationMinutes = model.durationMinutes;
            EditorBasePrice = model.basePrice;
            EditorIsActive = model.isActive;
            EditorImagePath = model.imagePath ?? string.Empty;
            EditorLastModifiedAt = model.lastModifiedAt;

            StatusMessage = $"Выбрана услуга: {EditorName}";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка загрузки услуги: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidateEditor()
    {
        if (EditorCollectionId <= 0)
        {
            StatusMessage = "Выберите коллекцию";
            return false;
        }

        if (string.IsNullOrWhiteSpace(EditorName))
        {
            StatusMessage = "Введите название услуги";
            return false;
        }

        if (string.IsNullOrWhiteSpace(EditorDescription))
        {
            StatusMessage = "Введите описание услуги";
            return false;
        }

        if (EditorDurationMinutes <= 0)
        {
            StatusMessage = "Длительность должна быть больше 0";
            return false;
        }

        if (EditorBasePrice < 0)
        {
            StatusMessage = "Стоимость не может быть отрицательной";
            return false;
        }

        return true;
    }

    private void ClearEditor()
    {
        EditorId = 0;
        EditorCollectionId = CollectionOptions.FirstOrDefault()?.id ?? 0;
        EditorName = string.Empty;
        EditorDescription = string.Empty;
        EditorDurationMinutes = 60;
        EditorBasePrice = 0;
        EditorIsActive = true;
        EditorImagePath = string.Empty;
        EditorLastModifiedAt = null;
        SelectedItem = null;
    }
}
