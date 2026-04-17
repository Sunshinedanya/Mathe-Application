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
        if (IsLoading)
        {
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Загрузка направлений...";

            var pageResult = await catalogDataService.GetDirectionsAsync(
                SearchText,
                SortAscending,
                CurrentPage,
                pageSize);

            Items = new ObservableCollection<DirectionListItem>(pageResult.items);
            TotalCount = pageResult.totalCount;
            TotalPages = Math.Max(1, pageResult.totalPages);

            if (CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
                pageResult = await catalogDataService.GetDirectionsAsync(
                    SearchText,
                    SortAscending,
                    CurrentPage,
                    pageSize);

                Items = new ObservableCollection<DirectionListItem>(pageResult.items);
                TotalCount = pageResult.totalCount;
                TotalPages = Math.Max(1, pageResult.totalPages);
            }

            if (SelectedItem is not null)
            {
                var currentSelected = Items.FirstOrDefault(item => item.id == SelectedItem.id);
                if (currentSelected is not null)
                {
                    SelectedItem = currentSelected;
                }
            }

            StatusMessage = TotalCount == 0
                ? "Направления не найдены"
                : $"Загружено направлений: {TotalCount}";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка загрузки направлений: {exception.Message}";
        }
        finally
        {
            IsLoading = false;
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
    private async Task PreviousPageAsync()
    {
        if (CurrentPage <= 1)
        {
            return;
        }

        CurrentPage--;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage >= TotalPages)
        {
            return;
        }

        CurrentPage++;
        await LoadAsync();
    }

    [RelayCommand]
    private void CreateNew()
    {
        SelectedItem = null;
        Id = 0;
        Name = string.Empty;
        Description = string.Empty;
        LastModifiedAt = null;
        StatusMessage = "Создание нового направления";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsLoading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            StatusMessage = "Введите название направления";
            return;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            StatusMessage = "Введите описание направления";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = Id == 0
                ? "Добавление направления..."
                : "Сохранение направления...";

            var savedId = await catalogDataService.SaveDirectionAsync(new DirectionEditorModel
            {
                id = Id,
                name = Name,
                description = Description,
                lastModifiedAt = LastModifiedAt
            });

            Id = savedId;
            await LoadAsync();
            await LoadSelectedAsync(savedId);

            StatusMessage = "Направление успешно сохранено";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка сохранения направления: {exception.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Id == 0)
        {
            StatusMessage = "Сначала выберите направление";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Удаление направления...";

            await catalogDataService.DeleteDirectionAsync(Id);
            CreateNew();
            await LoadAsync();

            StatusMessage = "Направление удалено";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка удаления направления: {exception.Message}";
        }
        finally
        {
            IsLoading = false;
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

            Id = model.id;
            Name = model.name;
            Description = model.description;
            LastModifiedAt = model.lastModifiedAt;
        }
        catch (Exception exception)
        {
            StatusMessage = $"Ошибка загрузки выбранного направления: {exception.Message}";
        }
    }
}
