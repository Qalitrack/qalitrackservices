import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Tag, Button, Typography, Input } from "antd";
import { ReloadOutlined, PlusCircleOutlined, SearchOutlined } from "@ant-design/icons";
import { fetchTransactions } from "../../store/weighingSlice";

const { Text } = Typography;

export default function IncompleteTransactionsTable({ onAddWeighing, refreshKey }) {
    const dispatch = useDispatch();
    const { transactions, loading } = useSelector((state) => state.weighing);
    const [searchText, setSearchText] = useState("");
    const [filteredData, setFilteredData] = useState([]);

    useEffect(() => {
        dispatch(fetchTransactions({ isCompleted: false, pageSize: 50 }));
    }, [dispatch, refreshKey]);

    useEffect(() => {
        const result = transactions
            .filter((t) => !t.isCompleted)
            .filter((t) => {
                const search = searchText.toLowerCase();
                return (
                    t.receiptNo?.toLowerCase().includes(search) ||
                    t.noPlate?.toLowerCase().includes(search) ||
                    t.driverName?.toLowerCase().includes(search)
                );
            })
            .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
        setFilteredData(result);
    }, [transactions, searchText]);

    const handleSelectRow = (record) => {
        onAddWeighing({
            ...record,
            firstWeight: record.firstWeight?.toString() || "",
            secondWeight: "", 
            expectedWeighings: record.expectedWeighings || 2
        });
    };

    const columns = [
        {
            title: "RECEIPT",
            dataIndex: "receiptNo",
            key: "receiptNo",
            width: 100,
            render: (text) => <Text strong className="text-[11px] text-amber-700">{text}</Text>,
        },
        {
            title: "VEHICLE",
            dataIndex: "noPlate",
            key: "noPlate",
            width: 110,
            render: (text) => <Tag className="font-mono font-bold text-[10px] m-0">{text}</Tag>,
        },
        {
            title: "LOGISTICS / COMMODITY",
            key: "logistics",
            render: (_, record) => (
                <div className="flex flex-col leading-tight">
                    <Text className="text-[10px] font-bold uppercase truncate max-w-[120px]">{record.driverName}</Text>
                    <Text type="secondary" className="text-[9px] truncate max-w-[150px]">
                        {record.commodityName || "No Product"} • {record.transporterName || "Unknown"}
                    </Text>
                </div>
            ),
        },
        {
            title: "WAITING",
            key: "status",
            width: 80,
            render: (_, record) => (
                <Tag color="blue" className="text-[9px] m-0 px-1 font-bold">
                    WT {(record.completedWeighings || 1) + 1}
                </Tag>
            ),
        },
        {
            title: "ACTION",
            key: "action",
            align: "right",
            width: 110,
            render: (_, record) => (
                <Button 
                    type="primary" 
                    size="small" 
                    icon={<PlusCircleOutlined style={{ fontSize: '10px' }} />}
                    onClick={() => handleSelectRow(record)} 
                    className="bg-amber-500 hover:bg-amber-600 border-none text-[10px] font-bold h-7 px-2"
                >
                    FINISH
                </Button>
            ),
        },
    ];

    return (
        <div className="h-full flex flex-col overflow-hidden">
            {/* Minimal Header */}
            <div className="px-2 py-1.5 bg-gray-50 border-b flex justify-between items-center shrink-0">
                <Input
                    placeholder="Search queue..."
                    prefix={<SearchOutlined className="text-gray-400 text-xs" />}
                    className="w-40 rounded-md h-7 text-[10px]"
                    value={searchText}
                    onChange={(e) => setSearchText(e.target.value)}
                    size="small"
                    allowClear
                />
                <Button 
                    size="small"
                    icon={<ReloadOutlined style={{ fontSize: '10px' }} />} 
                    onClick={() => dispatch(fetchTransactions({ isCompleted: false, pageSize: 50 }))}
                    className="h-7 w-7"
                />
            </div>
            
            <div className="flex-1">
                <Table
                    columns={columns}
                    dataSource={filteredData}
                    rowKey="id"
                    loading={loading}
                    size="small"
                    // Fixed to show only 2 transactions per page
                    pagination={{
                        pageSize: 2,
                        size: "small",
                        showSizeChanger: false,
                        className: "m-0 py-1.5 justify-center border-t",
                    }}
                    // Force the table body to a specific height to fit 2 rows exactly
                    scroll={{ y: 110 }} 
                    className="compact-queue-table"
                    locale={{ emptyText: <span className="text-[10px]">Yard Empty</span> }}
                />
            </div>

            <style jsx>{`
                .compact-queue-table :global(.ant-table-thead > tr > th) {
                    font-size: 10px;
                    padding: 4px 10px !important;
                    background: #fafafa;
                }
                .compact-queue-table :global(.ant-table-tbody > tr > td) {
                    padding: 4px 10px !important;
                    height: 50px; /* Fixed row height to ensure 2 fit */
                }
                /* Hide pagination if there is only 1 page to save space */
                .compact-queue-table :global(.ant-pagination-disabled) {
                    display: none;
                }
            `}</style>
        </div>
    );
}