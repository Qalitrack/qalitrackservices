import React, { useState, useEffect } from 'react';
import { createShift } from '../../../helpers/UserService/Shifts/Shifts';

// Enums from the server
const ShiftMode = {
    Open: 0,    // Anyone can log in
    Strict: 1   // Only allowed users can log in
};

const ShiftType = {
    OneTime: 1,
    Recurring: 2
};

const RecurrenceType = {
    None: 0,
    Daily: 1,
    Weekly: 2,
    Monthly: 3,
    Custom: 4
};

const AddShift = ({ isOpen, onClose, onShiftAdded }) => {
    const [formData, setFormData] = useState({
        name: '',
        description: '',
        startTime: '',
        endTime: '',
        mode: ShiftMode.Open.toString(),
        startDate: new Date().toISOString(),
        endDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
        type: ShiftType.Recurring.toString(),
        requiredStaffCount: 1,
        recurrenceType: RecurrenceType.Daily.toString(),
        recurrenceInterval: 1,
        customDays: [1, 2, 3, 4, 5], // Default to weekdays
        exceptionDates: []
    });

    const [errors, setErrors] = useState({});
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [newExceptionDate, setNewExceptionDate] = useState('');


    const validateForm = (data) => {
        const newErrors = {};
        if (!data.name.trim()) newErrors.name = 'Name is required';
        if (!data.startTime) newErrors.startTime = 'Start time is required';
        if (!data.endTime) newErrors.endTime = 'End time is required';
        if (data.requiredStaffCount < 1) newErrors.requiredStaffCount = 'At least 1 staff member is required';

        // Additional validation for recurring shifts
        if (parseInt(data.type, 10) === ShiftType.Recurring) {
            const startDate = new Date(data.startDate);
            const endDate = new Date(data.endDate);

            if (endDate <= startDate) {
                newErrors.endDate = 'End date must be after start date';
            }

            if (!data.recurrenceType || data.recurrenceType === RecurrenceType.None.toString()) {
                newErrors.recurrenceType = 'Please select a recurrence type';
            }

            if (parseInt(data.recurrenceType, 10) === RecurrenceType.Weekly &&
                (!data.customDays || data.customDays.length === 0)) {
                newErrors.customDays = 'Please select at least one day of the week';
            }

            if (data.recurrenceInterval < 1) {
                newErrors.recurrenceInterval = 'Interval must be at least 1';
            }
        }

        return newErrors;
    };

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));
        // Clear error when user types
        if (errors[name]) {
            setErrors(prev => {
                const newErrors = { ...prev };
                delete newErrors[name];
                return newErrors;
            });
        }
    };

    const handleTimeChange = (e, field) => {
        const { value } = e.target;
        const timeField = field === 'start' ? 'startTime' : 'endTime';
        const date = formData[timeField] ? new Date(formData[timeField]) : new Date();
        const [hours, minutes] = value.split(':');
        date.setHours(parseInt(hours, 10), parseInt(minutes, 10));

        setFormData(prev => ({
            ...prev,
            [timeField]: date.toISOString(),
            ...(field === 'start' && {
                durationMinutes: Math.round((new Date(prev.endTime || date.toISOString()).getTime() - date.getTime()) / (1000 * 60))
            }),
            ...(field === 'end' && {
                durationMinutes: Math.round((new Date(value).getTime() - new Date(prev.startTime || date.toISOString()).getTime()) / (1000 * 60))
            })
        }));
    };

    const handleDateChange = (e, field) => {
        const { value } = e.target;
        const dateField = field === 'start' ? 'startDate' : 'endDate';
        const date = formData[dateField] ? new Date(formData[dateField]) : new Date();
        const [year, month, day] = value.split('-');
        date.setFullYear(parseInt(year, 10), parseInt(month, 10) - 1, parseInt(day, 10));

        setFormData(prev => ({
            ...prev,
            [dateField]: date.toISOString()
        }));
    };

    const handleCustomDayToggle = (day) => {
        setFormData(prev => {
            const newDays = [...(prev.customDays || [])];
            const dayIndex = newDays.indexOf(day);

            if (dayIndex === -1) {
                newDays.push(day);
            } else {
                newDays.splice(dayIndex, 1);
            }

            return {
                ...prev,
                customDays: newDays.sort((a, b) => a - b)
            };
        });
    };

    // Handle adding exception date
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

    // Handle removing exception date
    const handleRemoveExceptionDate = (dateToRemove) => {
        setFormData(prev => ({
            ...prev,
            exceptionDates: prev.exceptionDates.filter(d => d !== dateToRemove)
        }));
    };

    // Format date for display
    const formatDisplayDate = (dateStr) => {
        if (!dateStr) return '';
        try {
            return new Date(dateStr).toLocaleDateString();
        } catch (e) {
            return dateStr;
        }
    };

    // Convert local time to ISO string without timezone conversion
    const localToUTC = (dateString) => {
        if (!dateString) return null;
        const date = new Date(dateString);
        // Format as YYYY-MM-DDTHH:mm:ss without timezone conversion
        return date.getFullYear() + '-' +
            String(date.getMonth() + 1).padStart(2, '0') + '-' +
            String(date.getDate()).padStart(2, '0') + 'T' +
            String(date.getHours()).padStart(2, '0') + ':' +
            String(date.getMinutes()).padStart(2, '0') + ':' +
            String(date.getSeconds()).padStart(2, '0');
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        // Clear previous errors and success messages
        setError('');
        setSuccess('');

        // Validate form
        const formErrors = validateForm(formData);
        if (Object.keys(formErrors).length > 0) {
            setErrors(formErrors);
            return;
        }

        // Additional validation for time
        const startTime = new Date(formData.startTime);
        const endTime = new Date(formData.endTime);
        if (endTime <= startTime) {
            setErrors(prev => ({
                ...prev,
                endTime: 'End time must be after start time'
            }));
            return;
        }

        // Additional validation for date range
        const startDate = new Date(formData.startDate);
        const endDate = new Date(formData.endDate);

        if (formData.type === ShiftType.Recurring.toString() && endDate <= startDate) {
            setErrors(prev => ({
                ...prev,
                endDate: 'End date must be after start date for recurring shifts'
            }));
            return;
        }

        // Check for too many instances (e.g., more than 100)
        if (formData.type === ShiftType.Recurring.toString()) {
            const daysDiff = Math.ceil((endDate - startDate) / (1000 * 60 * 60 * 24));
            let instanceCount = 0;

            if (formData.recurrenceType === RecurrenceType.Daily.toString()) {
                instanceCount = Math.ceil(daysDiff / formData.recurrenceInterval);
            } else if (formData.recurrenceType === RecurrenceType.Weekly.toString()) {
                const weeks = Math.ceil(daysDiff / 7);
                instanceCount = Math.ceil(weeks / formData.recurrenceInterval) * formData.customDays.length;
            } else if (formData.recurrenceType === RecurrenceType.Monthly.toString()) {
                // Rough estimate - actual count might be less due to varying month lengths
                const months = (endDate.getFullYear() - startDate.getFullYear()) * 12 +
                    (endDate.getMonth() - startDate.getMonth());
                instanceCount = Math.ceil(months / formData.recurrenceInterval);
            }

            if (instanceCount > 100) {
                setError(`This would create approximately ${instanceCount} instances. Please reduce the date range or increase the interval.`);
                return;
            }

            // Ask for confirmation if creating many instances
            if (instanceCount > 10) {
                const confirmed = window.confirm(
                    `This will create approximately ${instanceCount} instances. Do you want to continue?`
                );
                if (!confirmed) return;
            }
        }

        setIsLoading(true);

        try {
            // Convert local times to UTC before sending to server
            const startTimeUTC = localToUTC(formData.startTime);
            const endTimeUTC = localToUTC(formData.endTime);
            const startDateUTC = localToUTC(formData.startDate);
            const endDateUTC = formData.type === ShiftType.OneTime.toString()
                ? null
                : localToUTC(formData.endDate);

            // Prepare the payload
            const payload = {
                name: formData.name.trim(),
                description: formData.description.trim(),
                startTime: startTimeUTC,
                endTime: endTimeUTC,
                mode: parseInt(formData.mode, 10),
                startDate: startDateUTC,
                endDate: endDateUTC,
                type: parseInt(formData.type, 10),
                requiredStaffCount: parseInt(formData.requiredStaffCount, 10),
                recurrenceType: formData.type === ShiftType.OneTime.toString()
                    ? RecurrenceType.None
                    : parseInt(formData.recurrenceType, 10),
                recurrenceInterval: formData.type === ShiftType.OneTime.toString()
                    ? 0
                    : parseInt(formData.recurrenceInterval, 10),
                customDays: formData.type === ShiftType.OneTime.toString()
                    ? []
                    : formData.recurrenceType === RecurrenceType.Weekly.toString()
                        ? formData.customDays
                        : [],
                exceptionDates: formData.exceptionDates || [],
                status: 0 // Default status for new shift
            };


            // Call the API
            const createdShift = await createShift(payload);

            // Show success message
            setSuccess('Shift created successfully!');
            
            // Pass the created shift data back to the parent
            onShiftAdded(createdShift);

            // Close the form after a short delay
            setTimeout(() => {
                onClose();
            }, 1500);

        } catch (err) {
            console.error('Error creating shift:', err);
            setError(err.response?.data?.message || 'Failed to create shift. Please try again.');
        } finally {
            setIsLoading(false);
        }
    };

    if (!isOpen) return null;

    const formatDateForInput = (dateString) => {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toISOString().split('T')[0];
    };

    const formatTimeForInput = (dateString) => {
        if (!dateString) return '09:00';
        const date = new Date(dateString);
        return date.toTimeString().substring(0, 5);
    };

    const weekDays = [
        { value: 0, label: 'Sunday' },
        { value: 1, label: 'Monday' },
        { value: 2, label: 'Tuesday' },
        { value: 3, label: 'Wednesday' },
        { value: 4, label: 'Thursday' },
        { value: 5, label: 'Friday' },
        { value: 6, label: 'Saturday' },
    ];

    const shiftModes = [
        { value: ShiftMode.Open, label: 'Open (Anyone can log in)' },
        { value: ShiftMode.Strict, label: 'Strict (Only allowed users)' },
    ];

    const shiftTypes = [
        { value: ShiftType.OneTime, label: 'One Time' },
        { value: ShiftType.Recurring, label: 'Recurring' },
    ];

    const recurrenceTypes = [
        { value: RecurrenceType.None, label: 'None' },
        { value: RecurrenceType.Daily, label: 'Daily' },
        { value: RecurrenceType.Weekly, label: 'Weekly' },
        { value: RecurrenceType.Monthly, label: 'Monthly' },
        { value: RecurrenceType.Custom, label: 'Custom' },
    ];

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center p-4">
            <div className="bg-white rounded-lg shadow-xl p-6 w-full max-w-4xl max-h-[90vh] overflow-y-auto">
                <div className="flex justify-between items-center mb-4">
                    <h3 className="text-lg font-medium text-gray-900">Add New Shift</h3>
                    <button
                        onClick={onClose}
                        className="text-gray-400 hover:text-gray-500"
                        disabled={isLoading}
                    >
                        <span className="sr-only">Close</span>
                        <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {/* Name */}
                        <div className="col-span-2 md:col-span-1">
                            <label className="block text-sm font-medium text-gray-700">Name *</label>
                            <input
                                type="text"
                                name="name"
                                value={formData.name}
                                onChange={handleChange}
                                className={`mt-1 block w-full border ${
                                    errors.name ? 'border-red-500' : 'border-gray-300'
                                } rounded-md p-2`}
                                required
                                disabled={isLoading}
                            />
                            {errors.name && <p className="mt-1 text-sm text-red-600">{errors.name}</p>}
                        </div>

                        {/* Shift Type */}
                        <div className="col-span-2 md:col-span-1">
                            <label className="block text-sm font-medium text-gray-700">Shift Type *</label>
                            <select
                                name="type"
                                value={formData.type}
                                onChange={(e) => {
                                    handleChange(e);
                                    // Reset recurrence type when changing shift type
                                    if (e.target.value === ShiftType.OneTime.toString()) {
                                        setFormData(prev => ({
                                            ...prev,
                                            recurrenceType: RecurrenceType.None.toString(),
                                            customDays: []
                                        }));
                                    } else {
                                        setFormData(prev => ({
                                            ...prev,
                                            recurrenceType: RecurrenceType.Daily.toString()
                                        }));
                                    }
                                }}
                                className="mt-1 block w-full border border-gray-300 rounded-md p-2"
                                disabled={isLoading}
                            >
                                <option value={ShiftType.OneTime}>One-time Shift</option>
                                <option value={ShiftType.Recurring}>Recurring Shift</option>
                            </select>
                        </div>

                        {/* Start Date */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Start Date *</label>
                            <input
                                type="date"
                                value={formatDateForInput(formData.startDate)}
                                onChange={(e) => handleDateChange(e, 'start')}
                                min={formatDateForInput(new Date())}
                                className={`mt-1 block w-full border ${
                                    errors.startDate ? 'border-red-500' : 'border-gray-300'
                                } rounded-md p-2`}
                                required
                                disabled={isLoading}
                            />
                        </div>

                        {/* End Date */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700">End Date</label>
                            <input
                                type="date"
                                value={formatDateForInput(formData.endDate)}
                                onChange={(e) => handleDateChange(e, 'end')}
                                min={formatDateForInput(formData.startDate || new Date())}
                                className="mt-1 block w-full border border-gray-300 rounded-md p-2"
                                disabled={isLoading}
                            />
                        </div>

                        {/* Start Time */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Start Time *</label>
                            <input
                                type="time"
                                value={formatTimeForInput(formData.startTime)}
                                onChange={(e) => handleTimeChange(e, 'start')}
                                className={`mt-1 block w-full border ${
                                    errors.startTime ? 'border-red-500' : 'border-gray-300'
                                } rounded-md p-2`}
                                required
                                disabled={isLoading}
                            />
                            {errors.startTime && <p className="mt-1 text-sm text-red-600">{errors.startTime}</p>}
                        </div>

                        {/* End Time */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700">End Time *</label>
                            <input
                                type="time"
                                value={formatTimeForInput(formData.endTime)}
                                onChange={(e) => handleTimeChange(e, 'end')}
                                min={formatTimeForInput(formData.startTime)}
                                className={`mt-1 block w-full border ${
                                    errors.endTime ? 'border-red-500' : 'border-gray-300'
                                } rounded-md p-2`}
                                required
                                disabled={isLoading || !formData.startTime}
                            />
                            {errors.endTime && <p className="mt-1 text-sm text-red-600">{errors.endTime}</p>}
                        </div>

                        {/* Required Staff Count */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Required Staff *</label>
                            <input
                                type="number"
                                name="requiredStaffCount"
                                value={formData.requiredStaffCount}
                                onChange={handleChange}
                                min="1"
                                step="1"
                                className={`mt-1 block w-full border ${
                                    errors.requiredStaffCount ? 'border-red-500' : 'border-gray-300'
                                } rounded-md p-2`}
                                required
                                disabled={isLoading}
                            />
                            {errors.requiredStaffCount && (
                                <p className="mt-1 text-sm text-red-600">{errors.requiredStaffCount}</p>
                            )}
                        </div>

                        {/* Mode */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Mode *</label>
                            <select
                                name="mode"
                                value={formData.mode}
                                onChange={handleChange}
                                className="mt-1 block w-full border border-gray-300 rounded-md p-2"
                                required
                                disabled={isLoading}
                            >
                                <option value="0">Open Shift</option>
                                <option value="1">Closed Shift</option>
                            </select>
                        </div>

                        {/* Recurrence Settings - Only show for recurring shifts */}
                        {formData.type === ShiftType.Recurring.toString() && (
                            <div className="col-span-2 space-y-4 p-4 bg-gray-50 rounded-lg">
                                <h4 className="text-sm font-medium text-gray-700">Recurrence Settings</h4>

                                {/* Recurrence Pattern and Interval */}
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-16">
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">
                                            Repeats
                                            <span className="text-red-500">*</span>
                                        </label>
                                        <select
                                            name="recurrenceType"
                                            value={formData.recurrenceType}
                                            onChange={(e) => {
                                                handleChange(e);
                                                // Clear custom days when not in weekly mode
                                                if (e.target.value !== RecurrenceType.Weekly.toString()) {
                                                    setFormData(prev => ({
                                                        ...prev,
                                                        customDays: []
                                                    }));
                                                }
                                            }}
                                            className={`mt-1 block w-full border ${
                                                errors.recurrenceType ? 'border-red-500' : 'border-gray-300'
                                            } rounded-md p-2`}
                                            disabled={isLoading}
                                        >
                                            <option value={RecurrenceType.Daily}>Daily</option>
                                            <option value={RecurrenceType.Weekly}>Weekly on specific days</option>
                                            <option value={RecurrenceType.Monthly}>Monthly on the same date</option>
                                        </select>
                                        {errors.recurrenceType && (
                                            <p className="mt-1 text-sm text-red-600">{errors.recurrenceType}</p>
                                        )}
                                    </div>

                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">
                                            Repeat every
                                            <span className="text-red-500">*</span>
                                        </label>
                                        <div className="flex items-center space-x-2">
                                            <input
                                                type="number"
                                                name="recurrenceInterval"
                                                min="1"
                                                value={formData.recurrenceInterval}
                                                onChange={handleChange}
                                                className={`w-20 border ${
                                                    errors.recurrenceInterval ? 'border-red-500' : 'border-gray-300'
                                                } rounded-md p-2`}
                                                disabled={isLoading}
                                            />
                                            <span className="text-sm text-gray-600">
                                                {formData.recurrenceType === RecurrenceType.Daily.toString() ? 'day(s)' :
                                                    formData.recurrenceType === RecurrenceType.Weekly.toString() ? 'week(s)' : 'month(s)'}
                                            </span>
                                        </div>
                                        {errors.recurrenceInterval && (
                                            <p className="mt-1 text-sm text-red-600">{errors.recurrenceInterval}</p>
                                        )}
                                    </div>
                                </div>

                                {/* Days of week selector (only for weekly) */}
                                {formData.recurrenceType === RecurrenceType.Weekly.toString() && (
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">
                                            On days
                                            <span className="text-red-500">*</span>
                                        </label>
                                        <div className="flex flex-wrap gap-2">
                                            {weekDays.map(day => (
                                                <button
                                                    key={day.value}
                                                    type="button"
                                                    onClick={() => handleCustomDayToggle(day.value)}
                                                    className={`px-3 py-1 text-sm rounded-full ${
                                                        formData.customDays.includes(day.value)
                                                            ? 'bg-blue-500 text-white'
                                                            : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                                                    }`}
                                                >
                                                    {day.label.substring(0, 3)}
                                                </button>
                                            ))}
                                        </div>
                                        {errors.customDays && (
                                            <p className="mt-1 text-sm text-red-600">{errors.customDays}</p>
                                        )}
                                    </div>
                                )}
                            </div>
                        )}

                        {/* Exception Dates */}
                        <div className="col-span-2 space-y-2">
                            <h4 className="text-sm font-medium text-gray-700">Exception Dates</h4>
                            <p className="text-xs text-gray-500">Add dates when this shift should not occur (e.g., holidays)</p>
                            <div className="flex gap-2">
                                <input
                                    type="date"
                                    value={newExceptionDate}
                                    onChange={(e) => setNewExceptionDate(e.target.value)}
                                    className="flex-1 border border-gray-300 rounded-md p-2 text-sm"
                                    min={formatDateForInput(new Date())}
                                />
                                <button
                                    type="button"
                                    onClick={handleAddExceptionDate}
                                    disabled={!newExceptionDate || isLoading}
                                    className="px-3 py-2 bg-blue-50 text-blue-600 rounded-md text-sm font-medium hover:bg-blue-100 disabled:opacity-50 disabled:cursor-not-allowed"
                                >
                                    Add
                                </button>
                            </div>

                            {formData.exceptionDates.length > 0 && (
                                <div className="mt-2">
                                    <div className="flex flex-wrap gap-2">
                                        {formData.exceptionDates.map((date) => (
                                            <span
                                                key={date}
                                                className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
                                            >
                                                {formatDisplayDate(date)}
                                                <button
                                                    type="button"
                                                    onClick={() => handleRemoveExceptionDate(date)}
                                                    className="ml-1.5 inline-flex items-center justify-center h-4 w-4 rounded-full text-blue-400 hover:bg-blue-200 hover:text-blue-500"
                                                    disabled={isLoading}
                                                >
                                                    <svg className="h-2.5 w-2.5" fill="currentColor" viewBox="0 0 20 20">
                                                        <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
                                                    </svg>
                                                </button>
                                            </span>
                                        ))}
                                    </div>
                                </div>
                            )}
                        </div>

                        {/* Description */}
                        <div className="col-span-2">
                            <label className="block text-sm font-medium text-gray-700">Description</label>
                            <textarea
                                name="description"
                                value={formData.description}
                                onChange={handleChange}
                                className="mt-1 block w-full border border-gray-300 rounded-md p-2"
                                rows="3"
                                disabled={isLoading}
                            />
                        </div>
                    </div>

                    {/* Error and Success Messages */}
                    <div className="mt-4">
                        {error && <div className="text-red-600 text-sm font-medium p-3 bg-red-50 rounded-md">{error}</div>}
                        {success && <div className="text-green-600 text-sm font-medium p-3 bg-green-50 rounded-md">{success}</div>}
                    </div>

                    {/* Form Actions */}
                    <div className="flex justify-end gap-2 pt-4 border-t mt-6">
                        <button
                            type="button"
                            onClick={onClose}
                            disabled={isLoading}
                            className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            disabled={isLoading}
                            className="px-4 py-2 bg-amber-500 text-white rounded-md text-sm font-medium hover:bg-amber-600 disabled:opacity-50"
                        >
                            {isLoading ? 'Creating...' : 'Create Shift'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default AddShift;