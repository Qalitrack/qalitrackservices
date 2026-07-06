import React, { useState } from 'react';
import { backupAPI } from '../../../api/helpers/Backup/Backup';
import { Save, X, Database } from 'lucide-react';

const Backup = () => {
    const [error, setError] = useState(null);
    const [formData, setFormData] = useState({
        microservice: 'QalitrackDB',
        scheduleType: 'none',
        frequency: '',
        minute: '0',
        hour: '0',
        dayOfWeek: '1',
        dayOfMonth: '1',
        customCron: ''
    });

    const [scheduleStep, setScheduleStep] = useState('frequency');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [successMessage, setSuccessMessage] = useState('');


    const handleSubmit = async (e) => {
        e.preventDefault();
        setIsSubmitting(true);
        setError(null);

        try {
            const backupData = {
                microservice: formData.microservice,
                ...(formData.scheduleType === 'custom' && { cronSchedule: formData.customCron }),
                ...(formData.scheduleType !== 'none' && formData.scheduleType !== 'custom' && {
                    cronSchedule: getCronExpression()
                })
            };

            const response = await backupAPI.createBackup(backupData);

            if (response && response.message) {
                setSuccessMessage(response.message);
            } else {
                setSuccessMessage('Backup created successfully!');
            }

            setFormData(prev => ({
                microservice: 'QalitrackDB',
                scheduleType: 'none',
                frequency: '',
                minute: '0',
                hour: '0',
                dayOfWeek: '1',
                dayOfMonth: '1',
                customCron: ''
            }));

            setTimeout(() => setSuccessMessage(''), 5000);
        } catch (err) {
            setError(err.response?.data?.message || err.message || 'Failed to create backup');
        } finally {
            setIsSubmitting(false);
        }
    };

    const getCronExpression = () => {
        const { scheduleType, frequency, minute, hour, dayOfWeek, dayOfMonth } = formData;
        
        if (scheduleType === 'none') return '';
        if (scheduleType === 'custom') return formData.customCron;

        const padZero = num => String(num).padStart(2, '0');
        
        switch(frequency) {
            case 'minute':
                return `0 ${minute}/1 * * * ?`;
            case 'hourly':
                return `0 ${minute} * * * ?`;
            case 'daily':
                return `0 ${minute} ${hour} * * ?`;
            case 'weekly':
                return `0 ${minute} ${hour} ? * ${dayOfWeek} *`;
            case 'monthly':
                return `0 ${minute} ${hour} ${dayOfMonth} * ?`;
            case 'yearly':
                return `0 ${minute} ${hour} ${dayOfMonth} 1 ?`;
            default:
                return '';
        }
    };

    return (
        <div className="max-w-2xl mx-auto p-4">
            <div className="bg-white rounded-lg shadow p-6">
                <h2 className="text-xl font-semibold mb-6">Create New Backup</h2>

                {error && (
                    <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-4">
                        <div className="flex items-center">
                            <X className="h-5 w-5 text-red-400 mr-2" />
                            <span className="text-red-700">{error}</span>
                        </div>
                    </div>
                )}

                {successMessage && (
                    <div className="bg-green-50 border-l-4 border-green-400 p-4 mb-4">
                        <div className="flex items-center">
                            <Save className="h-5 w-5 text-green-400 mr-2" />
                            <span className="text-green-700">{successMessage}</span>
                        </div>
                    </div>
                )}

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Target Database</label>
                        <div className="flex items-center gap-2 p-2 border rounded bg-gray-50 text-sm text-gray-700">
                            <Database className="h-4 w-4 text-amber-500 shrink-0" />
                            <span className="font-medium">QalitrackDB</span>
                            <span className="text-gray-400 text-xs">(postgres-prod · all schemas)</span>
                        </div>
                    </div>

                    <div className="pt-4 border-t">
                        <h3 className="text-sm font-medium text-gray-700 mb-3">Schedule (Optional)</h3>

                        <div className="space-y-4">
                            {scheduleStep === 'frequency' && (
                                <div className="space-y-2">
                                    <p className="text-sm text-gray-600">How often should the backup run?</p>
                                    {[
                                        { value: 'none', label: 'One-time backup' },
                                        { value: 'minute', label: 'Every Minute' },
                                        { value: 'hourly', label: 'Hourly' },
                                        { value: 'daily', label: 'Daily' },
                                        { value: 'weekly', label: 'Weekly' },
                                        { value: 'monthly', label: 'Monthly' },
                                        { value: 'yearly', label: 'Yearly' },
                                        { value: 'custom', label: 'Custom Cron Expression' }
                                    ].map(option => (
                                        <button
                                            key={option.value}
                                            type="button"
                                            onClick={() => {
                                                if (option.value === 'custom') {
                                                    setScheduleStep('custom');
                                                    setFormData(prev => ({ ...prev, scheduleType: 'custom' }));
                                                } else if (option.value === 'none') {
                                                    setFormData(prev => ({ ...prev, scheduleType: 'none' }));
                                                } else {
                                                    setFormData(prev => ({ ...prev, scheduleType: 'recurring', frequency: option.value }));
                                                    setScheduleStep('details');
                                                }
                                            }}
                                            className={`w-full text-left p-3 rounded border ${formData.scheduleType === option.value ? 'border-amber-400 bg-amber-50' : 'border-gray-200 hover:bg-gray-50'}`}
                                        >
                                            <span className="font-medium">{option.label}</span>
                                        </button>
                                    ))}
                                </div>
                            )}

                            {scheduleStep === 'details' && (
                                <div className="space-y-4">
                                    <button
                                        type="button"
                                        onClick={() => setScheduleStep('frequency')}
                                        className="flex items-center text-sm text-gray-600 hover:text-gray-800"
                                    >
                                        ← Back
                                    </button>

                                    {formData.frequency === 'minute' && (
                                        <div className="space-y-2">
                                            <label className="block text-sm font-medium text-gray-700">
                                                Every <input
                                                    type="number"
                                                    min="1"
                                                    max="59"
                                                    value={formData.minute}
                                                    onChange={(e) => setFormData(prev => ({ ...prev, minute: e.target.value }))}
                                                    className="w-16 p-1 border rounded text-center"
                                                /> minute(s)
                                            </label>
                                        </div>
                                    )}

                                    {(formData.frequency === 'hourly' || formData.frequency === 'daily' || formData.frequency === 'weekly' || formData.frequency === 'monthly' || formData.frequency === 'yearly') && (
                                        <div className="space-y-4">
                                            <div>
                                                <label className="block text-sm font-medium text-gray-700 mb-1">At minute</label>
                                                <select
                                                    value={formData.minute}
                                                    onChange={(e) => setFormData(prev => ({ ...prev, minute: e.target.value }))}
                                                    className="w-full p-2 border rounded"
                                                >
                                                    {Array.from({ length: 60 }, (_, i) => (
                                                        <option key={i} value={i}>{String(i).padStart(2, '0')}</option>
                                                    ))}
                                                </select>
                                            </div>

                                            {(formData.frequency === 'daily' || formData.frequency === 'weekly' || formData.frequency === 'monthly' || formData.frequency === 'yearly') && (
                                                <div>
                                                    <label className="block text-sm font-medium text-gray-700 mb-1">At hour</label>
                                                    <select
                                                        value={formData.hour}
                                                        onChange={(e) => setFormData(prev => ({ ...prev, hour: e.target.value }))}
                                                        className="w-full p-2 border rounded"
                                                    >
                                                        {Array.from({ length: 24 }, (_, i) => (
                                                            <option key={i} value={i}>{String(i).padStart(2, '0')}:00</option>
                                                        ))}
                                                    </select>
                                                </div>
                                            )}

                                            {formData.frequency === 'weekly' && (
                                                <div>
                                                    <label className="block text-sm font-medium text-gray-700 mb-1">On day</label>
                                                    <select
                                                        value={formData.dayOfWeek}
                                                        onChange={(e) => setFormData(prev => ({ ...prev, dayOfWeek: e.target.value }))}
                                                        className="w-full p-2 border rounded"
                                                    >
                                                        <option value="1">Monday</option>
                                                        <option value="2">Tuesday</option>
                                                        <option value="3">Wednesday</option>
                                                        <option value="4">Thursday</option>
                                                        <option value="5">Friday</option>
                                                        <option value="6">Saturday</option>
                                                        <option value="0">Sunday</option>
                                                    </select>
                                                </div>
                                            )}

                                            {(formData.frequency === 'monthly' || formData.frequency === 'yearly') && (
                                                <div>
                                                    <label className="block text-sm font-medium text-gray-700 mb-1">
                                                        {formData.frequency === 'yearly' ? 'On day of month' : 'On day'}
                                                    </label>
                                                    <select
                                                        value={formData.dayOfMonth}
                                                        onChange={(e) => setFormData(prev => ({ ...prev, dayOfMonth: e.target.value }))}
                                                        className="w-full p-2 border rounded"
                                                    >
                                                        {Array.from({ length: 31 }, (_, i) => (
                                                            <option key={i + 1} value={i + 1}>
                                                                {i + 1}{i === 0 ? 'st' : i === 1 ? 'nd' : i === 2 ? 'rd' : 'th'}
                                                            </option>
                                                        ))}
                                                        <option value="L">Last day of month</option>
                                                    </select>
                                                </div>
                                            )}
                                        </div>
                                    )}
                                </div>
                            )}

                            {scheduleStep === 'custom' && (
                                <div className="space-y-2">
                                    <button
                                        type="button"
                                        onClick={() => setScheduleStep('frequency')}
                                        className="flex items-center text-sm text-gray-600 hover:text-gray-800"
                                    >
                                        ← Back
                                    </button>
                                    <div className="mt-2">
                                        <input
                                            type="text"
                                            name="customCron"
                                            value={formData.customCron}
                                            onChange={(e) => setFormData(prev => ({ ...prev, customCron: e.target.value }))}
                                            className="w-full p-2 border rounded"
                                            placeholder="e.g., 0 0 * * *"
                                        />
                                        <p className="text-xs text-gray-500 mt-1">
                                            Format: second minute hour day-of-month month day-of-week year(optional)
                                        </p>
                                    </div>
                                </div>
                            )}

                            {(formData.scheduleType !== 'none' && formData.scheduleType !== 'custom') && (
                                <div className="bg-blue-50 p-3 rounded text-sm text-blue-800">
                                    <p>Schedule: {getCronExpression()}</p>
                                </div>
                            )}
                        </div>
                    </div>

                    <div className="flex justify-end pt-4 border-t">
                        <button
                            type="button"
                            onClick={() => {
                                setFormData({
                                    microservice: 'QalitrackDB',
                                    scheduleType: 'none',
                                    customCron: '',
                                    frequency: '',
                                    minute: '0',
                                    hour: '0',
                                    dayOfWeek: '1',
                                    dayOfMonth: '1'
                                });
                                setScheduleStep('frequency');
                            }}
                            className="px-4 py-2 border rounded mr-2"
                        >
                            Reset
                        </button>
                        <button
                            type="submit"
                            disabled={isSubmitting}
                            className="px-4 py-2 bg-amber-600 text-white rounded disabled:opacity-50"
                        >
                            {isSubmitting ? 'Creating...' : 'Create Backup'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default Backup;