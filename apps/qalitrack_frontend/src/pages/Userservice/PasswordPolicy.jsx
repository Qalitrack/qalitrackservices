import React, { useState, useEffect } from 'react';
import { format, parseISO } from 'date-fns';
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
            // Ensure the response has data before updating the state
            if (updatedData && typeof updatedData === 'object') {
                setPolicy(updatedData);
                setOriginalPolicy(updatedData);
            } else {
                // If the response is not as expected, re-fetch the data to be safe
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
        return <div className="flex justify-center items-center h-32"><div>Loading...</div></div>;
    }

    if (error) {
        return <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-md" role="alert">{error}</div>;
    }

    return (
        <div className="bg-white shadow-lg md:shadow-xl rounded-xl p-6 md:p-8 max-w-full md:max-w-2xl mx-auto my-4 md:my-10 transition-all duration-300 md:hover:shadow-2xl md:hover:-translate-y-1">
            <h2 className="text-xl md:text-2xl font-bold text-amber-600 mb-6">Password Policy</h2>

            {isEditing ? (
                <form onSubmit={handleUpdate} className="space-y-6">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label htmlFor="minimumLength" className="block text-sm font-medium text-gray-700 mb-1">Minimum Length</label>
                            <input id="minimumLength" name="minimumLength" type="number" value={policy.minimumLength} onChange={handleChange} className="w-full p-3 border border-gray-300 rounded-lg shadow-sm focus:ring-amber-500 focus:border-amber-500" min={0} required />
                        </div>
                        <div>
                            <label htmlFor="maxAgeDays" className="block text-sm font-medium text-gray-700 mb-1">Max Age (Days)</label>
                            <input id="maxAgeDays" name="maxAgeDays" type="number" value={policy.maxAgeDays} onChange={handleChange} className="w-full p-3 border border-gray-300 rounded-lg shadow-sm focus:ring-amber-500 focus:border-amber-500" min={0} required />
                        </div>
                    </div>

                    <div className="space-y-4">
                        <div className="flex items-center justify-between bg-gray-50 p-3 rounded-lg">
                            <label htmlFor="requireUppercase" className="font-medium text-gray-700">Require Uppercase</label>
                            <input id="requireUppercase" name="requireUppercase" type="checkbox" checked={policy.requireUppercase} onChange={handleChange} className="h-5 w-5 text-amber-600 border-gray-300 rounded focus:ring-amber-500" />
                        </div>
                        <div className="flex items-center justify-between bg-gray-50 p-3 rounded-lg">
                            <label htmlFor="requireLowercase" className="font-medium text-gray-700">Require Lowercase</label>
                            <input id="requireLowercase" name="requireLowercase" type="checkbox" checked={policy.requireLowercase} onChange={handleChange} className="h-5 w-5 text-amber-600 border-gray-300 rounded focus:ring-amber-500" />
                        </div>
                        <div className="flex items-center justify-between bg-gray-50 p-3 rounded-lg">
                            <label htmlFor="requireDigit" className="font-medium text-gray-700">Require Digit</label>
                            <input id="requireDigit" name="requireDigit" type="checkbox" checked={policy.requireDigit} onChange={handleChange} className="h-5 w-5 text-amber-600 border-gray-300 rounded focus:ring-amber-500" />
                        </div>
                        <div className="flex items-center justify-between bg-gray-50 p-3 rounded-lg">
                            <label htmlFor="requireSpecialCharacter" className="font-medium text-gray-700">Require Special Character</label>
                            <input id="requireSpecialCharacter" name="requireSpecialCharacter" type="checkbox" checked={policy.requireSpecialCharacter} onChange={handleChange} className="h-5 w-5 text-amber-600 border-gray-300 rounded focus:ring-amber-500" />
                        </div>
                    </div>

                    <div className="flex justify-end space-x-4 pt-4">
                        <button type="button" onClick={handleCancel} className="px-6 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-amber-500">Cancel</button>
                        <button type="submit" className="px-6 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-amber-500 hover:bg-amber-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-amber-500 disabled:bg-gray-300" disabled={isUpdating}>
                            {isUpdating ? 'Saving...' : 'Save Changes'}
                        </button>
                    </div>
                </form>
            ) : (
                <div className="space-y-4">
                    {policy && Object.entries(policy)
                        .filter(([key]) => !['id', 'policyId', 'createdAt', 'updatedBy', 'createdBy','isDeleted'].includes(key))
                        .map(([key, value]) => {
                            if (key === 'updatedAt') {
                                return (
                                    <div key={key} className="flex justify-between items-center bg-gray-50 p-4 rounded-lg hover:bg-gray-100 transition-colors">
                                        <span className="font-medium text-gray-700">Updated At</span>
                                        <span className="text-gray-900 font-semibold">
                                            {format(parseISO(value), "PPPp")}
                                        </span>
                                    </div>
                                );
                            }
                            return (
                                <div key={key} className="flex justify-between items-center bg-gray-50 p-4 rounded-lg hover:bg-gray-100 transition-colors">
                                    <span className="font-medium text-gray-700">{key.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase())}</span>
                                    <span className="text-gray-900 font-semibold">{value != null ? value.toString() : 'N/A'}</span>
                                </div>
                            );
                        })}
                    <div className="flex justify-end pt-4">
                        <button onClick={() => setIsEditing(true)} className="px-6 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-amber-500 hover:bg-amber-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-amber-500">Update Policy</button>
                    </div>
                </div>
            )}

            {updateMessage.text && (
                <div className={`mt-4 p-3 rounded-lg text-center text-sm font-medium ${updateMessage.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                    {updateMessage.text}
                </div>
            )}
        </div>
    );
};

export default PasswordPolicy;
