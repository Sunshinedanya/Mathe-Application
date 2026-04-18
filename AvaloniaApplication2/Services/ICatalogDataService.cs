namespace AvaloniaApplication2.Services;

public interface ICatalogDataService
{
    Task InitializeDatabaseAsync();
    Task<PagedResult<ServiceListItem>> GetServicesAsync(string? searchText, bool sortAscending, int page, int pageSize);
    Task<PagedResult<CollectionListItem>> GetCollectionsAsync(string? searchText, bool sortAscending, int page, int pageSize);
    Task<PagedResult<DirectionListItem>> GetDirectionsAsync(string? searchText, bool sortAscending, int page, int pageSize);
    Task<List<LookupItem>> GetDirectionLookupAsync();
    Task<List<LookupItem>> GetCollectionLookupAsync();
    Task<ServiceEditorModel?> GetServiceByIdAsync(int id);
    Task<CollectionEditorModel?> GetCollectionByIdAsync(int id);
    Task<DirectionEditorModel?> GetDirectionByIdAsync(int id);
    Task<int> SaveServiceAsync(ServiceEditorModel model);
    Task<int> SaveCollectionAsync(CollectionEditorModel model);
    Task<int> SaveDirectionAsync(DirectionEditorModel model);
    Task DeleteServiceAsync(int id);
    Task DeleteCollectionAsync(int id);
    Task DeleteDirectionAsync(int id);
}
