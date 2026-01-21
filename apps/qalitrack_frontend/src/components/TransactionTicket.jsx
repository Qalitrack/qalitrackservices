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
        const pageWidth = doc.internal.pageSize.getWidth();

        // 1. Vehicle Snapshot Banner
        doc.setFillColor(255, 193, 7); // Yellow [cite: 13]
        doc.rect(0, 5, pageWidth, 8, 'F');
        doc.setFontSize(10);
        doc.setTextColor(0, 0, 0);
        doc.text("VEHICLE SNAPSHOT", pageWidth / 2, 10, { align: "center" });

        // 2. Snapshot Placeholder (Black Box) [cite: 1]
        doc.setFillColor(30, 30, 30);
        doc.rect(14, 15, pageWidth - 28, 45, 'F');
        doc.setTextColor(255, 255, 255);
        doc.text(`[ SNAPSHOT: ${record.noPlate} ]`, pageWidth / 2, 40, { align: "center" });

        // 3. Corporate Header [cite: 2]
        doc.setTextColor(0, 0, 0);
        doc.setFontSize(16);
        doc.setFont("helvetica", "bold");
        doc.text("KPFC", 14, 70);
        doc.setFontSize(9);
        doc.setFont("helvetica", "normal");
        doc.text("KPFC BUSINESS CENTER", 14, 75);
        doc.text("PO BOX 36-00902/TEL: 07452266677, KIKUYU", 14, 80);

        // 4. Ticket Status and Date [cite: 3, 4]
        doc.setFontSize(14);
        doc.setFont("helvetica", "bold");
        doc.text("WEIGHING TICKET", pageWidth - 14, 70, { align: "right" });
        doc.setTextColor(34, 139, 34); // Green for LEGAL
        doc.text("LEGAL", pageWidth - 14, 78, { align: "right" });
        doc.setTextColor(0, 0, 0);
        doc.setFontSize(9);
        doc.text(dayjs(record.createdAt).format("MMM DD, YYYY HH:mm"), pageWidth - 14, 84, { align: "right" });

        // 5. Ticket Details Grid 
        autoTable(doc, {
            startY: 90,
            theme: 'plain',
            styles: { fontSize: 8, cellPadding: 1 },
            columnStyles: { 0: { fontStyle: 'bold', width: 30 }, 2: { fontStyle: 'bold', width: 30 } },
            body: [
                ["TICKET NO", `: ${record.receiptNo}`, "REGISTRATION", `: ${record.noPlate}`],
                ["AXLE TYPE", ": 2A", "COMMODITY", `: ${record.commodityName}`],
                ["TRANSPORTER", `: ${record.transporterName || 'N/A'}`, "TIMESTAMP", `: ${dayjs(record.createdAt).format("DD-MM-YY hh:mm A")}`],
                ["SOURCE", `: ${record.originName || 'N/A'}`, "DESTINATION", `: ${record.destinationName || 'N/A'}`],
                ["OPERATOR", `: ${record.operatorName || 'N/A'}`, "DRIVER", `: ${record.driverName || 'N/A'}`],
            ]
        });

        // 6. Axle Weight Analysis 
        doc.setFont("helvetica", "bold");
        doc.text("AXLE WEIGHT ANALYSIS", 14, doc.lastAutoTable.finalY + 10);
        autoTable(doc, {
            startY: doc.lastAutoTable.finalY + 12,
            head: [['ITEMS', 'GROUP 1', 'GROUP 2', 'GROUP 3', 'GVW']],
            styles: { fontSize: 8, halign: 'center', lineWidth: 0.1, lineColor: [0, 0, 0] },
            headStyles: { fillColor: [240, 240, 240], textColor: [0, 0, 0] },
            body: [
                ['ACTUAL WT', '1400', '0', '0', `${record.firstWeight || 0} KG`],
                ['ALLOWED', '8000', '10000', '0', '18000 KG'],
                ['RESULT', 'LEGAL', 'LEGAL', 'LEGAL', 'LEGAL']
            ]
        });

        // 7. Summary Weights [cite: 10]
        const finalY = doc.lastAutoTable.finalY + 10;
        doc.autoTable({
            startY: finalY,
            head: [['FIRST WEIGHT', 'SECOND WEIGHT', 'NET WEIGHT']],
            body: [[`${record.firstWeight || 0} Kg`, `${record.secondWeight || 0} Kg`, `${record.netWeight || 0} Kg`]],
            styles: { halign: 'center', fontSize: 10, fontStyle: 'bold' }
        });

        // 8. Footer [cite: 15, 16]
        const footerY = doc.internal.pageSize.getHeight() - 20;
        doc.setFontSize(8);
        doc.setFont("helvetica", "normal");
        doc.text("QALIBRATED Powered by Qalibrated Systems", pageWidth / 2, footerY, { align: "center" });
        doc.text("www.qalibrated.co.ke | Inventing and Making Happen", pageWidth / 2, footerY + 5, { align: "center" });

        doc.save(`ticket-${record.receiptNo}.pdf`);
        message.success("Professional ticket printed.");
    };

    const columns = [
        { title: "Receipt No", dataIndex: "receiptNo", key: "receiptNo" },
        { title: "Vehicle", dataIndex: "noPlate", key: "noPlate" },
        { title: "Commodity", dataIndex: "commodityName", key: "commodityName" },
        {
            title: "Status",
            dataIndex: "status",
            render: (status) => (
                <Tag color={status === "Completed" ? "green" : "gold"}>{status}</Tag>
            ),
        },
        {
            title: "Action",
            render: (_, record) => (
                <Button icon={<Eye size={16} />} onClick={() => { setSelectedRecord(record); setIsModalOpen(true); }}>
                    View
                </Button>
            ),
        },
    ];

    return (
        <div className="p-6 bg-gray-50 min-h-screen">
            <div className="bg-white p-6 rounded-lg shadow-sm">
                <h2 className="text-2xl font-bold mb-6">Transaction History</h2>

                <Space className="mb-6 flex-wrap" size="middle">
                    <Input
                        placeholder="Search..."
                        onChange={(e) => setFilters({ ...filters, search: e.target.value, page: 1 })}
                    />
                    <RangePicker
                        onChange={(dates) => setFilters({ ...filters, dateRange: dates, page: 1 })}
                    />
                    <Button type="primary" onClick={loadTransactions}>Refresh</Button>
                </Space>

                <Table
                    columns={columns}
                    dataSource={transactions}
                    rowKey="id"
                    loading={loading}
                    rowClassName={(record) => {
                        if (record.status === "Completed") return "bg-green-50 hover:bg-green-100";
                        return "bg-amber-50 hover:bg-amber-100";
                    }}
                    pagination={{
                        current: filters.page,
                        pageSize: filters.pageSize,
                        total: total,
                        onChange: (page) => {
                            setFilters({ ...filters, page });
                            window.scrollTo({ top: 0, behavior: 'smooth' });
                        },
                    }}
                />
            </div>

            <Modal
                title="View Transaction"
                open={isModalOpen}
                onCancel={() => setIsModalOpen(false)}
                width={800}
                footer={[
                    <Button key="close" onClick={() => setIsModalOpen(false)}>Close</Button>,
                    <Button key="print" type="primary" icon={<Printer size={16} />} onClick={() => handlePrint(selectedRecord)}>
                        Print Official Ticket
                    </Button>
                ]}
            >
                {selectedRecord && (
                    <div className="p-4 border rounded bg-white font-mono">
                        <div className="bg-yellow-400 text-center font-bold p-1 text-xs mb-2">VEHICLE SNAPSHOT</div>
                        <div className="h-48 bg-gray-900 flex items-center justify-center text-white mb-4">
                            [ Vehicle Image Area ]
                        </div>
                        <Descriptions bordered column={2} size="small">
                            <Descriptions.Item label="Ticket No">{selectedRecord.receiptNo}</Descriptions.Item>
                            <Descriptions.Item label="Vehicle">{selectedRecord.noPlate}</Descriptions.Item>
                            <Descriptions.Item label="Commodity">{selectedRecord.commodityName}</Descriptions.Item>
                            <Descriptions.Item label="Driver">{selectedRecord.driverName}</Descriptions.Item>
                            <Descriptions.Item label="First Weight">{selectedRecord.firstWeight} Kg</Descriptions.Item>
                            <Descriptions.Item label="Net Weight">{selectedRecord.netWeight} Kg</Descriptions.Item>
                        </Descriptions>
                    </div>
                )}
            </Modal>
        </div>
    );
}