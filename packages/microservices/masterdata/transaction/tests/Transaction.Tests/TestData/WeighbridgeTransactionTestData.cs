/*
using Transaction.Core.Entities;

namespace Transaction.Tests.TestData;

public class WeighbridgeTransactionBuilder
{
    private readonly WeighbridgeTransaction _transaction = new();
    private int _ticketId = 1;
    private int _transporterId = 1;
    private int _vehicleId = 1;
    private int _operatorId = 1;
    private int _weighBridgeId = 1;
    private DateTime _now = DateTime.UtcNow;

    public WeighbridgeTransactionBuilder WithBasicInfo()
    {
        _transaction.TicketID = _ticketId++;
        _transaction.ReceiptNo = $"WB-{DateTime.Now:yyyyMMdd}-{_ticketId:D4}";
        _transaction.Status = "Active";
        return this;
    }

    public WeighbridgeTransactionBuilder WithWeighingInfo(bool includeSecondWeigh = true)
    {
        var random = new Random();
        _transaction.FirstWeight = (random.Next(1000, 5000) / 10.0m).ToString("F2");
        _transaction.FirstWeightDate = _now.AddMinutes(-30);
        
        if (includeSecondWeigh)
        {
            _transaction.SecondWeight = (random.Next(500, 2000) / 10.0m).ToString("F2");
            _transaction.SecondWeightDate = _now;
            _transaction.NetWeight = (decimal.Parse(_transaction.FirstWeight) - decimal.Parse(_transaction.SecondWeight)).ToString("F2");
        }
        
        return this;
    }

    public WeighbridgeTransactionBuilder WithVehicleInfo()
    {
        var vehicles = new[]
        {
            new { Plate = "KAA 100A", Type = "Truck", Axles = 3 },
            new { Plate = "KBB 200B", Type = "Trailer", Axles = 2 },
            new { Plate = "KCC 300C", Type = "Lorry", Axles = 2 }
        };
        
        var vehicle = vehicles[_vehicleId % vehicles.Length];
        _vehicleId++;
        
        _transaction.VehicleID = _vehicleId;
        _transaction.NoPlate = vehicle.Plate;
        _transaction.DriverName = $"Driver {_vehicleId}";
        
        return this;
    }

    public WeighbridgeTransactionBuilder WithCommodityInfo()
    {
        var commodities = new[]
        {
            new { Id = 1, Name = "Maize", Type = "Grain" },
            new { Id = 2, Name = "Wheat", Type = "Grain" },
            new { Id = 3, Name = "Coffee", Type = "Cash Crop" }
        };
        
        var commodity = commodities[_ticketId % commodities.Length];
        _transaction.CommodityID = commodity.Id;
        _transaction.CommodityName = commodity.Name;
        
        return this;
    }

    public WeighbridgeTransactionBuilder WithTransporterInfo()
    {
        var transporters = new[]
        {
            new { Id = 1, Name = "Transporter A", Code = "TRA" },
            new { Id = 2, Name = "Transporter B", Code = "TRB" },
            new { Id = 3, Name = "Transporter C", Code = "TRC" }
        };
        
        var transporter = transporters[_transporterId % transporters.Length];
        _transporterId++;
        
        _transaction.TransporterID = transporter.Id;
        _transaction.TransporterName = transporter.Name;
        
        return this;
    }

    public WeighbridgeTransactionBuilder WithWeighbridgeInfo()
    {
        var weighbridges = new[]
        {
            new { Id = 1, Name = "North Weighbridge", Location = "Nairobi" },
            new { Id = 2, Name = "South Weighbridge", Location = "Mombasa" },
            new { Id = 3, Name = "West Weighbridge", Location = "Kisumu" }
        };
        
        var weighbridge = weighbridges[_weighBridgeId % weighbridges.Length];
        _weighBridgeId++;
        
        _transaction.WeighBridgeID = weighbridge.Id;
        _transaction.WeighBridgeName = weighbridge.Name;
        _transaction.ScaleName = $"Scale {_weighBridgeId}";
        _transaction.OperatorID = _operatorId++;
        _transaction.OperatorName = $"Operator {_operatorId}";
        
        return this;
    }

    public WeighbridgeTransactionBuilder WithSecondWeighingInfo()
    {
        _transaction.WeighBridgeName2nd = _transaction.WeighBridgeName;
        _transaction.ScaleName2nd = $"{_transaction.ScaleName}-2nd";
        _transaction.OperatorID2nd = (_operatorId + 10).ToString();
        _transaction.OperatorName2nd = $"Operator {_operatorId + 10}";
        
        return this;
    }

    public WeighbridgeTransactionBuilder WithLocationInfo()
    {
        _transaction.OriginID = 1;
        _transaction.OriginName = "Nairobi Depot";
        _transaction.DestinationID = 2;
        _transaction.DestinationName = "Mombasa Port";
        
        return this;
    }

    public WeighbridgeTransactionBuilder WithCustomerSupplierInfo()
    {
        _transaction.SupplierID = 1;
        _transaction.SupplierName = "Farmers Co-op";
        _transaction.CustomerID = 2;
        _transaction.CustomerName = "Exporters Ltd";
        
        return this;
    }

    public WeighbridgeTransaction Build()
    {
        // Ensure required fields are set
        if (string.IsNullOrEmpty(_transaction.Status))
            _transaction.Status = "Active";
            
        if (_transaction.FirstWeightDate == default)
            _transaction.FirstWeightDate = _now.AddMinutes(-30);
            
        if (_transaction.SecondWeightDate == default && !string.IsNullOrEmpty(_transaction.SecondWeight))
            _transaction.SecondWeightDate = _now;
            
        return _transaction;
    }

    public static implicit operator WeighbridgeTransaction(WeighbridgeTransactionBuilder builder) => builder.Build();
}
*/
