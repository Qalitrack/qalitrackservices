'use client';

import React, { useState } from 'react';
import Sidebar from '@/components/dashboard/Sidebar';
import Analytics from '@/components/dashboard/Analytics';
import ProductManagement from '@/components/dashboard/ProductManagement';
import UserManagement from '@/components/dashboard/UserManagement';
import MyProfile from '@/components/dashboard/MyProfile';

export default function Dashboard() {
  const [selectedView, setSelectedView] = useState('analytics');

  const renderContent = () => {
    switch(selectedView) {
      case 'analytics':
        return <Analytics />;
      case 'products':
        return <ProductManagement />;
      case 'users':
        return <UserManagement />;
      case 'profile':
        return <MyProfile userId="12345" />;
      default:
        return <Analytics />;
    }
  };

  return (
    <div className="min-h-screen bg-gray-100 flex flex-col md:flex-row">
      <Sidebar selectedView={selectedView} setSelectedView={setSelectedView} />
      <main className="flex-1 p-6">
        {renderContent()}
      </main>
    </div>
  );
}