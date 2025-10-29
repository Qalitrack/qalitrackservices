import { useState, useMemo } from "react";
import { useDispatch, useSelector } from "react-redux";
import { completeWeighing, deactivateTransaction } from "../store/weighingSlice";
import { FileDown, FileSpreadsheet, Ban, Ticket } from "lucide-react";
import * as XLSX from "xlsx";
import jsPDF from "jspdf";
import "jspdf-autotable";
import toast from "react-hot-toast";

export default function WeighingTable({ className }) {
  const dispatch = useDispatch();
  const { transactions } = useSelector((state) => state.weighing);

  const [statusFilter, setStatusFilter] = useState("all");
  const [searchQuery, setSearchQuery] = useState("");
  const [w2Inputs, setW2Inputs] = useState({});

  const filteredTransactions = useMemo(() => {
    return transactions.filter((tx) => {
      const matchesStatus =
        statusFilter === "all" ||
        (statusFilter === "inqueue" && tx.w1 && !tx.w2 && !tx.deactivated) ||
        (statusFilter === "completed" && tx.w2) ||
        (statusFilter === "deactivated" && tx.deactivated);

      const matchesSearch =
        !searchQuery ||
        tx.vehicleId?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        tx.driverId?.toLowerCase().includes(searchQuery.toLowerCase());

      return matchesStatus && matchesSearch;
    });
  }, [transactions, statusFilter, searchQuery]);

  const handleCompleteWeighing = (tx) => {
    const w2 = parseFloat(w2Inputs[tx.id]);
    if (!w2) {
      toast.error("Please enter Weight 2");
      return;
    }

    if (tx.operation === "Inbound Product Receipt" && w2 >= tx.w1) {
      toast.error("Inbound: Weight 2 must be less than Weight 1");
      return;
    }
    if (tx.operation === "Outbound Product Dispatch" && w2 <= tx.w1) {
      toast.error("Outbound: Weight 2 must be greater than Weight 1");
      return;
    }

    dispatch(completeWeighing({ id: tx.id, w2 }));
    setW2Inputs((prev) => ({ ...prev, [tx.id]: "" }));
    toast.success("Weighing completed");
    generateTicket({ ...tx, w2 });
  };

  const exportToExcel = () => {
    const ws = XLSX.utils.json_to_sheet(
      filteredTransactions.map((tx) => ({
        VehicleID: tx.vehicleId,
        DriverID: tx.driverId,
        ProductID: tx.productId,
        Operation: tx.operation,
        Weight1: tx.w1,
        Weight2: tx.w2 ?? "",
        NetWeight: tx.w1 && tx.w2 ? (tx.operation === "Inbound Product Receipt" ? tx.w1 - tx.w2 : tx.w2 - tx.w1) : "",
        TTAT: tx.ttat ? formatSeconds(tx.ttat) : "",
        Status: tx.deactivated
          ? "Deactivated"
          : tx.w2
          ? "Completed"
          : "In Queue",
        Date: new Date(tx.date).toLocaleString(),
      }))
    );
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Weighing Data");
    XLSX.writeFile(wb, "weighing_data.xlsx");
  };

  const exportToPDF = () => {
    const doc = new jsPDF();
    doc.text("Weighing Transactions Report", 14, 10);
    doc.autoTable({
      head: [["Vehicle ID", "Driver ID", "Product ID", "Operation", "W1", "W2", "Net", "TTAT", "Status", "Date"]],
      body: filteredTransactions.map((tx) => [
        tx.vehicleId,
        tx.driverId,
        tx.productId,
        tx.operation,
        tx.w1,
        tx.w2 ?? "-",
        tx.w1 && tx.w2 ? (tx.operation === "Inbound Product Receipt" ? tx.w1 - tx.w2 : tx.w2 - tx.w1) : "-",
        tx.ttat ? formatSeconds(tx.ttat) : "-",
        tx.deactivated
          ? "Deactivated"
          : tx.w2
          ? "Completed"
          : "In Queue",
        new Date(tx.date).toLocaleString(),
      ]),
      startY: 20,
    });
    doc.save("weighing_report.pdf");
  };

  const generateTicket = (tx) => {
    const doc = new jsPDF();

    doc.setFontSize(18);
    doc.setFont("helvetica", "bold");
    doc.text("Qalibrated Systems Limited", 105, 15, { align: "center" });

    doc.setFontSize(12);
    doc.setFont("helvetica", "normal");
    doc.text("Weighbridge Ticket", 105, 25, { align: "center" });

    doc.line(14, 30, 196, 30);

    doc.setFontSize(10);
    doc.text(`Ticket No: ${tx.id}`, 14, 40);
    doc.text(`Date: ${new Date(tx.date).toLocaleString()}`, 150, 40);

    doc.autoTable({
      startY: 50,
      head: [["Field", "Value"]],
      body: [
        ["Vehicle ID", tx.vehicleId],
        ["Driver ID", tx.driverId],
        ["Product ID", tx.productId],
        ["Operation", tx.operation],
        ["Weight 1 (T)", tx.w1],
        ["Weight 2 (T)", tx.w2 ?? "-"],
        ["Net Weight (T)", tx.w1 && tx.w2 ? (tx.operation === "Inbound Product Receipt" ? tx.w1 - tx.w2 : tx.w2 - tx.w1) : "-"],
        ["TTAT", tx.ttat ? formatSeconds(tx.ttat) : "-"],
        [
          "Status",
          tx.deactivated ? "Deactivated" : tx.w2 ? "Completed" : "In Queue",
        ],
      ],
      headStyles: { fillColor: [255, 193, 7], textColor: 0 },
    });

    const finalY = doc.lastAutoTable.finalY + 15;
    doc.setFontSize(11);
    doc.text("Weighed & Verified By: ____________________", 14, finalY);

    doc.setFontSize(10);
    doc.text("Thank you for using Qalibrated Systems Weighbridge", 105, finalY + 20, { align: "center" });

    doc.save(`ticket_${tx.vehicleId}_${tx.id}.pdf`);
  };

  return (
    <div className={`bg-white shadow rounded-lg p-4 border ${className}`}>
      {/* Filters & Export */}
      <div className="flex flex-wrap gap-3 mb-4 items-center">
        <input
          type="text"
          placeholder="Search by Vehicle or Driver ID"
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="border rounded px-3 py-2"
        />
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="border rounded px-3 py-2"
        >
          <option value="all">All</option>
          <option value="inqueue">In Queue</option>
          <option value="completed">Completed</option>
          <option value="deactivated">Deactivated</option>
        </select>
        <button
          onClick={exportToExcel}
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 flex items-center gap-2"
        >
          <FileSpreadsheet size={16} /> Excel
        </button>
        <button
          onClick={exportToPDF}
          className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600 flex items-center gap-2"
        >
          <FileDown size={16} /> PDF
        </button>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border">
          <thead>
            <tr className="bg-gray-100 text-left">
              <th className="px-4 py-2 border">Vehicle ID</th>
              <th className="px-4 py-2 border">Driver ID</th>
              <th className="px-4 py-2 border">Product ID</th>
              <th className="px-4 py-2 border">Operation</th>
              <th className="px-4 py-2 border">Weight 1</th>
              <th className="px-4 py-2 border">Weight 2</th>
              <th className="px-4 py-2 border">Net</th>
              <th className="px-4 py-2 border">TTAT</th>
              <th className="px-4 py-2 border">Status</th>
              <th className="px-4 py-2 border">Date</th>
              <th className="px-4 py-2 border">Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredTransactions.map((tx) => (
              <tr key={tx.id} className="hover:bg-gray-50">
                <td className="px-4 py-2 border">{tx.vehicleId}</td>
                <td className="px-4 py-2 border">{tx.driverId}</td>
                <td className="px-4 py-2 border">{tx.productId}</td>
                <td className="px-4 py-2 border">{tx.operation}</td>
                <td className="px-4 py-2 border">{tx.w1 ?? "-"}</td>
                <td className="px-4 py-2 border">
                  {tx.w2 ?? (
                    <input
                      type="number"
                      placeholder="Enter W2"
                      value={w2Inputs[tx.id] || ""}
                      onChange={(e) =>
                        setW2Inputs((prev) => ({
                          ...prev,
                          [tx.id]: e.target.value,
                        }))
                      }
                      className="border rounded px-2 py-1 w-24"
                    />
                  )}
                </td>
                <td className="px-4 py-2 border">
                  {tx.w1 && tx.w2 ? (tx.operation === "Inbound Product Receipt" ? tx.w1 - tx.w2 : tx.w2 - tx.w1) : "-"}
                </td>
                <td className="px-4 py-2 border">
                  {tx.ttat ? formatSeconds(tx.ttat) : "-"}
                </td>
                <td className="px-4 py-2 border">
                  {tx.deactivated
                    ? "Deactivated"
                    : tx.w2
                    ? "Completed"
                    : "In Queue"}
                </td>
                <td className="px-4 py-2 border">
                  {new Date(tx.date).toLocaleString()}
                </td>
                <td className="px-4 py-2 border space-x-2">
                  {!tx.w2 && !tx.deactivated && (
                    <>
                      <button
                        className="px-3 py-1 bg-blue-500 text-white rounded hover:bg-blue-600"
                        onClick={() => handleCompleteWeighing(tx)}
                      >
                        Complete
                      </button>
                      <button
                        className="px-3 py-1 bg-yellow-500 text-white rounded hover:bg-yellow-600 flex items-center gap-1"
                        onClick={() => dispatch(deactivateTransaction(tx.id))}
                      >
                        <Ban size={14} /> Deactivate
                      </button>
                    </>
                  )}
                  {tx.w2 && (
                    <button
                      className="px-3 py-1 bg-green-600 text-white rounded hover:bg-green-700 flex items-center gap-1"
                      onClick={() => generateTicket(tx)}
                    >
                      <Ticket size={14} /> Ticket
                    </button>
                  )}
                </td>
              </tr>
            ))}
            {filteredTransactions.length === 0 && (
              <tr>
                <td colSpan="11" className="px-4 py-3 text-center text-gray-500">
                  No transactions found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function formatSeconds(sec) {
  const m = Math.floor(sec / 60).toString().padStart(2, "0");
  const s = (sec % 60).toString().padStart(2, "0");
  return `${m}:${s}`;
}