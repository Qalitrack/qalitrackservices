import React, { useState, useEffect } from "react";
import { useLocation } from "react-router-dom";
import {
  fetchShifts,
  fetchShiftUsers,
  assignShiftToUser,
  removeShiftFromUser,
  removeShiftFromRole,
  assignShiftToRole,
} from "../../helpers/UserService/Shifts/Shifts.js";
import { ChevronLeft, ChevronRight, Users, Tag } from "lucide-react";
import { fetchUsers } from "../../helpers/UserService/Users/users.js";
import { fetchRoles } from "../../helpers/UserService/Roles/Roles.js";
import { format, parseISO } from "date-fns";

function isValidDateString(dateString) {
  if (!dateString) return false;
  const d = parseISO(dateString);
  return d instanceof Date && !isNaN(d);
}

function formatTimeOnlyString(timeString) {
  if (!timeString) return "-";
  const match = timeString.match(/^(\d{2}):(\d{2})(:(\d{2}))?(\.(\d+))?$/);
  if (!match) return timeString;
  const [, hours, minutes, , seconds] = match;
  return `${hours}:${minutes}${seconds ? ":" + seconds : ""}`;
}

const Modal = ({ children, isOpen }) => {
  if (!isOpen) return null;
  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center">
      <div className="bg-white rounded-lg shadow-xl p-6 w-full max-w-lg m-4 relative max-h-[90vh] overflow-y-auto">
        {children}
      </div>
    </div>
  );
};

const ShiftAssignment = () => {
  const { state } = useLocation();
  const [shifts, setShifts] = useState([]);
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Modals
  const [isViewUsersModalOpen, setViewUsersModalOpen] = useState(false);
  const [isViewRolesModalOpen, setViewRolesModalOpen] = useState(false);

  // State for selected shift
  const [selectedShift, setSelectedShift] = useState(null);
  const [allUsers, setAllUsers] = useState([]);
  const [allRoles, setAllRoles] = useState([]);

  // Modal loading & errors
  const [modalLoading, setModalLoading] = useState(false);
  const [modalError, setModalError] = useState(null);

  // Checkbox states
  const [checkboxStateUsers, setCheckboxStateUsers] = useState({});
  const [checkboxStateRoles, setCheckboxStateRoles] = useState({});

  // Messages
  const [usersModalMessage, setUsersModalMessage] = useState({
    text: "",
    type: "",
  });
  const [rolesModalMessage, setRolesModalMessage] = useState({
    text: "",
    type: "",
  });

  // Auto clear messages after few seconds
  useEffect(() => {
    if (usersModalMessage.text) {
      const timer = setTimeout(
        () => setUsersModalMessage({ text: "", type: "" }),
        4000
      );
      return () => clearTimeout(timer);
    }
  }, [usersModalMessage]);

  useEffect(() => {
    if (rolesModalMessage.text) {
      const timer = setTimeout(
        () => setRolesModalMessage({ text: "", type: "" }),
        4000
      );
      return () => clearTimeout(timer);
    }
  }, [rolesModalMessage]);

  // Load shifts
  const loadData = async (page) => {
    setLoading(true);
    setError(null);
    try {
      const data = await fetchShifts(page, pagination.pageSize);
      setShifts(data.items || []);
      setPagination({
        page: data.page || 1,
        pageSize: data.pageSize || 10,
        totalCount: data.totalCount || 0,
        totalPages: data.totalPages || 1,
        hasPreviousPage: data.hasPreviousPage || false,
        hasNextPage: data.hasNextPage || false,
      });
    } catch (err) {
      setError(err.message || "Failed to fetch shifts.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData(pagination.page);
  }, [pagination.page]);

  // Pagination handlers
  const handlePreviousPage = () => {
    if (pagination.hasPreviousPage) {
      setPagination((p) => ({ ...p, page: p.page - 1 }));
    }
  };
  const handleNextPage = () => {
    if (pagination.hasNextPage) {
      setPagination((p) => ({ ...p, page: p.page + 1 }));
    }
  };
  const handlePageClick = (page) => {
    setPagination((p) => ({ ...p, page }));
  };

  // Handle Users Modal
  const handleViewUsersClick = async (shift) => {
    setSelectedShift(shift);
    setModalLoading(true);
    setModalError(null);
    try {
      const usersData = await fetchUsers(1, 1000);
      const users = usersData.items || [];
      const shiftUsers = await fetchShiftUsers(shift.id);
      const normalizedShiftUsers = Array.isArray(shiftUsers)
        ? shiftUsers
        : shiftUsers.users || [];
      const assignedUserIds = new Set(normalizedShiftUsers.map((u) => u.id));

      setAllUsers(users);
      setCheckboxStateUsers(
        users.reduce((acc, user) => {
          acc[user.id] = assignedUserIds.has(user.id);
          return acc;
        }, {})
      );

      setViewUsersModalOpen(true);
    } catch (err) {
      setModalError(err.message || "Failed to fetch users.");
    } finally {
      setModalLoading(false);
    }
  };

  const handleCheckboxChangeUser = async (userId) => {
    setModalLoading(true);
    setModalError(null);
    try {
      const isChecked = checkboxStateUsers[userId];
      if (isChecked) {
        await removeShiftFromUser(userId, selectedShift.id);
        setCheckboxStateUsers((prev) => ({ ...prev, [userId]: false }));
        setShifts((prev) =>
          prev.map((shift) =>
            shift.id === selectedShift.id
              ? { ...shift, assignedUsersCount: (shift.assignedUsersCount || 1) - 1 }
              : shift
          )
        );
        setUsersModalMessage({ text: "User removed from shift.", type: "success" });
      } else {
        await assignShiftToUser(userId, selectedShift.id);
        setCheckboxStateUsers((prev) => ({ ...prev, [userId]: true }));
        setShifts((prev) =>
          prev.map((shift) =>
            shift.id === selectedShift.id
              ? { ...shift, assignedUsersCount: (shift.assignedUsersCount || 0) + 1 }
              : shift
          )
        );
        setUsersModalMessage({ text: "User assigned to shift.", type: "success" });
      }
    } catch (err) {
      setUsersModalMessage({
        text: `Failed: ${err.message}`,
        type: "error",
      });
    } finally {
      setModalLoading(false);
    }
  };

  // Handle Roles Modal
  const handleViewRolesClick = async (shift) => {
    setSelectedShift(shift);
    setModalLoading(true);
    setModalError(null);
    try {
      const rolesData = await fetchRoles();
      const roles = rolesData.items || rolesData;
      const assignedRoles = shift.roles || [];
      const assignedIds = new Set(assignedRoles.map((r) => r.id));

      setAllRoles(roles);
      setCheckboxStateRoles(
        roles.reduce((acc, role) => {
          acc[role.id] = assignedIds.has(role.id);
          return acc;
        }, {})
      );

      setViewRolesModalOpen(true);
    } catch (err) {
      setModalError(err.message || "Failed to fetch roles.");
    } finally {
      setModalLoading(false);
    }
  };

  const handleCheckboxChangeRole = async (roleId) => {
    setModalLoading(true);
    setModalError(null);
    try {
      const isChecked = checkboxStateRoles[roleId];
      if (isChecked) {
        await removeShiftFromRole(roleId, selectedShift.id);
        setCheckboxStateRoles((prev) => ({ ...prev, [roleId]: false }));
        setShifts((prev) =>
          prev.map((shift) =>
            shift.id === selectedShift.id
              ? { ...shift, assignedRolesCount: (shift.assignedRolesCount || 1) - 1 }
              : shift
          )
        );
        setRolesModalMessage({ text: "Role removed from shift.", type: "success" });
      } else {
        await assignShiftToRole(roleId, selectedShift.id);
        setCheckboxStateRoles((prev) => ({ ...prev, [roleId]: true }));
        setShifts((prev) =>
          prev.map((shift) =>
            shift.id === selectedShift.id
              ? { ...shift, assignedRolesCount: (shift.assignedRolesCount || 0) + 1 }
              : shift
          )
        );
        setRolesModalMessage({ text: "Role assigned to shift.", type: "success" });
      }
    } catch (err) {
      setRolesModalMessage({
        text: `Failed: ${err.message}`,
        type: "error",
      });
    } finally {
      setModalLoading(false);
    }
  };

  // Loading / Error states
  if (loading) {
    return (
      <div className="flex justify-center items-center h-32">
        <div>Loading shifts...</div>
      </div>
    );
  }
  if (error) {
    return (
      <div
        className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-md"
        role="alert"
      >
        {error}
      </div>
    );
  }

  return (
    <div className="bg-white shadow-lg rounded-xl p-4 md:p-8 max-w-7xl mx-auto my-4 md:my-10">
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-xl md:text-2xl font-bold text-gray-800">
          Shift Assignment
        </h2>
      </div>

      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-800">
            <tr>
              <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase">
                Name
              </th>
              <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase">
                Start Time
              </th>
              <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase">
                End Time
              </th>
              <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase">
                Mode
              </th>
              <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase">
                Users
              </th>
              <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase">
                Roles
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {shifts.map((shift) => (
              <tr key={shift.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 text-sm font-medium text-gray-900">
                  {shift.name}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500">
                  {isValidDateString(shift.startTime)
                    ? format(parseISO(shift.startTime), "PPP p")
                    : formatTimeOnlyString(shift.startTime)}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500">
                  {isValidDateString(shift.endTime)
                    ? format(parseISO(shift.endTime), "PPP p")
                    : formatTimeOnlyString(shift.endTime)}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500">
                  {shift.mode}
                </td>
                <td className="px-6 py-4 text-sm">
                  <button
                    onClick={() => handleViewUsersClick(shift)}
                    className={`flex items-center ${
                      (shift.assignedUsersCount || 0) > 0
                        ? "text-green-600 font-semibold"
                        : "text-amber-500"
                    } hover:underline`}
                  >
                    <Users size={18} className="mr-1" />
                    {shift.assignedUsersCount || 0}
                  </button>
                </td>
                <td className="px-6 py-4 text-sm">
                  <button
                    onClick={() => handleViewRolesClick(shift)}
                    className={`flex items-center ${
                      (shift.assignedRolesCount ) 
                        ? "text-blue-600 font-semibold"
                        : "text-gray-500"
                    } hover:underline`}
                  >
                    <Tag size={18} className="mr-1" />
                    {shift.assignedRolesCount }
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      <div className="flex justify-between items-center mt-4 text-sm text-gray-700">
        <p>
          <span className="font-medium">
            {pagination.page * pagination.pageSize - pagination.pageSize + 1}
          </span>{" "}
          to{" "}
          <span className="font-medium">
            {Math.min(
              pagination.page * pagination.pageSize,
              pagination.totalCount
            )}
          </span>{" "}
          of <span className="font-medium">{pagination.totalCount}</span> rows
        </p>
        <div className="flex items-center gap-1">
          <button
            onClick={handlePreviousPage}
            disabled={!pagination.hasPreviousPage || loading}
            className="p-2 border rounded-md text-gray-500 hover:bg-gray-50 disabled:opacity-50"
          >
            <ChevronLeft size={16} />
          </button>
          {[...Array(pagination.totalPages).keys()].map((index) => (
            <button
              key={index}
              className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium ${
                pagination.page === index + 1
                  ? "bg-amber-500 text-white"
                  : "bg-white text-gray-700 hover:bg-gray-100"
              }`}
              onClick={() => handlePageClick(index + 1)}
            >
              {index + 1}
            </button>
          ))}
          <button
            onClick={handleNextPage}
            disabled={!pagination.hasNextPage || loading}
            className="p-2 border rounded-md text-gray-500 hover:bg-gray-50 disabled:opacity-50"
          >
            <ChevronRight size={16} />
          </button>
        </div>
      </div>

      {/* Users Modal */}
      <Modal isOpen={isViewUsersModalOpen}>
        <h3 className="text-lg font-bold mb-4">
          Manage Users for "{selectedShift?.name}"
        </h3>
        {usersModalMessage.text && (
          <div
            className={`p-3 mb-4 rounded-md text-sm font-medium ${
              usersModalMessage.type === "success"
                ? "bg-green-100 text-green-700"
                : "bg-red-100 text-red-700"
            }`}
          >
            {usersModalMessage.text}
          </div>
        )}
        {modalLoading ? (
          <div className="flex justify-center items-center h-32">
            <div>Loading users...</div>
          </div>
        ) : modalError ? (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-md">
            {modalError}
          </div>
        ) : (
          <>
            {allUsers.length > 0 ? (
              <ul className="space-y-2 max-h-96 overflow-y-auto">
                {allUsers.map((user) => (
                  <li
                    key={user.id}
                    className="flex items-center bg-gray-100 p-2 rounded-md text-sm"
                  >
                    <input
                      type="checkbox"
                      checked={checkboxStateUsers[user.id] || false}
                      onChange={() => handleCheckboxChangeUser(user.id)}
                      className="mr-2 h-4 w-4 text-amber-500 focus:ring-amber-500 border-gray-300 rounded"
                      disabled={modalLoading}
                    />
                    {user.firstName} {user.lastName} ({user.email})
                  </li>
                ))}
              </ul>
            ) : (
              <p className="text-sm text-gray-500">
                No users available. Please check database.
              </p>
            )}
          </>
        )}
        <div className="flex justify-end pt-4 sticky bottom-0 bg-white">
          <button
            type="button"
            onClick={() => setViewUsersModalOpen(false)}
            className="px-4 py-2 border border-gray-300 rounded-md text-sm bg-gray-50 hover:bg-gray-100"
          >
            Close
          </button>
        </div>
      </Modal>

      {/* Roles Modal */}
      <Modal isOpen={isViewRolesModalOpen}>
        <h3 className="text-lg font-bold mb-4">
          Manage Roles for "{selectedShift?.name}"
        </h3>
        {rolesModalMessage.text && (
          <div
            className={`p-3 mb-4 rounded-md text-sm font-medium ${
              rolesModalMessage.type === "success"
                ? "bg-green-100 text-green-700"
                : "bg-red-100 text-red-700"
            }`}
          >
            {rolesModalMessage.text}
          </div>
        )}
        {modalLoading ? (
          <div className="flex justify-center items-center h-32">
            <div>Loading roles...</div>
          </div>
        ) : modalError ? (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-md">
            {modalError}
          </div>
        ) : (
          <>
            {allRoles.length > 0 ? (
              <ul className="space-y-2 max-h-96 overflow-y-auto">
                {allRoles.map((role) => (
                  <li
                    key={role.id}
                    className="flex items-center bg-gray-100 p-2 rounded-md text-sm"
                  >
                    <input
                      type="checkbox"
                      checked={checkboxStateRoles[role.id] || false}
                      onChange={() => handleCheckboxChangeRole(role.id)}
                      className="mr-2 h-4 w-4 text-blue-500 focus:ring-blue-500 border-gray-300 rounded"
                      disabled={modalLoading}
                    />
                    {role.name}
                  </li>
                ))}
              </ul>
            ) : (
              <p className="text-sm text-gray-500">
                No roles available. Please check database.
              </p>
            )}
          </>
        )}
        <div className="flex justify-end pt-4 sticky bottom-0 bg-white">
          <button
            type="button"
            onClick={() => setViewRolesModalOpen(false)}
            className="px-4 py-2 border border-gray-300 rounded-md text-sm bg-gray-50 hover:bg-gray-100"
          >
            Close
          </button>
        </div>
      </Modal>
    </div>
  );
};

export default ShiftAssignment;
