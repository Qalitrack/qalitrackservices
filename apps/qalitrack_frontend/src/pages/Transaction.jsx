import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Tag, Button, Input, DatePicker, Select, Space, message, Modal, Descriptions } from "antd";
import dayjs from "dayjs";
import { fetchTransactions, fetchUserById } from "../store/weighingSlice";
import { Printer, Eye } from "lucide-react";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";

const { RangePicker } = DatePicker;
const { Option } = Select;

export default function Transactions() {
    const dispatch = useDispatch();
    const { transactions, loading, total, users } = useSelector((state) => state.weighing);

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

    const handlePrint = async (record) => {
        const doc = new jsPDF();
        
        // Modern colors: Soft Amber [255, 193, 7], Emerald Green [0, 150, 136], Slate Black [33, 33, 33], Light Amber [255, 249, 196]
        const amber = [255, 193, 7];
        const green = [0, 150, 136];
        const black = [33, 33, 33];
        const lightAmber = [255, 249, 196];
        const darkGreen = [0, 128, 0];

        // Company logo at top left, with placeholder if fails
        const logoUrl = 'https://cdn.brandfetch.io/idvHdvVGVZ/w/1280/h/905/idBSK3DzYW.jpeg?c=1bxid64Mup7aczewSAYMX&t=1765569910329';
        const logoImg = new Image();
        logoImg.crossOrigin = "anonymous";
        logoImg.src = logoUrl;

        // Wait for logo to load
        await new Promise((resolve) => {
            logoImg.onload = resolve;
            logoImg.onerror = () => {
                // Placeholder if logo fails
                doc.setFillColor(...amber);
                doc.circle(25, 20, 10, "F");
                doc.setTextColor(...black);
                doc.setFontSize(14);
                doc.text("QS", 20, 22);
                resolve();
            };
        });

        doc.addImage(logoImg, 'JPEG', 14, 10, 20, 20);

        // Date at top right
        doc.setFontSize(8);
        doc.text(dayjs().format("MMM DD, YYYY hh:mm A"), 160, 15);

        // LEGAL button below the date
        doc.setFillColor(...darkGreen);
        doc.roundedRect(170, 20, 30, 10, 5, 5, "F");
        doc.setTextColor(255, 255, 255);
        doc.text("LEGAL", 178, 26);

        // Centrally oriented company name and postal address immediately above the title
        doc.setFontSize(12);
        doc.setTextColor(...black);
        let companyText = "Qalibrated Systems Limited";
        let textWidth = doc.getTextWidth(companyText);
        let x = (doc.internal.pageSize.getWidth() - textWidth) / 2;
        doc.text(companyText, x, 40);

        doc.setFontSize(10);
        let addressText = "PO BOX 34463-00100 TEL: 0714999996, Nairobi";
        textWidth = doc.getTextWidth(addressText);
        x = (doc.internal.pageSize.getWidth() - textWidth) / 2;
        doc.text(addressText, x, 47);

        // Main title with green color and modern underline
        doc.setFontSize(18);
        doc.setTextColor(...green);
        doc.text("WEIGHING TICKET", 60, 60);
        doc.setDrawColor(...amber);
        doc.setLineWidth(0.5);
        doc.line(60, 62, 150, 62); // Thin underline

        // Ticket Details section with rounded rect
        let currentY = 70;
        doc.setFillColor(...amber);
        doc.roundedRect(14, currentY, 180, 8, 3, 3, "F");
        doc.setTextColor(...black);
        doc.setFontSize(12);
        doc.text("TICKET DETAILS", 80, currentY + 6);

        // Ticket Details table with modern styling (paired label-value)
        currentY += 10;
        const ticketDetails = [];
        if (record.receiptNo) ticketDetails.push(["Ticket No", record.receiptNo, "Registration", record.noPlate || 'N/A']);
        if (record.axleType) ticketDetails.push(["Axle Type", record.axleType, "Commodity", record.commodityName || 'N/A']);
        if (record.transporterName) ticketDetails.push(["Transporter", record.transporterName, "Timestamp", dayjs(record.timestamp || record.createdAt).format("DD-MM-YY hh:mm A") || 'N/A']);
        if (record.originName) ticketDetails.push(["Source", record.originName, "Destination", record.destinationName || 'N/A']);
        if (record.operatorName) ticketDetails.push(["Operator", record.operator, "Driver", record.driverName || 'N/A']);
        ticketDetails.push(["Weighing Status", `${record.completedWeighings || 0} / ${record.expectedWeighings || 2}`, "Current State", record.status || 'N/A']);
        if (record.supplierName || record.supplierId) ticketDetails.push(["Supplier", record.supplierName || record.supplierId, "Customer", record.customerName || record.customerId || 'N/A']);
        if (record.originName || record.originId) ticketDetails.push(["Origin", record.originName || record.originId, "Destination", record.destinationName || record.destinationId || 'N/A']);
        if (record.transporterName || record.transporterId) ticketDetails.push(["Transporter", record.transporterName || record.transporterId, "Weigh Mode", record.weighMode || 'N/A']);
        autoTable(doc, {
            startY: currentY,
            body: ticketDetails,
            theme: "plain",
            styles: { fillColor: lightAmber, textColor: black, fontSize: 10, lineWidth: 0.1, lineColor: amber },
            columnStyles: {
                0: { cellWidth: 40, fontStyle: 'bold' },
                1: { cellWidth: 50 },
                2: { cellWidth: 40, fontStyle: 'bold' },
                3: { cellWidth: 50 },
            },
            margin: { left: 14, right: 14 },
        });

        // Axle Weight Analysis section
        let lastY = doc.lastAutoTable.finalY + 5; // Reduced space for flow
        doc.setFillColor(...amber);
        doc.roundedRect(14, lastY, 180, 8, 3, 3, "F");
        doc.setTextColor(...black);
        doc.text("AXLE WEIGHT ANALYSIS", 70, lastY + 6);
        lastY += 10;

        // Axle data with modern highlights
        const axleData = record.axleWeights || {
            actual: { group1: 1400, group2: 0, group3: 0, group4: 0, gwv: "1400 KG" },
            pdf: { group1: 0.00, group2: 0, group3: 0, group4: 0, gwv: "N/A" },
            allowed: { group1: 8000, group2: 10000, group3: 0, group4: 0, gwv: "18000 KG" },
            allowed5: { group1: 8400, group2: 10500, group3: 0, group4: 0, gwv: "18000 KG" },
            excess: { group1: 0, group2: 0, group3: 0, group4: 0, gwv: "0 KG" },
            result: ["Legal", "Legal", "Legal", "Legal", "Legal"],
        };
        const axleHeaders = [["Items", "Group 1", "Group 2", "Group 3", "Group 4", "GWV"]];
        const axleRows = [
            ["Actual WT", axleData.actual.group1, axleData.actual.group2, axleData.actual.group3, axleData.actual.group4, axleData.actual.gwv],
            ["PDF", axleData.pdf.group1, axleData.pdf.group2, axleData.pdf.group3, axleData.pdf.group4, axleData.pdf.gwv],
            ["Allowed", axleData.allowed.group1, axleData.allowed.group2, axleData.allowed.group3, axleData.allowed.group4, axleData.allowed.gwv],
            ["Allowed-5%", axleData.allowed5.group1, axleData.allowed5.group2, axleData.allowed5.group3, axleData.allowed5.group4, axleData.allowed5.gwv],
            ["Excess", axleData.excess.group1, axleData.excess.group2, axleData.excess.group3, axleData.excess.group4, axleData.excess.gwv],
            ["Result", ...axleData.result],
        ];
        autoTable(doc, {
            head: axleHeaders,
            body: axleRows,
            startY: lastY,
            theme: "grid",
            headStyles: { fillColor: amber, textColor: black, lineWidth: 0.1, lineColor: black },
            bodyStyles: { fillColor: lightAmber, textColor: black, lineWidth: 0.1, lineColor: amber },
            didParseCell: (data) => {
                if (data.row.index === 5 && data.column.index > 0 && data.cell.text[0] === 'Legal') {
                    data.cell.styles.fillColor = green;
                    data.cell.styles.textColor = [255, 255, 255];
                } else if (data.row.index === 4 && data.column.index > 0 && parseFloat(data.cell.text[0]) > 0) {
                    data.cell.styles.fillColor = [244, 67, 54]; // red for excess
                    data.cell.styles.textColor = [255, 255, 255];
                }
            },
        });

        // Weights summary with rounded green boxes
        lastY = doc.lastAutoTable.finalY + 5; // Reduced space for flow
        doc.setFillColor(...green);
        doc.roundedRect(14, lastY, 60, 10, 3, 3, "F");
        doc.roundedRect(74, lastY, 60, 10, 3, 3, "F");
        doc.roundedRect(134, lastY, 60, 10, 3, 3, "F");
        doc.setFontSize(10);
        doc.setTextColor(255, 255, 255);
        doc.text("First Weight", 20, lastY + 7);
        doc.text("Second Weight", 80, lastY + 7);
        doc.text("Net Weight", 140, lastY + 7);
        lastY += 10;
        doc.setFillColor(...lightAmber);
        doc.roundedRect(14, lastY, 60, 10, 3, 3, "F");
        doc.roundedRect(74, lastY, 60, 10, 3, 3, "F");
        doc.roundedRect(134, lastY, 60, 10, 3, 3, "F");
        doc.setTextColor(...black);
        doc.text(`${record.firstWeight || 'N/A'} Kg`, 20, lastY + 7);
        doc.text(`${record.secondWeight || 'N/A'} Kg`, 80, lastY + 7);
        doc.text(`${record.netWeight || 'N/A'} Kg`, 140, lastY + 7);

        // Vehicle Snapshot section with border
        lastY += 15; // Slight space for flow
        doc.setFillColor(...amber);
        doc.roundedRect(14, lastY, 180, 8, 3, 3, "F");
        doc.setTextColor(...black);
        doc.text("VEHICLE SNAPSHOT", 80, lastY + 6);
        lastY += 10;

        // Function to add footer and save
        const addFooterAndSave = () => {
            // Footer
            lastY += 10; // Adjust for footer flow
            doc.setFillColor(...amber);
            doc.roundedRect(14, lastY, 180, 12, 3, 3, "F");
            doc.setFontSize(8);
            doc.setTextColor(...black);

            // Logo in footer if loaded
            doc.addImage(logoImg, 'JPEG', 20, lastY + 2, 8, 8);
            doc.text("Powered by Qalibrated Systems | www.qalibrated.co.ke | Inventing and Making Happen", 30, lastY + 8);

            doc.save(`ticket-${record.receiptNo}.pdf`);
        };

        if (record.vehicleSnapshotUrl) {
            const img = new Image();
            img.src = record.vehicleSnapshotUrl;
            img.crossOrigin = "anonymous";
            await new Promise((resolve) => {
                img.onload = resolve;
                img.onerror = () => {
                    // Placeholder for snapshot
                    doc.setFillColor(...lightAmber);
                    doc.roundedRect(14, lastY, 180, 100, 3, 3, "F");
                    doc.setTextColor(...black);
                    doc.text("Snapshot Placeholder", 80, lastY + 50);
                    lastY += 20;
                    resolve();
                };
            });
            if (img.complete) {
                // Border and image
                doc.setDrawColor(...amber);
                doc.setLineWidth(0.5);
                doc.roundedRect(14, lastY, 180, 100, 3, 3, "D");
                doc.addImage(img, "JPEG", 14, lastY, 180, 100);
                doc.setFontSize(8);
                doc.text(`Captured: ${dayjs(record.capturedTime).format("DD/MM/YYYY HH:mm:ss") || 'N/A'}`, 14, lastY + 102);
                doc.text(`Trigger Source: ${record.triggerSource || 'N/A'}`, 14, lastY + 108);
                lastY += 120;
            } else {
                lastY += 20;
            }
        } else {
            // Placeholder for no snapshot
            doc.setFillColor(...lightAmber);
            doc.roundedRect(14, lastY, 180, 50, 3, 3, "F");
            doc.setTextColor(...black);
            doc.text("No Snapshot Available", 80, lastY + 25);
            lastY += 60;
        }

        addFooterAndSave();

        message.success("Ticket generated successfully");
    };

    const openViewModal = async (record) => {
        try {
            const response = await fetch(`/Transaction?ReceiptNo=${record.receiptNo}`);
            const fullRecord = await response.json();
            if (fullRecord.operatorId) {
                const operator = await dispatch(fetchUserById(fullRecord.operatorId)).unwrap();
                fullRecord.operatorName = operator?.name || operator?.fullName || 'N/A';
            } else {
                // If no operatorId, fetch current user as fallback
                const currentUser = await dispatch(fetchCurrentUser()).unwrap();
                fullRecord.operatorId = currentUser?.id || 'N/A';
                fullRecord.operatorName = currentUser?.name || currentUser?.fullName || 'N/A';
            }
            setSelectedRecord(fullRecord);
        } catch (error) {
            message.error("Failed to fetch full transaction details");
            setSelectedRecord(record);
        }
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
        <div className="p-2 bg-gray-50 min-h-screen">
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
                            window.scrollTo({ top: 0, behavior: 'smooth' });
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
                        <Descriptions.Item label="Receipt Number">{selectedRecord.receiptNo || 'N/A'}</Descriptions.Item>
                        <Descriptions.Item label="Vehicle Plate">{selectedRecord.noPlate || 'N/A'}</Descriptions.Item>
                        <Descriptions.Item label="Driver Name">{selectedRecord.driverName || 'N/A'}</Descriptions.Item>
                        <Descriptions.Item label="Commodity">{selectedRecord.commodityName || 'N/A'}</Descriptions.Item>
                        <Descriptions.Item label="Weighing Progress">
                            {selectedRecord.completedWeighings || '0'} of {selectedRecord.expectedWeighings || '2'}
                        </Descriptions.Item>
                        <Descriptions.Item label="Current Status">
                            <Tag color={selectedRecord.status === "Completed" ? "green" : "gold"}>
                                {selectedRecord.status || 'N/A'}
                            </Tag>
                        </Descriptions.Item>
                        <Descriptions.Item label="Date Created">
                            {dayjs(selectedRecord.createdAt).format("DD MMMM YYYY, HH:mm") || 'N/A'}
                        </Descriptions.Item>
                        <Descriptions.Item label="Transporter">{selectedRecord.transporterName || 'N/A'}</Descriptions.Item>
                        <Descriptions.Item label="Source">{selectedRecord.originName || 'N/A'}</Descriptions.Item>
                        <Descriptions.Item label="Destination">{selectedRecord.destinationName|| 'N/A'}</Descriptions.Item>
                        <Descriptions.Item label="Operator">{selectedRecord.operatorName || 'N/A'}</Descriptions.Item>
                        <Descriptions.Item label="First Weight">{selectedRecord.firstWeight || 'N/A'} Kg</Descriptions.Item>
                        <Descriptions.Item label="Second Weight">{selectedRecord.secondWeight || 'N/A'} Kg</Descriptions.Item>
                        <Descriptions.Item label="Net Weight">{selectedRecord.netWeight || 'N/A'} Kg</Descriptions.Item>
                        <Descriptions.Item label="Captured Time">
                            {dayjs(selectedRecord.capturedTime).format("DD MMMM YYYY, HH:mm:ss") || 'N/A'}
                        </Descriptions.Item>
                        <Descriptions.Item label="Vehicle Snapshot">
                            {selectedRecord.vehicleSnapshotUrl ? (
                                <img src={selectedRecord.vehicleSnapshotUrl} alt="Snapshot" style={{ width: "100%" }} />
                            ) : 'N/A'}
                        </Descriptions.Item>
                        <Descriptions.Item label="Supplier">{selectedRecord.supplierName || record.supplierId || 'N/A'}</Descriptions.Item>
                        <Descriptions.Item label="Customer">{selectedRecord.customerName || record.customerId || 'N/A'}</Descriptions.Item>
                        <Descriptions.Item label="Is Completed">{selectedRecord.isCompleted ? 'Yes' : 'No'}</Descriptions.Item>
                        <Descriptions.Item label="Weigh Mode">{selectedRecord.weighMode || 'N/A'}</Descriptions.Item>
                    </Descriptions>
                )}
            </Modal>
        </div>
    );
}