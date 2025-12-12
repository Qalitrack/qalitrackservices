// src/pages/Dashboard.jsx
import React from 'react';
import { Card, Col, Row, Statistic, Divider, Select, Button } from 'antd';
import { TruckOutlined, CheckCircleOutlined, WarningOutlined, ClockCircleOutlined, DownloadOutlined } from '@ant-design/icons';
import { Line, Bar, Pie } from 'react-chartjs-2'; // Mock import for charting library

const { Option } = Select;

// --- Mock Data ---
const kpiData = [
  { title: "Today's Volume", value: "348", suffix: "Trucks", icon: <TruckOutlined />, color: "text-green-700" },
  { title: "Avg. Net Weight", value: "32.5", suffix: "Tonnes", icon: <CheckCircleOutlined />, color: "text-blue-700" },
  { title: "Tare Discrepancies", value: "4", suffix: "Alerts", icon: <WarningOutlined />, color: "text-red-600" },
  { title: "Avg. Turnaround Time", value: "12:30", suffix: "min", icon: <ClockCircleOutlined />, color: "text-gray-700" },
];

const throughputChartData = {
  labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
  datasets: [
    {
      label: 'Weighments Completed',
      data: [650, 590, 800, 810, 560, 550, 400],
      backgroundColor: 'rgba(5, 150, 105, 0.6)', // Tailwind green-700
      borderColor: 'rgba(5, 150, 105, 1)',
      borderWidth: 1,
    },
  ],
};

const productMixPieData = {
  labels: ['Sand', 'Aggregates (Type A)', 'Crushed Rock', 'Clay'],
  datasets: [
    {
      label: 'Product Mix (Tonnes)',
      data: [45, 30, 15, 10],
      backgroundColor: [
        '#059669', // green-600
        '#1D4ED8', // blue-700
        '#9CA3AF', // gray-400
        '#CA8A04', // amber-600
      ],
    },
  ],
};

const avgTurnaroundLineData = {
  labels: ['7am', '9am', '11am', '1pm', '3pm', '5pm'],
  datasets: [
    {
      label: 'Average Time (minutes)',
      data: [15, 12, 9, 11, 14, 18],
      fill: true,
      backgroundColor: 'rgba(29, 78, 216, 0.1)', // blue-700 light
      borderColor: '#1D4ED8',
      tension: 0.4,
    },
  ],
};

// --- Main Component ---
export default function Dashboard() {
  return (
    <div className="min-h-screen bg-gray-100 p-6">
      
      {/* HEADER AND FILTERS */}
      <Row justify="space-between" align="middle" className="mb-6">
        <h1 className="text-3xl font-bold text-gray-800">Weighbridge Analytics Dashboard</h1>
        <div className="flex space-x-4">
          <Select defaultValue="week" style={{ width: 120 }}>
            <Option value="day">Today</Option>
            <Option value="week">Last Week</Option>
            <Option value="month">Last Month</Option>
          </Select>
          <Button type="default" icon={<DownloadOutlined />}>
            Export Report (CSV)
          </Button>
        </div>
      </Row>

      {/* 1. KPI BAR */}
      <Row gutter={[24, 24]} className="mb-8">
        {kpiData.map((kpi, index) => (
          <Col span={6} key={index}>
            <Card className="shadow-lg border-t-4 border-green-700">
              <Statistic
                title={kpi.title}
                value={kpi.value}
                suffix={kpi.suffix}
                prefix={<span className={`text-3xl mr-2 ${kpi.color}`}>{kpi.icon}</span>}
                valueStyle={{ color: kpi.color }}
              />
            </Card>
          </Col>
        ))}
      </Row>

      {/* 2. CHARTS SECTION (BAR & PIE) */}
      <Row gutter={[24, 24]} className="mb-8">
        
        {/* Weekly Throughput (Bar Chart) */}
        <Col span={16}>
          <Card 
            title={<span className="text-xl font-semibold text-gray-700">Weekly Transaction Throughput (Units)</span>} 
            className="shadow-lg h-96"
          >
            <Bar data={throughputChartData} options={{ responsive: true, maintainAspectRatio: false }} />
          </Card>
        </Col>

        {/* Product Mix (Pie Chart) */}
        <Col span={8}>
          <Card 
            title={<span className="text-xl font-semibold text-gray-700">Product Weighment Mix (%)</span>} 
            className="shadow-lg h-96"
          >
            <div className="h-full flex items-center justify-center">
              <div className="w-4/5 h-4/5">
                <Pie data={productMixPieData} options={{ responsive: true, maintainAspectRatio: false }} />
              </div>
            </div>
          </Card>
        </Col>
      </Row>

      {/* 3. REPORTS/DETAILED ANALYTICS (Line Graph & Report Table Placeholder) */}
      <Row gutter={[24, 24]}>
        
        {/* Avg Turnaround Time (Line Graph) */}
        <Col span={12}>
          <Card 
            title={<span className="text-xl font-semibold text-gray-700">Vehicle Turnaround Time (Hourly)</span>} 
            className="shadow-lg h-96"
          >
            <Line data={avgTurnaroundLineData} options={{ responsive: true, maintainAspectRatio: false }} />
          </Card>
        </Col>
        
        {/* Compliance and Exceptions Report Placeholder */}
        <Col span={12}>
          <Card 
            title={<span className="text-xl font-semibold text-gray-700">Compliance & Exception Report</span>} 
            className="shadow-lg h-96"
          >
            <Divider className="my-2" />
            <p className="text-lg font-medium text-red-600 mb-2">Top 3 Tare Discrepancy Alerts (Last 7 Days)</p>
            <ul className="list-disc list-inside space-y-1 ml-4 text-gray-700">
                <li>Truck **KCA 001A**: Tare **+850kg** deviation. (10:30 AM, 11/28)</li>
                <li>Truck **KDA 777X**: Tare **-500kg** deviation. (09:15 AM, 11/27)</li>
                <li>Truck **KCF 321B**: **System Offline** during weighing. (02:00 PM, 11/26)</li>
            </ul>
            <Button type="link" className="mt-4 text-green-700">View Full Compliance Log</Button>

          </Card>
        </Col>
      </Row>
    </div>
  );
}