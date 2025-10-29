// src/components/weighing/TransactionList.jsx
import React, { useState, useMemo, useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import {
  fetchTransactionsList,
  addWeighing,
  completeTransaction,
  deactivateTransactionApi,
} from "../store/weighingSlice";
import { Ban, Printer, Search, Eye, X, Loader2, FileText } from "lucide-react";
import jsPDF from "jspdf";
import toast from "react-hot-toast";

export default function TransactionList() {
  const dispatch = useDispatch();
  const { transactions = [], loading, error } = useSelector(
    (state) => state.weighing
  );

  const [w2Inputs, setW2Inputs] = useState({});
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");

  // Fetch today’s transactions
  useEffect(() => {
    const today = new Date().toISOString().split("T")[0];
    dispatch(
      fetchTransactionsList({
        pageNumber: 1,
        pageSize: 100,
        startDate: today,
        endDate: today,
      })
    );
  }, [dispatch]);

  // Filtering
  const filteredTransactions = useMemo(() => {
    return transactions.filter((tx) => {
      const matchesSearch =
        tx.noPlate?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        tx.receiptNo?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        tx.driverName?.toLowerCase().includes(searchQuery.toLowerCase());

      const matchesStatus =
        statusFilter === "all" ||
        (statusFilter === "inqueue" && tx.firstWeight && !tx.secondWeight && !tx.deactivated) ||
        (statusFilter === "completed" && tx.secondWeight) ||
        (statusFilter === "deactivated" && tx.deactivated);

      return matchesSearch && matchesStatus;
    });
  }, [transactions, searchQuery, statusFilter]);

  // A6 TICKET (105 x 148 mm)
  const generateA6Ticket = (tx) => {
    const doc = new jsPDF({
      orientation: "portrait",
      unit: "mm",
      format: [105, 148], // A6 size
    });

    let y = 10;
    const lineHeight = 5;
    const fontSize = 8;
    const titleSize = 11;

    doc.setFont("helvetica", "bold");
    doc.setFontSize(titleSize);
    doc.text("WEIGHING TICKET", 52.5, y, { align: "center" });
    y += 8;

    doc.setFontSize(fontSize);
    doc.setFont("helvetica", "normal");

    const addLine = (label, value) => {
      doc.text(`${label}:`, 8, y);
      doc.text(`${value}`, 35, y);
      y += lineHeight;
    };

    addLine("Receipt No", tx.receiptNo);
    addLine("Plate No", tx.noPlate);
    addLine("Driver", tx.driverName || "-");
    addLine("Commodity", tx.commodity?.name || "-");
    addLine("Supplier", tx.supplier?.name || "-");
    addLine("Customer", tx.customer?.name || "-");
    addLine("Transporter", tx.transporter?.name || "-");
    addLine("Origin", tx.origin?.name || "-");
    addLine("Destination", tx.destination?.name || "-");
    addLine("1st Weight", `${tx.firstWeight?.toLocaleString() || "-"} kg`);
    addLine("2nd Weight", `${tx.secondWeight?.toLocaleString() || "-"} kg`);
    addLine("Net Weight", `${tx.netWeight?.toLocaleString() || "-"} kg`);
    addLine("Operation", tx.operation || "-");
    addLine("Weigh Mode", tx.weighMode || "-");
    addLine("TTAT", tx.ttat ? `${tx.ttat}s` : "-");
    addLine("Date", new Date(tx.createdAt).toLocaleString());

    // Optional: Add logo (uncomment if you have base64 logo)
    // doc.addImage(logoBase64, "PNG", 8, y, 25, 25);
    // y += 28;

    // Footer
    y += 5;
    doc.setFontSize(7);
    doc.text("Powered by QaliTrack", 52.5, y, { align: "center" });

    // Open in new tab for print
    doc.output("dataurlnewwindow");
    toast.success("Ticket ready – Print or Save");
  };

  // Complete transaction
  const validateAndComplete = async (tx) => {
    const w2 = parseFloat(w2Inputs[tx.receiptNo]);
    if (isNaN(w2) || w2 <= 0) {
      toast.error("Enter valid W2");
      return;
    }

    const isInbound = tx.operation?.toLowerCase().includes("inbound");
    if ((isInbound && w2 >= tx.firstWeight) || (!isInbound && w2 <= tx.firstWeight)) {
      toast.error(isInbound ? "W2 must be < W1" : "W2 must be > W1");
      return;
    }

    try {
      await dispatch(
        addWeighing({
          transactionId: tx.receiptNo,
          weight: w2,
          weighBridgeId: 1,
          operatorId: 1,
          notes: "UI second weigh",
        })
      ).unwrap();

      await dispatch(completeTransaction({ transactionId: tx.receiptNo })).unwrap();

      setW2Inputs((prev) => ({ ...prev, [tx.receiptNo]: "" }));
      toast.success("Completed!");
    } catch (err) {
      toast.error("Failed");
    }
  };

  // Deactivate
  const handleDeactivate = async (receiptNo) => {
    if (!confirm(`Deactivate ${receiptNo}?`)) return;
    try {
      await dispatch(deactivateTransactionApi(receiptNo)).unwrap();
      toast.success("Deactivated");
    } catch {
      toast.error("Failed");
    }
  };

  if (loading) return <div className="p-8 text-center"><Loader2 className="animate-spin inline" /></div>;
  if (error) return <div className="p-4 text-red-600 text-center">Error: {error}</div>;

  return (
    <div className="bg-white shadow rounded-lg p-4 border">
      <h2 className="text-lg font-semibold text-amber-600 mb-4">Transactions</h2>

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-2 mb-4">
        <div className="flex items-center border rounded px-2 flex-1">
          <Search size={16} className="text-gray-400" />
          <input
            type="text"
            placeholder="Plate, Receipt, Driver..."
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

      {/* Cards */}
      <div className="grid gap-4">
        {filteredTransactions.map((tx) => (
          <div
            key={tx.receiptNo}
            className={`border rounded-lg p-4 shadow-sm hover:shadow-md transition ${
              tx.deactivated ? "bg-gray-100 opacity-70" : "bg-gray-50"
            }`}
          >
            <div className="flex justify-between items-center mb-2">
              <div className="flex items-center gap-3">
                <span className="text-sm font-medium text-gray-500">
                  TX-{tx.receiptNo}
                </span>
                <span className="bg-blue-600 text-white text-sm font-semibold px-3 py-1 rounded">
                  {tx.noPlate}
                </span>
              </div>
              <span className="text-xs text-gray-400">
                {new Date(tx.createdAt).toLocaleTimeString()}
              </span>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              {/* Weights */}
              <div className="border rounded p-3 bg-white text-sm space-y-1">
                <div className="flex justify-between">
                  <span>1st:</span>
                  <span>{tx.firstWeight?.toLocaleString() || "-"} kg</span>
                </div>
                <div className="flex justify-between">
                  <span>2nd:</span>
                  <span>
                    {tx.secondWeight ? (
                      `${tx.secondWeight.toLocaleString()} kg`
                    ) : (
                      <input
                        type="number"
                        placeholder="W2"
                        value={w2Inputs[tx.receiptNo] || ""}
                        onChange={(e) =>
                          setW2Inputs((prev) => ({
                            ...prev,
                            [tx.receiptNo]: e.target.value,
                          }))
                        }
                        className="border rounded px-2 py-0.5 w-20 text-xs"
                      />
                    )}
                  </span>
                </div>
                <div className="flex justify-between font-semibold">
                  <span>Net:</span>
                  <span>{tx.netWeight?.toLocaleString() || "-"} kg</span>
                </div>
              </div>

              {/* Info */}
              <div className="text-sm space-y-1">
                <div><strong>Commodity:</strong> {tx.commodity?.name || "-"}</div>
                <div><strong>Supplier:</strong> {tx.supplier?.name || "-"}</div>
                <div><strong>Operation:</strong> {tx.operation || "-"}</div>
              </div>

              {/* Actions */}
              <div className="flex flex-col justify-between">
                <div className="text-sm">
                  <div><strong>Status:</strong>{" "}
                    {tx.deactivated ? "Deactivated" : tx.secondWeight ? "Completed" : "In Queue"}
                  </div>
                </div>

                <div className="flex flex-wrap gap-1 mt-2">
                  {!tx.secondWeight && !tx.deactivated && (
                    <>
                      <button
                        onClick={() => validateAndComplete(tx)}
                        className="px-2 py-1 bg-blue-500 text-white text-xs rounded hover:bg-blue-600"
                      >
                        Complete
                      </button>
                      <button
                        onClick={() => handleDeactivate(tx.receiptNo)}
                        className="px-2 py-1 bg-yellow-500 text-white text-xs rounded hover:bg-yellow-600 flex items-center gap-1"
                      >
                        <Ban size={11} /> Deactivate
                      </button>
                    </>
                  )}
                  {tx.secondWeight && (
                    <button
                      onClick={() => generateA6Ticket(tx)}
                      className="px-2 py-1 bg-green-600 text-white text-xs rounded hover:bg-green-700 flex items-center gap-1"
                    >
                      <FileText size={11} /> View Ticket
                    </button>
                  )}
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
    </div>
  );
}