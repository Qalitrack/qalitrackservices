import React, { useEffect, useMemo, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Tag, Button, Input, Typography, Space, Pagination } from "antd";
import { ReloadOutlined, SearchOutlined } from "@ant-design/icons";
import { fetchTransactions } from "../../store/weighingSlice";

const { Text } = Typography;

const PAGE_SIZE = 2; // Only show 2 transactions visible at once

export default function IncompleteTransactionsTable({ onAddWeighing, refreshKey }) {
  const dispatch = useDispatch();
  const { transactions = [], loading } = useSelector((state) => state.weighing);

  const [searchText, setSearchText] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  // Fetch incomplete transactions
  useEffect(() => {
    dispatch(fetchTransactions({ isCompleted: false, pageSize: 50 }));
  }, [dispatch, refreshKey]);

  // Filter by search text
  const filteredData = useMemo(() => {
    const lowerSearch = searchText.toLowerCase();

    return transactions
      .filter((t) => {
        if (!searchText) return true;
        return (
          t.receiptNo?.toLowerCase().includes(lowerSearch) ||
          t.noPlate?.toLowerCase().includes(lowerSearch) ||
          t.driverName?.toLowerCase().includes(lowerSearch) ||
          t.commodityName?.toLowerCase().includes(lowerSearch) ||
          t.transporterName?.toLowerCase().includes(lowerSearch)
        );
      })
      .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt)); // Newest first
  }, [transactions, searchText]);

  // Paginate: only 2 rows
  const paginatedData = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE;
    return filteredData.slice(start, start + PAGE_SIZE);
  }, [filteredData, currentPage]);

  const handleSelectRow = (record) => {
    onAddWeighing({
      ...record,
      firstWeight: record.firstWeight?.toString() || "",
      secondWeight: "",
      expectedWeighings: record.expectedWeighings || 2,
    });
  };

  const columns = [
    {
      title: "RECEIPT",
      dataIndex: "receiptNo",
      width: 105,
      render: (text) => (
        <Text strong className="text-[11px] font-mono text-amber-700">
          {text}
        </Text>
      ),
    },
    {
      title: "VEHICLE",
      dataIndex: "noPlate",
      width: 95,
      render: (text) => (
        <Tag className="font-mono font-black text-[11px] bg-black text-white border-0 m-0">
          {text}
        </Tag>
      ),
    },
    {
      title: "DRIVER",
      dataIndex: "driverName",
      width: 120,
      render: (text) => (
        <Text className="text-[10px] font-semibold text-gray-800 truncate block max-w-[110px]">
          {text || "Unknown Driver"}
        </Text>
      ),
    },
    {
      title: "COMMODITY",
      dataIndex: "commodityName",
      width: 110,
      render: (text) => (
        <Text type="secondary" className="text-[9px] truncate block max-w-[100px]">
          {text || "No Product"}
        </Text>
      ),
    },
    {
      title: "TRANSPORTER",
      dataIndex: "transporterName",
      width: 110,
      render: (text) => (
        <Text type="secondary" className="text-[9px] truncate block max-w-[100px]">
          {text || "Unknown Transporter"}
        </Text>
      ),
    },
    {
      title: "WAITING",
      width: 70,
      align: "center",
      render: (_, record) => {
        const nextWeigh = (record.completedWeighings || 0) + 1;
        return (
          <Tag color="amber" className="text-[9px] m-0 px-1 font-bold border-0">
            WT {nextWeigh}
          </Tag>
        );
      },
    },
    {
      title: "ACTION",
      align: "right",
      width: 100,
      render: (_, record) => (
        <Button
          type="primary"
          size="small"
          onClick={() => handleSelectRow(record)}
          className="bg-amber-500 hover:bg-amber-600 border-none text-white text-[10px] font-bold h-7 px-2 shadow-sm"
        >
          FINALIZE
        </Button>
      ),
    },
  ];

  return (
    <div className="h-full flex flex-col bg-white rounded-lg shadow-sm overflow-hidden">
      {/* Header */}
      <div className="px-3 py-2 bg-gray-50 border-b border-gray-200 flex justify-between items-center shrink-0">
        <Space size="middle">
          <Text className="text-[11px] font-black uppercase text-gray-700 tracking-wider">
            Active Yard Queue
          </Text>
          <Tag color="amber" className="font-bold text-[10px] border-0">
            {filteredData.length} waiting
          </Tag>
        </Space>
        <Space size="small">
          <Input
            placeholder="Search queue..."
            prefix={<SearchOutlined className="text-gray-400 text-xs" />}
            value={searchText}
            onChange={(e) => {
              setSearchText(e.target.value);
              setCurrentPage(1); // Reset pagination on search
            }}
            className="w-40 h-7 text-[10px] rounded"
            allowClear
          />
          <Button
            size="small"
            icon={<ReloadOutlined className="text-xs" />}
            onClick={() => dispatch(fetchTransactions({ isCompleted: false, pageSize: 50 }))}
            className="h-7 w-7 border-gray-300"
          />
        </Space>
      </div>

      {/* Table - Fixed height for ~2 rows */}
      <div className="flex-1 min-h-0">
        <Table
          rowKey="id"
          columns={columns}
          dataSource={paginatedData}
          loading={loading}
          size="small"
          pagination={false}
          scroll={{ y: 115 }} // Perfect for exactly 2 rows + header
          className="compact-queue-table"
          locale={{
            emptyText: (
              <div className="py-6 text-center">
                <Text className="text-gray-400 text-[12px] font-medium">
                  Yard Empty
                </Text>
              </div>
            ),
          }}
        />
      </div>

      {/* Pagination - Only shown if more than 2 */}
      {filteredData.length > PAGE_SIZE && (
        <div className="px-3 py-1.5 bg-gray-50 border-t border-gray-200 flex justify-center shrink-0">
          <Pagination
            current={currentPage}
            pageSize={PAGE_SIZE}
            total={filteredData.length}
            onChange={setCurrentPage}
            size="small"
            showSizeChanger={false}
            showQuickJumper={false}
            className="m-0"
          />
        </div>
      )}

      {/* Compact Styling */}
      <style jsx>{`
        .compact-queue-table :global(.ant-table-thead > tr > th) {
          background: #ffffff !important;
          font-size: 10px !important;
          font-weight: 800 !important;
          text-transform: uppercase;
          padding: 6px 8px !important;
          color: #4b5563 !important;
          border-bottom: 2px solid #f3f4f6 !important;
        }
        .compact-queue-table :global(.ant-table-tbody > tr > td) {
          padding: 8px 8px !important;
          border-bottom: 1px solid #f3f4f6;
        }
        .compact-queue-table :global(.ant-table-tbody > tr:hover > td) {
          background: #fffbeb !important;
        }
      `}</style>
    </div>
  );
}