'use client';

import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useRouter } from 'next/navigation';
import DataTable from './DataTable';
import ProductFormModal from './ProductFormModal';
import ConfirmationModal from './ConfirmationModal';
import config from '@/utils/config';

const ProductManagement = () => {
  const router = useRouter();
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  
  const [showConfirm, setShowConfirm] = useState(false);
  const [confirmMessage, setConfirmMessage] = useState('');
  const [confirmAction, setConfirmAction] = useState(null);
  const [showProductModal, setShowProductModal] = useState(false);
  const [editItem, setEditItem] = useState(null);

  const fetchData = React.useCallback(async () => {
    setLoading(true);
    setError('');
    
    const token = localStorage.getItem('authToken');
    if (!token) {
      router.push('/login');
      return;
    }

    try {
      const response = await axios.get(`${config.API_BASE_URL}/products`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      setProducts(response.data);
    } catch (err) {
      console.error('Error fetching products:', err);
      if (err.response?.status === 401 || err.response?.status === 403) {
        localStorage.removeItem('authToken');
        router.push('/login');
      } else {
        setError('Failed to load products. Please check server connection.');
      }
    } finally {
      setLoading(false);
    }
  }, [router]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleDelete = (id) => {
    setConfirmMessage('Are you sure you want to delete this product?');
    setConfirmAction(() => async () => {
      const token = localStorage.getItem('authToken');
      try {
        await axios.delete(`${config.API_BASE_URL}/products/${id}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        fetchData();
      } catch {
        setError('Failed to delete product.');
      } finally {
        setShowConfirm(false);
      }
    });
    setShowConfirm(true);
  };

  const handleSave = async (data, isEdit) => {
    const token = localStorage.getItem('authToken');
    const url = `${config.API_BASE_URL}/products${isEdit ? `/${data._id}` : ''}`;
    try {
      isEdit
        ? await axios.put(url, data, {
            headers: { Authorization: `Bearer ${token}` },
          })
        : await axios.post(url, data, {
            headers: { Authorization: `Bearer ${token}` },
          });
      fetchData();
    } catch {
      setError(`Failed to ${isEdit ? 'update' : 'create'} product`);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem('authToken');
    router.push('/login');
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold">Products Dashboard</h1>
          <p className="text-gray-600">Manage your inventory</p>
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

      <DataTable
        type="products"
        data={products}
        loading={loading}
        onCreate={() => setShowProductModal(true)}
        onEdit={(item) => {
          setEditItem(item);
          setShowProductModal(true);
        }}
        onDelete={handleDelete}
      />

      <ProductFormModal
        open={showProductModal}
        onClose={() => {
          setShowProductModal(false);
          setEditItem(null);
        }}
        onSubmit={(data, isEdit) => handleSave(data, isEdit)}
        initialData={editItem}
      />

      <ConfirmationModal
        open={showConfirm}
        message={confirmMessage}
        onConfirm={confirmAction}
        onCancel={() => setShowConfirm(false)}
      />
    </div>
  );
};

export default ProductManagement;