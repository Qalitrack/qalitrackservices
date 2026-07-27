import React, { useState, useEffect, useRef } from 'react';
import { Plus, X, Check, CalendarDays } from 'lucide-react';
import { createShift, updateShift } from '../../../api/helpers/UserService/Shifts/Shifts';

// Build "YYYY-MM-DD" from local year/month/day — avoids UTC midnight shift
const toLocalDateStr = (year, month, day) =>
    `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;

// Parse a "YYYY-MM-DD" string in local time (not UTC)
const parseLocalDate = (str) => {
    const [y, m, d] = str.split('-').map(Number);
    return new Date(y, m - 1, d);
};

const MONTH_NAMES = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December',
];
const DAY_NAMES = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'];

const ExceptionCalendar = ({ startDate, endDate, exceptionDates, onChange }) => {
    const rangeStart = new Date(startDate);
    rangeStart.setHours(0, 0, 0, 0);
    const rangeEnd = new Date(endDate || startDate);
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
        const lastOfPrev = new Date(viewYear, viewMonth, 0);
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
            <div className="flex items-center justify-between mb-2 gap-2">
                <button type="button" onClick={prevMonth} disabled={!canGoPrev()}
                    className="w-7 h-7 flex items-center justify-center rounded hover:bg-gray-100 disabled:opacity-30 disabled:cursor-not-allowed text-gray-600 font-bold"
                >‹</button>
                <span className="text-sm font-semibold text-gray-700 min-w-[130px] text-center">
                    {MONTH_NAMES[viewMonth]} {viewYear}
                </span>
                <button type="button" onClick={nextMonth} disabled={!canGoNext()}
                    className="w-7 h-7 flex items-center justify-center rounded hover:bg-gray-100 disabled:opacity-30 disabled:cursor-not-allowed text-gray-600 font-bold"
                >›</button>
            </div>

            <div className="grid grid-cols-7 mb-1">
                {DAY_NAMES.map(d => (
                    <div key={d} className="w-8 h-6 flex items-center justify-center text-xs font-medium text-gray-400">
                        {d}
                    </div>
                ))}
            </div>

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

// Internally represents a time-of-day as an ISO string (only the hour/minute are
// read) — convenient for the AM/PM control below; converted to "HH:MM:SS" (the
// TimeSpan format the backend expects) only when the payload is built.
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

    const isOvernight = (() => {
        if (!startValue || !value) return false;
        const sH = new Date(startValue).getHours(), sM = new Date(startValue).getMinutes();
        const eH = hour, eM = minute;
        return eH * 60 + eM < sH * 60 + sM;
    })();

    return (
        <div className="mt-1 flex flex-wrap items-center gap-2">
            <div className={[
                'flex items-center gap-1 h-9 border rounded-md px-2 bg-white',
                hasError ? 'border-red-500' : 'border-gray-300',
                disabled ? 'opacity-60' : '',
            ].join(' ')}>
                <select value={displayHour} onChange={e => setHour(e.target.value)} disabled={disabled}
                    className="text-sm border-0 outline-none bg-transparent text-gray-700 cursor-pointer"
                >
                    {[...Array(12)].map((_, i) => (
                        <option key={i + 1} value={i + 1}>{String(i + 1).padStart(2, '0')}</option>
                    ))}
                </select>
                <span className="text-gray-400 font-bold text-sm select-none">:</span>
                <select value={minute} onChange={e => setMinute(e.target.value)} disabled={disabled}
                    className="text-sm border-0 outline-none bg-transparent text-gray-700 cursor-pointer"
                >
                    {MINUTES.map(m => (
                        <option key={m} value={m}>{String(m).padStart(2, '0')}</option>
                    ))}
                </select>
                <div className="flex ml-2 rounded overflow-hidden border border-gray-200 text-xs font-medium">
                    <button type="button" disabled={disabled} onClick={() => setPeriod('AM')}
                        className={`px-2 py-0.5 transition-colors ${!isPM ? 'bg-amber-500 text-white' : 'bg-gray-100 text-gray-500 hover:bg-gray-200'}`}
                    >AM</button>
                    <button type="button" disabled={disabled} onClick={() => setPeriod('PM')}
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

// Enums — must match the backend exactly (UserService.Core.Enums)
const ShiftMode = { Open: 0, Strict: 1 };
const ShiftType = { OneTime: 1, Recurring: 2 };
const RecurrenceType = { None: 0, Daily: 1, Weekly: 2, Monthly: 3, Custom: 4 };

const WEEK_DAYS = [
    { value: 0, label: 'Sun' }, { value: 1, label: 'Mon' }, { value: 2, label: 'Tue' },
    { value: 3, label: 'Wed' }, { value: 4, label: 'Thu' }, { value: 5, label: 'Fri' }, { value: 6, label: 'Sat' },
];

const buildDefaultForm = () => {
    const s = new Date(); s.setHours(9, 0, 0, 0);
    const e = new Date(); e.setHours(17, 0, 0, 0);
    return {
        name: '',
        description: '',
        startTime: s.toISOString(),
        endTime: e.toISOString(),
        mode: String(ShiftMode.Open),
        startDate: new Date().toISOString().slice(0, 10),
        endDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10),
        type: String(ShiftType.Recurring),
        requiredStaffCount: 1,
        recurrenceType: String(RecurrenceType.Daily),
        recurrenceInterval: 1,
        customDays: [1, 2, 3, 4, 5],
        exceptionDates: [],
    };
};

// A TimeSpan like "09:00:00" (or with a days/fraction prefix/suffix) isn't a
// valid Date string on its own — anchor it to a dummy date to read hour/minute.
const timeSpanToIso = (value) => {
    const match = /(\d{1,2}):(\d{2})/.exec(value || '');
    const d = new Date();
    if (match) d.setHours(Number(match[1]), Number(match[2]), 0, 0);
    else d.setHours(9, 0, 0, 0);
    return d.toISOString();
};

const buildFormFromShift = (shift) => ({
    name: shift.name || '',
    description: shift.description || '',
    startTime: timeSpanToIso(shift.startTime),
    endTime: timeSpanToIso(shift.endTime),
    mode: String(shift.mode ?? ShiftMode.Open),
    startDate: (shift.startDate || '').slice(0, 10) || new Date().toISOString().slice(0, 10),
    endDate: (shift.endDate || '').slice(0, 10),
    type: String(shift.type ?? ShiftType.Recurring),
    requiredStaffCount: shift.requiredStaffCount || 1,
    recurrenceType: String(shift.recurrenceType ?? RecurrenceType.None),
    recurrenceInterval: shift.recurrenceInterval || 1,
    customDays: Array.isArray(shift.customDays) ? [...shift.customDays] : [],
    exceptionDates: Array.isArray(shift.exceptionDates)
        ? shift.exceptionDates.map(d => String(d).slice(0, 10))
        : [],
});

const toTimeSpan = (iso) => {
    const d = new Date(iso);
    return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}:00`;
};

const validateForm = (data, isEdit) => {
    const errors = {};
    if (!data.name.trim()) errors.name = 'Name is required';
    if (Number(data.requiredStaffCount) < 1) errors.requiredStaffCount = 'At least 1 staff member is required';

    if (!isEdit) {
        const today = new Date(); today.setHours(0, 0, 0, 0);
        if (parseLocalDate(data.startDate) < today) errors.startDate = 'Start date cannot be in the past';
    }

    if (data.endDate && parseLocalDate(data.endDate) < parseLocalDate(data.startDate)) {
        errors.endDate = 'End date must be after start date';
    }

    const sD = new Date(data.startTime), eD = new Date(data.endTime);
    if (sD.getHours() === eD.getHours() && sD.getMinutes() === eD.getMinutes()) {
        errors.endTime = 'End time cannot equal start time';
    }

    const isRecurring = Number(data.type) === ShiftType.Recurring;
    if (isRecurring) {
        if (Number(data.recurrenceType) === RecurrenceType.None) {
            errors.recurrenceType = 'Please select a recurrence type';
        }
        if (Number(data.recurrenceType) === RecurrenceType.Weekly && (!data.customDays || data.customDays.length === 0)) {
            errors.customDays = 'Please select at least one day of the week';
        }
        if (Number(data.recurrenceInterval) < 1) errors.recurrenceInterval = 'Interval must be at least 1';
    }

    return errors;
};

const buildPayload = (data, editingShift) => {
    const payload = {
        name: data.name.trim(),
        description: data.description.trim(),
        startTime: toTimeSpan(data.startTime),
        endTime: toTimeSpan(data.endTime),
        mode: Number(data.mode),
        startDate: data.startDate,
        endDate: data.endDate || null,
        requiredStaffCount: Number(data.requiredStaffCount) || 1,
        recurrenceType: Number(data.recurrenceType),
        recurrenceInterval: Number(data.recurrenceInterval) || 1,
        customDays: Number(data.recurrenceType) === RecurrenceType.Weekly ? data.customDays : [],
        exceptionDates: data.exceptionDates || [],
    };
    if (editingShift) {
        // UpdateShiftRequest requires Id in the body (route id is authoritative);
        // Type is immutable after creation and isn't part of that DTO at all.
        payload.id = editingShift.id;
    } else {
        payload.type = Number(data.type);
    }
    return payload;
};

const inputClass = (hasError) =>
    `mt-1 block w-full h-9 border ${hasError ? 'border-red-500' : 'border-gray-300'} rounded-md px-2 text-sm`;

const ShiftForm = ({ editingShift, onSaved, onCancelEdit, showMessage }) => {
    const [formData, setFormData] = useState(buildDefaultForm);
    const [errors, setErrors] = useState({});
    const [isSaving, setIsSaving] = useState(false);
    const [calendarOpen, setCalendarOpen] = useState(false);
    const calendarPopoverRef = useRef(null);

    useEffect(() => {
        setFormData(editingShift ? buildFormFromShift(editingShift) : buildDefaultForm());
        setErrors({});
        setCalendarOpen(false);
    }, [editingShift]);

    // Close the exception-date popover on an outside click rather than needing
    // an explicit "Done" button — keeps the calendar itself out of the layout
    // by default instead of always taking up a whole month's worth of space.
    useEffect(() => {
        if (!calendarOpen) return;
        const handleClickOutside = (e) => {
            if (calendarPopoverRef.current && !calendarPopoverRef.current.contains(e.target)) {
                setCalendarOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, [calendarOpen]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
        if (errors[name]) setErrors(prev => { const n = { ...prev }; delete n[name]; return n; });
    };

    const handleTypeChange = (e) => {
        const value = e.target.value;
        setFormData(prev => ({
            ...prev,
            type: value,
            recurrenceType: value === String(ShiftType.OneTime) ? String(RecurrenceType.None) : String(RecurrenceType.Daily),
            customDays: value === String(ShiftType.OneTime) ? [] : prev.customDays,
        }));
    };

    const handleCustomDayToggle = (day) => {
        setFormData(prev => {
            const has = prev.customDays.includes(day);
            const customDays = has ? prev.customDays.filter(d => d !== day) : [...prev.customDays, day].sort((a, b) => a - b);
            return { ...prev, customDays };
        });
        if (errors.customDays) setErrors(prev => { const n = { ...prev }; delete n.customDays; return n; });
    };

    const resetForm = () => {
        setFormData(buildDefaultForm());
        setErrors({});
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        const formErrors = validateForm(formData, !!editingShift);
        if (Object.keys(formErrors).length > 0) {
            setErrors(formErrors);
            return;
        }

        setIsSaving(true);
        try {
            const payload = buildPayload(formData, editingShift);
            if (editingShift) {
                await updateShift(editingShift.id, payload);
                showMessage('Shift updated successfully!', 'success');
            } else {
                await createShift(payload);
                showMessage('Shift created successfully!', 'success');
            }
            resetForm();
            onSaved();
        } catch (err) {
            showMessage(err?.response?.data?.message || err.message || 'Failed to save shift.', 'error');
        } finally {
            setIsSaving(false);
        }
    };

    // Recurrence only applies to Recurring shifts — for an existing shift that's
    // read-only (Type can't change after creation), so gate on its original type.
    const showRecurrence = Number(formData.type) === ShiftType.Recurring;

    // Rendered inside the recurrence panel (next to "Repeat Every") when recurring,
    // or as its own full-width block for one-time shifts that have no recurrence panel.
    // The calendar itself only appears in a popover on demand — it stays out of the
    // layout by default instead of permanently reserving a whole month's worth of space.
    const exceptionDatesBlock = (
        <div className="relative">
            <label className="text-xs font-semibold text-gray-700 block mb-1">Exception Dates</label>
            <div className="mt-1 flex flex-wrap items-center gap-1.5">
                <button type="button" onClick={() => setCalendarOpen(o => !o)} disabled={isSaving}
                    className="h-9 px-2.5 text-sm font-medium rounded-md border border-gray-300 bg-white hover:bg-gray-50 text-gray-700 flex items-center gap-1.5 shrink-0"
                >
                    <CalendarDays className="w-3.5 h-3.5" />
                    {formData.exceptionDates.length > 0 ? 'Edit dates' : 'Add exception'}
                </button>
                {formData.exceptionDates.map((date) => (
                    <span key={date} className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-700">
                        {parseLocalDate(date).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })}
                        <button type="button" disabled={isSaving} className="hover:text-red-900 leading-none"
                            onClick={() => setFormData(prev => ({ ...prev, exceptionDates: prev.exceptionDates.filter(d => d !== date) }))}
                        >×</button>
                    </span>
                ))}
            </div>

            {calendarOpen && (
                <div ref={calendarPopoverRef} className="absolute z-20 top-full left-0 mt-1 shadow-lg">
                    <div className="text-xs text-gray-500 mb-1">Click a date to mark/unmark it</div>
                    <ExceptionCalendar
                        startDate={formData.startDate}
                        endDate={formData.endDate || formData.startDate}
                        exceptionDates={formData.exceptionDates}
                        onChange={(dates) => setFormData(prev => ({ ...prev, exceptionDates: dates }))}
                    />
                </div>
            )}
        </div>
    );

    return (
        <div className="px-4 py-3 bg-gradient-to-r from-gray-50 to-amber-50/30 border-b border-amber-200 shadow-sm shrink-0">
            <form onSubmit={handleSubmit} className="space-y-3">
                <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                    <div className="col-span-2">
                        <label className="text-xs font-semibold text-gray-700 block mb-1">Name <span className="text-red-500">*</span></label>
                        <input name="name" value={formData.name} onChange={handleChange}
                            className={inputClass(errors.name)} placeholder="e.g., Morning Shift" disabled={isSaving} />
                        {errors.name && <p className="mt-1 text-xs text-red-600">{errors.name}</p>}
                    </div>

                    <div className="col-span-2">
                        <label className="text-xs font-semibold text-gray-700 block mb-1">Description</label>
                        <input name="description" value={formData.description} onChange={handleChange}
                            className={inputClass(false)} placeholder="Optional description" disabled={isSaving} />
                    </div>

                    <div>
                        <label className="text-xs font-semibold text-gray-700 block mb-1">Shift Type</label>
                        {editingShift ? (
                            <div className="mt-1 h-9 px-2 flex items-center text-sm text-gray-600 bg-gray-100 rounded-md border border-gray-200 truncate">
                                {Number(formData.type) === ShiftType.OneTime ? 'One-time' : 'Recurring'} (fixed at creation)
                            </div>
                        ) : (
                            <select name="type" value={formData.type} onChange={handleTypeChange}
                                className={inputClass(false)} disabled={isSaving}>
                                <option value={ShiftType.OneTime}>One-time Shift</option>
                                <option value={ShiftType.Recurring}>Recurring Shift</option>
                            </select>
                        )}
                    </div>

                    <div>
                        <label className="text-xs font-semibold text-gray-700 block mb-1">Mode</label>
                        <select name="mode" value={formData.mode} onChange={handleChange} className={inputClass(false)} disabled={isSaving}>
                            <option value={ShiftMode.Open}>Open (anyone can log in)</option>
                            <option value={ShiftMode.Strict}>Strict (only assigned users)</option>
                        </select>
                    </div>

                    <div>
                        <label className="text-xs font-semibold text-gray-700 block mb-1">Start Date <span className="text-red-500">*</span></label>
                        <input type="date" name="startDate" value={formData.startDate} onChange={handleChange}
                            className={inputClass(errors.startDate)} disabled={isSaving} />
                        {errors.startDate && <p className="mt-1 text-xs text-red-600">{errors.startDate}</p>}
                    </div>

                    <div>
                        <label className="text-xs font-semibold text-gray-700 block mb-1">End Date</label>
                        <input type="date" name="endDate" value={formData.endDate} onChange={handleChange}
                            min={formData.startDate} className={inputClass(errors.endDate)} disabled={isSaving} />
                        {errors.endDate && <p className="mt-1 text-xs text-red-600">{errors.endDate}</p>}
                    </div>

                    <div>
                        <label className="text-xs font-semibold text-gray-700 block mb-1">Start Time <span className="text-red-500">*</span></label>
                        <TimePicker value={formData.startTime} disabled={isSaving} hasError={!!errors.startTime}
                            onChange={(iso) => setFormData(prev => ({ ...prev, startTime: iso }))} />
                    </div>

                    <div>
                        <label className="text-xs font-semibold text-gray-700 block mb-1">End Time <span className="text-red-500">*</span></label>
                        <TimePicker value={formData.endTime} disabled={isSaving} hasError={!!errors.endTime}
                            startValue={formData.startTime}
                            onChange={(iso) => setFormData(prev => ({ ...prev, endTime: iso }))} />
                        {errors.endTime && <p className="mt-1 text-xs text-red-600">{errors.endTime}</p>}
                    </div>

                    <div>
                        <label className="text-xs font-semibold text-gray-700 block mb-1">Required Staff <span className="text-red-500">*</span></label>
                        <input type="number" name="requiredStaffCount" min="1" value={formData.requiredStaffCount}
                            onChange={handleChange} className={inputClass(errors.requiredStaffCount)} disabled={isSaving} />
                        {errors.requiredStaffCount && <p className="mt-1 text-xs text-red-600">{errors.requiredStaffCount}</p>}
                    </div>
                </div>

                {showRecurrence && (
                    <div className="p-3 bg-white/60 rounded-lg border border-amber-100 space-y-3">
                        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                            <div>
                                <label className="text-xs font-semibold text-gray-700 block mb-1">Repeats <span className="text-red-500">*</span></label>
                                <select
                                    name="recurrenceType"
                                    value={formData.recurrenceType}
                                    onChange={(e) => {
                                        handleChange(e);
                                        if (e.target.value !== String(RecurrenceType.Weekly)) {
                                            setFormData(prev => ({ ...prev, customDays: [] }));
                                        }
                                    }}
                                    className={inputClass(errors.recurrenceType)}
                                    disabled={isSaving}
                                >
                                    <option value={RecurrenceType.Daily}>Daily</option>
                                    <option value={RecurrenceType.Weekly}>Weekly on specific days</option>
                                    <option value={RecurrenceType.Monthly}>Monthly on the same date</option>
                                </select>
                                {errors.recurrenceType && <p className="mt-1 text-xs text-red-600">{errors.recurrenceType}</p>}
                            </div>

                            <div>
                                <label className="text-xs font-semibold text-gray-700 block mb-1">Repeat Every <span className="text-red-500">*</span></label>
                                <div className="flex items-center gap-2">
                                    <input type="number" name="recurrenceInterval" min="1" value={formData.recurrenceInterval}
                                        onChange={handleChange} className={inputClass(errors.recurrenceInterval) + ' w-20'} disabled={isSaving} />
                                    <span className="text-xs text-gray-600">
                                        {Number(formData.recurrenceType) === RecurrenceType.Daily ? 'day(s)'
                                            : Number(formData.recurrenceType) === RecurrenceType.Weekly ? 'week(s)' : 'month(s)'}
                                    </span>
                                </div>
                                {errors.recurrenceInterval && <p className="mt-1 text-xs text-red-600">{errors.recurrenceInterval}</p>}
                            </div>

                            {/* Exception dates sit here, using the space that's otherwise empty next to "Repeat Every" */}
                            <div className="col-span-2">
                                {exceptionDatesBlock}
                            </div>
                        </div>

                        {Number(formData.recurrenceType) === RecurrenceType.Weekly && (
                            <div>
                                <label className="text-xs font-semibold text-gray-700 block mb-1">On Days <span className="text-red-500">*</span></label>
                                <div className="flex flex-wrap gap-1.5">
                                    {WEEK_DAYS.map(day => (
                                        <button key={day.value} type="button" onClick={() => handleCustomDayToggle(day.value)}
                                            disabled={isSaving}
                                            className={`px-2.5 py-1 text-xs rounded-full font-medium transition-colors ${
                                                formData.customDays.includes(day.value)
                                                    ? 'bg-amber-500 text-white'
                                                    : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                                            }`}
                                        >{day.label}</button>
                                    ))}
                                </div>
                                {errors.customDays && <p className="mt-1 text-xs text-red-600">{errors.customDays}</p>}
                            </div>
                        )}
                    </div>
                )}

                {!showRecurrence && exceptionDatesBlock}

                <div className="flex gap-2 justify-end pt-1">
                    {editingShift && (
                        <button type="button" onClick={onCancelEdit} disabled={isSaving}
                            className="h-9 px-3 text-xs font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded transition-all flex items-center gap-1">
                            <X className="w-3 h-3" /> Cancel
                        </button>
                    )}
                    <button type="submit" disabled={isSaving}
                        className="h-9 px-3 text-xs font-semibold bg-amber-500 hover:bg-amber-600 text-white rounded shadow transition-all flex items-center gap-1 disabled:opacity-50">
                        {editingShift ? <Check className="w-3 h-3" /> : <Plus className="w-3 h-3" />}
                        {isSaving ? 'Saving...' : editingShift ? 'Update Shift' : 'Add Shift'}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default ShiftForm;
