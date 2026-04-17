using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication2.ViewModels;

public partial class CollectionsViewModel : ViewModelBase
{
    private readonly CatalogDataService catalogDataService;
    private readonly int pageSize;

    [ObservableProperty]
    private ObservableCollection<CollectionListItem> items = new();

    [ObservableProperty]
    private ObservableCollection<LookupItem> directionLookupItems = new();

    [ObservableProperty]
    private CollectionListItem? selectedItem;

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
    private int editId;

    [ObservableProperty]
    private int editDirectionId;

    [ObservableProperty]
    private string editName = string.Empty;

    [ObservableProperty]
    private string editDescription = string.Empty;

    [ObservableProperty]
    private DateTime? editLastModifiedAt;

    public CollectionsViewModel()
        : this(new CatalogDataService(), 3, "alphabetAsc")
    {
    }

    public CollectionsViewModel(CatalogDataService catalogDataService, int pageSize, string defaultSort = "alphabetAsc")
    {
        this.catalogDataService = catalogDataService;
        this.pageSize = pageSize <= 0 ? 3 : pageSize;
        sortAscending = !string.Equals(defaultSort, "alphabetDesc", StringComparison.OrdinalIgnoreCase);
    }

    partial void OnSelectedItemChanged(CollectionListItem? value)
    {
        if (value is null)
        {
            return;
        }

        _ = LoadEditorAsync(value.id);
    }

    public async Task LoadAsync()
    {
        if (isBusy)
        {
            return;
        }

        try
        {
            isBusy = true;
            statusMessage = "Загрузка коллекций...";

            await LoadLookupAsync();

            var result = await catalogDataService.GetCollectionsAsync(
                searchText,
                sortAscending,
                currentPage,
                pageSize);

            items = new ObservableCollection<CollectionListItem>(result.items);
            totalCount = result.totalCount;
            totalPages = Math.Max(1, result.totalPages);

            if (currentPage > totalPages)
            {
                currentPage = totalPages;
                result = await catalogDataService.GetCollectionsAsync(
                    searchText,
                    sortAscending,
                    currentPage,
                    pageSize);

                items = new ObservableCollection<CollectionListItem>(result.items);
                totalCount = result.totalCount;
                totalPages = Math.Max(1, result.totalPages);
            }

            statusMessage = totalCount == 0
                ? "Коллекции не найдены"
                : $"Загружено коллекций: {totalCount}";

            if (selectedItem is not null)
            {
                var updatedSelection = items.FirstOrDefault(item => item.id == selectedItem.id);
                if (updatedSelection is not null)
                {
                    selectedItem = updatedSelection;
                }
            }
        }
        catch (Exception exception)
        {
            statusMessage = $"Ошибка загрузки коллекций: {exception.Message}";
        }
        finally
        {
            isBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        currentPage = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ToggleSortAsync()
    {
        sortAscending = !sortAscending;
        currentPage = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (currentPage >= totalPages || isBusy)
        {
            return;
        }

        currentPage++;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (currentPage <= 1 || isBusy)
        {
            return;
        }

        currentPage--;
        await LoadAsync();
    }

    [RelayCommand]
    private void CreateNew()
    {
        editId = 0;
        editName = string.Empty;
        editDescription = string.Empty;
        editLastModifiedAt = null;
        selectedItem = null;

        if (directionLookupItems.Count > 0)
        {
            editDirectionId = directionLookupItems[0].id;
        }

        statusMessage = "Режим добавления новой коллекции";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (isBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(editName))
        {
            statusMessage = "Введите название коллекции";
            return;
        }

        if (string.IsNullOrWhiteSpace(editDescription))
        {
            statusMessage = "Введите описание коллекции";
            return;
        }

        if (editDirectionId <= 0)
        {
            statusMessage = "Выберите направление";
            return;
        }

        try
        {
            isBusy = true;
            statusMessage = "Сохранение коллекции...";

            var id = await catalogDataService.SaveCollectionAsync(new CollectionEditorModel
            {
                id = editId,
                directionId = editDirectionId,
                name = editName,
                description = editDescription,
                lastModifiedAt = editLastModifiedAt
            });

            editId = id;
            await LoadAsync();
            await LoadEditorAsync(id);

            selectedItem = items.FirstOrDefault(item => item.id == id);
            statusMessage = "Коллекция сохранена";
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
    private async Task DeleteAsync()
    {
        if (isBusy || editId <= 0)
        {
            return;
        }

        try
        {
            isBusy = true;
            statusMessage = "Удаление коллекции...";

            await catalogDataService.DeleteCollectionAsync(editId);

            CreateNew();
            await LoadAsync();

            statusMessage = "Коллекция удалена";
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

    private async Task LoadEditorAsync(int id)
    {
        try
        {
            var model = await catalogDataService.GetCollectionByIdAsync(id);
            if (model is null)
            {
                return;
            }

            editId = model.id;
            editDirectionId = model.directionId;
            editName = model.name;
            editDescription = model.description;
            editLastModifiedAt = model.lastModifiedAt;
            statusMessage = $"Выбрана коллекция: {model.name}";
        }
        catch (Exception exception)
        {
            statusMessage = $"Ошибка загрузки выбранной коллекции: {exception.Message}";
        }
    }

    private async Task LoadLookupAsync()
    {
        var directions = await catalogDataService.GetDirectionLookupAsync();
        directionLookupItems = new ObservableCollection<LookupItem>(directions);

        if (editDirectionId <= 0 && directionLookupItems.Count > 0)
        {
            editDirectionId = directionLookupItems[0].id;
        }
    }
}
