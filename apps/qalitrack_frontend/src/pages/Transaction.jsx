import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Tag, Button, Input, DatePicker, Select, Space, message } from "antd";
import dayjs from "dayjs";
import { fetchTransactions } from "../store/weighingSlice";
import { Printer } from "lucide-react";
import jsPDF from "jspdf"; // for printing tickets
import autoTable from "jspdf-autotable"; // table printing

const { RangePicker } = DatePicker;
const { Option } = Select;

export default function Transactions() {
    const dispatch = useDispatch();
    const { transactions, loading, total } = useSelector((state) => state.weighing);

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
            startDate: filters.dateRange ? filters.dateRange[0].format("YYYY-MM-DD") : undefined,
            endDate: filters.dateRange ? filters.dateRange[1].format("YYYY-MM-DD") : undefined,
            pageNumber: filters.page,
            pageSize: filters.pageSize,
        };
        dispatch(fetchTransactions(params));
    };

    const handlePrint = (record) => {
        const doc = new jsPDF();
        doc.setFontSize(14);
        doc.text("Weighbridge Ticket", 14, 20);

        const rows = [
            ["Receipt No", record.receiptNo],
            ["Vehicle", record.noPlate],
            ["Driver", record.driverName],
            ["Commodity", record.commodityName],
            ["Weighings", `${record.completedWeighings || 0} / ${record.expectedWeighings || 2}`],
            ["Status", record.status],
            ["Date", dayjs(record.createdAt).format("DD/MM/YYYY HH:mm")],
        ];

        autoTable(doc, {
            startY: 30,
            body: rows,
            theme: "grid",
        });

        doc.save(`ticket-${record.receiptNo}.pdf`);
        message.success("Ticket printed successfully");
    };

    const columns = [
        { title: "Receipt No", dataIndex: "receiptNo", key: "receiptNo", sorter: (a, b) => a.receiptNo.localeCompare(b.receiptNo) },
        { title: "Vehicle", dataIndex: "noPlate", key: "noPlate" },
        { title: "Driver", dataIndex: "driverName", key: "driverName" },
        { title: "Commodity", dataIndex: "commodityName", key: "commodityName" },
        {
            title: "Weighings",
            key: "weighings",
            render: (_, record) => <span>{record.completedWeighings || 0} / {record.expectedWeighings || 2}</span>,
        },
        {
            title: "Status",
            dataIndex: "status",
            key: "status",
            render: (status) => <Tag color={status === "Pending" ? "orange" : "blue"}>{status}</Tag>,
        },
        {
            title: "Action",
            key: "action",
            render: (_, record) => (
                <Space>
                    <Button
                        type="primary"
                        size="small"
                        icon={<Printer />}
                        disabled={record.completedWeighings === 0}
                        onClick={() => handlePrint(record)}
                    >
                        Print Ticket
                    </Button>
                </Space>
            ),
        },
    ];

    return (
        <div className="p-4">
            <h2 className="text-xl font-bold mb-4">Transactions</h2>

            {/* Filters */}
            <Space className="mb-4" wrap>
                <Input
                    placeholder="Search by Vehicle / Driver"
                    value={filters.search}
                    onChange={(e) => setFilters({ ...filters, search: e.target.value, page: 1 })}
                    style={{ width: 250 }}
                />
                <RangePicker
                    onChange={(dates) => setFilters({ ...filters, dateRange: dates, page: 1 })}
                    allowEmpty={[true, true]}
                />
                <Select
                    value={filters.pageSize}
                    onChange={(value) => setFilters({ ...filters, pageSize: value, page: 1 })}
                >
                    <Option value={5}>5 per page</Option>
                    <Option value={10}>10 per page</Option>
                    <Option value={20}>20 per page</Option>
                </Select>
                <Button onClick={loadTransactions}>Refresh</Button>
            </Space>

            <Table
                columns={columns}
                dataSource={transactions}
                rowKey="id"
                loading={loading}
                pagination={{
                    current: filters.page,
                    pageSize: filters.pageSize,
                    total: total,
                    onChange: (page, pageSize) => setFilters({ ...filters, page, pageSize }),
                }}
            />
        </div>
    );
}
