using AvaloniaApplication2.Services;
using AvaloniaApplication2.ViewModels;
using FluentAssertions;
using NSubstitute;

namespace AvaloniaApplication2.Tests;

// ---------------------------------------------------------------------------
// DirectionsViewModel — lastModifiedAt
// ---------------------------------------------------------------------------

public class DirectionsViewModelLastModifiedAtTests
{
    private static readonly DateTime FixedDate =
        new(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);

    /// <summary>
    /// Returns a mock that satisfies the minimum contract for LoadAsync:
    /// GetDirectionsAsync returns an empty page.
    /// </summary>
    private static ICatalogDataService CreateMinimalMock()
    {
        var mock = Substitute.For<ICatalogDataService>();
        mock.GetDirectionsAsync(Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(new PagedResult<DirectionListItem>([], 0, 1, 3));
        return mock;
    }

    // -----------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_SingleItem_HasCorrectLastModifiedAt()
    {
        // Arrange
        var mock = Substitute.For<ICatalogDataService>();
        mock.GetDirectionsAsync(Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(new PagedResult<DirectionListItem>(
                new List<DirectionListItem>
                {
                    new() { id = 1, name = "Beauty", description = "Desc", lastModifiedAt = FixedDate }
                },
                1, 1, 3));

        var vm = new DirectionsViewModel(mock, 3);

        // Act
        await vm.LoadAsync();

        // Assert
        vm.Items.Should().ContainSingle();
        vm.Items[0].lastModifiedAt.Should().Be(FixedDate);
    }

    [Fact]
    public async Task LoadAsync_MultipleItems_EachHasCorrectLastModifiedAt()
    {
        // Arrange
        var date1 = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2024, 3, 20, 12, 0, 0, DateTimeKind.Utc);
        var date3 = new DateTime(2024, 5, 5, 8, 0, 0, DateTimeKind.Utc);

        var mock = Substitute.For<ICatalogDataService>();
        mock.GetDirectionsAsync(Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(new PagedResult<DirectionListItem>(
                new List<DirectionListItem>
                {
                    new() { id = 1, name = "A", description = "d", lastModifiedAt = date1 },
                    new() { id = 2, name = "B", description = "d", lastModifiedAt = date2 },
                    new() { id = 3, name = "C", description = "d", lastModifiedAt = date3 }
                },
                3, 1, 3));

        var vm = new DirectionsViewModel(mock, 3);

        // Act
        await vm.LoadAsync();

        // Assert
        vm.Items[0].lastModifiedAt.Should().Be(date1);
        vm.Items[1].lastModifiedAt.Should().Be(date2);
        vm.Items[2].lastModifiedAt.Should().Be(date3);
    }

    [Fact]
    public async Task LoadAsync_EmptyPage_LastModifiedAtRemainsNull()
    {
        // Arrange
        var mock = CreateMinimalMock();
        var vm = new DirectionsViewModel(mock, 3);

        // Act
        await vm.LoadAsync();

        // Assert
        vm.Items.Should().BeEmpty();
        vm.LastModifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task SelectedItem_Set_LoadsLastModifiedAtFromService()
    {
        // Arrange
        var mock = Substitute.For<ICatalogDataService>();
        mock.GetDirectionsAsync(Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(new PagedResult<DirectionListItem>(
                new List<DirectionListItem>
                {
                    new() { id = 7, name = "Spa", description = "Desc", lastModifiedAt = FixedDate }
                },
                1, 1, 3));

        mock.GetDirectionByIdAsync(7)
            .Returns(new DirectionEditorModel
            {
                id = 7, name = "Spa", description = "Desc",
                lastModifiedAt = FixedDate
            });

        var vm = new DirectionsViewModel(mock, 3);
        await vm.LoadAsync();

        // Act — OnSelectedItemChanged fires fire-and-forget LoadSelectedAsync
        vm.SelectedItem = vm.Items[0];
        await Task.Delay(150); // wait for fire-and-forget to finish

        // Assert
        vm.LastModifiedAt.Should().Be(FixedDate);
    }

    [Fact]
    public async Task SelectedItem_SetToNull_DoesNotClearLastModifiedAt()
    {
        // Arrange
        var mock = CreateMinimalMock();
        var vm = new DirectionsViewModel(mock, 3)
        {
            LastModifiedAt = FixedDate
        };

        // Act — null triggers the early-return guard in OnSelectedItemChanged
        vm.SelectedItem = null;
        await Task.Delay(50);

        // Assert
        vm.LastModifiedAt.Should().Be(FixedDate);
    }

    [Fact]
    public void CreateNew_ClearsLastModifiedAt()
    {
        // Arrange
        var mock = CreateMinimalMock();
        var vm = new DirectionsViewModel(mock, 3) { LastModifiedAt = FixedDate };

        // Act
        vm.CreateNewCommand.Execute(null);

        // Assert
        vm.LastModifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_SetsLastModifiedAt_ViaLoadSelectedAsync()
    {
        // Arrange
        var savedDate = new DateTime(2024, 8, 1, 14, 0, 0, DateTimeKind.Utc);
        var mock = Substitute.For<ICatalogDataService>();

        // LoadAsync (called internally, but skipped due to IsLoading guard — see below)
        mock.GetDirectionsAsync(Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(new PagedResult<DirectionListItem>([], 0, 1, 3));

        mock.SaveDirectionAsync(Arg.Any<DirectionEditorModel>()).Returns(42);

        // LoadSelectedAsync is called directly after save → sets LastModifiedAt
        mock.GetDirectionByIdAsync(42)
            .Returns(new DirectionEditorModel
            {
                id = 42, name = "New Dir", description = "Desc",
                lastModifiedAt = savedDate
            });

        var vm = new DirectionsViewModel(mock, 3)
        {
            Name = "New Dir",
            Description = "Desc"
        };

        // Act
        await vm.SaveCommand.ExecuteAsync(null);

        // Assert
        vm.LastModifiedAt.Should().Be(savedDate);
    }

    [Fact]
    public async Task SaveAsync_ServiceReturnsNullForLoadSelected_LastModifiedAtIsNull()
    {
        // Arrange
        var mock = Substitute.For<ICatalogDataService>();
        mock.GetDirectionsAsync(Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(new PagedResult<DirectionListItem>([], 0, 1, 3));
        mock.SaveDirectionAsync(Arg.Any<DirectionEditorModel>()).Returns(99);
        mock.GetDirectionByIdAsync(99).Returns((DirectionEditorModel?)null);

        var vm = new DirectionsViewModel(mock, 3) { Name = "X", Description = "Y" };

        // Act
        await vm.SaveCommand.ExecuteAsync(null);

        // Assert — LoadSelectedAsync returns early when model is null
        vm.LastModifiedAt.Should().BeNull();
    }
}

// ---------------------------------------------------------------------------
// CollectionsViewModel — lastModifiedAt
// ---------------------------------------------------------------------------

public class CollectionsViewModelLastModifiedAtTests
{
    private static readonly DateTime FixedDate =
        new(2024, 7, 22, 9, 15, 0, DateTimeKind.Utc);

    private static ICatalogDataService CreateMock(
        IReadOnlyList<CollectionListItem>? collections = null,
        IReadOnlyList<LookupItem>? directionLookup = null)
    {
        var mock = Substitute.For<ICatalogDataService>();

        mock.GetDirectionLookupAsync()
            .Returns(directionLookup?.ToList() ?? []);

        mock.GetCollectionsAsync(Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(new PagedResult<CollectionListItem>(
                collections ?? [],
                collections?.Count ?? 0,
                1, 3));

        return mock;
    }

    // -----------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_SingleItem_HasCorrectLastModifiedAt()
    {
        // Arrange
        var mock = CreateMock(new List<CollectionListItem>
        {
            new() { id = 1, name = "Col A", description = "Desc", directionId = 1, directionName = "Dir", lastModifiedAt = FixedDate }
        });

        var vm = new CollectionsViewModel(mock, 3);

        // Act
        await vm.LoadAsync();

        // Assert
        vm.Items.Should().ContainSingle();
        vm.Items[0].lastModifiedAt.Should().Be(FixedDate);
    }

    [Fact]
    public async Task LoadAsync_MultipleItems_EachHasCorrectLastModifiedAt()
    {
        // Arrange
        var date1 = new DateTime(2024, 2, 14, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2024, 4, 1, 6, 0, 0, DateTimeKind.Utc);

        var mock = CreateMock(new List<CollectionListItem>
        {
            new() { id = 1, name = "Col A", description = "d", directionId = 1, directionName = "Dir", lastModifiedAt = date1 },
            new() { id = 2, name = "Col B", description = "d", directionId = 1, directionName = "Dir", lastModifiedAt = date2 }
        });

        var vm = new CollectionsViewModel(mock, 3);

        // Act
        await vm.LoadAsync();

        // Assert
        vm.Items[0].lastModifiedAt.Should().Be(date1);
        vm.Items[1].lastModifiedAt.Should().Be(date2);
    }

    [Fact]
    public async Task LoadAsync_EmptyPage_EditLastModifiedAtRemainsNull()
    {
        // Arrange
        var mock = CreateMock();
        var vm = new CollectionsViewModel(mock, 3);

        // Act
        await vm.LoadAsync();

        // Assert
        vm.Items.Should().BeEmpty();
        vm.EditLastModifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task SelectedItem_Set_LoadsEditLastModifiedAtFromService()
    {
        // Arrange
        var mock = CreateMock(new List<CollectionListItem>
        {
            new() { id = 5, name = "Col X", description = "d", directionId = 2, directionName = "Dir", lastModifiedAt = FixedDate }
        });

        mock.GetCollectionByIdAsync(5)
            .Returns(new CollectionEditorModel
            {
                id = 5, directionId = 2, name = "Col X", description = "d",
                lastModifiedAt = FixedDate
            });

        var vm = new CollectionsViewModel(mock, 3);
        await vm.LoadAsync();

        // Act
        vm.SelectedItem = vm.Items[0];
        await Task.Delay(150);

        // Assert
        vm.EditLastModifiedAt.Should().Be(FixedDate);
    }

    [Fact]
    public async Task SelectedItem_SetToNull_DoesNotClearEditLastModifiedAt()
    {
        // Arrange
        var mock = CreateMock();
        var vm = new CollectionsViewModel(mock, 3) { EditLastModifiedAt = FixedDate };

        // Act
        vm.SelectedItem = null;
        await Task.Delay(50);

        // Assert
        vm.EditLastModifiedAt.Should().Be(FixedDate);
    }

    [Fact]
    public void CreateNew_ClearsEditLastModifiedAt()
    {
        // Arrange
        var mock = CreateMock();
        var vm = new CollectionsViewModel(mock, 3) { EditLastModifiedAt = FixedDate };

        // Act
        vm.CreateNewCommand.Execute(null);

        // Assert
        vm.EditLastModifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task SelectedItem_Set_ServiceReturnsNull_EditLastModifiedAtRemainsNull()
    {
        // Arrange — simulates a race condition where the record was deleted
        var mock = CreateMock(new List<CollectionListItem>
        {
            new() { id = 99, name = "Ghost", description = "d", directionId = 1, directionName = "Dir", lastModifiedAt = FixedDate }
        });
        mock.GetCollectionByIdAsync(99).Returns((CollectionEditorModel?)null);

        var vm = new CollectionsViewModel(mock, 3) { EditLastModifiedAt = null };
        await vm.LoadAsync();

        // Act
        vm.SelectedItem = vm.Items[0];
        await Task.Delay(150);

        // Assert
        vm.EditLastModifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task SelectedItem_Set_DifferentDateThanListItem_UsesEditorModelDate()
    {
        // Arrange — list item carries a stale date; GetCollectionByIdAsync returns fresher date
        var staleDate  = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var freshDate  = new DateTime(2024, 12, 31, 23, 59, 0, DateTimeKind.Utc);

        var mock = CreateMock(new List<CollectionListItem>
        {
            new() { id = 3, name = "Col", description = "d", directionId = 1, directionName = "Dir", lastModifiedAt = staleDate }
        });

        mock.GetCollectionByIdAsync(3)
            .Returns(new CollectionEditorModel
            {
                id = 3, directionId = 1, name = "Col", description = "d",
                lastModifiedAt = freshDate  // fresher value from DB
            });

        var vm = new CollectionsViewModel(mock, 3);
        await vm.LoadAsync();

        // Act
        vm.SelectedItem = vm.Items[0];
        await Task.Delay(150);

        // Assert
        vm.EditLastModifiedAt.Should().Be(freshDate);
    }
}

// ---------------------------------------------------------------------------
// ServicesViewModel — lastModifiedAt
// ---------------------------------------------------------------------------

public class ServicesViewModelLastModifiedAtTests
{
    private static readonly DateTime FixedDate =
        new(2024, 9, 5, 16, 45, 0, DateTimeKind.Utc);

    private static ICatalogDataService CreateMock(
        IReadOnlyList<ServiceListItem>? services = null,
        IReadOnlyList<LookupItem>? collectionLookup = null)
    {
        var mock = Substitute.For<ICatalogDataService>();

        mock.GetCollectionLookupAsync()
            .Returns(collectionLookup?.ToList() ?? []);

        mock.GetServicesAsync(Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(new PagedResult<ServiceListItem>(
                services ?? [],
                services?.Count ?? 0,
                1, 3));

        return mock;
    }

    // -----------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_SingleItem_HasCorrectLastModifiedAt()
    {
        // Arrange
        var mock = CreateMock(new List<ServiceListItem>
        {
            new()
            {
                id = 10, name = "Haircut", description = "Desc", durationMinutes = 30,
                basePrice = 500m, isActive = true, collectionId = 1,
                collectionName = "Col A", directionName = "Dir A",
                lastModifiedAt = FixedDate
            }
        });

        var vm = new ServicesViewModel(mock, 3);

        // Act
        await vm.LoadAsync();

        // Assert
        vm.Items.Should().ContainSingle();
        vm.Items[0].lastModifiedAt.Should().Be(FixedDate);
    }

    [Fact]
    public async Task LoadAsync_MultipleItems_EachHasCorrectLastModifiedAt()
    {
        // Arrange
        var date1 = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2024, 5, 25, 12, 30, 0, DateTimeKind.Utc);
        var date3 = new DateTime(2024, 9, 1, 18, 0, 0, DateTimeKind.Utc);

        var mock = CreateMock(new List<ServiceListItem>
        {
            new() { id = 1, name = "S1", description = "d", collectionId = 1, collectionName = "C", directionName = "D", lastModifiedAt = date1 },
            new() { id = 2, name = "S2", description = "d", collectionId = 1, collectionName = "C", directionName = "D", lastModifiedAt = date2 },
            new() { id = 3, name = "S3", description = "d", collectionId = 1, collectionName = "C", directionName = "D", lastModifiedAt = date3 }
        });

        var vm = new ServicesViewModel(mock, 3);

        // Act
        await vm.LoadAsync();

        // Assert
        vm.Items[0].lastModifiedAt.Should().Be(date1);
        vm.Items[1].lastModifiedAt.Should().Be(date2);
        vm.Items[2].lastModifiedAt.Should().Be(date3);
    }

    [Fact]
    public async Task LoadAsync_EmptyPage_EditorLastModifiedAtRemainsNull()
    {
        // Arrange
        var mock = CreateMock();
        var vm = new ServicesViewModel(mock, 3);

        // Act
        await vm.LoadAsync();

        // Assert
        vm.Items.Should().BeEmpty();
        vm.EditorLastModifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task SelectedItem_Set_LoadsEditorLastModifiedAtFromService()
    {
        // Arrange
        var mock = CreateMock(new List<ServiceListItem>
        {
            new()
            {
                id = 20, name = "Manicure", description = "Desc", durationMinutes = 60,
                basePrice = 800m, isActive = true, collectionId = 2,
                collectionName = "Nails", directionName = "Beauty",
                lastModifiedAt = FixedDate
            }
        });

        mock.GetServiceByIdAsync(20)
            .Returns(new ServiceEditorModel
            {
                id = 20, collectionId = 2, name = "Manicure",
                description = "Desc", durationMinutes = 60,
                basePrice = 800m, isActive = true,
                lastModifiedAt = FixedDate
            });

        var vm = new ServicesViewModel(mock, 3);
        await vm.LoadAsync();

        // Act
        vm.SelectedItem = vm.Items[0];
        await Task.Delay(150);

        // Assert
        vm.EditorLastModifiedAt.Should().Be(FixedDate);
    }

    [Fact]
    public async Task SelectedItem_SetToNull_DoesNotClearEditorLastModifiedAt()
    {
        // Arrange
        var mock = CreateMock();
        var vm = new ServicesViewModel(mock, 3) { EditorLastModifiedAt = FixedDate };

        // Act
        vm.SelectedItem = null;
        await Task.Delay(50);

        // Assert
        vm.EditorLastModifiedAt.Should().Be(FixedDate);
    }

    [Fact]
    public void CreateNew_ClearsEditorLastModifiedAt()
    {
        // Arrange — preload a collection option so ClearEditor can pick it up
        var colLookup = new List<LookupItem> { new() { id = 1, name = "Col" } };
        var mock = CreateMock(collectionLookup: colLookup);

        var vm = new ServicesViewModel(mock, 3) { EditorLastModifiedAt = FixedDate };

        // Act
        vm.CreateNewCommand.Execute(null);

        // Assert
        vm.EditorLastModifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task SelectedItem_Set_ServiceReturnsNull_EditorLastModifiedAtRemainsNull()
    {
        // Arrange
        var mock = CreateMock(new List<ServiceListItem>
        {
            new()
            {
                id = 77, name = "Ghost Service", description = "d", collectionId = 1,
                collectionName = "C", directionName = "D", lastModifiedAt = FixedDate
            }
        });
        mock.GetServiceByIdAsync(77).Returns((ServiceEditorModel?)null);

        var vm = new ServicesViewModel(mock, 3) { EditorLastModifiedAt = null };
        await vm.LoadAsync();

        // Act
        vm.SelectedItem = vm.Items[0];
        await Task.Delay(150);

        // Assert
        vm.EditorLastModifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task SelectedItem_Set_DifferentDateThanListItem_UsesEditorModelDate()
    {
        // Arrange — list date may be stale; editor model carries the definitive value
        var staleDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var freshDate = new DateTime(2024, 11, 30, 23, 0, 0, DateTimeKind.Utc);

        var mock = CreateMock(new List<ServiceListItem>
        {
            new()
            {
                id = 55, name = "Svc", description = "d", collectionId = 1,
                collectionName = "C", directionName = "D", lastModifiedAt = staleDate
            }
        });

        mock.GetServiceByIdAsync(55)
            .Returns(new ServiceEditorModel
            {
                id = 55, collectionId = 1, name = "Svc", description = "d",
                durationMinutes = 30, basePrice = 100m, isActive = true,
                lastModifiedAt = freshDate
            });

        var vm = new ServicesViewModel(mock, 3);
        await vm.LoadAsync();

        // Act
        vm.SelectedItem = vm.Items[0];
        await Task.Delay(150);

        // Assert
        vm.EditorLastModifiedAt.Should().Be(freshDate);
    }
}
