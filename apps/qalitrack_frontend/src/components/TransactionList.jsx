import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchTransactions } from "../store/weighingSlice";
import { Table, Tag, Typography } from "antd";

const { Text } = Typography;

export default function TransactionList() {
  const dispatch = useDispatch();
  const { transactions = [], loading } = useSelector((state) => state.weighing);

  // Initial fetch on mount
  useEffect(() => {
    dispatch(fetchTransactions());
  }, [dispatch]);

  const columns = [
    {
      title: "Receipt No",
      dataIndex: "receiptNo",
      key: "receiptNo",
      render: (text) => <Text strong>{text}</Text>,
    },
    {
      title: "Plate",
      dataIndex: "noPlate",
      key: "noPlate",
    },
    {
      title: "Gross (W1)",
      dataIndex: "firstWeight",
      key: "firstWeight",
      render: (w) => w ? `${w} kg` : "-",
    },
    {
      title: "Tare (W2)",
      dataIndex: "secondWeight",
      key: "secondWeight",
      render: (w) => (w && w > 0) ? `${w} kg` : "-",
    },
    {
      title: "Net",
      key: "net",
      render: (_, record) => {
        const w1 = Number(record.firstWeight) || 0;
        const w2 = Number(record.secondWeight) || 0;
        const net = (w1 > 0 && w2 > 0) ? Math.abs(w1 - w2) : null;
            
        return net ? <Tag color="green">{net} kg</Tag> : <Tag>Pending</Tag>;
      },
    },
    {
      title: "Status",
      dataIndex: "isCompleted",
      key: "status",
      render: (completed) => (
        <Tag color={completed ? "success" : "warning"}>
          {completed ? "Completed" : "Pending Tare"}
        </Tag>
      ),
    },
    {
      title: "Date",
      dataIndex: "createdAt",
      key: "date",
      render: (date) => date ? new Date(date).toLocaleString() : '-',
    },
  ];

  return (
    <div className="p-6 max-w-7xl mx-auto">
      <h2 className="text-2xl font-bold mb-6 text-amber-700">Recent Transactions</h2>
      <Table
        dataSource={transactions}
        columns={columns}
        rowKey="id"
        loading={loading}
        pagination={{ pageSize: 10 }}
        scroll={{ x: 800 }}
      />
    </div>
  );
}