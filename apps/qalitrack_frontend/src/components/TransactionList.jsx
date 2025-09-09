import { Ban, Printer, Search, Eye, X } from "lucide-react";
import jsPDF from "jspdf";
import { useState, useMemo } from "react";
import toast from "react-hot-toast";

export default function TransactionList({ transactions, onDeactivate, onComplete }) {
  const [w2Inputs, setW2Inputs] = useState({});
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [selectedTx, setSelectedTx] = useState(null); // for modal

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
    doc.text(`Net: ${tx.net ?? "-"} kg`, 20, 100);
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

    onComplete(tx.id, w2);
    setW2Inputs((prev) => ({ ...prev, [tx.id]: "" }));
    toast.success("Transaction completed successfully!");
  };

  return (
    <div className="bg-white shadow rounded-lg p-4 border">
      <h2 className="text-lg font-semibold text-amber-600 mb-4">Transactions</h2>

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-2 mb-4">
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

      {/* Transaction Cards */}
      <div className="grid gap-4">
        {filteredTransactions.map((tx) => (
          <div
            key={tx.id}
            className={`border rounded-lg p-4 shadow-sm hover:shadow-md transition ${
              (tx.type === "inbound" && w2Inputs[tx.id] >= tx.w1) ||
              (tx.type === "outbound" && w2Inputs[tx.id] <= tx.w1)
                ? "bg-red-50"
                : "bg-gray-50"
            }`}
          >
            {/* Header */}
            <div className="flex justify-between items-center mb-2">
              <div className="flex items-center gap-3">
                <span className="text-sm font-medium text-gray-500">
                  TX-{tx.id}
                </span>
                <span className="bg-blue-600 text-white text-sm font-semibold px-3 py-1 rounded">
                  {tx.plate}
                </span>
                <span className="text-sm text-gray-600">
                  {tx.vehicleType} {tx.model}
                </span>
              </div>
              <span className="text-xs text-gray-400">
                {new Date(tx.date).toLocaleString()}
              </span>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              {/* Weights Panel */}
              <div className="border rounded p-3 bg-white text-sm space-y-1">
                <div className="flex justify-between">
                  <span>1st:</span>
                  <span>{tx.w1} kg</span>
                </div>
                <div className="flex justify-between">
                  <span>2nd:</span>
                  <span>
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
                        className="border rounded px-2 py-0.5 w-24 text-xs"
                      />
                    )}
                  </span>
                </div>
                <div className="flex justify-between font-semibold">
                  <span>Net:</span>
                  <span>{tx.net ?? "-"}</span>
                </div>
                <div className="flex justify-between text-gray-500">
                  <span>Op:</span>
                  <span>{tx.operator || "-"}</span>
                </div>
                <div className="flex justify-between text-gray-500">
                  <span>TTAT:</span>
                  <span>{tx.ttat ? `${tx.ttat}s` : "-"}</span>
                </div>
              </div>

              {/* Material Info */}
              <div className="text-sm space-y-1">
                <div>
                  <span className="font-medium">Material:</span>{" "}
                  {tx.material || "-"}
                </div>
                <div>
                  <span className="font-medium">Supplier:</span>{" "}
                  {tx.supplier || "-"}
                </div>
                <div>
                  <span className="font-medium">Order:</span>{" "}
                  {tx.orderId || "-"}
                </div>
                <div>
                  <span className="font-medium">Batch:</span>{" "}
                  {tx.batch || "-"}
                </div>
              </div>

              {/* Status / Actions */}
              <div className="flex flex-col justify-between">
                <div className="text-sm space-y-1">
                  <div>
                    <span className="font-medium">Status:</span>{" "}
                    {tx.deactivated
                      ? "Deactivated"
                      : tx.w2
                      ? "Completed"
                      : "In Queue"}
                  </div>
                  <div>
                    <span className="font-medium">Result:</span>{" "}
                    {tx.w2 ? "Passed" : "Pending"}
                  </div>
                  <div>
                    <span className="font-medium">Operation:</span>{" "}
                    {tx.operation || "-"}
                  </div>
                </div>

                <div className="flex flex-wrap gap-2 mt-2">
                  {!tx.w2 && !tx.deactivated && (
                    <>
                      <button
                        className="px-3 py-1 bg-blue-500 text-white text-xs rounded hover:bg-blue-600"
                        onClick={() => validateAndComplete(tx)}
                      >
                        Complete
                      </button>
                      <button
                        className="px-3 py-1 bg-yellow-500 text-white text-xs rounded hover:bg-yellow-600 flex items-center gap-1"
                        onClick={() => onDeactivate(tx.id)}
                      >
                        <Ban size={12} /> Deactivate
                      </button>
                    </>
                  )}
                  {tx.w2 && (
                    <button
                      className="px-3 py-1 bg-green-600 text-white text-xs rounded hover:bg-green-700 flex items-center gap-1"
                      onClick={() => printTicket(tx)}
                    >
                      <Printer size={12} /> Ticket
                    </button>
                  )}
                  <button
                    className="px-3 py-1 bg-gray-500 text-white text-xs rounded hover:bg-gray-600 flex items-center gap-1"
                    onClick={() => setSelectedTx(tx)}
                  >
                    <Eye size={12} /> View
                  </button>
                </div>
              </div>
            </div>
          </div>
        ))}
        {filteredTransactions.length === 0 && (
          <div className="text-center text-gray-500 py-4">
            No transactions found.
          </div>
        )}
      </div>

      {/* Modal */}
      {selectedTx && (
        <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-lg w-full max-w-2xl p-6 relative">
            <button
              className="absolute top-3 right-3 text-gray-500 hover:text-gray-700"
              onClick={() => setSelectedTx(null)}
            >
              <X size={20} />
            </button>
            <h3 className="text-lg font-semibold text-amber-600 mb-4">
              Transaction Details
            </h3>

            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <p><span className="font-medium">Plate:</span> {selectedTx.plate}</p>
                <p><span className="font-medium">Driver:</span> {selectedTx.driver}</p>
                <p><span className="font-medium">Order ID:</span> {selectedTx.orderId}</p>
                <p><span className="font-medium">Batch:</span> {selectedTx.batch}</p>
                <p><span className="font-medium">Type:</span> {selectedTx.type}</p>
              </div>
              <div>
                <p><span className="font-medium">Material:</span> {selectedTx.material}</p>
                <p><span className="font-medium">Supplier:</span> {selectedTx.supplier}</p>
                <p><span className="font-medium">Source Plant:</span> {selectedTx.sourcePlant}</p>
                <p><span className="font-medium">Destination:</span> {selectedTx.destination}</p>
                <p><span className="font-medium">Operation:</span> {selectedTx.operation}</p>
              </div>
            </div>

            <div className="mt-4 border-t pt-4 text-sm">
              <p><span className="font-medium">Weight 1:</span> {selectedTx.w1} kg</p>
              <p><span className="font-medium">Weight 2:</span> {selectedTx.w2 ?? "-"}</p>
              <p><span className="font-medium">Net:</span> {selectedTx.net ?? "-"}</p>
              <p><span className="font-medium">TTAT:</span> {selectedTx.ttat ? `${selectedTx.ttat}s` : "-"}</p>
              <p><span className="font-medium">Date:</span> {new Date(selectedTx.date).toLocaleString()}</p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
