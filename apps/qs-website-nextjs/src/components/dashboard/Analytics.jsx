'use client';

import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useRouter } from 'next/navigation';
import AnalyticsSection from './AnalyticsModal';
import DashboardHeader from './DashboardHeader';
import config from '@/utils/config';

const Analytics = () => {
  const router = useRouter();
  const [products, setProducts] = useState([]);
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const fetchData = React.useCallback(async () => {
    setLoading(true);
    setError('');
    
    const token = localStorage.getItem('authToken');
    if (!token) {
      router.push('/login');
      return;
    }

    try {
      const [prodRes, userRes] = await Promise.all([
        axios.get(`${config.API_BASE_URL}/products`, {
          headers: { Authorization: `Bearer ${token}` },
        }),
        axios.get(`${config.API_BASE_URL}/users`, {
          headers: { Authorization: `Bearer ${token}` },
        }),
      ]);
      setProducts(prodRes.data);
      setUsers(userRes.data);
    } catch (err) {
      console.error('Error fetching data:', err);
      if (err.response?.status === 401 || err.response?.status === 403) {
        localStorage.removeItem('authToken');
        router.push('/login');
      } else {
        setError('Failed to load data. Please check server connection.');
      }
    } finally {
      setLoading(false);
    }
  }, [router]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleLogout = () => {
    localStorage.removeItem('authToken');
    router.push('/login');
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <h1 className="text-2xl font-bold">Analytics Overview</h1>
          <button
            onClick={handleLogout}
            className="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600"
          >
            Logout
          </button>
        </div>
        <div className="text-center py-8">Loading...</div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold">Analytics Overview</h1>
          <p className="text-gray-600">Quick insights on products & users</p>
        </div>
        <button
          onClick={handleLogout}
          className="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600"
        >
          Logout
        </button>
      </div>
      
      {error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
          {error}
        </div>
      )}
      
      <AnalyticsSection products={products} users={users} />
    </div>
  );
};

export default Analytics;