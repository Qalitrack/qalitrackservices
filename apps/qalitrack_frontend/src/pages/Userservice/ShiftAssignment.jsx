import React, { useState, useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { fetchShifts, fetchShiftUsers, assignShiftToUser, removeShiftFromUser,removeShiftFromRole,assignShiftToRole  } from '../../helpers/UserService/Shifts/Shifts.js';
import { ChevronLeft, ChevronRight, Users, Tag } from 'lucide-react';
import { fetchUsers } from '../../helpers/UserService/Users/users.js';
import { fetchRoles } from "../../helpers/UserService/Roles/Roles.js";
import { format, parseISO } from 'date-fns';

function isValidDateString(dateString) {
    if (!dateString) return false;
    const d = parseISO(dateString);
    return d instanceof Date && !isNaN(d);
}

function formatTimeOnlyString(timeString) {
    if (!timeString) return '-';
    const match = timeString.match(/^(\d{2}):(\d{2})(:(\d{2}))?(\.(\d+))?$/);
    if (!match) return timeString;
    const [, hours, minutes, , seconds] = match;
    return `${hours}:${minutes}${seconds ? ':' + seconds : ''}`;
}

const Modal = ({ children, isOpen }) => {
    if (!isOpen) return null;
    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center">
            <div className="bg-white rounded-lg shadow-xl p-6 w-full max-w-md m-4">
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
    const [isViewUsersModalOpen, setViewUsersModalOpen] = useState(false);
    const [isViewRolesModalOpen, setViewRolesModalOpen] = useState(false);
    const [selectedShiftUsers, setSelectedShiftUsers] = useState([]);
    const [selectedShiftRoles, setSelectedShiftRoles] = useState([]);
    const [selectedShift, setSelectedShift] = useState(null);
    const [allUsers, setAllUsers] = useState([]);
    const [modalLoading, setModalLoading] = useState(false);
    const [modalError, setModalError] = useState(null);
    const [checkboxState, setCheckboxState] = useState({});
    const [usersModalMessage, setUsersModalMessage] = useState({ text: '', type: '' });
    const [rolesModalMessage, setRolesModalMessage] = useState({ text: '', type: '' });

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
            setError(err.message || 'Failed to fetch shifts.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData(pagination.page);
    }, [pagination.page]);

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

    const handleViewUsersClick = async (shift) => {
        setSelectedShift(shift);
        setModalLoading(true);
        setModalError(null);
        setUsersModalMessage({ text: '', type: '' });
        try {
            const usersData = await fetchUsers(1, 1000);
            const users = usersData.items || [];

            const shiftUsers = await fetchShiftUsers(shift.id);

            const normalizedShiftUsers = Array.isArray(shiftUsers) ? shiftUsers : shiftUsers.users || [];

            const assignedUserIds = new Set(normalizedShiftUsers.map(user => user.id));

            setAllUsers(users);
            setSelectedShiftUsers(normalizedShiftUsers);

            const initialCheckboxState = users.reduce((acc, user) => {
                acc[user.id] = assignedUserIds.has(user.id);
                return acc;
            }, {});

            setCheckboxState(initialCheckboxState);

            setViewUsersModalOpen(true);
        } catch (err) {
            setModalError(err.message || 'Failed to fetch users or shift assignments.');
        } finally {
            setModalLoading(false);
        }
    };

    const handleViewRolesClick = async (shift) => {
        setSelectedShift(shift);
        setModalLoading(true);
        setModalError(null);
        setRolesModalMessage({ text: '', type: '' });
        try {
            const rolesData = await fetchRoles();
            const allRoles = rolesData.items || rolesData;
            setSelectedShiftRoles(allRoles);

            setViewRolesModalOpen(true);
        } catch (err) {
            setModalError(err.message || 'Failed to fetch roles.');
        } finally {
            setModalLoading(false);
        }
    };
    const handleAddRole = async (roleId) => {
        setModalLoading(true);
        setModalError(null);
        setRolesModalMessage({ text: '', type: '' });
        try {
            // Call the API function and store the server's response
            const serverResponse = await assignShiftToRole(roleId, selectedShift.id);

            // Find the role from the list of all roles
            const roleToAdd = selectedShiftRoles.find(r => r.id === roleId);

            // Update the state of the selected shift by adding the new role
            if (roleToAdd) {
                setSelectedShift(prevShift => {
                    const newRoles = [...(prevShift.roles || []), roleToAdd];
                    return {
                        ...prevShift,
                        roles: newRoles,
                        assignedRolesCount: newRoles.length
                    };
                });
            }

            // Extract the specific message from the server response
            const successMessage = serverResponse?.data?.message || 'Role added successfully!';
            setRolesModalMessage({ text: successMessage, type: 'success' });
        } catch (err) {
            setModalError(err.message || 'Failed to add role.');
            setRolesModalMessage({ text: `Failed to add role: ${err.message}`, type: 'error' });
        } finally {
            setModalLoading(false);
        }
    };

    const handleRemoveRole = async (roleId) => {
        setModalLoading(true);
        setModalError(null);
        setRolesModalMessage({ text: '', type: '' });
        try {
            // Call the API function and store the server's response
            const serverResponse = await removeShiftFromRole(roleId, selectedShift.id);

            // Update the state of the selected shift by filtering out the removed role
            setSelectedShift(prevShift => {
                const newRoles = (prevShift.roles || []).filter(r => r.id !== roleId);
                return {
                    ...prevShift,
                    roles: newRoles,
                    assignedRolesCount: newRoles.length
                };
            });

            // Extract the specific message from the server response
            const successMessage = serverResponse?.data?.message || 'Role removed successfully!';
            setRolesModalMessage({ text: successMessage, type: 'success' });
        } catch (err) {
            setModalError(err.message || 'Failed to remove role.');
            setRolesModalMessage({ text: `Failed to remove role: ${err.message}`, type: 'error' });
        } finally {
            setModalLoading(false);
        }
    };
    const handleCheckboxChange = async (userId) => {
        setModalLoading(true);
        setModalError(null);
        setUsersModalMessage({ text: '', type: '' });
        try {
            const isChecked = checkboxState[userId];
            const userEmail = allUsers.find(u => u.id === userId)?.email || 'user';

            if (isChecked) {
                await removeShiftFromUser(userId, selectedShift.id);
                setCheckboxState((prev) => ({ ...prev, [userId]: false }));
                setSelectedShiftUsers((prev) => prev.filter((user) => user.id !== userId));
                setShifts((prev) =>
                    prev.map((shift) =>
                        shift.id === selectedShift.id
                            ? { ...shift, assignedUsersCount: (shift.assignedUsersCount || 0) - 1 }
                            : shift
                    )
                );
                setUsersModalMessage({ text: `${userEmail} removed from shift.`, type: 'success' });
            } else {
                await assignShiftToUser(userId, selectedShift.id);
                setCheckboxState((prev) => ({ ...prev, [userId]: true }));
                const user = allUsers.find((u) => u.id === userId);
                if (user) {
                    setSelectedShiftUsers((prev) => [...prev, user]);
                }
                setShifts((prev) =>
                    prev.map((shift) =>
                        shift.id === selectedShift.id
                            ? { ...shift, assignedUsersCount: (shift.assignedUsersCount || 0) + 1 }
                            : shift
                    )
                );
                setUsersModalMessage({ text: `${userEmail} assigned to shift.`, type: 'success' });
            }
        } catch (err) {
            setModalError(err.message || 'Failed to update shift assignment.');
            setUsersModalMessage({ text: `Failed to update assignment: ${err.message}`, type: 'error' });
            console.error('Error in handleCheckboxChange:', err);
        } finally {
            setModalLoading(false);
        }
    };

    if (loading) {
        return <div className="flex justify-center items-center h-32"><div>Loading shifts...</div></div>;
    }

    if (error) {
        return <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-md" role="alert">{error}</div>;
    }

    return (
        <div className="bg-white shadow-lg rounded-xl p-4 md:p-8 max-w-7xl mx-auto my-4 md:my-10">
            <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl md:text-2xl font-bold text-gray-800">Shift Assignment</h2>
            </div>

            <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-800">
                    <tr>
                        <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Name</th>
                        <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Start Time</th>
                        <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">End Time</th>
                        <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Mode</th>
                        <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Manage</th>
                        <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Roles</th>
                    </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                    {shifts.map((shift) => (
                        <tr key={shift.id} className="hover:bg-gray-50">
                            <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{shift.name}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                {isValidDateString(shift.startTime) ? format(parseISO(shift.startTime), 'PPP p') : formatTimeOnlyString(shift.startTime)}
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                {isValidDateString(shift.endTime) ? format(parseISO(shift.endTime), 'PPP p') : formatTimeOnlyString(shift.endTime)}
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{shift.mode}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                <button
                                    onClick={() => handleViewUsersClick(shift)}
                                    className={`flex items-center transition-colors ${
                                        (shift.assignedUsersCount || 0) > 0
                                            ? 'text-green-500 hover:text-green-600 font-bold'
                                            : 'text-amber-500 hover:text-amber-600'
                                    }`}
                                    title="View Users"
                                >
                                    <Users size={18} className="mr-1" />
                                    <span>{shift.assignedUsersCount || 0}</span>
                                </button>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                <button
                                    onClick={() => handleViewRolesClick(shift)}
                                    className="flex items-center text-blue-500 hover:text-blue-600 transition-colors"
                                    title="View Roles"
                                >
                                    <Tag size={18} className="mr-1" />
                                </button>
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>

            <div className="flex justify-between items-center mt-4 text-sm text-gray-700">
                <div>
                    <p className="text-gray-700">
                        <span className="font-medium">{pagination.page * pagination.pageSize - pagination.pageSize + 1}</span> to{' '}
                        <span className="font-medium">{Math.min(pagination.page * pagination.pageSize, pagination.totalCount)}</span> of{' '}
                        <span className="font-medium">{pagination.totalCount}</span> rows
                    </p>
                </div>
                <div className="flex items-center gap-1">
                    <button
                        onClick={handlePreviousPage}
                        disabled={!pagination.hasPreviousPage || loading}
                        className="p-2 border rounded-md text-gray-500 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        <ChevronLeft size={16} />
                    </button>
                    {[...Array(pagination.totalPages).keys()].map((index) => (
                        <button
                            key={index}
                            className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium ${
                                pagination.page === index + 1 ? 'bg-amber-500 text-white' : 'bg-white text-gray-700 hover:bg-gray-100'
                            }`}
                            onClick={() => handlePageClick(index + 1)}
                        >
                            {index + 1}
                        </button>
                    ))}
                    <button
                        onClick={handleNextPage}
                        disabled={!pagination.hasNextPage || loading}
                        className="p-2 border rounded-md text-gray-500 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        <ChevronRight size={16} />
                    </button>
                </div>
            </div>

            <Modal isOpen={isViewUsersModalOpen}>
                <h3 className="text-lg font-bold mb-4">Manage Users for Shift "{selectedShift?.name}"</h3>
                {usersModalMessage.text && (
                    <div className={`p-3 mb-4 rounded-md text-sm font-medium ${usersModalMessage.type === 'success' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                        {usersModalMessage.text}
                    </div>
                )}
                {modalLoading ? (
                    <div className="flex justify-center items-center h-32">
                        <div>Loading users...</div>
                    </div>
                ) : modalError ? (
                    <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-md" role="alert">
                        {modalError}
                    </div>
                ) : (
                    <>
                        {allUsers.length > 0 ? (
                            <ul className="space-y-2 max-h-96 overflow-y-auto">
                                {allUsers.map((user) => (
                                    <li key={user.id} className="flex items-center bg-gray-100 p-3 rounded-md text-sm font-medium">
                                        <input
                                            type="checkbox"
                                            checked={checkboxState[user.id] || false}
                                            onChange={() => handleCheckboxChange(user.id)}
                                            className="mr-2 h-4 w-4 text-amber-500 focus:ring-amber-500 border-gray-300 rounded"
                                            disabled={modalLoading}
                                        />
                                        <span>
                                            {user.firstName || 'Unknown'} {user.lastName || 'User'} ({user.email || 'No email'})
                                        </span>
                                    </li>
                                ))}
                            </ul>
                        ) : (
                            <p className="text-sm text-gray-500">No users available. Please check the user database or API configuration.</p>
                        )}
                    </>
                )}
                <div className="flex justify-end pt-4">
                    <button
                        type="button"
                        onClick={() => setViewUsersModalOpen(false)}
                        className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                        disabled={modalLoading}
                    >
                        Close
                    </button>
                </div>
            </Modal>

            <Modal isOpen={isViewRolesModalOpen}>
                <h3 className="text-lg font-bold mb-4">Manage Roles for Shift "{selectedShift?.name}"</h3>
                {rolesModalMessage.text && (
                    <div className={`p-3 mb-4 rounded-md text-sm font-medium ${rolesModalMessage.type === 'success' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                        {rolesModalMessage.text}
                    </div>
                )}
                {modalLoading ? (
                    <div className="flex justify-center items-center h-32">
                        <div>Loading roles...</div>
                    </div>
                ) : modalError ? (
                    <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-md" role="alert">
                        {modalError}
                    </div>
                ) : (
                    <>
                        {selectedShiftRoles.length > 0 ? (
                            <ul className="space-y-2 max-h-96 overflow-y-auto">
                                {selectedShiftRoles.map((role) => (
                                    <li key={role.id} className="flex items-center justify-between bg-gray-100 p-3 rounded-md text-sm font-medium">
                                        <span>{role.name}</span>
                                        <div className="flex space-x-2">
                                            <button
                                                onClick={() => handleAddRole(role.id)}
                                                className="px-2 py-1 bg-green-500 text-white rounded-md text-xs hover:bg-green-600 transition-colors"
                                            >
                                                Add
                                            </button>
                                            <button
                                                onClick={() => handleRemoveRole(role.id)}
                                                className="px-2 py-1 bg-red-500 text-white rounded-md text-xs hover:bg-red-600 transition-colors"
                                            >
                                                Delete
                                            </button>
                                        </div>
                                    </li>
                                ))}
                            </ul>
                        ) : (
                            <p className="text-sm text-gray-500">No roles available.</p>
                        )}
                    </>
                )}
                <div className="flex justify-end pt-4">
                    <button
                        type="button"
                        onClick={() => setViewRolesModalOpen(false)}
                        className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                    >
                        Close
                    </button>
                </div>
            </Modal>
        </div>
    );
};

export default ShiftAssignment;