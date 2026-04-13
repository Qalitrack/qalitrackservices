import React, { useState, useEffect } from 'react';
import { format, parseISO } from 'date-fns';
import { Lock, Edit2, X, Check } from 'lucide-react';
import { fetchPasswordPolicy, updatePasswordPolicy } from '../../api/helpers/UserService/PasswordPolicy/passwordpolicy';

const PasswordPolicy = () => {
    const [policy, setPolicy] = useState(null);
    const [originalPolicy, setOriginalPolicy] = useState(null);
    const [loading, setLoading] = useState(true);
    const [isEditing, setIsEditing] = useState(false);
    const [isUpdating, setIsUpdating] = useState(false);
    const [error, setError] = useState(null);
    const [updateMessage, setUpdateMessage] = useState({ text: '', type: '' });

    const loadPolicy = async () => {
        setLoading(true);
        setError(null);
        try {
            const data = await fetchPasswordPolicy();
            setPolicy(data);
            setOriginalPolicy(data);
        } catch (err) {
            setError(err.message || 'Failed to fetch policy.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadPolicy();
    }, []);

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setPolicy(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : Number(value),
        }));
    };

    const handleUpdate = async (e) => {
        e.preventDefault();
        setIsUpdating(true);
        setUpdateMessage({ text: 'Updating policy...', type: 'info' });
        try {
            const updatedData = await updatePasswordPolicy(policy);
            if (updatedData && typeof updatedData === 'object') {
                setPolicy(updatedData);
                setOriginalPolicy(updatedData);
            } else {
                await loadPolicy();
            }
            setUpdateMessage({ text: 'Password policy updated successfully!', type: 'success' });
            setIsEditing(false);
        } catch (err) {
            setUpdateMessage({ text: err.message || 'Failed to update policy.', type: 'error' });
        } finally {
            setIsUpdating(false);
            setTimeout(() => setUpdateMessage({ text: '', type: '' }), 5000);
        }
    };

    const handleCancel = () => {
        setPolicy(originalPolicy);
        setIsEditing(false);
    };

    if (loading) {
        return (
            <div className="h-full flex items-center justify-center">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-amber-500"></div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="h-full p-4">
                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md text-sm">{error}</div>
            </div>
        );
    }

    const HIDDEN_KEYS = ['id', 'policyId', 'createdAt', 'updatedBy', 'createdBy', 'isDeleted'];

    return (
        <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
            {/* Header */}
            <div className="px-4 py-3 bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50 border-b border-amber-200 flex items-center justify-between">
                <div className="flex items-center gap-2">
                    <div className="w-7 h-7 rounded-md bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-sm">
                        <Lock className="w-4 h-4 text-white" />
                    </div>
                    <div>
                        <span className="text-sm font-bold text-gray-900 block leading-tight">Password Policy</span>
                        <span className="text-xs text-amber-800 font-medium">Configure system-wide password requirements</span>
                    </div>
                </div>
                {!isEditing && (
                    <button
                        onClick={() => setIsEditing(true)}
                        className="flex items-center gap-1.5 h-7 px-3 text-xs font-semibold bg-gradient-to-r from-amber-500 to-orange-600 hover:from-amber-600 hover:to-orange-700 text-white rounded shadow transition-all"
                    >
                        <Edit2 className="w-3 h-3" />
                        Edit Policy
                    </button>
                )}
            </div>

            {/* Feedback */}
            {updateMessage.text && (
                <div className={`mx-4 mt-3 px-4 py-2 rounded-md text-sm font-medium border ${
                    updateMessage.type === 'success'
                        ? 'bg-green-50 border-green-200 text-green-700'
                        : updateMessage.type === 'error'
                        ? 'bg-red-50 border-red-200 text-red-700'
                        : 'bg-blue-50 border-blue-200 text-blue-700'
                }`}>
                    {updateMessage.text}
                </div>
            )}

            {/* Content */}
            <div className="flex-1 overflow-auto p-4">
                <div className="max-w-2xl mx-auto">
                    {isEditing ? (
                        <form onSubmit={handleUpdate} className="space-y-4">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <label htmlFor="minimumLength" className="block text-sm font-semibold text-gray-800 mb-1">Minimum Length</label>
                                    <input
                                        id="minimumLength" name="minimumLength" type="number"
                                        value={policy.minimumLength} onChange={handleChange}
                                        className="w-full px-3 py-2 text-sm border border-amber-200 rounded-md focus:ring-1 focus:ring-amber-400 focus:border-amber-400 outline-none"
                                        min={0} required
                                    />
                                </div>
                                <div>
                                    <label htmlFor="maxAgeDays" className="block text-sm font-semibold text-gray-800 mb-1">Max Age (Days)</label>
                                    <input
                                        id="maxAgeDays" name="maxAgeDays" type="number"
                                        value={policy.maxAgeDays} onChange={handleChange}
                                        className="w-full px-3 py-2 text-sm border border-amber-200 rounded-md focus:ring-1 focus:ring-amber-400 focus:border-amber-400 outline-none"
                                        min={0} required
                                    />
                                </div>
                            </div>

                            <div className="space-y-2">
                                {[
                                    { key: 'requireUppercase', label: 'Require Uppercase Letter' },
                                    { key: 'requireLowercase', label: 'Require Lowercase Letter' },
                                    { key: 'requireDigit', label: 'Require Digit' },
                                    { key: 'requireSpecialCharacter', label: 'Require Special Character' },
                                ].map(({ key, label }) => (
                                    <div key={key} className="flex items-center justify-between bg-amber-50 border border-amber-100 px-4 py-3 rounded-md">
                                        <label htmlFor={key} className="text-sm font-medium text-gray-700">{label}</label>
                                        <input
                                            id={key} name={key} type="checkbox"
                                            checked={policy[key]} onChange={handleChange}
                                            className="h-4 w-4 text-amber-600 border-gray-300 rounded focus:ring-amber-500"
                                        />
                                    </div>
                                ))}
                            </div>

                            <div className="flex justify-end gap-2 pt-2">
                                <button
                                    type="button" onClick={handleCancel}
                                    className="flex items-center gap-1.5 px-4 py-2 text-sm border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 transition-colors"
                                >
                                    <X className="w-3.5 h-3.5" /> Cancel
                                </button>
                                <button
                                    type="submit" disabled={isUpdating}
                                    className="flex items-center gap-1.5 px-4 py-2 text-sm bg-amber-500 hover:bg-amber-600 text-white rounded-md font-medium disabled:opacity-50 transition-colors"
                                >
                                    <Check className="w-3.5 h-3.5" />
                                    {isUpdating ? 'Saving...' : 'Save Changes'}
                                </button>
                            </div>
                        </form>
                    ) : (
                        <div className="space-y-2">
                            {policy && Object.entries(policy)
                                .filter(([key]) => !HIDDEN_KEYS.includes(key))
                                .map(([key, value]) => {
                                    const label = key.replace(/([A-Z])/g, ' $1').replace(/^./, s => s.toUpperCase());
                                    let displayValue;
                                    if (key === 'updatedAt') {
                                        displayValue = format(parseISO(value), 'PPPp');
                                    } else if (typeof value === 'boolean') {
                                        displayValue = value
                                            ? <span className="inline-flex items-center gap-1 text-green-700 font-semibold"><Check className="w-3.5 h-3.5" /> Yes</span>
                                            : <span className="text-gray-600 font-medium">No</span>;
                                    } else {
                                        displayValue = <span className="font-semibold text-gray-900">{value != null ? value.toString() : 'N/A'}</span>;
                                    }
                                    return (
                                        <div key={key} className="flex items-center justify-between bg-gray-50 hover:bg-amber-50 border border-gray-100 px-4 py-3 rounded-md transition-colors">
                                            <span className="text-sm font-medium text-gray-800">{label}</span>
                                            <span className="text-sm">{displayValue}</span>
                                        </div>
                                    );
                                })}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default PasswordPolicy;
