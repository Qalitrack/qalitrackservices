// src/pages/Weighing.jsx
import { useState, useMemo } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { startWeighing, completeWeighing, deactivateTransaction } from '../store/weighingSlice';
import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import 'jspdf-autotable';
import CameraGrid from '../components/CameraGrid';

export default function Weighing() {
  const dispatch = useDispatch();
  const transactions = useSelector(state => state.weighing.transactions);

  const [transactionType, setTransactionType] = useState('inbound');
  const [driverName, setDriverName] = useState('');
  const [driverId, setDriverId] = useState('');
  const [batchNo, setBatchNo] = useState('');
  const [plate, setPlate] = useState('');
  const [orderId, setOrderId] = useState('');
  const [w1, setW1] = useState('');
  const [w2Inputs, setW2Inputs] = useState({});
  const [statusFilter, setStatusFilter] = useState('all');
  const [searchQuery, setSearchQuery] = useState('');

  // Filter transactions
  const filteredTransactions = useMemo(() => {
    return transactions.filter(tx => {
      const matchesStatus =
        statusFilter === 'all' ||
        (statusFilter === 'inqueue' && tx.w1 && !tx.w2 && !tx.deactivated) ||
        (statusFilter === 'completed' && tx.w2);

      const matchesSearch =
        !searchQuery ||
        tx.plate?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        tx.orderId?.toLowerCase().includes(searchQuery.toLowerCase());

      return matchesStatus && matchesSearch;
    });
  }, [transactions, statusFilter, searchQuery]);

  const handleStartWeighing = () => {
    if (!plate || !orderId || !w1 || !driverName || !driverId || !batchNo) {
      alert('Please fill in all details');
      return;
    }
    dispatch(startWeighing({
      plate,
      orderId,
      transactionType,
      driverName,
      driverId,
      batchNo,
      w1: parseFloat(w1)
    }));
    setPlate('');
    setOrderId('');
    setDriverName('');
    setDriverId('');
    setBatchNo('');
    setW1('');
  };

  const handleCompleteWeighing = (id) => {
    const w2 = parseFloat(w2Inputs[id]);
    if (!w2) {
      alert('Please enter Weight 2');
      return;
    }
    dispatch(completeWeighing({ id, w2 }));
    setW2Inputs(prev => ({ ...prev, [id]: '' }));
  };

  const handleDeactivate = (id) => {
    dispatch(deactivateTransaction(id));
  };

  const exportToExcel = () => {
    const ws = XLSX.utils.json_to_sheet(
      filteredTransactions.map(tx => ({
        Plate: tx.plate,
        OrderID: tx.orderId,
        Type: tx.transactionType,
        Driver: tx.driverName,
        Batch: tx.batchNo,
        Weight1: tx.w1,
        Weight2: tx.w2 ?? '',
        NetWeight: tx.w1 && tx.w2 ? tx.w1 - tx.w2 : '',
        Status: tx.w2 ? 'Completed' : tx.deactivated ? 'Deactivated' : 'In Queue',
        Date: new Date(tx.date).toLocaleString()
      }))
    );
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Weighing Data');
    XLSX.writeFile(wb, 'weighing_data.xlsx');
  };

  const downloadTicket = (tx) => {
    const doc = new jsPDF();
    doc.setFontSize(16);
    doc.text('Qalitrack Weighbridge Ticket', 14, 20);
    doc.setFontSize(10);
    doc.text(`Date: ${new Date(tx.date).toLocaleString()}`, 14, 28);
    doc.text(`Transaction Type: ${tx.transactionType.toUpperCase()}`, 14, 34);
    doc.text(`Order ID: ${tx.orderId}`, 14, 40);
    doc.text(`Plate: ${tx.plate}`, 14, 46);
    doc.text(`Driver: ${tx.driverName} (ID: ${tx.driverId})`, 14, 52);
    doc.text(`Batch No: ${tx.batchNo}`, 14, 58);
    doc.text(`Weight 1: ${tx.w1} kg`, 14, 64);
    doc.text(`Weight 2: ${tx.w2} kg`, 14, 70);
    doc.text(`Net Weight: ${tx.w1 - tx.w2} kg`, 14, 76);
    doc.text('Thank you for using Qalitrack', 14, 90);
    doc.save(`ticket_${tx.orderId}.pdf`);
  };

  return (
    <div className="p-4 space-y-6">
      <h2 className="text-xl font-semibold mb-4">Weighing Transactions</h2>

      {/* Camera Views */}
      <CameraGrid />

      {/* New Transaction Form */}
      <div className="border p-4 rounded bg-gray-50 space-y-3">
        <select value={transactionType} onChange={(e) => setTransactionType(e.target.value)} className="border rounded px-3 py-2">
          <option value="inbound">Inbound</option>
          <option value="outbound">Outbound</option>
        </select>
        <input type="text" placeholder="Driver Name" value={driverName} onChange={(e) => setDriverName(e.target.value)} className="border rounded px-3 py-2" />
        <input type="text" placeholder="Driver ID" value={driverId} onChange={(e) => setDriverId(e.target.value)} className="border rounded px-3 py-2" />
        <input type="text" placeholder="Batch No" value={batchNo} onChange={(e) => setBatchNo(e.target.value)} className="border rounded px-3 py-2" />
        <input type="text" placeholder="Plate" value={plate} onChange={(e) => setPlate(e.target.value)} className="border rounded px-3 py-2" />
        <input type="text" placeholder="Order ID" value={orderId} onChange={(e) => setOrderId(e.target.value)} className="border rounded px-3 py-2" />
        <input type="number" placeholder="Weight 1 (kg)" value={w1} onChange={(e) => setW1(e.target.value)} className="border rounded px-3 py-2" />
        <button onClick={handleStartWeighing} className="px-4 py-2 bg-green-600 text-white rounded">Start Weighing</button>
      </div>

      {/* Filters & Export */}
      <div className="flex gap-2">
        <input type="text" placeholder="Search" value={searchQuery} onChange={(e) => setSearchQuery(e.target.value)} className="border rounded px-3 py-2" />
        <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="border rounded px-3 py-2">
          <option value="all">All</option>
          <option value="inqueue">In Queue</option>
          <option value="completed">Completed</option>
        </select>
        <button onClick={exportToExcel} className="px-4 py-2 bg-blue-500 text-white rounded">Export Excel</button>
      </div>

      {/* Transactions Table */}
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border">
          <thead className="bg-gray-100">
            <tr>
              <th className="px-4 py-2 border">Type</th>
              <th className="px-4 py-2 border">Plate</th>
              <th className="px-4 py-2 border">Order ID</th>
              <th className="px-4 py-2 border">Driver</th>
              <th className="px-4 py-2 border">Batch</th>
              <th className="px-4 py-2 border">W1</th>
              <th className="px-4 py-2 border">W2</th>
              <th className="px-4 py-2 border">Net</th>
              <th className="px-4 py-2 border">Status</th>
              <th className="px-4 py-2 border">Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredTransactions.map(tx => (
              <tr key={tx.id}>
                <td className="border px-4 py-2">{tx.transactionType}</td>
                <td className="border px-4 py-2">{tx.plate}</td>
                <td className="border px-4 py-2">{tx.orderId}</td>
                <td className="border px-4 py-2">{tx.driverName}</td>
                <td className="border px-4 py-2">{tx.batchNo}</td>
                <td className="border px-4 py-2">{tx.w1}</td>
                <td className="border px-4 py-2">
                  {tx.w2 ?? (
                    <input type="number" value={w2Inputs[tx.id] || ''} onChange={(e) => setW2Inputs(prev => ({ ...prev, [tx.id]: e.target.value }))} className="border rounded px-2 py-1 w-20" />
                  )}
                </td>
                <td className="border px-4 py-2">{tx.w1 && tx.w2 ? tx.w1 - tx.w2 : '-'}</td>
                <td className="border px-4 py-2">{tx.w2 ? 'Completed' : tx.deactivated ? 'Deactivated' : 'In Queue'}</td>
                <td className="border px-4 py-2 flex gap-1">
                  {!tx.w2 && !tx.deactivated && (
                    <>
                      <button onClick={() => handleCompleteWeighing(tx.id)} className="px-3 py-1 bg-blue-500 text-white rounded">Complete</button>
                      <button onClick={() => handleDeactivate(tx.id)} className="px-3 py-1 bg-gray-500 text-white rounded">Deactivate</button>
                    </>
                  )}
                  {tx.w2 && <button onClick={() => downloadTicket(tx)} className="px-3 py-1 bg-yellow-500 text-white rounded">Ticket</button>}
                </td>
              </tr>
            ))}
            {filteredTransactions.length === 0 && (
              <tr>
                <td colSpan="10" className="text-center py-4 text-gray-500">No transactions found.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
