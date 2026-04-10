using FluentAssertions;
using NSubstitute;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Warehouses.DTO;
using SmartInventory.Application.Features.Warehouses.Queries.GetAllWarehouses;
using SmartInventory.Domain.Entities;
using SmartInventory.UnitTests.Common;
using Xunit;

namespace SmartInventory.UnitTests.Warehouses
{
    public class GetAllWarehousesQueryHandlerTests
    {
        private readonly IApplicationDbContext _db;
        private readonly GetAllWarehousesQueryHandler _handler;
        private readonly ICacheService _cache;
        private readonly List<Warehouse> _warehouses;

        public GetAllWarehousesQueryHandlerTests()
        {
            _warehouses = WarehouseLoader.LoadWarehouses();
            var mockWarehousesSet = MockDbSetHelper.CreateMockDbSet(_warehouses);
            _db = Substitute.For<IApplicationDbContext>();
            _db.Warehouses.Returns(mockWarehousesSet);

            _cache = Substitute.For<ICacheService>();
            _cache.GetAsync<List<WarehouseDto>>(Arg.Any<string>()).Returns((List<WarehouseDto>?)null); // Simulate cache miss

            _handler = new GetAllWarehousesQueryHandler(_db, _cache);
        }

        [Fact]
        public async Task GetAllWarehousesQueryHandler_Returns_List_of_Warehouses()
        {
            // Arrange
            var query = new GetAllWarehousesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(_warehouses.Count);
        }

        [Fact]
        public async Task Handle_Returns_Empty_List_When_No_Warehouses_Exist()
        {
            // Arrange
            var emptySet = MockDbSetHelper.CreateMockDbSet(new List<Warehouse>());
            _db.Warehouses.Returns(emptySet);
            var query = new GetAllWarehousesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Returns_Warehouses_From_Cache_When_Cache_Hit()
        {
            // Arrange
            var key = $"{nameof(GetAllWarehousesQueryHandler)}:GetAllWarehouses";
            var cachedDtos = _warehouses.Select(w => new WarehouseDto { Id = w.Id, Name = w.Name }).ToList();
            _cache.GetAsync<List<WarehouseDto>>(key).Returns(cachedDtos);
            var query = new GetAllWarehousesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeSameAs(cachedDtos);
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Stores_Warehouses_In_Cache_After_DB_Hit()
        {
            // Arrange
            var key = $"{nameof(GetAllWarehousesQueryHandler)}:GetAllWarehouses";
            var query = new GetAllWarehousesQuery();

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            await _cache.Received(1).SetAsync(key, Arg.Any<List<WarehouseDto>>());
        }
    }
}
