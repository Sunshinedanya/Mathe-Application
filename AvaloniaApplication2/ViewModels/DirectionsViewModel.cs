using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaApplication2.Services;

namespace AvaloniaApplication2.ViewModels;

public partial class DirectionsViewModel : ViewModelBase
{
    private readonly CatalogDataService catalogDataService;
    private readonly int pageSize;

    [ObservableProperty]
    private ObservableCollection<DirectionListItem> items = new();

    [ObservableProperty]
    private DirectionListItem? selectedItem;

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
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "Готово";

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private DateTime? lastModifiedAt;

    public DirectionsViewModel()
        : this(new CatalogDataService(), 3, "alphabetAsc")
    {
    }

    public DirectionsViewModel(CatalogDataService catalogDataService, int pageSize, string defaultSort = "alphabetAsc")
    {
        this.catalogDataService = catalogDataService;
        this.pageSize = pageSize <= 0 ? 3 : pageSize;
        sortAscending = !string.Equals(defaultSort, "alphabetDesc", StringComparison.OrdinalIgnoreCase);
    }

    partial void OnSelectedItemChanged(DirectionListItem? value)
    {
        if (value is null)
        {
            return;
        }

        _ = LoadSelectedAsync(value.id);
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (isLoading)
        {
            return;
        }

        try
        {
            isLoading = true;
            statusMessage = "Загрузка направлений...";

            var pageResult = await catalogDataService.GetDirectionsAsync(
                searchText,
                sortAscending,
                currentPage,
                pageSize);

            items = new ObservableCollection<DirectionListItem>(pageResult.items);
            totalCount = pageResult.totalCount;
            totalPages = Math.Max(1, pageResult.totalPages);

            if (currentPage > totalPages)
            {
                currentPage = totalPages;
                pageResult = await catalogDataService.GetDirectionsAsync(
                    searchText,
                    sortAscending,
                    currentPage,
                    pageSize);

                items = new ObservableCollection<DirectionListItem>(pageResult.items);
                totalCount = pageResult.totalCount;
                totalPages = Math.Max(1, pageResult.totalPages);
            }

            if (selectedItem is not null)
            {
                var currentSelected = items.FirstOrDefault(item => item.id == selectedItem.id);
                if (currentSelected is not null)
                {
                    selectedItem = currentSelected;
                }
            }

            statusMessage = totalCount == 0
                ? "Направления не найдены"
                : $"Загружено направлений: {totalCount}";
        }
        catch (Exception exception)
        {
            statusMessage = $"Ошибка загрузки направлений: {exception.Message}";
        }
        finally
        {
            isLoading = false;
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
    private async Task PreviousPageAsync()
    {
        if (currentPage <= 1)
        {
            return;
        }

        currentPage--;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (currentPage >= totalPages)
        {
            return;
        }

        currentPage++;
        await LoadAsync();
    }

    [RelayCommand]
    private void CreateNew()
    {
        selectedItem = null;
        id = 0;
        name = string.Empty;
        description = string.Empty;
        lastModifiedAt = null;
        statusMessage = "Создание нового направления";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (isLoading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            statusMessage = "Введите название направления";
            return;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            statusMessage = "Введите описание направления";
            return;
        }

        try
        {
            isLoading = true;
            statusMessage = id == 0
                ? "Добавление направления..."
                : "Сохранение направления...";

            var savedId = await catalogDataService.SaveDirectionAsync(new DirectionEditorModel
            {
                id = id,
                name = name,
                description = description,
                lastModifiedAt = lastModifiedAt
            });

            id = savedId;
            await LoadAsync();
            await LoadSelectedAsync(savedId);

            statusMessage = "Направление успешно сохранено";
        }
        catch (Exception exception)
        {
            statusMessage = $"Ошибка сохранения направления: {exception.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (id == 0)
        {
            statusMessage = "Сначала выберите направление";
            return;
        }

        try
        {
            isLoading = true;
            statusMessage = "Удаление направления...";

            await catalogDataService.DeleteDirectionAsync(id);
            CreateNew();
            await LoadAsync();

            statusMessage = "Направление удалено";
        }
        catch (Exception exception)
        {
            statusMessage = $"Ошибка удаления направления: {exception.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task LoadSelectedAsync(int directionId)
    {
        try
        {
            var model = await catalogDataService.GetDirectionByIdAsync(directionId);
            if (model is null)
            {
                return;
            }

            id = model.id;
            name = model.name;
            description = model.description;
            lastModifiedAt = model.lastModifiedAt;
        }
        catch (Exception exception)
        {
            statusMessage = $"Ошибка загрузки выбранного направления: {exception.Message}";
        }
    }
}
