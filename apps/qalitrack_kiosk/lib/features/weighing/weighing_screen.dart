import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../shared/models/biometric_models.dart';
import '../../shared/models/transaction_models.dart';
import '../../shared/services/transaction_service.dart';
import '../../shared/services/weight_service.dart';
import 'package:flutter_gen/gen_l10n/app_localizations.dart';

class WeighingScreen extends StatefulWidget {
  final DriverDto driver;

  const WeighingScreen({Key? key, required this.driver}) : super(key: key);

  @override
  State<WeighingScreen> createState() => _WeighingScreenState();
}

class _WeighingScreenState extends State<WeighingScreen> {
  final TransactionService _transactionService = TransactionService();
  final WeightService _weightService = WeightService();
  
  TransactionDto? _currentTransaction;
  WeightReadingDto? _currentWeight;
  bool _isWeighing = false;
  bool _isProcessingTransaction = false;
  WeighingStep _currentStep = WeighingStep.welcome;
  String _vehicleNumber = '';

  @override
  void initState() {
    super.initState();
    _startWeightMonitoring();
  }

  void _startWeightMonitoring() {
    _weightService.startWeightStream().listen((weight) {
      if (mounted) {
        setState(() {
          _currentWeight = weight;
        });
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    
    return Scaffold(
      appBar: AppBar(
        title: Text(l10n?.weighingInProgress ?? 'Weighing in Progress'),
        actions: [
          IconButton(
            onPressed: _handleCancel,
            icon: const Icon(Icons.close),
          ),
        ],
      ),
      body: Padding(
        padding: const EdgeInsets.all(24.0),
        child: Column(
          children: [
            _buildDriverInfo(),
            const SizedBox(height: 32),
            _buildWeighingFlow(),
            const Spacer(),
            _buildCurrentWeight(),
            const SizedBox(height: 32),
            _buildActionButtons(),
          ],
        ),
      ),
    );
  }

  Widget _buildDriverInfo() {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Row(
          children: [
            CircleAvatar(
              radius: 30,
              backgroundImage: widget.driver.profilePhotoUrl.isNotEmpty
                  ? NetworkImage(widget.driver.profilePhotoUrl)
                  : null,
              child: widget.driver.profilePhotoUrl.isEmpty
                  ? Text(widget.driver.firstName[0])
                  : null,
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    widget.driver.fullName,
                    style: const TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  Text(
                    'ID: ${widget.driver.employeeId}',
                    style: const TextStyle(
                      fontSize: 16,
                      color: Colors.grey,
                    ),
                  ),
                  Text(
                    'Phone: ${widget.driver.phoneNumber}',
                    style: const TextStyle(fontSize: 14),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildWeighingFlow() {
    switch (_currentStep) {
      case WeighingStep.welcome:
        return _buildWelcomeStep();
      case WeighingStep.vehicleEntry:
        return _buildVehicleEntryStep();
      case WeighingStep.tareWeighing:
        return _buildTareWeighingStep();
      case WeighingStep.loading:
        return _buildLoadingStep();
      case WeighingStep.grossWeighing:
        return _buildGrossWeighingStep();
      case WeighingStep.completed:
        return _buildCompletedStep();
    }
  }

  Widget _buildWelcomeStep() {
    final l10n = AppLocalizations.of(context);
    
    return Column(
      children: [
        const Icon(
          Icons.scale,
          size: 80,
          color: Colors.blue,
        ),
        const SizedBox(height: 16),
        Text(
          l10n?.welcome ?? 'Welcome',
          style: const TextStyle(
            fontSize: 24,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 8),
        Text(
          'Ready to start weighing process',
          style: const TextStyle(fontSize: 16),
          textAlign: TextAlign.center,
        ),
      ],
    );
  }

  Widget _buildVehicleEntryStep() {
    return Column(
      children: [
        const Icon(
          Icons.local_shipping,
          size: 80,
          color: Colors.orange,
        ),
        const SizedBox(height: 16),
        const Text(
          'Enter Vehicle Information',
          style: TextStyle(
            fontSize: 24,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 16),
        TextField(
          decoration: const InputDecoration(
            labelText: 'Vehicle Number',
            border: OutlineInputBorder(),
            prefixIcon: Icon(Icons.directions_car),
          ),
          onChanged: (value) {
            setState(() {
              _vehicleNumber = value;
            });
          },
          textAlign: TextAlign.center,
          style: const TextStyle(fontSize: 20),
        ),
      ],
    );
  }

  Widget _buildTareWeighingStep() {
    final l10n = AppLocalizations.of(context);
    
    return Column(
      children: [
        const Icon(
          Icons.scale,
          size: 80,
          color: Colors.green,
        ),
        const SizedBox(height: 16),
        Text(
          'Tare Weighing',
          style: const TextStyle(
            fontSize: 24,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 8),
        Text(
          'Drive onto the scale for empty vehicle weight',
          style: const TextStyle(fontSize: 16),
          textAlign: TextAlign.center,
        ),
        if (_currentWeight?.isStable == true) ...[
          const SizedBox(height: 16),
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: Colors.green.shade100,
              borderRadius: BorderRadius.circular(8),
            ),
            child: Text(
              l10n?.stableWeight ?? 'Stable Weight',
              style: const TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
                color: Colors.green,
              ),
            ),
          ),
        ],
      ],
    );
  }

  Widget _buildLoadingStep() {
    return Column(
      children: [
        const Icon(
          Icons.hourglass_bottom,
          size: 80,
          color: Colors.blue,
        ),
        const SizedBox(height: 16),
        const Text(
          'Loading Process',
          style: TextStyle(
            fontSize: 24,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 8),
        const Text(
          'Please proceed to load your vehicle',
          style: TextStyle(fontSize: 16),
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 16),
        const CircularProgressIndicator(),
      ],
    );
  }

  Widget _buildGrossWeighingStep() {
    final l10n = AppLocalizations.of(context);
    
    return Column(
      children: [
        const Icon(
          Icons.scale,
          size: 80,
          color: Colors.purple,
        ),
        const SizedBox(height: 16),
        Text(
          'Gross Weighing',
          style: const TextStyle(
            fontSize: 24,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 8),
        const Text(
          'Drive onto the scale for loaded vehicle weight',
          style: TextStyle(fontSize: 16),
          textAlign: TextAlign.center,
        ),
        if (_currentWeight?.isStable == true) ...[
          const SizedBox(height: 16),
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: Colors.green.shade100,
              borderRadius: BorderRadius.circular(8),
            ),
            child: Text(
              l10n?.stableWeight ?? 'Stable Weight',
              style: const TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
                color: Colors.green,
              ),
            ),
          ),
        ],
      ],
    );
  }

  Widget _buildCompletedStep() {
    final l10n = AppLocalizations.of(context);
    
    return Column(
      children: [
        const Icon(
          Icons.check_circle,
          size: 80,
          color: Colors.green,
        ),
        const SizedBox(height: 16),
        Text(
          l10n?.weighingComplete ?? 'Weighing Complete',
          style: const TextStyle(
            fontSize: 24,
            fontWeight: FontWeight.bold,
            color: Colors.green,
          ),
        ),
        if (_currentTransaction != null) ...[
          const SizedBox(height: 16),
          _buildTransactionSummary(),
        ],
      ],
    );
  }

  Widget _buildTransactionSummary() {
    final l10n = AppLocalizations.of(context);
    final transaction = _currentTransaction!;
    
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          children: [
            Text(
              'Transaction Summary',
              style: const TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 16),
            _buildSummaryRow(
              l10n?.vehicleNumber ?? 'Vehicle Number',
              _vehicleNumber,
            ),
            _buildSummaryRow(
              l10n?.driverName ?? 'Driver Name',
              widget.driver.fullName,
            ),
            _buildSummaryRow(
              l10n?.tareWeight ?? 'Tare Weight',
              '${transaction.tareWeight?.toStringAsFixed(1) ?? '0.0'} ${l10n?.kg ?? 'kg'}',
            ),
            _buildSummaryRow(
              l10n?.grossWeight ?? 'Gross Weight',
              '${transaction.grossWeight?.toStringAsFixed(1) ?? '0.0'} ${l10n?.kg ?? 'kg'}',
            ),
            _buildSummaryRow(
              l10n?.netWeight ?? 'Net Weight',
              '${transaction.netWeight?.toStringAsFixed(1) ?? '0.0'} ${l10n?.kg ?? 'kg'}',
            ),
            _buildSummaryRow(
              l10n?.timestamp ?? 'Timestamp',
              _formatDateTime(transaction.timestamp),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSummaryRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: const TextStyle(fontWeight: FontWeight.w500)),
          Text(value),
        ],
      ),
    );
  }

  Widget _buildCurrentWeight() {
    final l10n = AppLocalizations.of(context);
    
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          children: [
            Text(
              l10n?.currentWeight ?? 'Current Weight',
              style: const TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              '${_currentWeight?.weight.toStringAsFixed(1) ?? '0.0'} ${l10n?.kg ?? 'kg'}',
              style: TextStyle(
                fontSize: 36,
                fontWeight: FontWeight.bold,
                color: _currentWeight?.isStable == true ? Colors.green : Colors.orange,
              ),
            ),
            const SizedBox(height: 8),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
              decoration: BoxDecoration(
                color: _currentWeight?.isStable == true 
                    ? Colors.green.shade100 
                    : Colors.orange.shade100,
                borderRadius: BorderRadius.circular(16),
              ),
              child: Text(
                _currentWeight?.isStable == true 
                    ? (l10n?.stableWeight ?? 'Stable')
                    : 'Stabilizing...',
                style: TextStyle(
                  color: _currentWeight?.isStable == true ? Colors.green : Colors.orange,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildActionButtons() {
    final l10n = AppLocalizations.of(context);
    
    switch (_currentStep) {
      case WeighingStep.welcome:
        return ElevatedButton(
          onPressed: _startWeighingProcess,
          style: ElevatedButton.styleFrom(
            minimumSize: const Size(double.infinity, 60),
            textStyle: const TextStyle(fontSize: 18),
          ),
          child: Text(l10n?.startWeighing ?? 'Start Weighing Process'),
        );
        
      case WeighingStep.vehicleEntry:
        return Row(
          children: [
            Expanded(
              child: OutlinedButton(
                onPressed: _handleCancel,
                style: OutlinedButton.styleFrom(
                  minimumSize: const Size(0, 60),
                ),
                child: Text(l10n?.cancel ?? 'Cancel'),
              ),
            ),
            const SizedBox(width: 16),
            Expanded(
              flex: 2,
              child: ElevatedButton(
                onPressed: _vehicleNumber.isNotEmpty ? _proceedToTareWeighing : null,
                style: ElevatedButton.styleFrom(
                  minimumSize: const Size(0, 60),
                ),
                child: Text(l10n?.continueButton ?? 'Continue'),
              ),
            ),
          ],
        );
        
      case WeighingStep.tareWeighing:
        return ElevatedButton(
          onPressed: _currentWeight?.isStable == true ? _captureWeight : null,
          style: ElevatedButton.styleFrom(
            minimumSize: const Size(double.infinity, 60),
          ),
          child: const Text('Capture Tare Weight'),
        );
        
      case WeighingStep.loading:
        return ElevatedButton(
          onPressed: _proceedToGrossWeighing,
          style: ElevatedButton.styleFrom(
            minimumSize: const Size(double.infinity, 60),
          ),
          child: const Text('Proceed to Gross Weighing'),
        );
        
      case WeighingStep.grossWeighing:
        return ElevatedButton(
          onPressed: _currentWeight?.isStable == true ? _captureWeight : null,
          style: ElevatedButton.styleFrom(
            minimumSize: const Size(double.infinity, 60),
          ),
          child: const Text('Capture Gross Weight'),
        );
        
      case WeighingStep.completed:
        return Row(
          children: [
            Expanded(
              child: OutlinedButton(
                onPressed: _printReceipt,
                style: OutlinedButton.styleFrom(
                  minimumSize: const Size(0, 60),
                ),
                child: Text(l10n?.printReceipt ?? 'Print Receipt'),
              ),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: ElevatedButton(
                onPressed: _startNewTransaction,
                style: ElevatedButton.styleFrom(
                  minimumSize: const Size(0, 60),
                ),
                child: Text(l10n?.newTransaction ?? 'New Transaction'),
              ),
            ),
          ],
        );
    }
  }

  Future<void> _startWeighingProcess() async {
    setState(() {
      _currentStep = WeighingStep.vehicleEntry;
    });
  }

  Future<void> _proceedToTareWeighing() async {
    setState(() {
      _currentStep = WeighingStep.tareWeighing;
      _isProcessingTransaction = true;
    });

    try {
      final request = CreateTransactionRequest(
        driverId: widget.driver.id,
        vehicleId: _vehicleNumber,
        transactionType: 'weighing',
      );

      _currentTransaction = await _transactionService.createTransaction(request);
    } catch (e) {
      // Handle error
    } finally {
      setState(() {
        _isProcessingTransaction = false;
      });
    }
  }

  Future<void> _captureWeight() async {
    if (_currentWeight == null || _currentTransaction == null) return;

    setState(() {
      _isProcessingTransaction = true;
    });

    try {
      final weightType = _currentStep == WeighingStep.tareWeighing 
          ? WeightType.tare 
          : WeightType.gross;

      final request = UpdateWeightRequest(
        transactionId: _currentTransaction!.id,
        weight: _currentWeight!.weight,
        weightType: weightType.toString().split('.').last,
        timestamp: DateTime.now(),
      );

      await _transactionService.updateWeight(request);

      if (_currentStep == WeighingStep.tareWeighing) {
        setState(() {
          _currentStep = WeighingStep.loading;
        });
      } else {
        // Complete the transaction
        await _completeTransaction();
      }
    } catch (e) {
      // Handle error
    } finally {
      setState(() {
        _isProcessingTransaction = false;
      });
    }
  }

  Future<void> _proceedToGrossWeighing() async {
    setState(() {
      _currentStep = WeighingStep.grossWeighing;
    });
  }

  Future<void> _completeTransaction() async {
    // Finalize the transaction
    _currentTransaction = await _transactionService.completeTransaction(
      _currentTransaction!.id,
    );
    
    setState(() {
      _currentStep = WeighingStep.completed;
    });
  }

  void _printReceipt() {
    // Implement receipt printing
  }

  void _startNewTransaction() {
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(
        builder: (context) => WeighingScreen(driver: widget.driver),
      ),
    );
  }

  void _handleCancel() {
    Navigator.of(context).pop();
  }

  String _formatDateTime(DateTime dateTime) {
    return '${dateTime.day}/${dateTime.month}/${dateTime.year} '
           '${dateTime.hour.toString().padLeft(2, '0')}:'
           '${dateTime.minute.toString().padLeft(2, '0')}';
  }

  @override
  void dispose() {
    _weightService.stopWeightStream();
    super.dispose();
  }
}

enum WeighingStep {
  welcome,
  vehicleEntry,
  tareWeighing,
  loading,
  grossWeighing,
  completed,
}