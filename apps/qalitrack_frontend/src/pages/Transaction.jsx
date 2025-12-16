import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Tag, Button, Input, DatePicker, Select, Space, message, Modal, Descriptions } from "antd";
import dayjs from "dayjs";
import { fetchTransactions } from "../store/weighingSlice";
import { Printer, Eye } from "lucide-react";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";

const { RangePicker } = DatePicker;
const { Option } = Select;

export default function Transactions() {
    const dispatch = useDispatch();
    const { transactions, loading, total } = useSelector((state) => state.weighing);

    const [selectedRecord, setSelectedRecord] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [filters, setFilters] = useState({
        search: "",
        dateRange: null,
        page: 1,
        pageSize: 10,
    });

    useEffect(() => {
        loadTransactions();
    }, [filters, dispatch]);

    const loadTransactions = () => {
        const params = {
            search: filters.search,
            startDate: filters.dateRange?.[0] ? filters.dateRange[0].format("YYYY-MM-DD") : undefined,
            endDate: filters.dateRange?.[1] ? filters.dateRange[1].format("YYYY-MM-DD") : undefined,
            pageNumber: filters.page,
            pageSize: filters.pageSize,
        };
        dispatch(fetchTransactions(params));
    };

    const handlePrint = (record) => {
        const doc = new jsPDF();
        
        // Add company header (customize with your logo URL if available)
        doc.setFontSize(10);
        doc.setTextColor(100);
        doc.text("KPFC BUSINESS CENTER", 14, 15);
        doc.text("PO BOX 36-00902 TEL: 074526667, KIKUYU", 14, 20);
        doc.setFontSize(8);
        doc.text(dayjs().format("MMM DD, YYYY hh:mm A"), 160, 15); // Date on right
        doc.setFillColor(0, 128, 0);
        doc.roundedRect(170, 20, 30, 5, 2, 2, "F");
        doc.setTextColor(255);
        doc.text("LEGAL", 178, 24);

        // Main title
        doc.setFontSize(16);
        doc.setTextColor(0);
        doc.text("WEIGHING TICKET", 70, 35);

        // Ticket Details section
        doc.setFontSize(12);
        doc.setFillColor(255, 204, 102);
        doc.rect(14, 40, 180, 8, "F");
        doc.setTextColor(0);
        doc.text("TICKET DETAILS", 80, 46);

        // Ticket Details table (two-column layout for key-value pairs)
        const ticketDetails = [
            ["Ticket No", record.receiptNo || "N/A"],
            ["Axle Type", record.axleType || "N/A"],
            ["Transporter", record.transporter || "N/A"],
            ["Source", record.source || "N/A"],
            ["Operator", record.operator || "N/A"],
            ["Commodity", record.commodityName || "N/A"],
            ["Destination", record.destination || "N/A"],
            ["Timestamp", dayjs(record.timestamp || record.createdAt).format("MM-DD-YY hh:mm A") || "N/A"],
            ["Driver", record.driverName || "N/A"],
        ];
        autoTable(doc, {
            startY: 50,
            body: ticketDetails,
            theme: "grid",
            styles: { fillColor: [255, 255, 204], textColor: 0, fontSize: 10 },
            columnStyles: { 0: { cellWidth: 50 } },
        });

        // Axle Weight Analysis section
        let lastY = doc.lastAutoTable.finalY + 10;
        doc.setFontSize(12);
        doc.setFillColor(255, 204, 102);
        doc.rect(14, lastY, 180, 8, "F");
        doc.text("AXLE WEIGHT ANALYSIS", 70, lastY + 6);
        lastY += 10;

        // Sample axle data (replace with record.axleWeights if it's an array of objects)
        const axleData = record.axleWeights || [
            { group1: 1400, group2: 0, group3: 0, group4: 0, gwv: "1400 KG" },
            { group1: 0.00, group2: 0, group3: 0, group4: 0, gwv: "N/A" }, // PDF row
            { group1: 8000, group2: 10000, group3: 0, group4: 0, gwv: "18000 KG" }, // Allowed
            { group1: 8400, group2: 10500, group3: 0, group4: 0, gwv: "18000 KG" }, // Allowed-5%
            { group1: 0, group2: 0, group3: 0, group4: 0, gwv: "0 KG" }, // Excess
        ];
        const axleHeaders = [["Items", "Group 1", "Group 2", "Group 3", "Group 4", "GWV"]];
        const axleRows = [
            ["Actual WT", ...Object.values(axleData[0]).slice(0, -1), axleData[0].gwv],
            ["PDF", ...Object.values(axleData[1]).slice(0, -1), axleData[1].gwv],
            ["Allowed", ...Object.values(axleData[2]).slice(0, -1), axleData[2].gwv],
            ["Allowed-5%", ...Object.values(axleData[3]).slice(0, -1), axleData[3].gwv],
            ["Excess", ...Object.values(axleData[4]).slice(0, -1), axleData[4].gwv],
            ["Result", "Legal", "Legal", "Legal", "Legal", "Legal"],
        ];
        autoTable(doc, {
            head: axleHeaders,
            body: axleRows,
            startY: lastY,
            theme: "grid",
            headStyles: { fillColor: [255, 204, 102], textColor: 0 },
            bodyStyles: { fillColor: [255, 255, 204] },
        });

        // Weights summary
        lastY = doc.lastAutoTable.finalY + 10;
        doc.setFillColor(255, 204, 102);
        doc.rect(14, lastY, 60, 10, "F");
        doc.rect(74, lastY, 60, 10, "F");
        doc.rect(134, lastY, 60, 10, "F");
        doc.setFontSize(10);
        doc.text("First Weight", 20, lastY + 7);
        doc.text("Second Weight", 80, lastY + 7);
        doc.text("Net Weight", 140, lastY + 7);
        lastY += 10;
        doc.text(`${record.firstWeight || 1400} Kg`, 20, lastY + 7);
        doc.text(`${record.secondWeight || 1400} Kg`, 80, lastY + 7);
        doc.text(`${record.netWeight || 0} Kg`, 140, lastY + 7);

        // Vehicle Snapshot section (if image URL available)
        lastY += 20;
        doc.setFillColor(255, 204, 102);
        doc.rect(14, lastY, 180, 8, "F");
        doc.text("VEHICLE SNAPSHOT", 80, lastY + 6);
        lastY += 10;
        if (record.vehicleSnapshotUrl) {
            // Assuming you have a way to load the image (e.g., via canvas for CORS)
            const img = new Image();
            img.src = record.vehicleSnapshotUrl;
            img.crossOrigin = "anonymous";
            img.onload = () => {
                doc.addImage(img, "JPEG", 14, lastY, 180, 100); // Adjust size as needed
                doc.save(`ticket-${record.receiptNo}.pdf`);
            };
        } else {
            doc.text("No snapshot available", 14, lastY + 10);
            lastY += 20;
        }

        // Footer
        lastY += 110; // Adjust based on image height
        doc.setFillColor(255, 204, 102);
        doc.rect(14, lastY, 180, 10, "F");
        doc.setFontSize(8);
        doc.text("Powered by Calibrated Systems | www.calibrated.co.ke | Inventing and Making Happen", 20, lastY + 7);

        if (!record.vehicleSnapshotUrl) {
            doc.save(`ticket-${record.receiptNo}.pdf`);
        }
        message.success("Ticket generated successfully");
    };

    const openViewModal = (record) => {
        setSelectedRecord(record);
        setIsModalOpen(true);
    };

    const columns = [
        { title: "Receipt No", dataIndex: "receiptNo", key: "receiptNo" },
        { title: "Vehicle", dataIndex: "noPlate", key: "noPlate" },
        { title: "Driver", dataIndex: "driverName", key: "driverName" },
        { title: "Commodity", dataIndex: "commodityName", key: "commodityName" },
        {
            title: "Status",
            dataIndex: "status",
            key: "status",
            render: (status) => (
                <Tag color={status === "Completed" ? "green" : "gold"}>
                    {status.toUpperCase()}
                </Tag>
            ),
        },
        {
            title: "Action",
            key: "action",
            render: (_, record) => (
                <Button 
                    type="default" 
                    icon={<Eye size={16} />} 
                    onClick={() => openViewModal(record)}
                >
                    View
                </Button>
            ),
        },
    ];

    return (
        <div className="p-6 bg-gray-50 min-h-screen">
            <div className="bg-white p-6 rounded-lg shadow-sm">
                <h2 className="text-2xl font-bold mb-6 text-gray-800">Transaction Registry</h2>

                {/* Filters Section */}
                <Space className="mb-6 flex-wrap bg-gray-50 p-4 rounded-md border border-gray-100" size="middle">
                    <Input
                        placeholder="Search Vehicle / Driver"
                        className="w-64"
                        allowClear
                        onChange={(e) => setFilters({ ...filters, search: e.target.value, page: 1 })}
                    />
                    <RangePicker
                        onChange={(dates) => setFilters({ ...filters, dateRange: dates, page: 1 })}
                    />
                    <Select
                        defaultValue={10}
                        onChange={(val) => setFilters({ ...filters, pageSize: val, page: 1 })}
                        className="w-32"
                    >
                        <Option value={10}>10 / Page</Option>
                        <Option value={20}>20 / Page</Option>
                        <Option value={50}>50 / Page</Option>
                    </Select>
                    <Button type="primary" onClick={loadTransactions}>Apply Filters</Button>
                </Space>

                <Table
                    columns={columns}
                    dataSource={transactions}
                    rowKey="id"
                    loading={loading}
                    // APPLYING CUSTOM ROW COLORS
                    rowClassName={(record) => {
                        if (record.status === "Completed") return "bg-green-50 hover:bg-green-100 transition-colors";
                        if (record.status === "In Progress") return "bg-amber-50 hover:bg-amber-100 transition-colors";
                        return "";
                    }}
                    pagination={{
                        current: filters.page,
                        pageSize: filters.pageSize,
                        total: total,
                        showSizeChanger: false,
                        onChange: (page) => {
                            setFilters({ ...filters, page });
                            window.scrollTo({ top: 0, behavior: 'smooth' }); // Smooth scroll
                        },
                    }}
                />
            </div>

            {/* View Transaction Modal */}
            <Modal
                title={`Transaction Details - ${selectedRecord?.receiptNo}`}
                open={isModalOpen}
                onCancel={() => setIsModalOpen(false)}
                footer={[
                    <Button key="close" onClick={() => setIsModalOpen(false)}>Close</Button>,
                    <Button 
                        key="print" 
                        type="primary" 
                        icon={<Printer size={16} />} 
                        onClick={() => handlePrint(selectedRecord)}
                        className="bg-blue-600"
                    >
                        Print Ticket
                    </Button>
                ]}
                width={700}
            >
                {selectedRecord && (
                    <Descriptions bordered column={1} className="mt-4">
                        <Descriptions.Item label="Receipt Number">{selectedRecord.receiptNo}</Descriptions.Item>
                        <Descriptions.Item label="Vehicle Plate">{selectedRecord.noPlate}</Descriptions.Item>
                        <Descriptions.Item label="Driver Name">{selectedRecord.driverName}</Descriptions.Item>
                        <Descriptions.Item label="Commodity">{selectedRecord.commodityName}</Descriptions.Item>
                        <Descriptions.Item label="Weighing Progress">
                            {selectedRecord.completedWeighings} of {selectedRecord.expectedWeighings}
                        </Descriptions.Item>
                        <Descriptions.Item label="Current Status">
                            <Tag color={selectedRecord.status === "Completed" ? "green" : "gold"}>
                                {selectedRecord.status}
                            </Tag>
                        </Descriptions.Item>
                        <Descriptions.Item label="Date Created">
                            {dayjs(selectedRecord.createdAt).format("DD MMMM YYYY, HH:mm")}
                        </Descriptions.Item>
                        <Descriptions.Item label="Axle Type">{selectedRecord.axleType || "N/A"}</Descriptions.Item>
                        <Descriptions.Item label="Transporter">{selectedRecord.transporter || "N/A"}</Descriptions.Item>
                        <Descriptions.Item label="Source">{selectedRecord.source || "N/A"}</Descriptions.Item>
                        <Descriptions.Item label="Destination">{selectedRecord.destination || "N/A"}</Descriptions.Item>
                        <Descriptions.Item label="Operator">{selectedRecord.operator || "N/A"}</Descriptions.Item>
                        <Descriptions.Item label="Timestamp">
                            {dayjs(selectedRecord.timestamp).format("DD MMMM YYYY, HH:mm") || "N/A"}
                        </Descriptions.Item>
                        <Descriptions.Item label="Axle Weights">{JSON.stringify(selectedRecord.axleWeights) || "N/A"}</Descriptions.Item>
                        <Descriptions.Item label="First Weight">{selectedRecord.firstWeight || "N/A"} Kg</Descriptions.Item>
                        <Descriptions.Item label="Second Weight">{selectedRecord.secondWeight || "N/A"} Kg</Descriptions.Item>
                        <Descriptions.Item label="Net Weight">{selectedRecord.netWeight || "N/A"} Kg</Descriptions.Item>
                        <Descriptions.Item label="Vehicle Snapshot">
                            {selectedRecord.vehicleSnapshotUrl ? (
                                <img src={selectedRecord.vehicleSnapshotUrl} alt="Snapshot" style={{ width: "100%" }} />
                            ) : "N/A"}
                        </Descriptions.Item>
                    </Descriptions>
                )}
            </Modal>
        </div>
    );
}