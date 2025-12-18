import React, { useEffect, useState, useCallback } from "react";
import { useDispatch, useSelector } from "react-redux";
import {
  Table,
  Tag,
  Button,
  Input,
  DatePicker,
  Space,
  Modal,
  Descriptions,
  message,
} from "antd";
import dayjs from "dayjs";
import { fetchTransactions, fetchUserById } from "../store/weighingSlice";
import { Printer, Eye, Search } from "lucide-react";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import logoSvg from "../assets/logo.svg";

const { RangePicker } = DatePicker;

export default function Transactions() {
  const dispatch = useDispatch();
  const { transactions, loading, total } = useSelector(
    (state) => state.weighing
  );

  const [selectedRecord, setSelectedRecord] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isPreviewOpen, setIsPreviewOpen] = useState(false);
  const [previewRecord, setPreviewRecord] = useState(null);

  const [filters, setFilters] = useState({
    search: "",
    dateRange: null,
    page: 1,
    pageSize: 10,
  });

  /* ================= FETCH ================= */
  const loadTransactions = useCallback(() => {
    dispatch(
      fetchTransactions({
        search: filters.search || undefined,
        startDate: filters.dateRange?.[0]?.format("YYYY-MM-DD"),
        endDate: filters.dateRange?.[1]?.format("YYYY-MM-DD"),
        pageNumber: filters.page,
        pageSize: filters.pageSize,
      })
    );
  }, [dispatch, filters]);

  useEffect(() => {
    loadTransactions();
  }, [loadTransactions]);

  /* ================= VIEW ================= */
  const openViewModal = async (record) => {
    let enriched = { ...record };

    if (record.operatorId) {
      try {
        const operator = await dispatch(
          fetchUserById(record.operatorId)
        ).unwrap();
        enriched.operatorName = operator?.fullName || operator?.name || "N/A";
      } catch (err) {
        console.warn("Failed to fetch operator:", err);
        enriched.operatorName = "Unknown Operator";
      }
    } else {
      enriched.operatorName = "N/A";
    }

    setSelectedRecord(enriched);
    setIsModalOpen(true);
  };

  /* ================= PRINT ================= */
  const handlePrint = (record) => {
    if (!record) return;

    message.loading({ content: "Generating PDF ticket...", duration: 0 });

    const doc = new jsPDF();
    const amber = [255, 152, 0];
    const slate = [33, 33, 33];
    const lightGray = [245, 245, 245];

    // Logo with fallback
    try {
      doc.addImage(logoSvg, "SVG", 14, 10, 25, 25);
    } catch {
      doc.setFillColor(...amber);
      doc.roundedRect(14, 10, 25, 25, 3, 3, "F");
      doc.setTextColor(255, 255, 255);
      doc.setFontSize(14);
      doc.setFont("helvetica", "bold");
      doc.text("QS", 26.5, 23, { align: "center" });
    }

    doc.setFontSize(14);
    doc.setFont("helvetica", "bold");
    doc.setTextColor(...slate);
    doc.text("QALIBRATED SYSTEMS LTD", 105, 15, { align: "center" });

    doc.setFontSize(9);
    doc.setFont("helvetica", "normal");
    doc.text("PO BOX 34463-00100, NAIROBI", 105, 20, { align: "center" });
    doc.text("TEL: +254 714 999 996 | WWW.QALIBRATED.CO.KE", 105, 25, { align: "center" });

    doc.setFontSize(16);
    doc.setTextColor(...amber);
    doc.text("WEIGHING TICKET", 105, 40, { align: "center" });

    let y = 45;
    doc.setFillColor(...amber);
    doc.rect(14, y, 182, 7, "F");
    doc.setTextColor(255, 255, 255);
    doc.setFontSize(10);
    doc.text("TICKET DETAILS", 105, y + 5, { align: "center" });

    y += 7;
    autoTable(doc, {
      startY: y,
      theme: "plain",
      styles: { fontSize: 8.5, cellPadding: 1.5 },
      columnStyles: {
        0: { fontStyle: "bold", cellWidth: 32 },
        1: { cellWidth: 58 },
        2: { fontStyle: "bold", cellWidth: 32 },
        3: { cellWidth: 58 },
      },
      body: [
        ["TICKET NO", `: ${record.receiptNo}`, "REGISTRATION", `: ${record.noPlate}`],
        ["AXLE TYPE", `: ${record.axleType || "N/A"}`, "COMMODITY", `: ${record.commodityName}`],
        ["TRANSPORTER", `: ${record.transporterName}`, "TIMESTAMP", `: ${dayjs(record.createdAt).format("DD-MM-YY : hh:mm A")}`],
        ["SUPPLIER", `: ${record.supplierName || "N/A"}`, "CUSTOMER", `: ${record.customerName || "N/A"}`],
        ["SOURCE", `: ${record.originName || "N/A"}`, "DESTINATION", `: ${record.destinationName || "N/A"}`],
        ["OPERATOR", `: ${record.operatorName || "N/A"}`, "DRIVER", `: ${record.driverName || "N/A"}`],
        ["WEIGH MODE", `: ${record.weighMode || "N/A"}`, "STATUS", `: ${record.status || "N/A"}`],
      ],
    });

    y = doc.lastAutoTable.finalY + 5;
    doc.setFillColor(...amber);
    doc.rect(14, y, 182, 7, "F");
    doc.setTextColor(255, 255, 255);
    doc.text("AXLE WEIGHT ANALYSIS", 105, y + 5, { align: "center" });

    y += 7;
    autoTable(doc, {
      startY: y,
      headStyles: { fillColor: lightGray, textColor: slate, fontStyle: "bold" },
      body: [
        ["ITEMS", "GROUP 1", "GROUP 2", "GROUP 3", "GROUP 4", "GVW"],
        ["ACTUAL WT", `${record.firstWeight} KG`, "0", "0", "0", `${record.firstWeight} KG`],
        ["PDF", "0.00", "0", "0", "0", "N/A"],
        ["ALLOWED", "8000", "10000", "0", "0", "18000 KG"],
        ["ALLOWED+5%", "8400", "10500", "0", "0", "18000 KG"],
        ["EXCESS", "0", "0", "0", "0", "0 KG"],
        ["RESULT", "LEGAL", "LEGAL", "LEGAL", "LEGAL", "LEGAL"],
      ],
      theme: "grid",
      styles: { fontSize: 8, halign: "center" },
    });

    y = doc.lastAutoTable.finalY + 5;
    autoTable(doc, {
      startY: y,
      head: [["FIRST WEIGHT", "SECOND WEIGHT", "NET WEIGHT"]],
      body: [[`${record.firstWeight} Kg`, `${record.secondWeight ?? 0} Kg`, `${record.netWeight ?? 0} Kg`]],
      theme: "grid",
      headStyles: { fillColor: amber, halign: "center" },
      styles: { halign: "center", fontSize: 10, fontStyle: "bold" },
    });

    y = doc.lastAutoTable.finalY + 10;
    doc.setFontSize(10);
    doc.setTextColor(...slate);
    doc.text("VEHICLE SECURITY SNAPSHOT", 14, y);
    y += 5;

    if (record.vehicleSnapshotUrl) {
      try {
        doc.addImage(record.vehicleSnapshotUrl, "JPEG", 14, y, 182, 60);
        y += 65;
      } catch {
        doc.text("Visual evidence not available", 14, y);
        y += 10;
      }
    } else {
      doc.text("Visual evidence not available", 14, y);
      y += 10;
    }

    doc.setFontSize(8);
    doc.setFont("helvetica", "italic");
    doc.text("QALIBRATED SYSTEMS-INVENTING AND MAKING HAPPEN", 105, y + 10, { align: "center" });
    doc.setFont("helvetica", "normal");
    doc.text("Powered by Qalibrated Systems | www.qalibrated.co.ke", 105, y + 15, { align: "center" });

    message.destroy();
    message.success("PDF generated successfully!");
    doc.save(`Ticket_${record.receiptNo}.pdf`);
  };

  /* ================= TICKET PREVIEW COMPONENT ================= */
  const TicketPreview = ({ record, onClose, onDownload }) => {
    if (!record) return null;

    return (
      <div className="bg-white p-8 font-sans text-sm leading-relaxed">
        {/* Header */}
        <div className="flex justify-between items-start mb-6">
          <img src={logoSvg} alt="Logo" className="h-14" onError={(e) => { e.target.style.display = 'none'; }} />
          <div className="text-right">
            <h1 className="text-2xl font-bold text-slate-800">QALIBRATED SYSTEMS LTD</h1>
            <p>PO BOX 34463-00100, NAIROBI</p>
            <p>TEL: +254 714 999 996 | WWW.QALIBRATED.CO.KE</p>
          </div>
        </div>

        <h2 className="text-center text-3xl font-bold text-amber-500 my-8">WEIGHING TICKET</h2>

        {/* Ticket Details */}
        <div className="bg-amber-500 text-white py-2 text-center font-bold mb-4">TICKET DETAILS</div>
        <table className="w-full text-xs mb-6">
          <tbody>
            {[
              ["TICKET NO", record.receiptNo, "REGISTRATION", record.noPlate],
              ["AXLE TYPE", record.axleType || "N/A", "COMMODITY", record.commodityName],
              ["TRANSPORTER", record.transporterName, "TIMESTAMP", dayjs(record.createdAt).format("DD-MM-YY : hh:mm A")],
              ["SUPPLIER", record.supplierName || "N/A", "CUSTOMER", record.customerName || "N/A"],
              ["SOURCE", record.originName || "N/A", "DESTINATION", record.destinationName || "N/A"],
              ["OPERATOR", record.operatorName || "N/A", "DRIVER", record.driverName || "N/A"],
              ["WEIGH MODE", record.weighMode || "N/A", "STATUS", record.status || "N/A"],
            ].map((row, i) => (
              <tr key={i}>
                <td className="font-bold py-1">{row[0]}</td>
                <td className="py-1">: {row[1]}</td>
                <td className="font-bold py-1 pl-10">{row[2]}</td>
                <td className="py-1">: {row[3]}</td>
              </tr>
            ))}
          </tbody>
        </table>

        {/* Axle Analysis */}
        <div className="bg-amber-500 text-white py-2 text-center font-bold mb-3">AXLE WEIGHT ANALYSIS</div>
        <table className="w-full border border-gray-400 text-xs text-center mb-6">
          <thead className="bg-gray-200">
            <tr>
              <th className="border border-gray-400 py-2">ITEMS</th>
              <th className="border border-gray-400">GROUP 1</th>
              <th className="border border-gray-400">GROUP 2</th>
              <th className="border border-gray-400">GROUP 3</th>
              <th className="border border-gray-400">GROUP 4</th>
              <th className="border border-gray-400">GVW</th>
            </tr>
          </thead>
          <tbody>
            <tr><td className="border py-1 font-medium">ACTUAL WT</td><td>{record.firstWeight} KG</td><td>0</td><td>0</td><td>0</td><td>{record.firstWeight} KG</td></tr>
            <tr><td className="border py-1">ALLOWED</td><td>8000</td><td>10000</td><td>0</td><td>0</td><td>18000 KG</td></tr>
            <tr><td className="border py-1">ALLOWED+5%</td><td>8400</td><td>10500</td><td>0</td><td>0</td><td>18000 KG</td></tr>
            <tr><td className="border py-1">EXCESS</td><td>0</td><td>0</td><td>0</td><td>0</td><td>0 KG</td></tr>
            <tr><td className="border py-1 font-bold">RESULT</td><td className="text-green-600 font-bold">LEGAL</td><td className="text-green-600 font-bold">LEGAL</td><td className="text-green-600 font-bold">LEGAL</td><td className="text-green-600 font-bold">LEGAL</td><td className="text-green-600 font-bold">LEGAL</td></tr>
          </tbody>
        </table>

        {/* Weight Summary */}
        <table className="w-full border-4 border-amber-500 text-center text-lg font-bold mb-8">
          <thead className="bg-amber-500 text-white">
            <tr>
              <th className="py-3">FIRST WEIGHT</th>
              <th className="py-3">SECOND WEIGHT</th>
              <th className="py-3">NET WEIGHT</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td className="py-4">{record.firstWeight} Kg</td>
              <td className="py-4">{record.secondWeight ?? 0} Kg</td>
              <td className="py-4">{record.netWeight ?? 0} Kg</td>
            </tr>
          </tbody>
        </table>

        {/* Snapshot */}
        <div className="mb-8">
          <h3 className="font-bold text-lg mb-3">VEHICLE SECURITY SNAPSHOT</h3>
          {record.vehicleSnapshotUrl ? (
            <img src={record.vehicleSnapshotUrl} alt="Vehicle" className="w-full border-2 border-gray-300 rounded" />
          ) : (
            <p className="italic text-gray-500 bg-gray-100 p-8 text-center rounded">Visual evidence not available</p>
          )}
        </div>

        {/* Footer */}
        <div className="text-center text-xs italic text-gray-600">
          <p>QALIBRATED SYSTEMS-INVENTING AND MAKING HAPPEN</p>
          <p>Powered by Qalibrated Systems | www.qalibrated.co.ke</p>
        </div>

        {/* Preview Actions */}
        <div className="flex justify-end gap-4 mt-10">
          <Button size="large" onClick={onClose}>Close Preview</Button>
          <Button size="large" type="primary" className="bg-amber-600" icon={<Printer />} onClick={onDownload}>
            Download PDF
          </Button>
        </div>
      </div>
    );
  };

  /* ================= TABLE COLUMNS ================= */
  const columns = [
    { title: "Receipt No", dataIndex: "receiptNo", width: 140 },
    {
      title: "Vehicle",
      dataIndex: "noPlate",
      width: 130,
      render: (t) => <Tag color="blue">{t}</Tag>,
    },
    { title: "Driver", dataIndex: "driverName", width: 150 },
    { title: "Commodity", dataIndex: "commodityName", width: 160 },
    {
      title: "Status",
      dataIndex: "status",
      width: 120,
      render: (status) => (
        <Tag color={status === "Completed" ? "green" : "orange"} className="font-semibold">
          {status?.toUpperCase()}
        </Tag>
      ),
    },
    {
      title: "Action",
      width: 120,
      fixed: "right",
      render: (_, record) => (
        <Button size="small" icon={<Eye size={14} />} onClick={() => openViewModal(record)}>
          View
        </Button>
      ),
    },
  ];

  return (
    <div className="h-screen p-4 bg-gray-50 flex flex-col">
      {/* Filters */}
      <div className="mb-3 bg-white p-4 rounded-lg shadow-sm border flex flex-wrap items-center gap-3">
        <Input
          allowClear
          placeholder="Search vehicle / receipt..."
          prefix={<Search size={16} />}
          className="w-64 h-10"
          onChange={(e) =>
            setFilters({ ...filters, search: e.target.value, page: 1 })
          }
        />
        <RangePicker
          className="h-10"
          onChange={(dates) =>
            setFilters({ ...filters, dateRange: dates, page: 1 })
          }
        />
        <Button
          type="primary"
          className="bg-amber-500 border-none px-6 h-10 font-bold"
          onClick={loadTransactions}
        >
          FILTER
        </Button>
      </div>

      {/* Table */}
      <div className="flex-1 overflow-hidden bg-white rounded-lg border">
        <Table
          columns={columns}
          dataSource={transactions}
          rowKey="id"
          loading={loading}
          size="middle"
          scroll={{ y: "calc(100% - 50px)" }}
          pagination={{
            current: filters.page,
            pageSize: filters.pageSize,
            total,
            showSizeChanger: true,
            pageSizeOptions: [10, 20, 50],
            onChange: (page, pageSize) =>
              setFilters({ ...filters, page, pageSize }),
          }}
        />
      </div>

      {/* View Details Modal */}
      <Modal
        title={<div className="font-bold text-amber-600">TRANSACTION DETAILS: {selectedRecord?.receiptNo}</div>}
        open={isModalOpen}
        onCancel={() => setIsModalOpen(false)}
        width={800}
        footer={[
          <Button
            key="preview"
            icon={<Eye size={16} />}
            onClick={() => {
              setPreviewRecord(selectedRecord);
              setIsPreviewOpen(true);
            }}
          >
            Preview Ticket
          </Button>,
          <Button
            key="print"
            type="primary"
            icon={<Printer size={16} />}
            className="bg-amber-600 h-10"
            onClick={() => handlePrint(selectedRecord)}
          >
            Direct Print (Download PDF)
          </Button>,
        ]}
      >
        {selectedRecord && (
          <Descriptions bordered column={2} size="small" className="mt-4">
            <Descriptions.Item label="Receipt No">{selectedRecord.receiptNo}</Descriptions.Item>
            <Descriptions.Item label="Vehicle Plate">{selectedRecord.noPlate}</Descriptions.Item>
            <Descriptions.Item label="Driver">{selectedRecord.driverName}</Descriptions.Item>
            <Descriptions.Item label="Commodity">{selectedRecord.commodityName}</Descriptions.Item>
            <Descriptions.Item label="Transporter">{selectedRecord.transporterName}</Descriptions.Item>
            <Descriptions.Item label="Supplier">{selectedRecord.supplierName || "N/A"}</Descriptions.Item>
            <Descriptions.Item label="Customer">{selectedRecord.customerName || "N/A"}</Descriptions.Item>
            <Descriptions.Item label="Source">{selectedRecord.originName || "N/A"}</Descriptions.Item>
            <Descriptions.Item label="Destination">{selectedRecord.destinationName || "N/A"}</Descriptions.Item>
            <Descriptions.Item label="First Weight">{selectedRecord.firstWeight} Kg</Descriptions.Item>
            <Descriptions.Item label="Second Weight">{selectedRecord.secondWeight ?? 0} Kg</Descriptions.Item>
            <Descriptions.Item label="Net Weight" className="font-bold text-blue-600">
              {selectedRecord.netWeight ?? 0} Kg
            </Descriptions.Item>
            <Descriptions.Item label="Operator">{selectedRecord.operatorName}</Descriptions.Item>
            <Descriptions.Item label="Weigh Mode">{selectedRecord.weighMode || "N/A"}</Descriptions.Item>
            <Descriptions.Item label="Status">
              <Tag color={selectedRecord.status === "Completed" ? "green" : "orange"}>{selectedRecord.status}</Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Date Created">{dayjs(selectedRecord.createdAt).format("LLL")}</Descriptions.Item>
            <Descriptions.Item label="Vehicle Proof" span={2}>
              {selectedRecord.vehicleSnapshotUrl ? (
                <img src={selectedRecord.vehicleSnapshotUrl} className="w-full rounded border" alt="Snapshot" />
              ) : (
                "No Image Available"
              )}
            </Descriptions.Item>
          </Descriptions>
        )}
      </Modal>

      {/* Ticket Preview Modal */}
      <Modal
        title={<div className="font-bold text-amber-600">TICKET PREVIEW: {previewRecord?.receiptNo}</div>}
        open={isPreviewOpen}
        onCancel={() => setIsPreviewOpen(false)}
        width={1100}
        footer={null}
        destroyOnClose
      >
        <div className="overflow-auto max-h-[80vh] bg-gray-50 p-4 rounded">
          <TicketPreview
            record={previewRecord}
            onClose={() => setIsPreviewOpen(false)}
            onDownload={() => {
              handlePrint(previewRecord);
              setIsPreviewOpen(false);
            }}
          />
        </div>
      </Modal>
    </div>
  );
}