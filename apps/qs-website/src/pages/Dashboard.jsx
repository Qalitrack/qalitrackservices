import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

import Sidebar from "../components/dashboard/Sidebar";
import DashboardHeader from "../components/dashboard/DashboardHeader";
import AnalyticsSection from "../components/dashboard/AnalyticsModal";
import DataTable from "../components/dashboard/DataTable";
import ConfirmationModal from "../components/dashboard/ConfirmationModal";
import ProductFormModal from "../components/dashboard/ProductFormModal";
import UserFormModal from "../components/dashboard/UserFormModal";
import MyProfile from "../components/dashboard/MyProfile";

import { API_BASE_URL } from '../utils/config';

const Dashboard = () => {
  const navigate = useNavigate();
  const [selectedView, setSelectedView] = useState("analytics");
  const [products, setProducts] = useState([]);
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  // Modal states
  const [showConfirm, setShowConfirm] = useState(false);
  const [confirmMessage, setConfirmMessage] = useState("");
  const [confirmAction, setConfirmAction] = useState(null);

  const [showProductModal, setShowProductModal] = useState(false);
  const [showUserModal, setShowUserModal] = useState(false);
  const [editItem, setEditItem] = useState(null);

  const token = localStorage.getItem("authToken");

  // Redirect if not logged in
  useEffect(() => {
    if (!token) {
      navigate("/login");
    }
  }, [token, navigate]);

  // Fetch data from backend
  const fetchData = React.useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const [prodRes, userRes] = await Promise.all([
        axios.get(`${API_BASE_URL}/products`, {
          headers: { Authorization: `Bearer ${token}` },
        }),
        axios.get(`${API_BASE_URL}/users`, {
          headers: { Authorization: `Bearer ${token}` },
        }),
      ]);
      setProducts(prodRes.data);
      setUsers(userRes.data);
    } catch (err) {
      console.error("Error fetching data:", err);
      if (err.response?.status === 401 || err.response?.status === 403) {
        // Token invalid or expired → force logout
        localStorage.removeItem("authToken");
        navigate("/login");
      } else {
        setError("Failed to load data. Please check server connection.");
      }
    } finally {
      setLoading(false);
    }
  }, [token, navigate]);

  useEffect(() => {
    if (token) {
      fetchData();
    }
  }, [token, fetchData]);

  const handleDelete = (type, id) => {
    setConfirmMessage(`Are you sure you want to delete this ${type}?`);
    setConfirmAction(() => async () => {
      try {
        await axios.delete(`${API_BASE_URL}/${type}/${id}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        fetchData();
      } catch {
        setError("Failed to delete item.");
      } finally {
        setShowConfirm(false);
      }
    });
    setShowConfirm(true);
  };

  const handleSave = async (type, data, isEdit) => {
    const url = `${API_BASE_URL}/${type}${isEdit ? `/${data._id}` : ""}`;
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
      setError(`Failed to ${isEdit ? "update" : "create"} ${type}`);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("authToken");
    navigate("/login");
  };

  return (
    <div className="min-h-screen flex flex-col md:flex-row bg-gray-100">
      {/* Sidebar Navigation */}
      <Sidebar selectedView={selectedView} setSelectedView={setSelectedView} />
      

      {/* Main Content */}
      <main className="flex-1 p-4 space-y-6">
          <button
          onClick={handleLogout}
          className="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600"
        >
          Logout
        </button>
        {/* Dashboard Header */}
        <DashboardHeader
          title={
            selectedView === "analytics"
              ? "Analytics Overview"
              : selectedView === "products"
              ? "Products Dashboard"
              : "Users Dashboard"
          }
          subtitle={
            selectedView === "analytics"
              ? "Quick insights on products & users"
              : "Manage your inventory & user base"
          }
          error={error}
        />

        {/* Logout Button */}
      

        {/* Conditional Views */}
        {selectedView === "profile" ? (
          <MyProfile userId={"12345"} />
        ) : selectedView === "analytics" ? (
          <AnalyticsSection products={products} users={users} />
        ) : (
          <DataTable
            type={selectedView}
            data={selectedView === "products" ? products : users}
            loading={loading}
            onCreate={() =>
              selectedView === "products"
                ? setShowProductModal(true)
                : setShowUserModal(true)
            }
            onEdit={(item) => {
              setEditItem(item);
              selectedView === "products"
                ? setShowProductModal(true)
                : setShowUserModal(true);
            }}
            onDelete={(id) => handleDelete(selectedView, id)}
          />
        )}
      </main>

      {/* Modals */}
      <ProductFormModal
        open={showProductModal}
        onClose={() => {
          setShowProductModal(false);
          setEditItem(null);
        }}
        onSubmit={(data, isEdit) => handleSave("products", data, isEdit)}
        initialData={editItem}
      />

      <UserFormModal
        open={showUserModal}
        onClose={() => {
          setShowUserModal(false);
          setEditItem(null);
        }}
        onSubmit={(data, isEdit) => handleSave("users", data, isEdit)}
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

export default Dashboard;
