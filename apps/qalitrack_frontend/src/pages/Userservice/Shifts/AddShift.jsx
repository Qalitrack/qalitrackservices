import React, { useState, useEffect } from 'react';
import { createShift } from '../../../api/helpers/UserService/Shifts/Shifts';

// Build "YYYY-MM-DD" from local year/month/day — avoids UTC midnight shift
const toLocalDateStr = (year, month, day) =>
    `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;

// Parse a "YYYY-MM-DD" string in local time (not UTC)
const parseLocalDate = (str) => {
    const [y, m, d] = str.split('-').map(Number);
    return new Date(y, m - 1, d);
};

const MONTH_NAMES = [
    'January','February','March','April','May','June',
    'July','August','September','October','November','December',
];
const DAY_NAMES = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'];

const ExceptionCalendar = ({ startDate, endDate, exceptionDates, onChange }) => {
    const rangeStart = new Date(startDate);
    rangeStart.setHours(0, 0, 0, 0);
    const rangeEnd = new Date(endDate);
    rangeEnd.setHours(23, 59, 59, 999);

    const [viewYear, setViewYear] = useState(rangeStart.getFullYear());
    const [viewMonth, setViewMonth] = useState(rangeStart.getMonth());

    useEffect(() => {
        const d = new Date(startDate);
        setViewYear(d.getFullYear());
        setViewMonth(d.getMonth());
    }, [startDate]);

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const exceptionSet = new Set(exceptionDates);
    const firstDay = new Date(viewYear, viewMonth, 1).getDay();
    const daysInMonth = new Date(viewYear, viewMonth + 1, 0).getDate();

    const canGoPrev = () => {
        const lastOfPrev = new Date(viewYear, viewMonth, 0); // last day of prev month
        return lastOfPrev >= rangeStart;
    };
    const canGoNext = () => {
        const firstOfNext = new Date(viewYear, viewMonth + 1, 1);
        return firstOfNext <= rangeEnd;
    };

    const prevMonth = () => {
        if (viewMonth === 0) { setViewMonth(11); setViewYear(y => y - 1); }
        else setViewMonth(m => m - 1);
    };
    const nextMonth = () => {
        if (viewMonth === 11) { setViewMonth(0); setViewYear(y => y + 1); }
        else setViewMonth(m => m + 1);
    };

    const handleDayClick = (day) => {
        const date = new Date(viewYear, viewMonth, day);
        if (date < rangeStart || date > rangeEnd) return;
        const key = toLocalDateStr(viewYear, viewMonth, day);
        const next = exceptionSet.has(key)
            ? exceptionDates.filter(d => d !== key)
            : [...exceptionDates, key].sort();
        onChange(next);
    };

    const cells = [];
    for (let i = 0; i < firstDay; i++) cells.push(null);
    for (let d = 1; d <= daysInMonth; d++) cells.push(d);
    while (cells.length % 7 !== 0) cells.push(null);

    return (
        <div className="border border-gray-200 rounded-lg p-3 bg-white select-none inline-block">
            {/* Month navigation */}
            <div className="flex items-center justify-between mb-2 gap-2">
                <button
                    type="button"
                    onClick={prevMonth}
                    disabled={!canGoPrev()}
                    className="w-7 h-7 flex items-center justify-center rounded hover:bg-gray-100 disabled:opacity-30 disabled:cursor-not-allowed text-gray-600 font-bold"
                >‹</button>
                <span className="text-sm font-semibold text-gray-700 min-w-[130px] text-center">
                    {MONTH_NAMES[viewMonth]} {viewYear}
                </span>
                <button
                    type="button"
                    onClick={nextMonth}
                    disabled={!canGoNext()}
                    className="w-7 h-7 flex items-center justify-center rounded hover:bg-gray-100 disabled:opacity-30 disabled:cursor-not-allowed text-gray-600 font-bold"
                >›</button>
            </div>

            {/* Day-of-week headers */}
            <div className="grid grid-cols-7 mb-1">
                {DAY_NAMES.map(d => (
                    <div key={d} className="w-8 h-6 flex items-center justify-center text-xs font-medium text-gray-400">
                        {d}
                    </div>
                ))}
            </div>

            {/* Day cells */}
            <div className="grid grid-cols-7">
                {cells.map((day, i) => {
                    if (!day) return <div key={`b-${i}`} className="w-8 h-8" />;

                    const date = new Date(viewYear, viewMonth, day);
                    const inRange = date >= rangeStart && date <= rangeEnd;
                    const key = toLocalDateStr(viewYear, viewMonth, day);
                    const isException = exceptionSet.has(key);
                    const isToday = date.getTime() === today.getTime();

                    return (
                        <button
                            key={day}
                            type="button"
                            onClick={() => handleDayClick(day)}
                            disabled={!inRange}
                            title={isException ? 'Click to remove exception' : inRange ? 'Click to mark as exception' : ''}
                            className={[
                                'w-8 h-8 rounded-full text-xs flex items-center justify-center transition-colors',
                                !inRange
                                    ? 'text-gray-300 cursor-not-allowed'
                                    : isException
                                        ? 'bg-red-500 text-white hover:bg-red-600 cursor-pointer font-medium'
                                        : isToday
                                            ? 'text-amber-700 font-bold ring-1 ring-amber-400 hover:bg-amber-100 cursor-pointer'
                                            : 'text-gray-700 hover:bg-amber-100 cursor-pointer',
                            ].filter(Boolean).join(' ')}
                        >
                            {day}
                        </button>
                    );
                })}
            </div>

            {/* Legend */}
            <div className="mt-2 flex items-center gap-3 text-xs text-gray-500 border-t pt-2">
                <span className="flex items-center gap-1">
                    <span className="w-3 h-3 rounded-full bg-red-500 inline-block" /> Exception
                </span>
                <span className="flex items-center gap-1">
                    <span className="w-3 h-3 rounded-full ring-1 ring-amber-400 inline-block" /> Today
                </span>
            </div>
        </div>
    );
};

const MINUTES = [0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55];

const TimePicker = ({ value, onChange, disabled, hasError, startValue }) => {
    const parseTime = (iso) => {
        if (!iso) return { hour: 9, minute: 0 };
        const d = new Date(iso);
        return { hour: d.getHours(), minute: d.getMinutes() };
    };

    const { hour, minute } = parseTime(value);
    const isPM = hour >= 12;
    const displayHour = hour % 12 || 12;

    const buildISO = (h24, m) => {
        const d = new Date();
        d.setHours(h24, m, 0, 0);
        return d.toISOString();
    };

    const setHour = (h12) => {
        let h24 = parseInt(h12);
        if (isPM && h24 !== 12) h24 += 12;
        if (!isPM && h24 === 12) h24 = 0;
        onChange(buildISO(h24, minute));
    };

    const setMinute = (m) => onChange(buildISO(hour, parseInt(m)));

    const setPeriod = (period) => {
        let h24 = hour;
        if (period === 'PM' && h24 < 12) h24 += 12;
        if (period === 'AM' && h24 >= 12) h24 -= 12;
        onChange(buildISO(h24, minute));
    };

    // Detect overnight: this field's time is before the start time
    const isOvernight = (() => {
        if (!startValue || !value) return false;
        const sH = new Date(startValue).getHours(), sM = new Date(startValue).getMinutes();
        const eH = hour, eM = minute;
        return eH * 60 + eM < sH * 60 + sM;
    })();

    return (
        <div className="flex flex-wrap items-center gap-2">
            <div className={[
                'flex items-center gap-1 border rounded-md px-2 py-1.5 bg-white',
                hasError ? 'border-red-500' : 'border-gray-300',
                disabled ? 'opacity-60' : '',
            ].join(' ')}>
                <select
                    value={displayHour}
                    onChange={e => setHour(e.target.value)}
                    disabled={disabled}
                    className="text-sm border-0 outline-none bg-transparent text-gray-700 cursor-pointer"
                >
                    {[...Array(12)].map((_, i) => (
                        <option key={i + 1} value={i + 1}>{String(i + 1).padStart(2, '0')}</option>
                    ))}
                </select>
                <span className="text-gray-400 font-bold text-sm select-none">:</span>
                <select
                    value={minute}
                    onChange={e => setMinute(e.target.value)}
                    disabled={disabled}
                    className="text-sm border-0 outline-none bg-transparent text-gray-700 cursor-pointer"
                >
                    {MINUTES.map(m => (
                        <option key={m} value={m}>{String(m).padStart(2, '0')}</option>
                    ))}
                </select>
                <div className="flex ml-2 rounded overflow-hidden border border-gray-200 text-xs font-medium">
                    <button type="button" disabled={disabled}
                        onClick={() => setPeriod('AM')}
                        className={`px-2 py-0.5 transition-colors ${!isPM ? 'bg-amber-500 text-white' : 'bg-gray-100 text-gray-500 hover:bg-gray-200'}`}
                    >AM</button>
                    <button type="button" disabled={disabled}
                        onClick={() => setPeriod('PM')}
                        className={`px-2 py-0.5 transition-colors ${isPM ? 'bg-amber-500 text-white' : 'bg-gray-100 text-gray-500 hover:bg-gray-200'}`}
                    >PM</button>
                </div>
            </div>
            {isOvernight && (
                <span className="text-xs font-medium text-indigo-600 bg-indigo-50 border border-indigo-200 px-2 py-0.5 rounded-full">
                    +1 day (overnight)
                </span>
            )}
        </div>
    );
};

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
    const [formData, setFormData] = useState(() => {
        const s = new Date(); s.setHours(9, 0, 0, 0);
        const e = new Date(); e.setHours(17, 0, 0, 0);
        return {
        name: '',
        description: '',
        startTime: s.toISOString(),
        endTime: e.toISOString(),
        mode: ShiftMode.Open.toString(),
        startDate: new Date().toISOString(),
        endDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
        type: ShiftType.Recurring.toString(),
        requiredStaffCount: 1,
        recurrenceType: RecurrenceType.Daily.toString(),
        recurrenceInterval: 1,
        customDays: [1, 2, 3, 4, 5], // Default to weekdays
        exceptionDates: []
        };
    });

    const [errors, setErrors] = useState({});
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');


    const validateForm = (data) => {
        const newErrors = {};
        const now = new Date();
        const startDate = new Date(data.startDate);
        const endDate = new Date(data.endDate);
        const startTime = new Date(data.startTime);
        const endTime = new Date(data.endTime);

        // Basic validations
        if (!data.name.trim()) newErrors.name = 'Name is required';
        if (data.requiredStaffCount < 1) newErrors.requiredStaffCount = 'At least 1 staff member is required';

        // Date validations
        if (startDate < now.setHours(0, 0, 0, 0)) {
            newErrors.startDate = 'Start date cannot be in the past';
        }

        if (endDate < startDate) {
            newErrors.endDate = 'End date must be after start date';
        }

        // Reject zero-duration shifts only (start === end time means 0 minutes, which is never valid)
        if (data.startTime && data.endTime) {
            const sH = new Date(data.startTime).getHours(), sM = new Date(data.startTime).getMinutes();
            const eH = new Date(data.endTime).getHours(),   eM = new Date(data.endTime).getMinutes();
            if (sH === eH && sM === eM) {
                newErrors.endTime = 'End time cannot equal start time';
            }
        }

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

    const handleTimeChange = (field, iso) => {
        const timeField = field === 'start' ? 'startTime' : 'endTime';
        setErrors(prev => { const e = { ...prev }; delete e.endTime; return e; });
        setFormData(prev => ({ ...prev, [timeField]: iso }));
    };

    const handleDateChange = (e, field) => {
        const { value } = e.target;
        const dateField = field === 'start' ? 'startDate' : 'endDate';
        const date = new Date(value);

        // Clear any previous date-related errors
        setErrors(prev => {
            const newErrors = { ...prev };
            delete newErrors[`${dateField}Invalid`];
            delete newErrors[`${field}Date`];
            return newErrors;
        });

        setFormData(prev => ({
            ...prev,
            [dateField]: date.toISOString(),
            // If changing start date and end date is before new start date, update end date
            ...(field === 'start' && {
                endDate: new Date(prev.endDate) < date ? date.toISOString() : prev.endDate
            })
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

    // Handle removing exception date
    const handleRemoveExceptionDate = (dateToRemove) => {
        setFormData(prev => ({
            ...prev,
            exceptionDates: prev.exceptionDates.filter(d => d !== dateToRemove)
        }));
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
            const interval = parseInt(formData.recurrenceInterval, 10);
            let instanceCount = 0;

            const exceptionSet = new Set(formData.exceptionDates); // "YYYY-MM-DD" strings

            const isException = (date) => {
                const key = toLocalDateStr(date.getFullYear(), date.getMonth(), date.getDate());
                return exceptionSet.has(key);
            };

            if (formData.recurrenceType === RecurrenceType.Daily.toString()) {
                const start = new Date(startDate);
                start.setHours(0, 0, 0, 0);
                const end = new Date(endDate);
                end.setHours(0, 0, 0, 0);
                const cursor = new Date(start);
                while (cursor <= end) {
                    const dayIndex = Math.floor((cursor - start) / (24 * 60 * 60 * 1000));
                    if (dayIndex % interval === 0 && !isException(cursor)) {
                        instanceCount++;
                    }
                    cursor.setDate(cursor.getDate() + 1);
                }
            } else if (formData.recurrenceType === RecurrenceType.Weekly.toString()) {
                const start = new Date(startDate);
                start.setHours(0, 0, 0, 0);
                const end = new Date(endDate);
                end.setHours(0, 0, 0, 0);
                const cursor = new Date(start);
                const msPerWeek = 7 * 24 * 60 * 60 * 1000;
                while (cursor <= end) {
                    const weekIndex = Math.floor((cursor - start) / msPerWeek);
                    if (formData.customDays.includes(cursor.getDay()) && weekIndex % interval === 0 && !isException(cursor)) {
                        instanceCount++;
                    }
                    cursor.setDate(cursor.getDate() + 1);
                }
            } else if (formData.recurrenceType === RecurrenceType.Monthly.toString()) {
                const start = new Date(startDate);
                const end = new Date(endDate);
                let cursor = new Date(start);
                while (cursor <= end) {
                    if (!isException(cursor)) {
                        instanceCount++;
                    }
                    cursor.setMonth(cursor.getMonth() + interval);
                }
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
                                    errors.startDate || errors.startDateInvalid ? 'border-red-500' : 'border-gray-300'
                                } rounded-md p-2`}
                                required
                                disabled={isLoading}
                            />
                            {(errors.startDate || errors.startDateInvalid) && (
                                <p className="mt-1 text-sm text-red-600">{errors.startDate || errors.startDateInvalid}</p>
                            )}
                        </div>

                        {/* End Date */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700">End Date</label>
                            <input
                                type="date"
                                value={formatDateForInput(formData.endDate)}
                                onChange={(e) => handleDateChange(e, 'end')}
                                min={formatDateForInput(formData.startDate || new Date())}
                                className={`mt-1 block w-full border ${
                                    errors.endDate || errors.endDateInvalid ? 'border-red-500' : 'border-gray-300'
                                } rounded-md p-2`}
                                disabled={isLoading}
                            />
                            {(errors.endDate || errors.endDateInvalid) && (
                                <p className="mt-1 text-sm text-red-600">{errors.endDate || errors.endDateInvalid}</p>
                            )}
                        </div>

                        {/* Start Time */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Start Time *</label>
                            <TimePicker
                                value={formData.startTime}
                                onChange={(iso) => handleTimeChange('start', iso)}
                                disabled={isLoading}
                                hasError={!!errors.startTime}
                            />
                            {errors.startTime && <p className="mt-1 text-sm text-red-600">{errors.startTime}</p>}
                        </div>

                        {/* End Time */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700">End Time *</label>
                            <TimePicker
                                value={formData.endTime}
                                onChange={(iso) => handleTimeChange('end', iso)}
                                disabled={isLoading}
                                hasError={!!errors.endTime}
                                startValue={formData.startTime}
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
                            <p className="text-xs text-gray-500">
                                Click any date in the shift range to mark it as an exception (e.g. holidays). Click again to remove.
                            </p>

                            <div className="flex flex-wrap gap-4 items-start">
                                <ExceptionCalendar
                                    startDate={formData.startDate}
                                    endDate={formData.endDate}
                                    exceptionDates={formData.exceptionDates}
                                    onChange={(dates) => setFormData(prev => ({ ...prev, exceptionDates: dates }))}
                                />

                                {formData.exceptionDates.length > 0 && (
                                    <div className="flex-1 min-w-[180px]">
                                        <p className="text-xs font-medium text-gray-500 mb-2">
                                            {formData.exceptionDates.length} exception{formData.exceptionDates.length !== 1 ? 's' : ''} selected
                                        </p>
                                        <div className="flex flex-wrap gap-1.5">
                                            {formData.exceptionDates.map((date) => (
                                                <span
                                                    key={date}
                                                    className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-700"
                                                >
                                                    {parseLocalDate(date).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
                                                    <button
                                                        type="button"
                                                        onClick={() => handleRemoveExceptionDate(date)}
                                                        disabled={isLoading}
                                                        className="hover:text-red-900 leading-none"
                                                        title="Remove"
                                                    >×</button>
                                                </span>
                                            ))}
                                        </div>
                                    </div>
                                )}
                            </div>
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