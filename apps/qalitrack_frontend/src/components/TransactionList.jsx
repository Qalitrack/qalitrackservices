import { Ban, Printer, Search } from "lucide-react";
import jsPDF from "jspdf";
import { useState, useMemo } from "react";
import toast from "react-hot-toast";

export default function TransactionList({ transactions, onDeactivate, onComplete }) {
  const [w2Inputs, setW2Inputs] = useState({});
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");

  // Filtering logic
  const filteredTransactions = useMemo(() => {
    return transactions.filter((tx) => {
      const matchesSearch =
        tx.plate?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        tx.orderId?.toLowerCase().includes(searchQuery.toLowerCase());

      const matchesStatus =
        statusFilter === "all" ||
        (statusFilter === "inqueue" && tx.w1 && !tx.w2 && !tx.deactivated) ||
        (statusFilter === "completed" && tx.w2) ||
        (statusFilter === "deactivated" && tx.deactivated);

      return matchesSearch && matchesStatus;
    });
  }, [transactions, searchQuery, statusFilter]);

  // Ticket printer
  const printTicket = (tx) => {
    const doc = new jsPDF();
    doc.text("Weighing Ticket", 20, 20);
    doc.text(`Plate: ${tx.plate}`, 20, 40);
    doc.text(`Driver: ${tx.driver}`, 20, 50);
    doc.text(`Order ID: ${tx.orderId}`, 20, 60);
    doc.text(`Type: ${tx.type}`, 20, 70);
    doc.text(`Weight 1: ${tx.w1} kg`, 20, 80);
    doc.text(`Weight 2: ${tx.w2 ?? "-"} kg`, 20, 90);
    doc.text(
      `Net: ${tx.w1 && tx.w2 ? tx.w1 - tx.w2 : "-"} kg`,
      20,
      100
    );
    doc.text(`Date: ${new Date(tx.date).toLocaleString()}`, 20, 110);
    doc.save(`ticket_${tx.plate}_${tx.id}.pdf`);
    toast.success("Ticket downloaded");
  };

  // Validation before completing
  const validateAndComplete = (tx) => {
    const w2 = parseFloat(w2Inputs[tx.id]);
    if (!w2) {
      toast.error("Please enter the second weight (W2).");
      return;
    }

    if (tx.type === "inbound" && w2 >= tx.w1) {
      toast.error("Inbound transaction rule failed! W2 must be less than W1.");
      return;
    }

    if (tx.type === "outbound" && w2 <= tx.w1) {
      toast.error("Outbound transaction rule failed! W2 must be greater than W1.");
      return;
    }

    // If valid
    onComplete(tx.id, w2);
    setW2Inputs((prev) => ({ ...prev, [tx.id]: "" }));
    toast.success("Transaction completed successfully!");
  };

  return (
    <div className="bg-white shadow rounded-lg p-4 border">
      <h2 className="text-lg font-semibold text-amber-600 mb-4">
        Transactions
      </h2>

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-2 mb-4">
        {/* Search */}
        <div className="flex items-center border rounded px-2 flex-1">
          <Search size={16} className="text-gray-400" />
          <input
            type="text"
            placeholder="Search by Plate or Order ID..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="flex-1 px-2 py-1 outline-none"
          />
        </div>

        {/* Status Filter */}
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
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="min-w-full border">
          <thead className="bg-gray-100 text-left">
            <tr>
              <th className="px-3 py-2 border">Plate</th>
              <th className="px-3 py-2 border">Type</th>
              <th className="px-3 py-2 border">W1</th>
              <th className="px-3 py-2 border">W2</th>
              <th className="px-3 py-2 border">Net</th>
              <th className="px-3 py-2 border">Status</th>
              <th className="px-3 py-2 border">Date</th>
              <th className="px-3 py-2 border">Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredTransactions.map((tx) => (
              <tr
                key={tx.id}
                className={`hover:bg-gray-50 ${
                  // highlight row if rule would fail
                  (tx.type === "inbound" && w2Inputs[tx.id] >= tx.w1) ||
                  (tx.type === "outbound" && w2Inputs[tx.id] <= tx.w1)
                    ? "bg-red-100"
                    : ""
                }`}
              >
                <td className="px-3 py-2 border">{tx.plate}</td>
                <td className="px-3 py-2 border">{tx.type}</td>
                <td className="px-3 py-2 border">{tx.w1}</td>
                <td className="px-3 py-2 border">
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
                <td className="px-3 py-2 border">
                  {tx.w1 && tx.w2 ? tx.w1 - tx.w2 : "-"}
                </td>
                <td className="px-3 py-2 border">
                  {tx.deactivated
                    ? "Deactivated"
                    : tx.w2
                    ? "Completed"
                    : "In Queue"}
                </td>
                <td className="px-3 py-2 border">
                  {new Date(tx.date).toLocaleString()}
                </td>
                <td className="px-3 py-2 border space-x-2">
                  {!tx.w2 && !tx.deactivated && (
                    <>
                      <button
                        className="px-3 py-1 bg-blue-500 text-white rounded hover:bg-blue-600"
                        onClick={() => validateAndComplete(tx)}
                      >
                        Complete
                      </button>
                      <button
                        className="px-3 py-1 bg-yellow-500 text-white rounded hover:bg-yellow-600 flex items-center gap-1"
                        onClick={() => onDeactivate(tx.id)}
                      >
                        <Ban size={14} /> Deactivate
                      </button>
                    </>
                  )}
                  {tx.w2 && (
                    <button
                      className="px-3 py-1 bg-green-600 text-white rounded hover:bg-green-700 flex items-center gap-1"
                      onClick={() => printTicket(tx)}
                    >
                      <Printer size={14} /> Ticket
                    </button>
                  )}
                </td>
              </tr>
            ))}
            {filteredTransactions.length === 0 && (
              <tr>
                <td
                  colSpan="8"
                  className="px-3 py-3 text-center text-gray-500"
                >
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
