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
    let enriched = record;

    if (record.operatorId) {
      try {
        const operator = await dispatch(
          fetchUserById(record.operatorId)
        ).unwrap();
        enriched = {
          ...record,
          operatorName: operator?.fullName || operator?.name || "N/A",
        };
      } catch {}
    }

    setSelectedRecord(enriched);
    setIsModalOpen(true);
  };

  /* ================= PRINT (UPDATED SECTION) ================= */
  const handlePrint = (record) => {
    if (!record) return;

    const doc = new jsPDF();
    const amber = [255, 152, 0];
    const slate = [33, 33, 33];
    const green = [16, 185, 129];
    const lightGray = [245, 245, 245];

    // 1. Header & Logo
    try {
      doc.addImage(logoSvg, "SVG", 14, 10, 25, 25);
    } catch {
      doc.setFillColor(...amber);
      doc.roundedRect(14, 10, 20, 20, 1, 1, "F");
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

    // 2. Ticket Details Header
    let y = 45;
    doc.setFillColor(...amber);
    doc.rect(14, y, 182, 7, "F");
    doc.setTextColor(255, 255, 255);
    doc.setFontSize(10);
    doc.text("TICKET DETAILS", 105, y + 5, { align: "center" });

    // 3. Ticket Information Table
    y += 7;
    autoTable(doc, {
      startY: y,
      theme: "plain",
      styles: { fontSize: 9, cellPadding: 2 },
      columnStyles: {
        0: { fontStyle: "bold", cellWidth: 35 },
        1: { cellWidth: 55 },
        2: { fontStyle: "bold", cellWidth: 35 },
        3: { cellWidth: 55 },
      },
      body: [
        ["TICKET NO", `: ${record.receiptNo}`, "REGISTRATION", `: ${record.noPlate}`],
        ["AXLE TYPE", `: ${record.axleType || "N/A"}`, "COMMODITY", `: ${record.commodityName}`],
        ["TRANSPORTER", `: ${record.transporterName}`, "TIMESTAMP", `: ${dayjs(record.createdAt).format("DD-MM-YY : hh:mm A")}`],
        ["SOURCE", `: ${record.originName || "N/A"}`, "DESTINATION", `: ${record.destinationName || "N/A"}`],
        ["OPERATOR", `: ${record.operatorName || "N/A"}`, "DRIVER", `: ${record.driverName || "N/A"}`],
      ],
    });

    // 4. Axle Weight Analysis
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

    // 5. Weight Summary
    y = doc.lastAutoTable.finalY + 5;
    autoTable(doc, {
      startY: y,
      head: [["FIRST WEIGHT", "SECOND WEIGHT", "NET WEIGHT"]],
      body: [[`${record.firstWeight} Kg`, `${record.secondWeight || 0} Kg`, `${record.netWeight || 0} Kg`]],
      theme: "grid",
      headStyles: { fillColor: amber, halign: "center" },
      styles: { halign: "center", fontSize: 10, fontStyle: "bold" },
    });

    // 6. Footer
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

    doc.save(`Ticket_${record.receiptNo}.pdf`);
  };

  /* ================= TABLE ================= */
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
        <Tag
          color={status === "Completed" ? "green" : "orange"}
          className="font-semibold"
        >
          {status?.toUpperCase()}
        </Tag>
      ),
    },
    {
      title: "Action",
      width: 120,
      fixed: "right",
      render: (_, record) => (
        <Button
          size="small"
          icon={<Eye size={14} />}
          onClick={() => openViewModal(record)}
        >
          View
        </Button>
      ),
    },
  ];

  return (
    <div className="h-screen p-4 bg-gray-50 flex flex-col">
      {/* ================= FILTERS ================= */}
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

      {/* ================= TABLE ================= */}
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

      {/* ================= MODAL ================= */}
      <Modal
        title={
          <div className="font-bold text-amber-600">
            TRANSACTION DETAILS: {selectedRecord?.receiptNo}
          </div>
        }
        open={isModalOpen}
        onCancel={() => setIsModalOpen(false)}
        width={800}
        footer={[
          <Button
            key="print"
            type="primary"
            icon={<Printer size={16} />}
            className="bg-amber-600 h-10"
            onClick={() => handlePrint(selectedRecord)}
          >
            PRINT TICKET
          </Button>,
        ]}
      >
        {selectedRecord && (
          <Descriptions bordered column={2} size="small" className="mt-4">
            <Descriptions.Item label="Receipt No">
              {selectedRecord.receiptNo}
            </Descriptions.Item>
            <Descriptions.Item label="Vehicle Plate">
              {selectedRecord.noPlate}
            </Descriptions.Item>
            <Descriptions.Item label="Driver">
              {selectedRecord.driverName}
            </Descriptions.Item>
            <Descriptions.Item label="Commodity">
              {selectedRecord.commodityName}
            </Descriptions.Item>
            <Descriptions.Item label="Transporter">
              {selectedRecord.transporterName}
            </Descriptions.Item>
            <Descriptions.Item label="Supplier">
              {selectedRecord.supplierName}
            </Descriptions.Item>
            <Descriptions.Item label="Customer">
              {selectedRecord.customerName}
            </Descriptions.Item>
            <Descriptions.Item label="Source">
              {selectedRecord.originName}
            </Descriptions.Item>
            <Descriptions.Item label="Destination">
              {selectedRecord.destinationName}
            </Descriptions.Item>
            <Descriptions.Item label="First Weight">
              {selectedRecord.firstWeight} Kg
            </Descriptions.Item>
            <Descriptions.Item label="Second Weight">
              {selectedRecord.secondWeight} Kg
            </Descriptions.Item>
            <Descriptions.Item label="Net Weight" className="font-bold text-blue-600">
              {selectedRecord.netWeight} Kg
            </Descriptions.Item>
            <Descriptions.Item label="Operator">
              {selectedRecord.operatorName}
            </Descriptions.Item>
            <Descriptions.Item label="Weigh Mode">
              {selectedRecord.weighMode}
            </Descriptions.Item>
            <Descriptions.Item label="Status">
              <Tag color="orange">{selectedRecord.status}</Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Date Created">
              {dayjs(selectedRecord.createdAt).format("LLL")}
            </Descriptions.Item>
            <Descriptions.Item label="Vehicle Proof" span={2}>
              {selectedRecord.vehicleSnapshotUrl ? (
                <img
                  src={selectedRecord.vehicleSnapshotUrl}
                  className="w-full rounded border"
                  alt="Snapshot"
                />
              ) : (
                "No Image Available"
              )}
            </Descriptions.Item>
          </Descriptions>
        )}
      </Modal>
    </div>
  );
}