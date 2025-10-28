# Weighbridge Multi-Vehicle Workflow

## Overview

The system **fully supports** the real-world scenario where multiple vehicles are weighed with time gaps between their first and second weighings. Trucks don't need to complete both weighings immediately - they can leave, other trucks can be weighed, and the original truck can return hours or even days later to complete its transaction.

## How It Works

### Transaction States

Each transaction has a clear state progression:

1. **Pending** - Transaction created, no weighings yet
2. **InProgress** - At least one weighing completed, waiting for more
3. **Completed** - All expected weighings done, transaction immutable

### Typical Workflow

#### Step 1: First Weighing
```
Truck A arrives (full load)
↓
Operator creates transaction or looks up existing one
↓
First weighing: 45,000 kg
↓
Status: InProgress
↓
Truck leaves to offload
```

#### Step 2: Other Vehicles
```
While Truck A is away:
- Truck B: First weighing → Second weighing → Completed
- Truck C: First weighing → InProgress (leaves)
- Truck D: First weighing → Second weighing → Completed
```

#### Step 3: Truck Returns
```
Truck A returns (empty)
↓
Operator finds incomplete transaction by:
  - Receipt Number: "WB-2025-001"
  - Vehicle Plate: "ABC-123"
↓
Second weighing: 15,000 kg
↓
Status: Completed
Net Weight: 30,000 kg (45,000 - 15,000)
```

## API Endpoints

### 1. Create Transaction
```http
POST /api/transactions
```
```json
{
  "receiptNo": "WB-2025-001",
  "noPlate": "ABC-123",
  "driverName": "John Doe",
  "transporterId": 1,
  "transporterName": "Fast Transport Ltd",
  "commodityId": 100,
  "commodityName": "Wheat",
  "expectedWeighings": 2
}
```

### 2. Find Incomplete Transaction by Receipt
```http
GET /api/transactions/receipt/{receiptNo}
```
Example: `GET /api/transactions/receipt/WB-2025-001`

Returns the transaction even if it's not completed yet.

### 3. Find Incomplete Transactions by Vehicle
```http
GET /api/transactions/incomplete/vehicle/{noPlate}
```
Example: `GET /api/transactions/incomplete/vehicle/ABC-123`

Returns all incomplete transactions for this vehicle.

### 4. Add Weighing
```http
POST /api/transactions/weighings
```
```json
{
  "transactionId": "guid-string",
  "weight": 15000,
  "weighBridgeId": 1,
  "weighBridgeName": "Main Weighbridge",
  "scaleName": "Scale-01",
  "operatorId": 101,
  "operatorName": "Operator Alice",
  "notes": "Second weighing - empty truck"
}
```

The system automatically:
- Tracks which weighing this is (1st, 2nd, etc.)
- Updates transaction status
- Calculates net weight when all weighings are complete
- Creates audit logs for each weighing

## Service Methods

### Find Incomplete Transactions

```csharp
// By vehicle plate
var incomplete = await transactionService
    .GetIncompleteTransactionsByVehicleAsync("ABC-123");

// By receipt number
var transaction = await transactionService
    .GetByReceiptNoAsync("WB-2025-001");

// Check if still in progress
if (!transaction.IsCompleted)
{
    // Can add more weighings
}
```

### Add Weighing

```csharp
var dto = new AddWeighingDto
{
    TransactionId = existingTransactionId,
    Weight = 15000,
    WeighBridgeId = 1,
    WeighBridgeName = "Main Weighbridge",
    ScaleName = "Scale-01",
    OperatorId = 101,
    OperatorName = "Operator Alice"
};

var result = await transactionService.AddWeighingAsync(dto);

// Transaction auto-completes when all weighings are done
if (result.IsCompleted)
{
    Console.WriteLine($"Net Weight: {result.NetWeight}kg");
}
```

## Real-World Example

See the integration test in:
```
tests/Transaction.Tests/Integration/WeighbridgeWorkflowTests.cs
```

This test demonstrates:
1. Truck A: First weighing → leaves
2. Truck B: Complete transaction (both weighings)
3. Truck C: First weighing → leaves
4. Truck A: Returns for second weighing → completes
5. System correctly tracks all 3 transactions independently

## Test Results

Run the workflow test to see it in action:

```bash
dotnet test --filter "FullyQualifiedName~RealWorld_TruckReturnsLaterForSecondWeighing"
```

**Output:**
```
✓ Truck A (ABC-123) - First weighing: 45000kg
  Status: InProgress, Receipt: WB-2025-001
  Truck A leaves to offload...

✓ Truck B (XYZ-789) - Completed both weighings
  First: 38000kg, Second: 12000kg
  Net Weight: 26000kg

✓ Truck A (ABC-123) returns for second weighing
  Found incomplete transaction by receipt: WB-2025-001

✓ Truck A (ABC-123) - Transaction Completed!
  NET WEIGHT: 30000kg (Wheat)
```

## Key Features

### 1. Transaction Continuity
- Transactions remain active until completed
- No time limit between weighings
- Each transaction has a unique ID (GUID)

### 2. Multiple Search Methods
- By Receipt Number (exact match, case-insensitive)
- By Vehicle Plate (finds all incomplete for that vehicle)
- By Transaction ID (direct lookup)

### 3. Automatic State Management
- First weighing: Pending → InProgress
- Last weighing: InProgress → Completed
- Automatic net weight calculation
- Timestamp tracking for each weighing

### 4. Audit Trail
- Every weighing creates an audit log
- Tracks operator, timestamp, and details
- Complete history of transaction

### 5. Data Integrity
- Soft deletes (data never lost)
- Transaction immutability after completion
- Reweigh functionality for corrections

## Database Schema

### WeighbridgeTransaction
```
Id (GUID)
ReceiptNo (unique)
NoPlate
ExpectedWeighings (default: 2)
CompletedWeighings (counter)
FirstWeight, FirstWeightTimestamp
SecondWeight, SecondWeightTimestamp
NetWeight, NetWeightCalculatedTimestamp
Status (Pending, InProgress, Completed)
IsCompleted
```

### WeighingRecord
```
Id
WeighbridgeTransactionId (FK)
WeighingSequence (1, 2, etc.)
Weight
WeighingDate
WeighBridgeId
OperatorId
Notes
```

## Summary

✅ **Yes, the system fully supports your workflow!**

- Multiple vehicles can be weighed simultaneously
- Transactions can span hours or days
- Easy lookup by receipt or vehicle plate
- Automatic completion when all weighings are done
- Complete audit trail
- Data integrity maintained

The system is designed for exactly this real-world weighbridge scenario.
