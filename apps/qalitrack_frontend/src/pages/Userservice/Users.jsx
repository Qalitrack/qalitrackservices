import React, { useState, useEffect } from 'react';
import { fetchUsers, fetchDeletedUsers, deleteUser, restoreUser } from '../../helpers/UserService/Users/users.js';
import { Edit, Trash2, PlusCircle, ChevronLeft, ChevronRight, RefreshCw } from 'lucide-react';
import { format, parseISO, formatDistanceToNow } from 'date-fns';

const Users = () => {
    const [users, setUsers] = useState([]);
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
            console.log('Fetched data:', data); // Debug
            setUsers(data.items || []);
            setPagination({
                page: data.page || 1,
                pageSize: data.pageSize || 10,
                totalCount: data.totalCount || 0,
                totalPages: data.totalPages || 1,
                hasPreviousPage: data.hasPreviousPage || false,
                hasNextPage: data.hasNextPage || false,
            });
        } catch (err) {
            setError(err.message || 'Failed to fetch users.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData(pagination.page, showDeleted);
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
                    <button className="flex items-center gap-2 px-4 py-2 bg-white text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors shadow">
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
                        <th scope="col" className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Status</th>
                        <th scope="col" className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Created At</th>
                        <th scope="col" className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">Last Updated</th>
                        <th scope="col" className="relative px-6 py-4 text-right text-xs font-semibold text-white uppercase tracking-wider">Actions</th>
                    </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                    {users.map((user) => (
                        <tr key={user.id} className={`hover:bg-gray-50 ${user.isDeleted ? 'opacity-60 bg-gray-100' : ''}`}>
                            <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{user.email}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{user.firstName} {user.lastName}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{user.roles.join(', ') || 'N/A'}</td>
                            <td className="px-6 py-4 whitespace-nowrap">
                                    <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusBadge(user)}`}>
                                        {getStatusText(user)}
                                    </span>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                {format(parseISO(user.createdAt), "PPP")}
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                {formatDistanceToNow(parseISO(user.updatedAt), { addSuffix: true })}
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-2">
                                <button
                                    className="text-amber-600 hover:text-amber-900 transition-colors disabled:text-gray-400 disabled:cursor-not-allowed"
                                    title="Edit User"
                                    disabled={user.isDeleted || actionLoading === user.id}
                                >
                                    <Edit size={18} />
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
        </div>
    );
};

export default Users;