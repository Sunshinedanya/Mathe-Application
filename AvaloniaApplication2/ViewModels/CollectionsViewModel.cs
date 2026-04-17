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
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Загрузка коллекций...";

            await LoadLookupAsync();

            var result = await catalogDataService.GetCollectionsAsync(
                SearchText,
                SortAscending,
                CurrentPage,
                pageSize);

            Items = new ObservableCollection<CollectionListItem>(result.items);
            TotalCount = result.totalCount;
            TotalPages = Math.Max(1, result.totalPages);

            if (CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
                result = await catalogDataService.GetCollectionsAsync(
                    SearchText,
                    SortAscending,
                    CurrentPage,
                    pageSize);

                Items = new ObservableCollection<CollectionListItem>(result.items);
                TotalCount = result.totalCount;
                TotalPages = Math.Max(1, result.totalPages);
            }

            StatusMessage = TotalCount == 0
                ? "Коллекции не найдены"
                : $"Загружено коллекций: {TotalCount}";

            if (SelectedItem is not null)
            {
                var updatedSelection = Items.FirstOrDefault(item => item.id == SelectedItem.id);
                if (updatedSelection is not null)
                {
                    SelectedItem = updatedSelection;
                }
            }
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка загрузки коллекций: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ToggleSortAsync()
    {
        SortAscending = !SortAscending;
        CurrentPage = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage >= TotalPages || IsBusy)
        {
            return;
        }

        CurrentPage++;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage <= 1 || IsBusy)
        {
            return;
        }

        CurrentPage--;
        await LoadAsync();
    }

    [RelayCommand]
    private void CreateNew()
    {
        EditId = 0;
        EditName = string.Empty;
        EditDescription = string.Empty;
        EditLastModifiedAt = null;
        SelectedItem = null;

        if (DirectionLookupItems.Count > 0)
        {
            EditDirectionId = DirectionLookupItems[0].id;
        }

        StatusMessage = "Режим добавления новой коллекции";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(EditName))
        {
            StatusMessage = "Введите название коллекции";
            return;
        }

        if (string.IsNullOrWhiteSpace(EditDescription))
        {
            StatusMessage = "Введите описание коллекции";
            return;
        }

        if (EditDirectionId <= 0)
        {
            StatusMessage = "Выберите направление";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Сохранение коллекции...";

            var id = await catalogDataService.SaveCollectionAsync(new CollectionEditorModel
            {
                id = EditId,
                directionId = EditDirectionId,
                name = EditName,
                description = EditDescription,
                lastModifiedAt = EditLastModifiedAt
            });

            EditId = id;
            await LoadAsync();
            await LoadEditorAsync(id);

            SelectedItem = Items.FirstOrDefault(item => item.id == id);
            StatusMessage = "Коллекция сохранена";
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
    private async Task DeleteAsync()
    {
        if (IsBusy || EditId <= 0)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Удаление коллекции...";

            await catalogDataService.DeleteCollectionAsync(EditId);

            CreateNew();
            await LoadAsync();

            StatusMessage = "Коллекция удалена";
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

    private async Task LoadEditorAsync(int id)
    {
        try
        {
            var model = await catalogDataService.GetCollectionByIdAsync(id);
            if (model is null)
            {
                return;
            }

            EditId = model.id;
            EditDirectionId = model.directionId;
            EditName = model.name;
            EditDescription = model.description;
            EditLastModifiedAt = model.lastModifiedAt;
            StatusMessage = $"Выбрана коллекция: {model.name}";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка загрузки выбранной коллекции: {exception.Message}";
        }
    }

    private async Task LoadLookupAsync()
    {
        var directions = await catalogDataService.GetDirectionLookupAsync();
        DirectionLookupItems = new ObservableCollection<LookupItem>(directions);

        if (EditDirectionId <= 0 && DirectionLookupItems.Count > 0)
        {
            EditDirectionId = DirectionLookupItems[0].id;
        }
    }
}
