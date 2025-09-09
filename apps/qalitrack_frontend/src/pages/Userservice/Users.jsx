import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchUsers, fetchDeletedUsers, deleteUser, restoreUser, updateUser, fetchUserById, resetPassword, assignRoleToUser, removeRoleFromUser, fetchUserRoles, fetchUserShifts, createUser } from '../../helpers/UserService/Users/users.js';
import { fetchRoles } from '../../helpers/UserService/Roles/Roles.js';
import { Edit, Trash2, PlusCircle, ChevronLeft, ChevronRight, RefreshCw, Mail, Phone, Save, XCircle, FileText, Key, ShieldAlert, ShieldCheck, Users as UsersIcon, Clock } from 'lucide-react';
import { format, parseISO, formatDistanceToNow } from 'date-fns';

const Modal = ({ children, isOpen, onClose, size = "md" }) => {
    if (!isOpen) return null;

    const sizeClasses = {
        sm: "max-w-sm",
        md: "max-w-md",
        lg: "max-w-2xl",
        xl: "max-w-4xl",
        xxl: "max-w-6xl"
    };

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center p-4">
            <div className={`bg-white rounded-lg shadow-xl p-6 w-full ${sizeClasses[size]} max-h-[90vh] overflow-y-auto`}>
                {children}
            </div>
        </div>
    );
};

const Users = () => {
    const [users, setUsers] = useState([]);
    const [roles, setRoles] = useState([]);
    const [pagination, setPagination] = useState({
        page: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
    });
    const [loading, setLoading] = useState(true);
    const [actionLoading, setActionLoading] = useState(null);
    const [error, setError] = useState(null);
    const [feedbackMessage, setFeedbackMessage] = useState({ text: '', type: '' });
    const [showDeleted, setShowDeleted] = useState(false);
    const [isEditModalOpen, setEditModalOpen] = useState(false);
    const [isAddModalOpen, setAddModalOpen] = useState(false);
    const [selectedUser, setSelectedUser] = useState(null);
    const [newUser, setNewUser] = useState({
        firstName: '',
        lastName: '',
        mobileNumber: '',
        email: '',
        password: '',
        confirmPassword: ''
    });
    const [isUpdating, setIsUpdating] = useState(false);
    const [modalFeedback, setModalFeedback] = useState({ text: '', type: '' });
    const [isLogsModalOpen, setLogsModalOpen] = useState(false);
    const [isResetPasswordModalOpen, setResetPasswordModalOpen] = useState(false);
    const [userDetails, setUserDetails] = useState({});
    const [isManageRolesModalOpen, setManageRolesModalOpen] = useState(false);
    const [isViewRolesModalOpen, setViewRolesModalOpen] = useState(false);
    const [selectedUserRoles, setSelectedUserRoles] = useState([]);
    const [initialUserRoleIds, setInitialUserRoleIds] = useState([]);
    const [availableRoles, setAvailableRoles] = useState([]);
    const [isViewShiftsModalOpen, setViewShiftsModalOpen] = useState(false);
    const [selectedUserShifts, setSelectedUserShifts] = useState([]);
    const navigate = useNavigate();
    const [userShiftCounts, setUserShiftCounts] = useState({});

    const showMessage = (text, type) => {
        setFeedbackMessage({ text, type });
        setTimeout(() => {
            setFeedbackMessage({ text: '', type: '' });
        }, 3000);
    };

    const loadData = async (page, deleted) => {
        setLoading(true);
        setError(null);
        try {
            const fetchFunction = deleted ? fetchDeletedUsers : fetchUsers;
            const data = await fetchFunction(page, pagination.pageSize);
            const fetchedUsers = data.items || [];
            setUsers(fetchedUsers);
            setPagination({
                page: data.page || 1,
                pageSize: data.pageSize || 10,
                totalCount: data.totalCount || 0,
                totalPages: data.totalPages || 1,
                hasPreviousPage: data.hasPreviousPage || false,
                hasNextPage: data.hasNextPage || false,
            });

            // Load shift counts for the fetched users
            if (fetchedUsers.length > 0) {
                await loadShiftCounts(fetchedUsers);
            }
        } catch (err) {
            setError(err.message || 'Failed to fetch users.');
        } finally {
            setLoading(false);
        }
    };

    const loadRoles = async () => {
        setLoading(true);
        setError(null);
        try {
            const data = await fetchRoles();
            setRoles(data || []);
        } catch (err) {
            setError(err.message || 'Failed to fetch roles.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData(pagination.page, showDeleted);
        loadRoles();
    }, [pagination.page, showDeleted]);

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

    const handleToggleShowDeleted = () => {
        setShowDeleted((prev) => !prev);
        setPagination((p) => ({ ...p, page: 1 }));
    };

    const loadUserDetails = async (userId) => {
        if (!userId || userDetails[userId]) return; // Don't fetch if no ID or already fetched

        try {
            const user = await fetchUserById(userId);
            setUserDetails(prev => ({ ...prev, [userId]: user.email }));
        } catch (error) {
            console.error(`Failed to fetch user ${userId}`, error);
            setUserDetails(prev => ({ ...prev, [userId]: 'Unknown' })); // Handle error case
        }
    };

    const handleAddUserClick = () => {
        setNewUser({
            firstName: '',
            lastName: '',
            mobileNumber: '',
            email: '',
            password: '',
            confirmPassword: ''
        });
        setModalFeedback({ text: '', type: '' });
        setAddModalOpen(true);
    };

    const handleNewUserInputChange = (e) => {
        const { name, value } = e.target;
        setNewUser(prev => ({ ...prev, [name]: value }));
    };

    const handleCreateUser = async (e) => {
        e.preventDefault();

        // Validation
        if (!newUser.firstName || !newUser.lastName || !newUser.email || !newUser.password) {
            setModalFeedback({ text: 'Please fill in all required fields.', type: 'error' });
            return;
        }

        if (newUser.password !== newUser.confirmPassword) {
            setModalFeedback({ text: 'Passwords do not match.', type: 'error' });
            return;
        }

        if (newUser.password.length < 6) {
            setModalFeedback({ text: 'Password must be at least 6 characters long.', type: 'error' });
            return;
        }

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(newUser.email)) {
            setModalFeedback({ text: 'Please enter a valid email address.', type: 'error' });
            return;
        }

        setIsUpdating(true);
        setModalFeedback({ text: '', type: '' });

        try {
            await createUser(newUser);
            setModalFeedback({ text: 'User created successfully!', type: 'success' });
            await loadData(pagination.page, showDeleted);
            setTimeout(() => {
                setAddModalOpen(false);
            }, 2000);
        } catch (err) {
            setModalFeedback({ text: err.message || 'Failed to create user.', type: 'error' });
            console.error("Failed to create user:", err);
        } finally {
            setIsUpdating(false);
        }
    };

    const handleEditClick = (user) => {
        setSelectedUser({ ...user });
        setModalFeedback({ text: '', type: '' });
        setEditModalOpen(true);
    };

    const handleLogsClick = (user) => {
        setSelectedUser(user);
        loadUserDetails(user.createdBy);
        loadUserDetails(user.updatedBy);
        setLogsModalOpen(true);
    };

    const handleViewRolesClick = (user) => {
        setSelectedUser(user);
        setViewRolesModalOpen(true);
    };

    const handleResetPasswordClick = (user) => {
        setSelectedUser(user);
        setModalFeedback({ text: '', type: '' });
        setResetPasswordModalOpen(true);
    };

    const handleManageRolesClick = async (user) => {
        setSelectedUser(user);
        setModalFeedback({ text: '', type: '' });
        setManageRolesModalOpen(true);
        try {
            const userRolesData = await fetchUserRoles(user.id);
            const roleIds = userRolesData.map(role => role.id);
            setSelectedUserRoles(roleIds);
            setInitialUserRoleIds(roleIds); // Store the initial roles
        } catch (err) {
            setModalFeedback({ text: err.message || 'Failed to fetch user roles.', type: 'error' });
        }
    };

    const handleViewShiftsClick = async (user) => {
        setSelectedUser(user);
        setViewShiftsModalOpen(true);
        try {
            const shifts = await fetchUserShifts(user.id);
            setSelectedUserShifts(shifts);
        } catch (err) {
            setSelectedUserShifts([]);
        }
    };

    const handleConfirmResetPassword = async () => {
        if (!selectedUser) return;

        setIsUpdating(true);
        setModalFeedback({ text: '', type: '' });
        try {
            await resetPassword(selectedUser.id);
            setModalFeedback({ text: 'Password reset email sent successfully!', type: 'success' });
            setTimeout(() => {
                setResetPasswordModalOpen(false);
            }, 3000);
        } catch (err) {
            setModalFeedback({ text: err.message || 'Failed to reset password.', type: 'error' });
            console.error("Failed to reset password:", err);
        } finally {
            setIsUpdating(false);
        }
    };

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setSelectedUser(prev => ({ ...prev, [name]: value }));
    };

    const handleUpdate = async (e) => {
        e.preventDefault();
        if (!selectedUser) return;

        setIsUpdating(true);
        setModalFeedback({ text: '', type: '' });
        try {
            const { id, ...userData } = selectedUser;
            await updateUser(id, userData);
            setModalFeedback({ text: 'User updated successfully!', type: 'success' });
            await loadData(pagination.page, showDeleted);
            setTimeout(() => {
                setEditModalOpen(false);
            }, 3000);
        } catch (err) {
            setModalFeedback({ text: err.message || 'Failed to update user.', type: 'error' });
            console.error("Failed to update user:", err);
        } finally {
            setIsUpdating(false);
        }
    };

    const handleToggleDelete = async (user) => {
        const action = user.isDeleted ? 'restore' : 'delete';
        const actionVerb = user.isDeleted ? 'restored' : 'deleted';

        setActionLoading(user.id);
        try {
            if (!user.isDeleted && !showDeleted) {
                setUsers((prevUsers) => prevUsers.filter((u) => u.id !== user.id));
            } else if (user.isDeleted && showDeleted) {
                setUsers((prevUsers) => prevUsers.filter((u) => u.id !== user.id));
            }

            if (user.isDeleted) {
                await restoreUser(user.id);
            } else {
                await deleteUser(user.id);
            }
            showMessage(`User successfully ${actionVerb}.`, 'success');
            loadData(pagination.page, showDeleted);
        } catch (err) {
            showMessage(err.message || `Failed to ${action} user.`, 'error');
            loadData(pagination.page, showDeleted);
        } finally {
            setActionLoading(null);
        }
    };

    const handleRoleChange = (roleId) => {
        setSelectedUserRoles(prev =>
            prev.includes(roleId)
                ? prev.filter(id => id !== roleId)
                : [...prev, roleId]
        );
    };

    const handleSaveRoles = async () => {
        if (!selectedUser) return;

        setIsUpdating(true);
        setModalFeedback({ text: '', type: '' });

        const rolesToAdd = selectedUserRoles.filter(roleId => !initialUserRoleIds.includes(roleId));
        const rolesToRemove = initialUserRoleIds.filter(roleId => !selectedUserRoles.includes(roleId));

        try {
            // Using Promise.all to run requests in parallel for efficiency
            await Promise.all([
                ...rolesToRemove.map(roleId => removeRoleFromUser(selectedUser.id, roleId)),
                ...rolesToAdd.map(roleId => assignRoleToUser(selectedUser.id, roleId))
            ]);

            setModalFeedback({ text: 'Roles updated successfully!', type: 'success' });
            await loadData(pagination.page, showDeleted);
            setTimeout(() => {
                setManageRolesModalOpen(false);
            }, 2000);
        } catch (err) {
            setModalFeedback({ text: err.message || 'Failed to update roles.', type: 'error' });
        } finally {
            setIsUpdating(false);
        }
    };

    const loadShiftCounts = async (users) => {
        const shiftCounts = {};

        // Fetch shift counts for all users in parallel
        const promises = users.map(async (user) => {
            try {
                const shifts = await fetchUserShifts(user.id);
                shiftCounts[user.id] = shifts.length;
            } catch (error) {
                console.error(`Failed to fetch shifts for user ${user.id}:`, error);
                shiftCounts[user.id] = 0;
            }
        });

        await Promise.all(promises);
        setUserShiftCounts(shiftCounts);
    };

    const getStatusBadge = (user) => {
        if (user.isDeleted) return 'bg-gray-200 text-gray-800';
        return user.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800';
    };

    const getStatusText = (user) => {
        if (user.isDeleted) return 'Deleted';
        return user.isActive ? 'Online' : 'Offline';
    };

    if (loading) {
        return <div className="flex justify-center items-center h-32"><div>Loading users...</div></div>;
    }

    if (error) {
        return <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-md" role="alert">{error}</div>;
    }

    return (
        <div className="bg-white shadow-lg rounded-xl p-4 md:p-8 max-w-7xl mx-auto my-4 md:my-10">
            <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl md:text-2xl font-bold text-gray-800">Users</h2>
                <div className="flex items-center gap-4">
                    <div className="flex items-center gap-2">
                        <label htmlFor="show-deleted" className="text-sm font-medium text-gray-700">Show Deleted</label>
                        <input
                            type="checkbox"
                            id="show-deleted"
                            checked={showDeleted}
                            onChange={handleToggleShowDeleted}
                            className="h-4 w-4 rounded border-gray-300 text-amber-600 focus:ring-amber-500"
                        />
                    </div>
                    <button
                        onClick={handleAddUserClick}
                        className="flex items-center gap-2 px-4 py-2 bg-amber-500 text-white border border-amber-500 rounded-lg hover:bg-amber-600 transition-colors shadow"
                        >
                    <PlusCircle size={18} />
                    <span>Add User</span>
                </button>
                </div>
            </div>

            {feedbackMessage.text && (
                <div className={`p-3 rounded-lg mb-4 text-center text-sm font-medium ${feedbackMessage.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                    {feedbackMessage.text}
                </div>
            )}

            <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-800">
                    <tr>
                        <th scope="col" className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Email</th>
                        <th scope="col" className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Name</th>
                        <th scope="col" className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Roles</th>
                        <th scope="col" className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Shifts</th>
                        <th scope="col" className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Status</th>
                        <th scope="col" className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Last Updated</th>
                        <th scope="col" className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Actions</th>
                    </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                    {users.map((user) => (
                        <tr key={user.id} className={`hover:bg-gray-50 ${user.isDeleted ? 'opacity-60 bg-gray-100' : ''}`}>
                            <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{user.email}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{user.firstName} {user.lastName}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                <button
                                    onClick={() => handleViewRolesClick(user)}
                                    className="flex items-center text-amber-500 hover:text-amber-600 transition-colors"
                                    title="View Roles"
                                >
                                    <UsersIcon size={18} className="mr-1" />
                                    <span>{user.roles ? user.roles.length : '0'}</span>
                                </button>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                <button
                                    onClick={() => handleViewShiftsClick(user)}
                                    className="flex items-center text-purple-500 hover:text-purple-600 transition-colors"
                                    title="View Shifts"
                                >
                                    <Clock size={18} className="mr-1" />
                                    <span>{userShiftCounts[user.id] || 0}</span>
                                </button>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap">
                                    <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusBadge(user)}`}>
                                        {getStatusText(user)}
                                    </span>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                {formatDistanceToNow(parseISO(user.updatedAt), { addSuffix: true })}
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-left text-sm font-medium space-x-2">
                                <button
                                    onClick={() => handleLogsClick(user)}
                                    className="text-gray-600 hover:text-gray-900 transition-colors"
                                    title="View Logs"
                                >
                                    <FileText size={18} />
                                </button>
                                <button
                                    onClick={() => handleEditClick(user)}
                                    className="text-amber-600 hover:text-amber-900 transition-colors disabled:text-gray-400 disabled:cursor-not-allowed"
                                    title="Edit User"
                                    disabled={user.isDeleted || actionLoading === user.id}
                                >
                                    <Edit size={18} />
                                </button>
                                <button
                                    onClick={() => handleManageRolesClick(user)}
                                    className="text-green-600 hover:text-green-900 transition-colors disabled:text-gray-400 disabled:cursor-not-allowed"
                                    title="Manage Roles"
                                    disabled={user.isDeleted || actionLoading === user.id}
                                >
                                    <ShieldCheck size={18} />
                                </button>
                                <button
                                    onClick={() => handleResetPasswordClick(user)}
                                    className="text-blue-600 hover:text-blue-900 transition-colors disabled:text-gray-400 disabled:cursor-not-allowed"
                                    title="Reset Password"
                                    disabled={user.isDeleted || actionLoading === user.id}
                                >
                                    <Key size={18} />
                                </button>
                                <button
                                    onClick={() => handleToggleDelete(user)}
                                    className={`${
                                        user.isDeleted ? 'text-blue-600 hover:text-blue-800' : 'text-red-600 hover:text-red-800'
                                    } transition-colors ${actionLoading === user.id ? 'opacity-50 cursor-not-allowed' : ''}`}
                                    title={user.isDeleted ? 'Restore User' : 'Delete User'}
                                    disabled={actionLoading === user.id}
                                >
                                    {user.isDeleted ? <RefreshCw size={18} /> : <Trash2 size={18} />}
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
                        <span className="font-medium">{pagination.page * pagination.pageSize - pagination.pageSize + 1}</span> to <span className="font-medium">{Math.min(pagination.page * pagination.pageSize, pagination.totalCount)}</span> of <span className="font-medium">{pagination.totalCount}</span> rows
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
                                pagination.page === index + 1
                                    ? 'bg-amber-500 text-white'
                                    : 'bg-white text-gray-700 hover:bg-gray-100'
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
            {/* Add User Modal */}
            <Modal isOpen={isAddModalOpen} onClose={() => setAddModalOpen(false)}>
                <h3 className="text-lg font-bold mb-4">Add New User</h3>
                {modalFeedback.text && (
                    <div className={`p-3 rounded-lg mb-4 text-center text-sm font-medium ${modalFeedback.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                        {modalFeedback.text}
                    </div>
                )}
                <form onSubmit={handleCreateUser} className="space-y-4">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label htmlFor="firstName" className="block text-sm font-medium text-gray-700">First Name *</label>
                            <input
                                type="text"
                                name="firstName"
                                id="firstName"
                                value={newUser.firstName}
                                onChange={handleNewUserInputChange}
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                                required
                            />
                        </div>
                        <div>
                            <label htmlFor="lastName" className="block text-sm font-medium text-gray-700">Last Name *</label>
                            <input
                                type="text"
                                name="lastName"
                                id="lastName"
                                value={newUser.lastName}
                                onChange={handleNewUserInputChange}
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                                required
                            />
                        </div>
                    </div>

                    <div>
                        <label htmlFor="email" className="block text-sm font-medium text-gray-700">Email *</label>
                        <input
                            type="email"
                            name="email"
                            id="email"
                            value={newUser.email}
                            onChange={handleNewUserInputChange}
                            className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            required
                        />
                    </div>

                    <div>
                        <label htmlFor="mobileNumber" className="block text-sm font-medium text-gray-700">Mobile Number</label>
                        <input
                            type="tel"
                            name="mobileNumber"
                            id="mobileNumber"
                            value={newUser.mobileNumber}
                            onChange={handleNewUserInputChange}
                            className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                        />
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label htmlFor="password" className="block text-sm font-medium text-gray-700">Password *</label>
                            <input
                                type="password"
                                name="password"
                                id="password"
                                value={newUser.password}
                                onChange={handleNewUserInputChange}
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                                required
                                minLength="6"
                            />
                        </div>
                        <div>
                            <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700">Confirm Password *</label>
                            <input
                                type="password"
                                name="confirmPassword"
                                id="confirmPassword"
                                value={newUser.confirmPassword}
                                onChange={handleNewUserInputChange}
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                                required
                                minLength="6"
                            />
                        </div>
                    </div>

                    <div className="flex justify-end space-x-3 pt-4">
                        <button
                            type="button"
                            onClick={() => setAddModalOpen(false)}
                            className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            disabled={isUpdating}
                            className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-amber-600 hover:bg-amber-700 disabled:bg-gray-300"
                        >
                            {isUpdating ? 'Creating...' : 'Create User'}
                        </button>
                    </div>
                </form>
            </Modal>


            <Modal isOpen={isEditModalOpen} onClose={() => setEditModalOpen(false)}>
                <h3 className="text-lg font-bold mb-4">Edit User</h3>
                {modalFeedback.text && (
                    <div className={`p-3 rounded-lg mb-4 text-center text-sm font-medium ${modalFeedback.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                        {modalFeedback.text}
                    </div>
                )}
                {selectedUser && (
                    <form onSubmit={handleUpdate} className="space-y-4">
                        <div>
                            <label htmlFor="firstName" className="block text-sm font-medium text-gray-700">First Name</label>
                            <input
                                type="text"
                                name="firstName"
                                id="firstName"
                                value={selectedUser.firstName}
                                onChange={handleInputChange}
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            />
                        </div>
                        <div>
                            <label htmlFor="lastName" className="block text-sm font-medium text-gray-700">Last Name</label>
                            <input
                                type="text"
                                name="lastName"
                                id="lastName"
                                value={selectedUser.lastName}
                                onChange={handleInputChange}
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            />
                        </div>
                        <div>
                            <label htmlFor="email" className="block text-sm font-medium text-gray-700">Email</label>
                            <input
                                type="email"
                                name="email"
                                id="email"
                                value={selectedUser.email}
                                onChange={handleInputChange}
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            />
                        </div>
                        <div>
                            <label htmlFor="mobileNumber" className="block text-sm font-medium text-gray-700">Mobile Number</label>
                            <input
                                type="text"
                                name="mobileNumber"
                                id="mobileNumber"
                                value={selectedUser.mobileNumber || ''}
                                onChange={handleInputChange}
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            />
                        </div>
                        <div className="flex justify-end space-x-3 pt-4">
                            <button type="button" onClick={() => setEditModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">
                                Cancel
                            </button>
                            <button type="submit" disabled={isUpdating} className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-amber-600 hover:bg-amber-700 disabled:bg-gray-300">
                                {isUpdating ? 'Saving...' : 'Save Changes'}
                            </button>
                        </div>
                    </form>
                )}
            </Modal>

            <Modal isOpen={isLogsModalOpen} onClose={() => setLogsModalOpen(false)}>
                <div className="bg-gray-100 p-6 rounded-lg shadow-md">
                    <h3 className="text-lg font-bold mb-4">Audit Logs for "{selectedUser?.email}"</h3>
                    {selectedUser && (
                        <div className="space-y-4">
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Created At:</p>
                                <p className="text-gray-600">{format(parseISO(selectedUser.createdAt), "PPP p")}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Created By:</p>
                                <p className="text-gray-600">{userDetails[selectedUser.createdBy] || selectedUser.createdBy || 'N/A'}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Last Updated At:</p>
                                <p className="text-gray-600">{format(parseISO(selectedUser.updatedAt), "PPP p")}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Updated By:</p>
                                <p className="text-gray-600">{userDetails[selectedUser.updatedBy] || selectedUser.updatedBy || 'N/A'}</p>
                            </div>
                        </div>
                    )}
                    <div className="flex justify-end mt-6">
                        <button
                            type="button"
                            onClick={() => setLogsModalOpen(false)}
                            className="px-4 py-2 bg-amber-500 hover:bg-amber-600 text-white rounded-md text-sm font-medium transition-colors duration-200"
                        >
                            Close
                        </button>
                    </div>
                </div>
            </Modal>

            <Modal isOpen={isResetPasswordModalOpen} onClose={() => setResetPasswordModalOpen(false)}>
                <div className="text-center">
                    <ShieldAlert className="mx-auto h-12 w-12 text-amber-500" />
                    <h3 className="mt-2 text-lg font-bold text-gray-800">Reset Password</h3>
                    <p className="mt-2 text-sm text-gray-600">
                        Are you sure you want to reset the password for "{selectedUser?.email}"? An email with reset instructions will be sent to them.
                    </p>
                    {modalFeedback.text && (
                        <div className={`mt-4 p-3 rounded-lg text-center text-sm font-medium ${modalFeedback.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                            {modalFeedback.text}
                        </div>
                    )}
                </div>
                {!modalFeedback.text || modalFeedback.type !== 'success' ? (
                    <div className="mt-6 flex justify-center space-x-4">
                        <button onClick={() => setResetPasswordModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">
                            Cancel
                        </button>
                        <button onClick={handleConfirmResetPassword} disabled={isUpdating} className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-amber-600 hover:bg-amber-700 disabled:bg-gray-400">
                            {isUpdating ? 'Sending...' : 'Reset Password'}
                        </button>
                    </div>
                ) : (
                    <div className="mt-6 flex justify-center">
                        <button onClick={() => setResetPasswordModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">
                            Close
                        </button>
                    </div>
                )}
            </Modal>

            <Modal isOpen={isManageRolesModalOpen} onClose={() => setManageRolesModalOpen(false)}>
                <div className="p-6">
                    <h3 className="text-lg font-bold mb-4">Manage Roles for "{selectedUser?.email}"</h3>
                    {modalFeedback.text && (
                        <div className={`p-3 rounded-lg mb-4 text-center text-sm font-medium ${modalFeedback.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                            {modalFeedback.text}
                        </div>
                    )}
                    <div className="space-y-4">
                        {roles.map((role) => {
                            const isAssigned = selectedUserRoles.includes(role.id);
                            return (
                                <div key={role.id} className="flex items-center justify-between py-2 border-b">
                                    <div className="flex items-center">
                                        <input
                                            type="checkbox"
                                            id={`role-${role.id}`}
                                            name="user-role"
                                            checked={isAssigned}
                                            onChange={() => handleRoleChange(role.id)}
                                            className="h-4 w-4 rounded border-gray-300 text-amber-600 focus:ring-amber-500"
                                        />
                                        <label htmlFor={`role-${role.id}`} className="ml-2 block text-sm font-medium text-gray-700">
                                            {role.name}
                                        </label>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                    <div className="flex justify-between items-center mt-4">
                        <button
                            onClick={() => setSelectedUserRoles([])}
                            className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                        >
                            Clear Selection
                        </button>
                        <div className="space-x-3">
                            <button
                                onClick={() => setManageRolesModalOpen(false)}
                                className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleSaveRoles}
                                disabled={isUpdating}
                                className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-amber-600 hover:bg-amber-700 disabled:bg-gray-300"
                            >
                                {isUpdating ? 'Saving...' : 'Save'}
                            </button>
                        </div>
                    </div>
                </div>
            </Modal>

            <Modal isOpen={isViewRolesModalOpen} onClose={() => setViewRolesModalOpen(false)}>
                <h3 className="text-lg font-bold mb-4">Roles for "{selectedUser?.email}"</h3>
                {selectedUser?.roles && selectedUser.roles.length > 0 ? (
                    <ul className="space-y-2">
                        {selectedUser.roles.map(roleId => (
                            <li key={roleId} className="bg-gray-100 p-3 rounded-md text-sm font-medium">
                                {roles.find(r => r.id === roleId)?.name || roleId}
                            </li>
                        ))}
                    </ul>
                ) : (
                    <p className="text-sm text-gray-500">No roles assigned to this user.</p>
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

            <Modal isOpen={isViewShiftsModalOpen} onClose={() => setViewShiftsModalOpen(false)} size="xl">
                <div className="p-6 w-full max-w-4xl mx-auto">
                    <h3 className="text-lg font-bold mb-6">Shifts for "{selectedUser?.email}"</h3>
                    {selectedUserShifts.length > 0 ? (
                        <div className="w-full">
                            <table className="w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                <tr>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Mode</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Assigned At</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                                </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                {selectedUserShifts.map((raw) => {
                                    const id = raw.shiftId || raw.id;
                                    const name = raw.shiftName || raw.name || 'Untitled Shift';
                                    const mode = raw.shiftMode || raw.mode || 'N/A';
                                    const assignedAt = raw.assignedAt || raw.startTime || null;
                                    const active = typeof raw.isActive === 'boolean' ? raw.isActive : !!raw.active;
                                    const fmt = (val) => {
                                        if (!val) return 'N/A';
                                        try { return format(parseISO(val), 'PPP p'); } catch { return val; }
                                    };

                                    return (
                                        <tr key={id || Math.random()} className="hover:bg-gray-50 transition-colors duration-150">
                                            <td className="px-6 py-4 text-sm font-medium text-gray-900">{name}</td>
                                            <td className="px-6 py-4 text-sm text-gray-500">{mode}</td>
                                            <td className="px-6 py-4 text-sm text-gray-500">{fmt(assignedAt)}</td>
                                            <td className="px-6 py-4">
                    <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                        active ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                    }`}>
                      {active ? 'Active' : 'Inactive'}
                    </span>
                                            </td>
                                        </tr>
                                    );
                                })}
                                </tbody>
                            </table>
                        </div>
                    ) : (
                        <div className="text-center py-8">
                            <p className="text-sm text-gray-500">No shifts found for this user.</p>
                        </div>
                    )}

                    <div className="flex justify-end mt-6 pt-4 border-t border-gray-200">
                        <button
                            type="button"
                            onClick={() => setViewShiftsModalOpen(false)}
                            className="px-6 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors duration-200"
                        >
                            Close
                        </button>
                    </div>
                </div>
            </Modal>
        </div>
    );
};

export default Users;
