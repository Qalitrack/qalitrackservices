import React, { useState, useEffect } from 'react';
import {
  fetchShifts,
  fetchDeletedShifts,
  updateShift,
  deleteShift,
  createShift,
} from '../../helpers/UserService/Shifts/Shifts.js';
import { fetchUserById } from '../../helpers/UserService/Users/users.js';
import {
  ChevronLeft,
  ChevronRight,
  Edit,
  Trash2,
  PlusCircle,
  FileText,
} from 'lucide-react';
import { format, parseISO } from 'date-fns';

function isValidDateString(dateString) {
  if (!dateString) return false;
  const d = parseISO(dateString);
  return d instanceof Date && !isNaN(d);
}

function formatTimeOnlyString(timeString) {
  if (!timeString) return '-';
  try {
    const date = new Date(timeString);
    if (isNaN(date.getTime())) return timeString;
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  } catch (e) {
    return timeString;
  }
}

function formatDateTime(dateTimeString) {
  if (!dateTimeString) return '-';
  try {
    const date = new Date(dateTimeString);
    if (isNaN(date.getTime())) return dateTimeString;
    return date.toLocaleString([], {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  } catch (e) {
    return dateTimeString;
  }
}

const Modal = ({ children, isOpen }) => {
  if (!isOpen) return null;
  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center">
      <div className="bg-white rounded-lg shadow-xl p-6 w-full max-w-lg mx-4">
        {children}
      </div>
    </div>
  );
};

// Helper to check if a date string is a valid ISO date
function isValidISODate(dateString) {
    if (!dateString) return false;
    const date = parseISO(dateString);
    return date instanceof Date && !isNaN(date);
}

const Shifts = () => {
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
    const [feedbackMessage, setFeedbackMessage] = useState({ text: '', type: '' });
    const [isViewUsersModalOpen, setViewUsersModalOpen] = useState(false);
    const [selectedShiftUsers, setSelectedShiftUsers] = useState([]);
    const [selectedShift, setSelectedShift] = useState(null);
    const [showDeleted, setShowDeleted] = useState(false);
    const [isEditModalOpen, setEditModalOpen] = useState(false);
    const [selectedShiftToEdit, setSelectedShiftToEdit] = useState(null);
    const [editForm, setEditForm] = useState({
        name: '',
        description: '',
        startTime: '',
        durationHours: 1,
        mode: '0', // '0' for Open, '1' for Closed
        autoRepeatDaily: false,
    });
    const [editFormErrors, setEditFormErrors] = useState({});
    const [isDeleteModalOpen, setDeleteModalOpen] = useState(false);
    const [shiftToDelete, setShiftToDelete] = useState(null);
    const [isAddModalOpen, setAddModalOpen] = useState(false);
    const [addForm, setAddForm] = useState({
        name: '',
        description: '',
        startTime: '',
        durationHours: 1,
        mode: '0', // '0' for Open, '1' for Closed
        autoRepeatDaily: false,
    });
    const [addFormErrors, setAddFormErrors] = useState({});
    const [addLoading, setAddLoading] = useState(false);
    const [addError, setAddError] = useState('');
    const [addSuccess, setAddSuccess] = useState('');
    const [isLogsModalOpen, setLogsModalOpen] = useState(false);
    const [logsShift, setLogsShift] = useState(null);
    const [userDetails, setUserDetails] = useState({});

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
            const fetchFunction = deleted ? fetchDeletedShifts : fetchShifts;
            const data = await fetchFunction(page, pagination.pageSize);
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
        loadData(pagination.page, showDeleted);
        // eslint-disable-next-line
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

    const handleViewUsersClick = (shift) => {
        setSelectedShift(shift);
        setSelectedShiftUsers(shift.assignedUsers || []);
        setViewUsersModalOpen(true);
    };

    const handleEditClick = (shift) => {
        setSelectedShiftToEdit(shift);
        setEditForm({
            name: shift.name || '',
            description: shift.description || '',
            startTime: shift.startTime ? shift.startTime.slice(0, 16) : '', // for datetime-local input
            durationHours: shift.durationMinutes ? Math.round(shift.durationMinutes / 60) : 1, // Convert minutes to hours
            mode: String(shift.mode || '0'), // Ensure mode is a string for the select
            autoRepeatDaily: !!shift.autoRepeatDaily,
        });
        setEditFormErrors({});
        setEditModalOpen(true);
    };

    const validateForm = (formData) => {
        const errors = {};
        if (!formData.name?.trim()) errors.name = 'Name is required';
        if (!formData.startTime) errors.startTime = 'Start time is required';
        if (formData.durationHours <= 0 || !Number.isInteger(Number(formData.durationHours))) {
            errors.durationHours = 'Duration must be a positive whole number';
        }
        return errors;
    };

    const handleEditFormChange = (e) => {
        const { name, value, type, checked } = e.target;
        
        // Handle numeric inputs
        let processedValue = value;
        if (name === 'durationHours') {
            processedValue = value === '' ? '' : Math.max(1, Math.floor(Number(value)));
        }
        
        const updatedForm = {
            ...editForm,
            [name]: type === 'checkbox' ? checked : processedValue,
        };
        
        setEditForm(updatedForm);
        
        // Clear error for the current field when user types
        if (editFormErrors[name]) {
            const errors = { ...editFormErrors };
            delete errors[name];
            setEditFormErrors(errors);
        }
    };

    const handleEditFormSubmit = async (e) => {
        e.preventDefault();
        if (!selectedShiftToEdit) return;
        
        // Validate form
        const errors = validateForm(editForm);
        if (Object.keys(errors).length > 0) {
            setEditFormErrors(errors);
            return;
        }
        
        try {
            // Convert startTime to ISO string and hours to minutes
            const payload = {
                ...editForm,
                startTime: editForm.startTime ? new Date(editForm.startTime).toISOString() : '',
                durationMinutes: Math.round(Number(editForm.durationHours) * 60), // Convert hours to minutes
                mode: Number(editForm.mode) // Ensure mode is a number
            };
            
            await updateShift(selectedShiftToEdit.id, payload);
            showMessage('Shift updated successfully!', 'success');
            setEditModalOpen(false);
            setSelectedShiftToEdit(null);
            setEditFormErrors({});
            loadData(pagination.page, showDeleted);
        } catch (err) {
            showMessage(err?.response?.data?.message || 'Failed to update shift.', 'error');
        }
    };

    const handleEditModalClose = () => {
        setEditModalOpen(false);
        setSelectedShiftToEdit(null);
        setEditFormErrors({});
    };

    const handleDeleteClick = (shift) => {
        setShiftToDelete(shift);
        setDeleteModalOpen(true);
    };

    const handleConfirmDelete = async () => {
        if (!shiftToDelete) return;
        try {
            await deleteShift(shiftToDelete.id);
            showMessage('Shift deleted successfully!', 'success');
            setDeleteModalOpen(false);
            setShiftToDelete(null);
            loadData(pagination.page, showDeleted);
        } catch (err) {
            showMessage('Failed to delete shift.', 'error');
        }
    };

    const handleCancelDelete = () => {
        setDeleteModalOpen(false);
        setShiftToDelete(null);
    };

    const handleAddClick = () => {
        setAddForm({
            name: '',
            description: '',
            startTime: '',
            durationMinutes: 0,
            mode: 0,
            autoRepeatDaily: false,
        });
        setAddError('');
        setAddSuccess('');
        setAddModalOpen(true);
    };

    const handleAddFormChange = (e) => {
        const { name, value, type, checked } = e.target;
        
        // Handle numeric inputs
        let processedValue = value;
        if (name === 'durationHours') {
            processedValue = value === '' ? '' : Math.max(1, Math.floor(Number(value)));
        }
        
        const updatedForm = {
            ...addForm,
            [name]: type === 'checkbox' ? checked : processedValue,
        };
        
        setAddForm(updatedForm);
        
        // Clear error for the current field when user types
        if (addFormErrors[name]) {
            const errors = { ...addFormErrors };
            delete errors[name];
            setAddFormErrors(errors);
        }
    };

    const handleAddFormSubmit = async (e) => {
        e.preventDefault();
        
        // Validate form
        const errors = validateForm(addForm);
        if (Object.keys(errors).length > 0) {
            setAddFormErrors(errors);
            return;
        }
        
        setAddLoading(true);
        setAddError('');
        setAddSuccess('');
        
        try {
            const payload = {
                name: addForm.name.trim(),
                description: addForm.description.trim(),
                startTime: addForm.startTime ? new Date(addForm.startTime).toISOString() : '',
                durationMinutes: Math.round(Number(addForm.durationHours) * 60), // Convert hours to minutes
                mode: Number(addForm.mode), // Ensure mode is a number
                autoRepeatDaily: addForm.autoRepeatDaily,
            };
            
            await createShift(payload);
            setAddSuccess('Shift created successfully!');
            setAddFormErrors({});
            loadData(pagination.page, showDeleted);
            
            // Reset form but keep it open for adding another shift
            setAddForm({
                name: '',
                description: '',
                startTime: '',
                durationHours: 1,
                mode: '0',
                autoRepeatDaily: false,
            });
            
        } catch (err) {
            setAddError(err?.response?.data?.message || err.message || 'Failed to create shift.');
        } finally {
            setAddLoading(false);
        }
    };

    const handleAddModalClose = () => {
        setAddModalOpen(false);
        setAddFormErrors({});
        // Reset form when closing
        setAddForm({
            name: '',
            description: '',
            startTime: '',
            durationHours: 1,
            mode: '0',
            autoRepeatDaily: false,
        });
    };

    const handleLogsClick = (shift) => {
        setLogsShift(shift);
        if (shift?.createdBy) loadUserDetails(shift.createdBy);
        if (shift?.updatedBy) loadUserDetails(shift.updatedBy);
        setLogsModalOpen(true);
    };

    const handleLogsModalClose = () => {
        setLogsModalOpen(false);
        setLogsShift(null);
    };

    // Fetch user email by ID and cache it
    const loadUserDetails = async (userId) => {
        if (!userId || userDetails[userId]) return;
        try {
            const user = await fetchUserById(userId);
            setUserDetails(prev => ({ ...prev, [userId]: user.email }));
        } catch (error) {
            setUserDetails(prev => ({ ...prev, [userId]: 'Unknown' }));
        }
    };
    
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
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-4">
        <h2 className="text-xl md:text-2xl font-bold text-gray-800">Shifts</h2>
        <button
          className="flex items-center gap-2 px-4 py-2 bg-amber-500 text-white rounded-lg hover:bg-amber-600 transition-colors shadow"
          onClick={() => setAddModalOpen(true)}
        >
          <PlusCircle size={18} />
          <span>Add Shift</span>
        </button>
      </div>

      {feedbackMessage.text && (
        <div
          className={`p-3 rounded-lg mb-4 text-center text-sm font-medium ${
            feedbackMessage.type === 'success'
              ? 'bg-green-100 text-green-800'
              : 'bg-red-100 text-red-800'
          }`}
        >
          {feedbackMessage.text}
        </div>
      )}

      <div className="flex items-center gap-2 mb-4">
        <input
          id="show-deleted"
          type="checkbox"
          checked={showDeleted}
          onChange={(e) => {
            setShowDeleted(e.target.checked);
            setPagination((p) => ({ ...p, page: 1 }));
          }}
          className="mr-2"
        />
        <label
          htmlFor="show-deleted"
          className="text-sm font-medium text-gray-700"
        >
          Show Deleted
        </label>
      </div>

      {/* Responsive table */}
      <div className="overflow-x-auto">
        <table className="hidden md:table min-w-[800px] w-full divide-y divide-gray-200">
          <thead className="bg-gray-800">
            <tr>
              <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">
                Name
              </th>
              <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">
                Description
              </th>
              <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">
                Start
              </th>
              <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">
                End
              </th>
              <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">
                Mode
              </th>
              <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">
                Active
              </th>
              <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">
                Users
              </th>
              <th className="px-6 py-4 text-left text-xs font-semibold text-white uppercase tracking-wider">
                Actions
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
                  {shift.description}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500">
                  {isValidDateString(shift.startTime)
                    ? format(parseISO(shift.startTime), 'PPP p')
                    : formatTimeOnlyString(shift.startTime)}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500">
                  {isValidDateString(shift.endTime)
                    ? format(parseISO(shift.endTime), 'PPP p')
                    : formatTimeOnlyString(shift.endTime)}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500">
                  {shift.mode}
                </td>
                <td className="px-6 py-4 text-sm">
                  <span
                    className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                      shift.isActive
                        ? 'bg-green-100 text-green-800'
                        : 'bg-red-100 text-red-800'
                    }`}
                  >
                    {shift.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className="px-6 py-4 text-sm text-gray-500">
                  <button
                    onClick={() => handleViewUsersClick(shift)}
                    className="flex items-center text-amber-500 hover:text-amber-600 transition-colors"
                    title="View Users"
                  >
                    <FileText size={18} className="mr-1" />
                    <span>{shift.assignedUsersCount || 0}</span>
                  </button>
                </td>
                <td className="px-6 py-4 text-sm font-medium space-x-2">
                  {!showDeleted && (
                    <>
                        <button className="text-gray-600 hover:text-gray-900 transition-colors" title="Logs" onClick={() => handleLogsClick(shift)}>
                            <FileText size={18} />
                        </button>
                      <button
                        className="text-amber-600 hover:text-amber-900 transition-colors"
                        title="Edit Shift"
                        onClick={() => handleEditClick(shift)}
                      >
                        <Edit size={18} />
                      </button>
                      <button
                        className="text-red-600 hover:text-red-800 transition-colors"
                        title="Delete Shift"
                        onClick={() => handleDeleteClick(shift)}
                      >
                        <Trash2 size={18} />
                      </button>
                    </>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {/* Mobile card view */}
        <div className="space-y-4 md:hidden">
          {shifts.map((shift) => (
            <div
              key={shift.id}
              className="border rounded-lg p-4 shadow-sm bg-white space-y-2"
            >
              <div className="flex justify-between items-center">
                <h3 className="font-semibold text-gray-900">{shift.name}</h3>
                <span
                  className={`px-2 py-1 text-xs rounded-full ${
                    shift.isActive
                      ? 'bg-green-100 text-green-800'
                      : 'bg-red-100 text-red-800'
                  }`}
                >
                  {shift.isActive ? 'Active' : 'Inactive'}
                </span>
              </div>
              <p className="text-sm text-gray-600">{shift.description}</p>
              <div className="text-sm text-gray-500">
                <p>
                  <span className="font-medium text-gray-700">Start:</span>{' '}
                  {isValidDateString(shift.startTime)
                    ? format(parseISO(shift.startTime), 'PPP p')
                    : formatTimeOnlyString(shift.startTime)}
                </p>
                <p>
                  <span className="font-medium text-gray-700">End:</span>{' '}
                  {isValidDateString(shift.endTime)
                    ? format(parseISO(shift.endTime), 'PPP p')
                    : formatTimeOnlyString(shift.endTime)}
                </p>
                <p>
                  <span className="font-medium text-gray-700">Mode:</span>{' '}
                  {shift.mode}
                </p>
              </div>
              <div className="flex justify-between items-center pt-2">
                <button
                  onClick={() => handleViewUsersClick(shift)}
                  className="flex items-center text-amber-500 hover:text-amber-600 transition-colors text-sm"
                >
                  <FileText size={16} className="mr-1" />
                  <span>{shift.assignedUsersCount || 0} Users</span>
                </button>
                <div className="flex space-x-3">
                  {!showDeleted && (
                    <>
                      <button
                        className="text-amber-600 hover:text-amber-900 transition-colors"
                        onClick={() => handleEditClick(shift)}
                      >
                        <Edit size={16} />
                      </button>
                      <button
                        className="text-red-600 hover:text-red-800 transition-colors"
                        onClick={() => handleDeleteClick(shift)}
                      >
                        <Trash2 size={16} />
                      </button>
                    </>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Pagination */}
      <div className="flex flex-wrap justify-center md:justify-between items-center mt-4 text-sm text-gray-700 gap-2">
        <div>
          <p>
            <span className="font-medium">
              {pagination.page * pagination.pageSize - pagination.pageSize + 1}
            </span>{' '}
            to{' '}
            <span className="font-medium">
              {Math.min(pagination.page * pagination.pageSize, pagination.totalCount)}
            </span>{' '}
            of <span className="font-medium">{pagination.totalCount}</span> rows
          </p>
        </div>
        <div className="flex items-center gap-1 flex-wrap">
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
            className="p-2 border rounded-md text-gray-500 hover:bg-gray-50 disabled:opacity-50"
          >
            <ChevronRight size={16} />
          </button>
        </div>
      </div>

      {/* Modals (unchanged except responsive width already applied in <Modal>) */}
        <Modal isOpen={isViewUsersModalOpen}>
            <h3 className="text-lg font-bold mb-4">Users Assigned to Shift "{selectedShift?.name}"</h3>
            {selectedShiftUsers.length > 0 ? (
                <ul className="space-y-2">
                    {selectedShiftUsers.map(user => (
                        <li key={user.id} className="bg-gray-100 p-3 rounded-md text-sm font-medium">
                            {user.firstName} {user.lastName} ({user.email})
                        </li>
                    ))}
                </ul>
            ) : (
                <p className="text-sm text-gray-500">No users assigned to this shift.</p>
            )}
            <div className="flex justify-end pt-4">
                <button
                    type="button"
                    onClick={() => setViewUsersModalOpen(false)}
                    className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                >
                    Close
                </button>
            </div>
        </Modal>

        <Modal isOpen={isEditModalOpen}>
            <h3 className="text-lg font-bold mb-4">Edit Shift</h3>
            <form onSubmit={handleEditFormSubmit} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium text-gray-700">Name</label>
                    <div>
                    <input 
                        type="text" 
                        name="name" 
                        value={editForm.name} 
                        onChange={handleEditFormChange} 
                        className={`mt-1 block w-full border ${editFormErrors.name ? 'border-red-500' : 'border-gray-300'} rounded-md p-2`} 
                        required 
                    />
                    {editFormErrors.name && <p className="mt-1 text-sm text-red-600">{editFormErrors.name}</p>}
                </div>
                </div>
                <div>
                    <label className="block text-sm font-medium text-gray-700">Description</label>
                    <input type="text" name="description" value={editForm.description} onChange={handleEditFormChange} className="mt-1 block w-full border border-gray-300 rounded-md p-2" required />
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <label className="block text-sm font-medium text-gray-700">Start Date</label>
                        <input 
                            type="date" 
                            name="startDate"
                            value={editForm.startTime ? editForm.startTime.split('T')[0] : ''}
                            onChange={(e) => {
                                const time = editForm.startTime ? editForm.startTime.split('T')[1] : '09:00';
                                handleEditFormChange({
                                    target: {
                                        name: 'startTime',
                                        value: e.target.value ? `${e.target.value}T${time}` : ''
                                    }
                                });
                            }}
                            className={`mt-1 block w-full border ${editFormErrors.startTime ? 'border-red-500' : 'border-gray-300'} rounded-md p-2`}
                            required
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-gray-700">Start Time</label>
                        <input 
                            type="time" 
                            name="startTimeOnly"
                            value={editForm.startTime ? editForm.startTime.split('T')[1]?.substring(0, 5) || '09:00' : '09:00'}
                            onChange={(e) => {
                                const date = editForm.startTime ? editForm.startTime.split('T')[0] : new Date().toISOString().split('T')[0];
                                handleEditFormChange({
                                    target: {
                                        name: 'startTime',
                                        value: `${date}T${e.target.value}`
                                    }
                                });
                            }}
                            className={`mt-1 block w-full border ${editFormErrors.startTime ? 'border-red-500' : 'border-gray-300'} rounded-md p-2`}
                            required
                        />
                    </div>
                    {editFormErrors.startTime && <p className="mt-1 text-sm text-red-600 col-span-2">{editFormErrors.startTime}</p>}
                </div>
                <div>
                    <div>
                        <label className="block text-sm font-medium text-gray-700">Duration</label>
                        <div className="mt-1 space-y-1">
                            <div className="flex items-center">
                                <div className="relative flex-1">
                                    <input
                                        type="number"
                                        name="durationHours"
                                        value={editForm.durationHours}
                                        onChange={handleEditFormChange}
                                        className={`w-full border ${editFormErrors.durationHours ? 'border-red-500' : 'border-gray-300'} rounded-md p-2 pr-16`}
                                        min="0.5"
                                        step="0.5"
                                        placeholder="e.g. 1.5"
                                        required
                                    />
                                    <div className="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
                                        <span className="text-gray-500 text-sm">
                                            {editForm.durationHours == 1 ? 'hour' : 'hours'}
                                        </span>
                                    </div>
                                </div>
                            </div>
                            <p className="text-xs text-gray-500">
                                {editForm.durationHours === 1 ? '1 hour' : `${editForm.durationHours} hours`} ({Math.floor(editForm.durationHours * 60)} minutes)
                            </p>
                            {editFormErrors.durationHours && (
                                <p className="text-sm text-red-600">{editFormErrors.durationHours}</p>
                            )}
                        </div>
                    </div>
                </div>
                <div>
                    <label className="block text-sm font-medium text-gray-700">Mode</label>
                    <select
                        name="mode"
                        value={editForm.mode}
                        onChange={handleEditFormChange}
                        className="mt-1 block w-full border border-gray-300 rounded-md p-2"
                        required
                    >
                        <option value="0">Open</option>
                        <option value="1">Closed</option>
                    </select>
                </div>
                <div className="flex items-center">
                    <input type="checkbox" name="autoRepeatDaily" checked={editForm.autoRepeatDaily} onChange={handleEditFormChange} className="mr-2" />
                    <label className="text-sm font-medium text-gray-700">Auto Repeat Daily</label>
                </div>
                <div className="flex justify-end gap-2 pt-2">
                    <button type="button" onClick={handleEditModalClose} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">Cancel</button>
                    <button type="submit" className="px-4 py-2 bg-amber-500 text-white rounded-md text-sm font-medium hover:bg-amber-600">Save</button>
                </div>
            </form>
        </Modal>

        <Modal isOpen={isDeleteModalOpen}>
            <h3 className="text-lg font-bold mb-4 text-red-700">Confirm Delete</h3>
            <p className="mb-4">Are you sure you want to delete the shift <span className="font-semibold">{shiftToDelete?.name}</span>? This action cannot be undone.</p>
            <div className="flex justify-end gap-2">
                <button onClick={handleCancelDelete} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">Cancel</button>
                <button onClick={handleConfirmDelete} className="px-4 py-2 bg-red-600 text-white rounded-md text-sm font-medium hover:bg-red-700">Delete</button>
            </div>
        </Modal>

        <Modal isOpen={isAddModalOpen}>
            <h3 className="text-lg font-bold mb-4">Add Shift</h3>
            <form onSubmit={handleAddFormSubmit} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium text-gray-700">Name</label>
                    <div>
                    <input 
                        type="text" 
                        name="name" 
                        value={addForm.name} 
                        onChange={handleAddFormChange} 
                        className={`mt-1 block w-full border ${addFormErrors.name ? 'border-red-500' : 'border-gray-300'} rounded-md p-2`} 
                        required 
                        disabled={addLoading} 
                    />
                    {addFormErrors.name && <p className="mt-1 text-sm text-red-600">{addFormErrors.name}</p>}
                </div>
                </div>
                <div>
                    <label className="block text-sm font-medium text-gray-700">Description</label>
                    <input type="text" name="description" value={addForm.description} onChange={handleAddFormChange} className="mt-1 block w-full border border-gray-300 rounded-md p-2" required disabled={addLoading} />
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <label className="block text-sm font-medium text-gray-700">Start Date</label>
                        <input 
                            type="date" 
                            name="startDate"
                            value={addForm.startTime ? addForm.startTime.split('T')[0] : ''}
                            onChange={(e) => {
                                const time = addForm.startTime ? addForm.startTime.split('T')[1] : '09:00';
                                handleAddFormChange({
                                    target: {
                                        name: 'startTime',
                                        value: e.target.value ? `${e.target.value}T${time}` : ''
                                    }
                                });
                            }}
                            className={`mt-1 block w-full border ${addFormErrors.startTime ? 'border-red-500' : 'border-gray-300'} rounded-md p-2`}
                            required
                            disabled={addLoading}
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-gray-700">Start Time</label>
                        <input 
                            type="time" 
                            name="startTimeOnly"
                            value={addForm.startTime ? addForm.startTime.split('T')[1]?.substring(0, 5) || '09:00' : '09:00'}
                            onChange={(e) => {
                                const date = addForm.startTime ? addForm.startTime.split('T')[0] : new Date().toISOString().split('T')[0];
                                handleAddFormChange({
                                    target: {
                                        name: 'startTime',
                                        value: `${date}T${e.target.value}`
                                    }
                                });
                            }}
                            className={`mt-1 block w-full border ${addFormErrors.startTime ? 'border-red-500' : 'border-gray-300'} rounded-md p-2`}
                            required
                            disabled={addLoading}
                        />
                    </div>
                    {addFormErrors.startTime && <p className="mt-1 text-sm text-red-600 col-span-2">{addFormErrors.startTime}</p>}
                </div>
                <div>
                    <div>
                        <label className="block text-sm font-medium text-gray-700">Duration</label>
                        <div className="mt-1 space-y-1">
                            <div className="flex items-center">
                                <div className="relative flex-1">
                                    <input
                                        type="number"
                                        name="durationHours"
                                        value={addForm.durationHours}
                                        onChange={handleAddFormChange}
                                        className={`w-full border ${addFormErrors.durationHours ? 'border-red-500' : 'border-gray-300'} rounded-md p-2 pr-16`}
                                        min="0.5"
                                        step="0.5"
                                        placeholder="e.g. 1.5"
                                        required
                                        disabled={addLoading}
                                    />
                                    <div className="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
                                        <span className="text-gray-500 text-sm">
                                            {addForm.durationHours == 1 ? 'hour' : 'hours'}
                                        </span>
                                    </div>
                                </div>
                            </div>
                            <p className="text-xs text-gray-500">
                                {addForm.durationHours === 1 ? '1 hour' : `${addForm.durationHours} hours`} ({Math.floor(addForm.durationHours * 60)} minutes)
                            </p>
                            {addFormErrors.durationHours && (
                                <p className="text-sm text-red-600">{addFormErrors.durationHours}</p>
                            )}
                        </div>
                    </div>
                </div>
                <div>
                    <label className="block text-sm font-medium text-gray-700">Mode</label>
                    <select
                        name="mode"
                        value={addForm.mode}
                        onChange={handleAddFormChange}
                        className="mt-1 block w-full border border-gray-300 rounded-md p-2"
                        required
                        disabled={addLoading}
                    >
                        <option value="0">Open</option>
                        <option value="1">Closed</option>
                    </select>
                </div>
                <div className="flex items-center">
                    <input type="checkbox" name="autoRepeatDaily" checked={addForm.autoRepeatDaily} onChange={handleAddFormChange} className="mr-2" disabled={addLoading} />
                    <label className="text-sm font-medium text-gray-700">Auto Repeat Daily</label>
                </div>
                {addError && <div className="text-red-600 text-sm font-medium">{addError}</div>}
                {addSuccess && <div className="text-green-600 text-sm font-medium">{addSuccess}</div>}
                <div className="flex justify-end gap-2 pt-2">
                    <button type="button" onClick={handleAddModalClose} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">Close</button>
                    <button type="submit" className="px-4 py-2 bg-amber-500 text-white rounded-md text-sm font-medium hover:bg-amber-600" disabled={addLoading}>Save</button>
                </div>
            </form>
        </Modal>

        {/* Logs Modal */}
        <Modal isOpen={isLogsModalOpen}>
            <div className="bg-gray-100 p-6 rounded-lg shadow-md">
                <h3 className="text-lg font-bold mb-4">Audit Logs for "{logsShift?.name}"</h3>
                {logsShift && (
                    <div className="space-y-4">
                        {/* Deleted badge if applicable */}
                        {logsShift.isDeleted && (
                            <div className="flex items-center mb-2">
                                <span className="bg-red-100 text-red-700 px-3 py-1 rounded-full text-xs font-semibold">Deleted Shift</span>
                            </div>
                        )}
                        <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                            <p className="font-semibold text-gray-700">Created At:</p>
                            <p className="text-gray-600">{isValidISODate(logsShift.createdAt) ? format(parseISO(logsShift.createdAt), "PPP p") : '-'}</p>
                        </div>
                        <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                            <p className="font-semibold text-gray-700">Created By:</p>
                            <p className="text-gray-600">{userDetails[logsShift.createdBy] || logsShift.createdBy || 'N/A'}</p>
                        </div>
                        <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                            <p className="font-semibold text-gray-700">Updated At:</p>
                            <p className="text-gray-600">{
                                isValidISODate(logsShift.updatedAt) ? format(parseISO(logsShift.updatedAt), "PPP p") : '-'
                            }</p>
                        </div>
                        <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                            <p className="font-semibold text-gray-700">Updated By:</p>
                            <p className="text-gray-600">{userDetails[logsShift.updatedBy] || logsShift.updatedBy || 'N/A'}</p>
                        </div>
                    </div>
                )}
                <div className="flex justify-end mt-6">
                    <button
                        type="button"
                        onClick={handleLogsModalClose}
                        className="px-4 py-2 bg-amber-500 hover:bg-amber-600 text-white rounded-md text-sm font-medium transition-colors duration-200"
                    >
                        Close
                    </button>
                </div>
            </div>
        </Modal>
    </div>
  );
};

export default Shifts;
