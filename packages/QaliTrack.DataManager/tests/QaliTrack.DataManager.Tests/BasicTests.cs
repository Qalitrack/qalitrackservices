namespace QaliTrack.DataManager.Tests;

public class BasicTests
{
    [Fact]
    public void WeightMeasurement_Should_Have_Valid_Id()
    {
        // Arrange & Act
        var measurement = new WeightMeasurement();
        
        // Assert
        measurement.Id.Should().NotBe(Guid.Empty);
        measurement.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        measurement.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void WeighingTransaction_Should_Have_Valid_Properties()
    {
        // Arrange & Act
        var transaction = new WeighingTransaction
        {
            TransactionNumber = "TXN-TEST-001",
            TransactionType = "Purchase",
            Status = "Initiated"
        };
        
        // Assert
        transaction.Id.Should().NotBe(Guid.Empty);
        transaction.TransactionNumber.Should().Be("TXN-TEST-001");
        transaction.TransactionType.Should().Be("Purchase");
        transaction.Status.Should().Be("Initiated");
        transaction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TenantEntity_Should_Have_OrganizationId()
    {
        // Arrange & Act
        var measurement = new WeightMeasurement
        {
            OrganizationId = Guid.NewGuid()
        };
        
        // Assert
        measurement.OrganizationId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task DbContext_Should_Create_Database()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataManagerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Act & Assert
        using var context = new DataManagerDbContext(options);
        var created = await context.Database.EnsureCreatedAsync();
        
        created.Should().BeTrue();
        context.WeightMeasurements.Should().NotBeNull();
        context.WeighingTransactions.Should().NotBeNull();
    }

    [Fact]
    public async Task Repository_Should_Add_And_Retrieve_Entity()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataManagerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataManagerDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var orgId = Guid.NewGuid();
        var measurement = new WeightMeasurement
        {
            Weight = 1000.5m,
            MeasurementType = "Entry",
            WeighbridgeId = Guid.NewGuid(),
            TransactionId = Guid.NewGuid(),
            OrganizationId = orgId,
            MeasurementTime = DateTime.UtcNow
        };

        // Act
        context.WeightMeasurements.Add(measurement);
        await context.SaveChangesAsync();

        var retrieved = await context.WeightMeasurements.FirstOrDefaultAsync(w => w.Id == measurement.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Weight.Should().Be(1000.5m);
        retrieved.MeasurementType.Should().Be("Entry");
        retrieved.OrganizationId.Should().Be(orgId);
    }
}