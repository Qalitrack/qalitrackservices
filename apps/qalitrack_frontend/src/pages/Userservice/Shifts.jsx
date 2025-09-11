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
  const match = timeString.match(/^(\d{2}):(\d{2})(:(\d{2}))?(\.(\d+))?$/);
  if (!match) return timeString;
  const [, hours, minutes, , seconds] = match;
  return `${hours}:${minutes}${seconds ? ':' + seconds : ''}`;
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
    durationMinutes: 0,
    mode: 0,
    autoRepeatDaily: false,
  });
  const [isDeleteModalOpen, setDeleteModalOpen] = useState(false);
  const [shiftToDelete, setShiftToDelete] = useState(null);
  const [isAddModalOpen, setAddModalOpen] = useState(false);
  const [addForm, setAddForm] = useState({
    name: '',
    description: '',
    startTime: '',
    durationMinutes: 0,
    mode: 0,
    autoRepeatDaily: false,
  });
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
      startTime: shift.startTime ? shift.startTime.slice(0, 16) : '',
      durationMinutes: shift.durationMinutes || 0,
      mode: shift.mode || 0,
      autoRepeatDaily: !!shift.autoRepeatDaily,
    });
    setEditModalOpen(true);
  };

  const handleDeleteClick = (shift) => {
    setShiftToDelete(shift);
    setDeleteModalOpen(true);
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
    </div>
  );
};

export default Shifts;
