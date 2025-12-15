import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Tag, Button } from "antd";
import { fetchTransactions } from "../../store/weighingSlice";

export default function IncompleteTransactionsTable({ onAddWeighing, refreshKey }) {
    const dispatch = useDispatch();
    const { transactions, loading } = useSelector((state) => state.weighing);
    const [localTransactions, setLocalTransactions] = useState([]);

    useEffect(() => {
        dispatch(fetchTransactions({ isCompleted: false, pageSize: 50 }));
    }, [dispatch, refreshKey]);

    useEffect(() => {
        const incomplete = transactions.filter((t) => !t.isCompleted);
        setLocalTransactions(incomplete);
    }, [transactions]);

    const columns = [
        { title: "Receipt No", dataIndex: "receiptNo", key: "receiptNo" },
        { title: "Vehicle", dataIndex: "noPlate", key: "noPlate" },
        { title: "Driver", dataIndex: "driverName", key: "driverName" },
        { title: "Commodity", dataIndex: "commodityName", key: "commodityName" },
        {
            title: "Weighings",
            key: "weighings",
            render: (_, record) => (
                <span>
          {record.completedWeighings || 0} / {record.expectedWeighings || 2}
        </span>
            ),
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
                <Button type="primary" size="small" onClick={() => onAddWeighing(record)} disabled={record.isCompleted}>
                    Add Weighing
                </Button>
            ),
        },
    ];

    return (
        <div>
            <div className="flex justify-between items-center mb-4">
                <h3 className="text-lg font-semibold">Incomplete Transactions</h3>
                <Button onClick={() => dispatch(fetchTransactions({ isCompleted: false, pageSize: 50 }))}>Refresh</Button>
            </div>
            <Table
                columns={columns}
                dataSource={localTransactions}
                rowKey="id"
                loading={loading}
                pagination={{ pageSize: 10 }}
            />
        </div>
    );
}