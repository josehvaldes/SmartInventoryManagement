using FluentAssertions;
using NSubstitute;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Warehouses.DTO;
using SmartInventory.Application.Features.Warehouses.Queries.GetWarehouseById;
using SmartInventory.Domain.Entities;
using SmartInventory.UnitTests.Common;
using Xunit;

namespace SmartInventory.UnitTests.Warehouses
{
    public class GetWarehouseByIdQueryHandlerTests
    {
        private readonly IApplicationDbContext _db;
        private readonly GetWarehouseByIdQueryHandler _handler;
        private readonly ICacheService _cache;
        private readonly List<Warehouse> _warehouses;

        public GetWarehouseByIdQueryHandlerTests()
        {
            _warehouses = WarehouseLoader.LoadWarehouses();
            var mockWarehousesSet = MockDbSetHelper.CreateMockDbSet(_warehouses);
            _db = Substitute.For<IApplicationDbContext>();
            _db.Warehouses.Returns(mockWarehousesSet);

            _cache = Substitute.For<ICacheService>();
            _cache.GetAsync<WarehouseDto>(Arg.Any<string>()).Returns((WarehouseDto?)null); // Simulate cache miss

            _handler = new GetWarehouseByIdQueryHandler(_db, _cache);
        }

        [Fact]
        public async Task Handle_Return_Warehouse_by_ID_success()
        {
            var warehouse = _warehouses.FirstOrDefault() ?? throw new InvalidOperationException("No warehouses available for testing.");
            // Arrange
            var query = new GetWarehouseByIdQuery(warehouse.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(warehouse.Id);
        }

        [Fact]
        public async Task Handle_Returns_Warehouse_From_Cache_When_Cache_Hit()
        {
            // Arrange
            var warehouse = _warehouses.First();
            var cachedDto = new WarehouseDto { Id = warehouse.Id, Name = warehouse.Name };
            _cache.GetAsync<WarehouseDto>($"Warehouse:{warehouse.Id}").Returns(cachedDto);
            var query = new GetWarehouseByIdQuery(warehouse.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeSameAs(cachedDto);
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Throws_EntityNotFoundException_When_Warehouse_Not_Found()
        {
            // Arrange
            var query = new GetWarehouseByIdQuery(Guid.NewGuid());

            // Act
            var act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage("*Warehouse*");
        }

        [Fact]
        public async Task Handle_Stores_Warehouse_In_Cache_After_DB_Hit()
        {
            // Arrange
            var warehouse = _warehouses.First();
            var query = new GetWarehouseByIdQuery(warehouse.Id);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            await _cache.Received(1).SetAsync(
                $"Warehouse:{warehouse.Id}",
                Arg.Any<WarehouseDto>(),
                TimeSpan.FromMinutes(5));
        }
    }
}
