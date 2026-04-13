import React, { useState, useEffect } from 'react';
import { updateMicroservice } from '../../../api/helpers/Backup/Microservice.js';
import { Save, X, AlertCircle, CheckCircle, Database, Globe, Calendar } from 'lucide-react';

const statusOptions = [
    { value: 0, label: 'Active' },
    { value: 1, label: 'Inactive' },
    { value: 2, label: 'Paused' }
];

const EditMicroservice = ({ service, onClose, onSuccess }) => {
    const [formData, setFormData] = useState({
        name: '',
        connectionString: '',
        status: 0
    });

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

    useEffect(() => {
        if (service) {
            setFormData({
                name: service.name || '',
                connectionString: service.connectionString || '',
                status: service.status || 0
            });
        }
    }, [service]);

    const handleInputChange = (e) => {
        const { name, value, type } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: name === 'status' ? parseInt(value, 10) : value
        }));
        if (error) setError('');
    };

    const validateForm = () => {
        if (!formData.name.trim()) {
            setError('Service name is required');
            return false;
        }
        if (!formData.connectionString.trim()) {
            setError('Connection string is required');
            return false;
        }
        return true;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!validateForm() || !service) return;

        setLoading(true);
        setError('');

        try {
            const updateData = {
                ...formData,
                lastBackupAt: null // Explicitly set to null as requested
            };

            console.log('Updating microservice with data:', updateData);
            const result = await updateMicroservice(service.name, updateData);

            if (result) {
                setSuccess(true);
                console.log('Microservice updated successfully:', result);

                if (onSuccess) {
                    onSuccess(result);
                }

                // Close the modal after a short delay
                setTimeout(() => {
                    if (onClose) onClose();
                }, 1500);
            } else {
                setError('Failed to update microservice. Please try again.');
            }
        } catch (err) {
            setError(err.message || 'An error occurred while updating the microservice');
            console.error('Error updating microservice:', err);
        } finally {
            setLoading(false);
        }
    };

    if (!service) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
                {/* Header */}
                <div className="bg-amber-500 text-white px-6 py-4 rounded-t-lg flex justify-between items-center">
                    <h2 className="text-xl font-semibold">Edit Microservice</h2>
                    <button
                        onClick={onClose}
                        className="text-white hover:text-gray-200 focus:outline-none"
                        disabled={loading}
                    >
                        <X size={24} />
                    </button>
                </div>

                {/* Form */}
                <div className="p-6">
                    {error && (
                        <div className="mb-4 p-3 bg-red-50 text-red-700 rounded-md flex items-start">
                            <AlertCircle className="w-5 h-5 mr-2 mt-0.5 flex-shrink-0" />
                            <span>{error}</span>
                        </div>
                    )}

                    {success ? (
                        <div className="text-center py-8">
                            <CheckCircle className="w-12 h-12 text-green-500 mx-auto mb-4" />
                            <h3 className="text-xl font-medium text-gray-900 mb-2">Success!</h3>
                            <p className="text-gray-600">Microservice updated successfully.</p>
                        </div>
                    ) : (
                        <form onSubmit={handleSubmit}>
                            <div className="space-y-4">
                                {/* Name Field */}
                                <div>
                                    <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
                                        Service Name
                                    </label>
                                    <input
                                        type="text"
                                        id="name"
                                        name="name"
                                        value={formData.name}
                                        onChange={handleInputChange}
                                        className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                                        disabled={loading}
                                        required
                                    />
                                </div>

                                {/* Connection String */}
                                <div>
                                    <label htmlFor="connectionString" className="block text-sm font-medium text-gray-700 mb-1">
                                        Connection String
                                    </label>
                                    <textarea
                                        id="connectionString"
                                        name="connectionString"
                                        value={formData.connectionString}
                                        onChange={handleInputChange}
                                        rows={4}
                                        className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500 font-mono text-xs text-gray-700 bg-gray-50 resize-none"
                                        disabled={loading}
                                        required
                                        spellCheck={false}
                                    />
                                </div>

                                {/* Status */}
                                <div>
                                    <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-1">
                                        Status
                                    </label>
                                    <select
                                        id="status"
                                        name="status"
                                        value={formData.status}
                                        onChange={handleInputChange}
                                        className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                                        disabled={loading}
                                    >
                                        {statusOptions.map((option) => (
                                            <option key={option.value} value={option.value}>
                                                {option.label}
                                            </option>
                                        ))}
                                    </select>
                                </div>
                            </div>

                            {/* Form Actions */}
                            <div className="mt-6 flex justify-end space-x-3">
                                <button
                                    type="button"
                                    onClick={onClose}
                                    disabled={loading}
                                    className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-amber-500 disabled:opacity-50"
                                >
                                    Cancel
                                </button>
                                <button
                                    type="submit"
                                    disabled={loading}
                                    className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-amber-500 hover:bg-amber-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-amber-500 disabled:opacity-50"
                                >
                                    {loading ? (
                                        <>
                                            <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                            </svg>
                                            Updating...
                                        </>
                                    ) : (
                                        <>
                                            <Save className="-ml-1 mr-2 h-4 w-4" />
                                            Update Service
                                        </>
                                    )}
                                </button>
                            </div>
                        </form>
                    )}
                </div>
            </div>
        </div>
    );
};

export default EditMicroservice;
