import React, { useState, useEffect } from 'react';
import { updateShift } from '../../../api/helpers/UserService/Shifts/Shifts';
import { format, parseISO } from 'date-fns';
import { X, Clock, Calendar, Users, Check, Plus, Loader2 } from 'lucide-react';

// Enums from the server
const ShiftMode = {
    Open: 0,    // Anyone can log in
    Strict: 1   // Only allowed users can log in
};

const RecurrenceType = {
    None: 0,
    Daily: 1,
    Weekly: 2,
    Monthly: 3,
    Custom: 4
};

const ShiftEdit = ({ isOpen, onClose, shift, onSave }) => {
    const [formData, setFormData] = useState({
        id: '',
        name: '',
        description: '',
        startTime: '',
        endTime: '',
        mode: ShiftMode.Open,
        startDate: new Date().toISOString().split('T')[0],
        endDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
        requiredStaffCount: 1,
        recurrenceType: RecurrenceType.None,
        recurrenceInterval: 1,
        customDays: [],
        exceptionDates: []
    });

    const [newExceptionDate, setNewExceptionDate] = useState('');

    const [errors, setErrors] = useState({});
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    // Format time from ISO string to HH:MM format for input fields
    const formatTimeForInput = (timeString) => {
        if (!timeString) return '';
        try {
            // If it's already in HH:MM format, return as is
            if (typeof timeString === 'string' && timeString.match(/^\d{2}:\d{2}$/)) {
                return timeString;
            }
            // If it's a full ISO date string, extract the time part
            const date = new Date(timeString);
            if (isNaN(date.getTime())) return ''; // Invalid date
            
            const hours = String(date.getHours()).padStart(2, '0');
            const minutes = String(date.getMinutes()).padStart(2, '0');
            return `${hours}:${minutes}`;
        } catch (e) {
            return '';
        }
    };

    useEffect(() => {
        if (shift) {
            // Parse the shift data to match our form structure
            const startDate = shift.startDate ? new Date(shift.startDate).toISOString().split('T')[0] : '';
            const endDate = shift.endDate ? new Date(shift.endDate).toISOString().split('T')[0] : '';
            
            // Format times for the input fields
            const formattedStartTime = formatTimeForInput(shift.startTime);
            const formattedEndTime = formatTimeForInput(shift.endTime);

            setFormData({
                id: shift.id || '',
                name: shift.name || '',
                description: shift.description || '',
                startTime: formattedStartTime,
                endTime: formattedEndTime,
                mode: shift.mode ?? ShiftMode.Open,
                startDate: startDate || new Date().toISOString().split('T')[0],
                endDate: endDate || new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
                requiredStaffCount: shift.requiredStaffCount || 1,
                recurrenceType: shift.recurrenceType ?? RecurrenceType.None,
                recurrenceInterval: shift.recurrenceInterval || 1,
                customDays: Array.isArray(shift.customDays) ? [...shift.customDays] : [],
                exceptionDates: Array.isArray(shift.exceptionDates) 
                    ? shift.exceptionDates.map(d => new Date(d).toISOString().split('T')[0])
                    : []
            });
            setNewExceptionDate('');
        }
    }, [shift]);

    const validateForm = (data) => {
        const newErrors = {};
        const now = new Date();
        now.setHours(0, 0, 0, 0); // Set to start of day for date comparison

        // Required fields
        if (!data.name?.trim()) newErrors.name = 'Shift name is required';
        if (!data.startTime) newErrors.startTime = 'Start time is required';
        if (!data.endTime) newErrors.endTime = 'End time is required';
        if (!data.startDate) newErrors.startDate = 'Start date is required';
        if (!data.endDate) newErrors.endDate = 'End date is required';

        // Numeric validations
        if (data.requiredStaffCount < 1) {
            newErrors.requiredStaffCount = 'At least 1 staff member is required';
        }

        // Date validations
        const startDate = new Date(data.startDate);
        const endDate = new Date(data.endDate);
        const today = new Date(now);
        today.setHours(0, 0, 0, 0);

        if (startDate.toString() === 'Invalid Date') {
            newErrors.startDate = 'Invalid start date';
        } else if (startDate < today) {
            newErrors.startDate = 'Start date cannot be in the past';
        }

        if (endDate.toString() === 'Invalid Date') {
            newErrors.endDate = 'Invalid end date';
        } else if (endDate < startDate) {
            newErrors.endDate = 'End date must be after start date';
        }

        // Time validation
        if (data.startTime && data.endTime) {
            const [startHours, startMinutes] = data.startTime.split(':').map(Number);
            const [endHours, endMinutes] = data.endTime.split(':').map(Number);

            if (startHours > 23 || startMinutes > 59 || isNaN(startHours) || isNaN(startMinutes)) {
                newErrors.startTime = 'Invalid start time';
            }

            if (endHours > 23 || endMinutes > 59 || isNaN(endHours) || isNaN(endMinutes)) {
                newErrors.endTime = 'Invalid end time';
            }

            // Only validate time order if dates are the day
            if (startDate.toDateString() === endDate.toDateString() && !newErrors.startTime && !newErrors.endTime) {
                const startTime = new Date(startDate);
                startTime.setHours(startHours, startMinutes);
                
                const endTime = new Date(endDate);
                endTime.setHours(endHours, endMinutes);
                
                if (endTime <= startTime) {
                    newErrors.endTime = 'End time must be after start time';
                }
            }
        }

        // Recurrence validations
        if (data.recurrenceType !== RecurrenceType.None) {
            if (data.recurrenceInterval < 1) {
                newErrors.recurrenceInterval = 'Interval must be at least 1';
            }

            if (data.recurrenceType === RecurrenceType.Custom &&
                (!data.customDays || data.customDays.length === 0)) {
                newErrors.customDays = 'At least one day must be selected';
            }
        }

        return newErrors;
    };

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;

        // Clear any previous errors for this field
        if (errors[name]) {
            setErrors(prev => ({
                ...prev,
                [name]: undefined
            }));
        }

        // Clear related errors for date/time fields
        if (name === 'startDate' || name === 'endDate') {
            setErrors(prev => {
                const newErrors = { ...prev };
                delete newErrors[`${name}Invalid`];
                delete newErrors[`${name === 'startDate' ? 'endDate' : 'startDate'}`];
                return newErrors;
            });
        }

        setFormData(prev => {
            // Handle different input types
            let newValue;

            switch (type) {
                case 'checkbox':
                    newValue = checked;
                    break;
                case 'number':
                    newValue = value === '' ? '' : parseInt(value, 10);
                    break;
                case 'date':
                    // Store date in YYYY-MM-DD format
                    newValue = value;
                    
                    // If changing start date and end date is before new start date, update end date
                    if (name === 'startDate' && prev.endDate) {
                        const newStartDate = new Date(value);
                        const currentEndDate = new Date(prev.endDate);
                        
                        if (currentEndDate < newStartDate) {
                            // Set end date to be the same as start date if it's before
                            return {
                                ...prev,
                                [name]: value,
                                endDate: value
                            };
                        }
                    }
                    break;
                case 'time':
                    // Ensure time is in HH:MM format
                    newValue = value;
                    
                    // If changing start time and end time is before new start time, update end time
                    if (name === 'startTime' && value && prev.endTime) {
                        const [startHours, startMins] = value.split(':').map(Number);
                        const [endHours, endMins] = prev.endTime.split(':');
                        
                        const startTotal = startHours * 60 + startMins;
                        const endTotal = parseInt(endHours, 10) * 60 + parseInt(endMins, 10);
                        
                        if (endTotal <= startTotal) {
                            // Set end time to be 1 hour after start time
                            const newEndTime = new Date();
                            newEndTime.setHours(startHours + 1, startMins);
                            
                            // Update end time in the form
                            const newEndTimeStr = `${String(newEndTime.getHours()).padStart(2, '0')}:${String(newEndTime.getMinutes()).padStart(2, '0')}`;
                            
                            return {
                                ...prev,
                                [name]: value,
                                endTime: localToUTC(newEndTime)
                            };
                        }
                    }
                    break;
                default:
                    newValue = value;
            }

            // Special handling for numeric fields
            if (['requiredStaffCount', 'mode', 'recurrenceType', 'recurrenceInterval', 'status'].includes(name)) {
                newValue = value === '' ? '' : parseInt(value, 10);
            }

            // Update form data
            return {
                ...prev,
                [name]: newValue !== undefined ? newValue : value
            };
        });
    
    };

    const handleCustomDayChange = (day) => {
        setFormData(prev => ({
            ...prev,
            customDays: prev.customDays.includes(day)
                ? prev.customDays.filter(d => d !== day)
                : [...prev.customDays, day].sort((a, b) => a - b)
        }));

        // Clear custom days error if any
        if (errors.customDays) {
            setErrors(prev => ({
                ...prev,
                customDays: undefined
            }));
        }
    };

    const handleAddExceptionDate = () => {
        if (!newExceptionDate) return;

        const dateStr = new Date(newExceptionDate).toISOString().split('T')[0];

        if (!formData.exceptionDates.includes(dateStr)) {
            setFormData(prev => ({
                ...prev,
                exceptionDates: [...prev.exceptionDates, dateStr].sort()
            }));

            setNewExceptionDate('');
        }
    };

    const handleRemoveExceptionDate = (dateToRemove) => {
        setFormData(prev => ({
            ...prev,
            exceptionDates: prev.exceptionDates.filter(d => d !== dateToRemove)
        }));
    };

    // Handle time changes with timezone support
    const handleTimeChange = (e, field) => {
        const { value } = e.target;
        const timeField = field === 'start' ? 'startTime' : 'endTime';
        
        // Clear any previous time-related errors
        setErrors(prev => {
            const newErrors = { ...prev };
            delete newErrors[`${timeField}Invalid`];
            delete newErrors[`${field}Time`];
            return newErrors;
        });

        // Create a date object with the current date and the new time
        const date = new Date();
        const [hours, minutes] = value.split(':'); 
        
        // Only update if we have valid hours and minutes
        if (hours !== undefined && minutes !== undefined) {
            date.setHours(parseInt(hours, 10), parseInt(minutes, 10));
            
            // Convert to ISO string to maintain timezone information
            const timeString = localToUTC(date);
            
            setFormData(prev => ({
                ...prev,
                [timeField]: timeString,
                // Update duration if both times are set
                ...(field === 'start' && prev.endTime && {
                    durationMinutes: Math.round((new Date(prev.endTime).getTime() - date.getTime()) / (1000 * 60))
                }),
                ...(field === 'end' && prev.startTime && {
                    durationMinutes: Math.round((date.getTime() - new Date(prev.startTime).getTime()) / (1000 * 60))
                })
            }));
        }
    };

    // Convert local time to ISO string without timezone conversion
    const localToUTC = (date) => {
        if (!date) return null;
        // Format as YYYY-MM-DDTHH:mm:ss without timezone conversion
        return date.getFullYear() + '-' +
            String(date.getMonth() + 1).padStart(2, '0') + '-' +
            String(date.getDate()).padStart(2, '0') + 'T' +
            String(date.getHours()).padStart(2, '0') + ':' +
            String(date.getMinutes()).padStart(2, '0') + ':' +
            String(date.getSeconds()).padStart(2, '0');
    };
    
    // Format time for display in input fields (HH:MM)
    const formatTimeForDisplay = (dateTimeString) => {
        if (!dateTimeString) return '';
        try {
            const date = new Date(dateTimeString);
            return String(date.getHours()).padStart(2, '0') + ':' + 
                   String(date.getMinutes()).padStart(2, '0');
        } catch (e) {
            return '';
        }
    };

    // Format time as HH:MM:SS for API (TimeSpan format)
    const formatTimeForApi = (timeString) => {
        if (!timeString) return '00:00:00';
        try {
            const date = new Date(timeString);
            const hours = String(date.getHours()).padStart(2, '0');
            const minutes = String(date.getMinutes()).padStart(2, '0');
            return `${hours}:${minutes}:00`; // Return in HH:MM:SS format for TimeSpan
        } catch (e) {
            return '00:00:00';
        }
    };

    // Format date as YYYY-MM-DD for API
    const formatDateForApi = (dateString) => {
        if (!dateString) return null;
        const date = new Date(dateString);
        return date.toISOString().split('T')[0];
    };

    // Format date for display in input fields (YYYY-MM-DD)
    const formatDateForInput = (dateString) => {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toISOString().split('T')[0];
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!shift) return;

        // Clear previous messages
        setError('');
        setSuccess('');

        // Validate form
        const formErrors = validateForm(formData);
        if (Object.keys(formErrors).length > 0) {
            setErrors(formErrors);
            return;
        }

        setIsLoading(true);

        try {
            // Format the time values for the API (HH:mm:ss)
            const formattedStartTime = formatTimeForApi(formData.startTime);
            const formattedEndTime = formatTimeForApi(formData.endTime);

            // Prepare the data for submission
            const submissionData = {
                id: shift.id,
                name: formData.name.trim(),
                description: formData.description?.trim() || '',
                startTime: formattedStartTime,
                endTime: formattedEndTime,
                mode: Number(formData.mode),
                startDate: formData.startDate, // Already in YYYY-MM-DD format
                endDate: formData.endDate || null,
                requiredStaffCount: Math.max(1, Number(formData.requiredStaffCount) || 1),
                recurrenceType: Number(formData.recurrenceType),
                recurrenceInterval: Math.max(1, Number(formData.recurrenceInterval) || 1),
                customDays: Array.isArray(formData.customDays)
                    ? formData.customDays.map(Number).filter(n => !isNaN(n) && n >= 0 && n <= 6)
                    : [],
                exceptionDates: Array.isArray(formData.exceptionDates)
                    ? formData.exceptionDates.map(d => new Date(d).toISOString().split('T')[0])
                    : []
            };


            // Call the update API
            const response = await updateShift(shift.id, submissionData);

            if (response && response.success) {
                setSuccess('Shift updated successfully');
                // Refresh the shifts list and close the modal after a short delay
                setTimeout(() => {
                    if (onSave) onSave();
                    onClose();
                }, 1500);
            } else {
                setError(response?.message || 'Failed to update shift');
            }
        } catch (err) {
            setError(err.response?.data?.message || 'An error occurred while updating the shift');
        } finally {
            setIsLoading(false);
        }
    };

    if (!isOpen) return null;

    // Format dates for display
    const formatDisplayDate = (dateStr) => {
        if (!dateStr) return '';
        try {
            return new Date(dateStr).toLocaleDateString();
        } catch (e) {
            return dateStr;
        }
    };

    return (
        <div className="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center p-4 z-50">
            <div className="bg-white rounded-xl shadow-xl w-full max-w-4xl max-h-[90vh] overflow-y-auto">
                {/* Header */}
                <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex justify-between items-center z-10">
                    <h2 className="text-xl font-semibold text-gray-900">Edit Shift</h2>
                    <button
                        onClick={onClose}
                        className="text-gray-400 hover:text-gray-500 transition-colors"
                        disabled={isLoading}
                    >
                        <X className="h-5 w-5" />
                        <span className="sr-only">Close</span>
                    </button>
                </div>

                {/* Messages */}
                <div className="px-6 pt-4">
                    {error && (
                        <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm flex items-start">
                            <span className="flex-shrink-0">⚠️</span>
                            <span className="ml-2">{error}</span>
                        </div>
                    )}

                    {success && (
                        <div className="mb-4 p-3 bg-green-50 border border-green-200 text-green-700 rounded-lg text-sm flex items-start">
                            <Check className="h-4 w-4 flex-shrink-0 mt-0.5" />
                            <span className="ml-2">{success}</span>
                        </div>
                    )}
                </div>

                {/* Form */}
                <form onSubmit={handleSubmit} className="p-6 space-y-6">
                    {/* Basic Information */}
                    <div className="space-y-6">
                        <div>
                            <h3 className="text-lg font-medium text-gray-700 mb-4">Shift Details</h3>
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                {/* Shift Name */}
                                <div>
                                    <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
                                        Shift Name <span className="text-red-500">*</span>
                                    </label>
                                    <input
                                        type="text"
                                        id="name"
                                        name="name"
                                        value={formData.name}
                                        onChange={handleChange}
                                        className={`block w-full rounded-md border shadow-sm ${errors.name ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : 'border-gray-300 focus:ring-blue-500 focus:border-blue-500'} sm:text-sm`}
                                        placeholder="e.g., Morning Shift"
                                        disabled={isLoading}
                                    />
                                    {errors.name && (
                                        <p className="mt-1 text-sm text-red-600">{errors.name}</p>
                                    )}
                                </div>

                                {/* Description */}
                                <div>
                                    <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
                                        Description
                                    </label>
                                    <input
                                        type="text"
                                        id="description"
                                        name="description"
                                        value={formData.description}
                                        onChange={handleChange}
                                        className="block w-full rounded-md border border-gray-300 shadow-sm focus:border-amber-300 focus:ring-amber-500 sm:text-sm"
                                        placeholder="Optional description"
                                        disabled={isLoading}
                                    />
                                </div>
                            </div>
                        </div>

                        {/* Date & Time */}
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            {/* Start Date */}
                            <div>
                                <label htmlFor="startDate" className="block text-sm font-medium text-gray-700 mb-1">
                                    Start Date <span className="text-red-500">*</span>
                                </label>
                                <div className="relative rounded-md shadow-sm">
                                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                        <Calendar className="h-4 w-4 text-gray-400" />
                                    </div>
                                    <input
                                        type="date"
                                        id="startDate"
                                        name="startDate"
                                        value={formData.startDate}
                                        onChange={handleChange}
                                        className={`block w-full pl-10 rounded-md border ${errors.startDate ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : 'border-gray-300 focus:ring-amber-500 focus:border-amber-500'} sm:text-sm`}
                                        disabled={isLoading}
                                    />
                                </div>
                                {errors.startDate && (
                                    <p className="mt-1 text-sm text-red-600">{errors.startDate}</p>
                                )}
                            </div>

                            {/* End Date */}
                            <div>
                                <label htmlFor="endDate" className="block text-sm font-medium text-gray-700 mb-1">
                                    End Date <span className="text-red-500">*</span>
                                </label>
                                <div className="relative rounded-md shadow-sm">
                                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                        <Calendar className="h-4 w-4 text-gray-400" />
                                    </div>
                                    <input
                                        type="date"
                                        id="endDate"
                                        name="endDate"
                                        value={formData.endDate}
                                        onChange={handleChange}
                                        className={`block w-full pl-10 rounded-md border ${errors.endDate ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : 'border-gray-300 focus:ring-amber-500 focus:border-amber-500'} sm:text-sm`}
                                        disabled={isLoading}
                                    />
                                </div>
                                {errors.endDate && (
                                    <p className="mt-1 text-sm text-red-600">{errors.endDate}</p>
                                )}
                            </div>

                            {/* Start Time */}
                            <div>
                                <label htmlFor="startTime" className="block text-sm font-medium text-gray-700 mb-1">
                                    Start Time <span className="text-red-500">*</span>
                                </label>
                                <div className="relative rounded-md shadow-sm">
                                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                        <Clock className="h-4 w-4 text-gray-400" />
                                    </div>
                                    <input
                                        type="time"
                                        id="startTime"
                                        name="startTime"
                                        value={formatTimeForDisplay(formData.startTime)}
                                        onChange={(e) => handleTimeChange(e, 'start')}
                                        className={`block w-full pl-10 rounded-md border ${errors.startTime ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : 'border-gray-300 focus:ring-amber-500 focus:border-amber-500'} sm:text-sm`}
                                        disabled={isLoading}
                                    />
                                </div>
                                {errors.startTime && (
                                    <p className="mt-1 text-sm text-red-600">{errors.startTime}</p>
                                )}
                            </div>

                            {/* End Time */}
                            <div>
                                <label htmlFor="endTime" className="block text-sm font-medium text-gray-700 mb-1">
                                    End Time <span className="text-red-500">*</span>
                                </label>
                                <div className="relative rounded-md shadow-sm">
                                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                        <Clock className="h-4 w-4 text-gray-400" />
                                    </div>
                                    <input
                                        type="time"
                                        id="endTime"
                                        name="endTime"
                                        value={formatTimeForDisplay(formData.endTime)}
                                        onChange={(e) => handleTimeChange(e, 'end')}
                                        className={`block w-full pl-10 rounded-md border ${errors.endTime ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : 'border-gray-300 focus:ring-amber-500 focus:border-amber-500'} sm:text-sm`}
                                        disabled={isLoading}
                                    />
                                </div>
                                {errors.endTime && (
                                    <p className="mt-1 text-sm text-red-600">{errors.endTime}</p>
                                )}
                            </div>
                        </div>

                        {/* Staffing & Mode */}
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            {/* Required Staff Count */}
                            <div>
                                <label htmlFor="requiredStaffCount" className="block text-sm font-medium text-gray-700 mb-1">
                                    Required Staff <span className="text-red-500">*</span>
                                </label>
                                <div className="relative rounded-md shadow-sm">
                                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                        <Users className="h-4 w-4 text-gray-400" />
                                    </div>
                                    <input
                                        type="number"
                                        id="requiredStaffCount"
                                        name="requiredStaffCount"
                                        min="1"
                                        value={formData.requiredStaffCount}
                                        onChange={handleChange}
                                        className={`block w-full pl-10 rounded-md ${errors.requiredStaffCount ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : 'border-gray-300 focus:ring-amber-500 focus:border-amber-500'} sm:text-sm`}
                                        disabled={isLoading}
                                    />
                                </div>
                                {errors.requiredStaffCount && (
                                    <p className="mt-1 text-sm text-red-600">{errors.requiredStaffCount}</p>
                                )}
                            </div>

                            {/* Mode */}
                            <div>
                                <label htmlFor="mode" className="block text-sm font-medium text-gray-700 mb-1">
                                    Shift Mode
                                </label>
                                <select
                                    id="mode"
                                    name="mode"
                                    value={formData.mode}
                                    onChange={handleChange}
                                    className="mt-1 block w-full pl-3 pr-10 py-2 text-base border border-gray-300 focus:outline-none focus:ring-amber-400 focus:border-amber-500 sm:text-sm rounded-md"
                                    disabled={isLoading}
                                >
                                    <option value={ShiftMode.Open}>Open (Anyone can check in)</option>
                                    <option value={ShiftMode.Strict}>Strict (Only assigned staff)</option>
                                </select>
                            </div>
                        </div>

                        {/* Recurrence Settings */}
                        <div className="border-t border-gray-200 pt-6">
                            <h3 className="text-lg font-medium text-gray-900 mb-4">Recurrence Settings</h3>

                            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                {/* Recurrence Type */}
                                <div>
                                    <label htmlFor="recurrenceType" className="block text-sm font-medium text-gray-700 mb-1">
                                        Recurrence Type
                                    </label>
                                    <select
                                        id="recurrenceType"
                                        name="recurrenceType"
                                        value={formData.recurrenceType}
                                        onChange={handleChange}
                                        className={`mt-1 block w-full pl-3 pr-10 py-2 text-base border ${errors.recurrenceType ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : 'border-gray-300 focus:ring-amber-500 focus:border-amber-500'} focus:outline-none sm:text-sm rounded-md`}
                                        disabled={isLoading}
                                    >
                                        <option value={RecurrenceType.None}>Does not repeat</option>
                                        <option value={RecurrenceType.Daily}>Daily</option>
                                        <option value={RecurrenceType.Weekly}>Weekly</option>
                                        <option value={RecurrenceType.Monthly}>Monthly</option>
                                        <option value={RecurrenceType.Custom}>Custom</option>
                                    </select>
                                    {errors.recurrenceType && (
                                        <p className="mt-1 text-sm text-red-600">{errors.recurrenceType}</p>
                                    )}
                                </div>

                                {/* Recurrence Interval */}
                                {formData.recurrenceType !== RecurrenceType.None && (
                                    <div>
                                        <label htmlFor="recurrenceInterval" className="block text-sm font-medium text-gray-700 mb-1">
                                            Repeat Every
                                        </label>
                                        <div className="mt-1 relative rounded-md shadow-sm">
                                            <input
                                                type="number"
                                                id="recurrenceInterval"
                                                name="recurrenceInterval"
                                                min="1"
                                                value={formData.recurrenceInterval}
                                                onChange={handleChange}
                                                className={`block w-full rounded-md ${errors.recurrenceInterval ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : 'border-gray-300 focus:ring-amber-500 focus:border-amber-500'} sm:text-sm`}
                                                disabled={isLoading}
                                            />
                                            <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                                                <span className="text-gray-500 sm:text-sm">
                                                    {formData.recurrenceType === RecurrenceType.Daily ? 'day(s)' :
                                                        formData.recurrenceType === RecurrenceType.Weekly ? 'week(s)' :
                                                            formData.recurrenceType === RecurrenceType.Monthly ? 'month(s)' : 'time(s)'}
                                                </span>
                                            </div>
                                        </div>
                                        {errors.recurrenceInterval && (
                                            <p className="mt-1 text-sm text-red-600">{errors.recurrenceInterval}</p>
                                        )}
                                    </div>
                                )}
                            </div>

                            {/* Custom Days */}
                            {formData.recurrenceType === RecurrenceType.Custom && (
                                <div className="mt-4">
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Repeat On <span className="text-red-500">*</span>
                                    </label>
                                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-2">
                                        {[
                                            { value: 0, label: 'Sunday' },
                                            { value: 1, label: 'Monday' },
                                            { value: 2, label: 'Tuesday' },
                                            { value: 3, label: 'Wednesday' },
                                            { value: 4, label: 'Thursday' },
                                            { value: 5, label: 'Friday' },
                                            { value: 6, label: 'Saturday' }
                                        ].map((day) => (
                                            <div key={day.value} className="flex items-center">
                                                <input
                                                    id={`day-${day.value}`}
                                                    name="customDays"
                                                    type="checkbox"
                                                    checked={formData.customDays.includes(day.value)}
                                                    onChange={() => handleCustomDayChange(day.value)}
                                                    className="h-4 w-4 text-amber-600 focus:ring-amber-500 border-gray-300 rounded"
                                                    disabled={isLoading}
                                                />
                                                <label htmlFor={`day-${day.value}`} className="ml-2 block text-sm text-gray-700">
                                                    {day.label}
                                                </label>
                                            </div>
                                        ))}
                                    </div>
                                    {errors.customDays && (
                                        <p className="mt-1 text-sm text-red-600">{errors.customDays}</p>
                                    )}
                                </div>
                            )}
                        </div>

                        {/* Exception Dates */}
                        <div className="border-t border-gray-200 pt-6">
                            <h3 className="text-lg font-medium text-gray-900 mb-4">Exception Dates</h3>
                            <p className="text-sm text-gray-500 mb-4">
                                Add dates when this shift should not occur (e.g., holidays)
                            </p>

                            <div className="flex">
                                <div className="flex-1">
                                    <input
                                        type="date"
                                        value={newExceptionDate}
                                        onChange={(e) => setNewExceptionDate(e.target.value)}
                                        className="block w-full rounded-md border border-gray-300 shadow-sm focus:border-amber-500 focus:ring-amber-500 sm:text-sm"
                                        disabled={isLoading}
                                    />
                                </div>
                                <button
                                    type="button"
                                    onClick={handleAddExceptionDate}
                                    className="ml-3 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-amber-500 hover:bg-amber-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-amber-500 disabled:opacity-50"
                                    disabled={!newExceptionDate || isLoading}
                                >
                                    <Plus className="-ml-1 mr-2 h-4 w-3" />
                                    Add Exception
                                </button>
                            </div>

                            {formData.exceptionDates.length > 0 && (
                                <div className="mt-4">
                                    <h4 className="text-sm font-medium text-gray-700 mb-2">Scheduled Exceptions:</h4>
                                    <div className="flex flex-wrap gap-2">
                                        {formData.exceptionDates.map((date) => (
                                            <span
                                                key={date}
                                                className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
                                            >
                                                {formatDisplayDate(date)}
                                                <button
                                                    type="button"
                                                    onClick={() => handleRemoveExceptionDate(date)}
                                                    className="ml-1.5 inline-flex items-center justify-center h-4 w-4 rounded-full text-blue-400 hover:bg-blue-200 hover:text-blue-500"
                                                    disabled={isLoading}
                                                >
                                                    <X className="h-3 w-3" />
                                                    <span className="sr-only">Remove</span>
                                                </button>
                                            </span>
                                        ))}
                                    </div>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Form Actions */}
                    <div className="flex justify-end space-x-3 pt-6 border-t border-gray-200">
                        <button
                            type="button"
                            onClick={onClose}
                            className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
                            disabled={isLoading}
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className="inline-flex justify-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-amber-500 hover:bg-amber-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
                            disabled={isLoading}
                        >
                            {isLoading ? (
                                <>
                                    <Loader2 className="animate-spin -ml-1 mr-2 h-4 w-4" />
                                    Saving...
                                </>
                            ) : 'Save Changes'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default ShiftEdit;