import React, { useState } from 'react';
import { createMicroservice } from '../../../helpers/Backup/Microservice.js';
import { Plus, Save, X, AlertCircle, CheckCircle, Database, Globe, Calendar } from 'lucide-react';

const AddMicroservice = ({ onClose, onSuccess }) => {
    const [formData, setFormData] = useState({
        name: '',
        dbName: '',
        dbUser: 'postgres',
        dbPassword: 'postgres',
        dbHost: 'localhost',
        dbPort: '5432',
        connectionString: ''
    });

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

    const handleInputChange = (e) => {
        const { name, value, type } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'number' ? parseInt(value) : value
        }));

        // Clear error when user starts typing
        if (error) setError('');
    };

    const validateForm = () => {
        if (!formData.name.trim()) {
            setError('Service name is required');
            return false;
        }

        if (!formData.dbName.trim()) {
            setError('Database name is required');
            return false;
        }

        return true;
    };

    const handleSubmit = async () => {

        if (!validateForm()) return;

        setLoading(true);
        setError('');

        try {
            // Generate connection string
            const connectionString = `postgres://${formData.dbUser}:${formData.dbPassword}@${formData.dbHost}:${formData.dbPort}/${formData.dbName}`;

            // Prepare data for submission
            const submitData = {
                name: formData.name.trim(),
                connectionString: connectionString,
                lastBackupAt: null
            };

            // lastBackupAt is already set to null in the submitData object

            console.log('Submitting microservice data:', submitData);

            const result = await createMicroservice(submitData);

            if (result) {
                setSuccess(true);
                console.log('Microservice created successfully:', result);

                // Call success callback if provided
                if (onSuccess) {
                    onSuccess(result);
                }

                // Reset form after successful creation
                setTimeout(() => {
                    handleReset();
                    if (onClose) onClose();
                }, 1500);
            } else {
                setError('Failed to create microservice. Please try again.');
            }
        } catch (err) {
            setError(err.message || 'An error occurred while creating the microservice');
            console.error('Error creating microservice:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleReset = () => {
        setFormData({
            name: '',
            connectionString: '',
            status: 0,
            lastBackupAt: ''
        });
        setError('');
        setSuccess(false);
    };

    const getCurrentDateTime = () => {
        const now = new Date();
        const year = now.getFullYear();
        const month = String(now.getMonth() + 1).padStart(2, '0');
        const day = String(now.getDate()).padStart(2, '0');
        const hours = String(now.getHours()).padStart(2, '0');
        const minutes = String(now.getMinutes()).padStart(2, '0');

        return `${year}-${month}-${day}T${hours}:${minutes}`;
    };

    return (
        <div className="bg-white rounded-lg shadow-lg border max-w-2xl mx-auto">
            <div className="flex items-center justify-between p-6 border-b">
                <div className="flex items-center space-x-3">
                    <div className="p-2 bg-blue-50 rounded-lg">
                        <Plus className="w-6 h-6 text-blue-600" />
                    </div>
                    <div>
                        <h2 className="text-xl font-semibold text-gray-900">Add New Microservice</h2>
                        <p className="text-sm text-gray-600">Create a new microservice entry</p>
                    </div>
                </div>
                {onClose && (
                    <button
                        onClick={onClose}
                        className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                    >
                        <X className="w-5 h-5 text-gray-500" />
                    </button>
                )}
            </div>

            <div className="p-6 space-y-6">
                {/* Success Message */}
                {success && (
                    <div className="bg-green-50 border border-green-200 rounded-lg p-4 flex items-center space-x-2">
                        <CheckCircle className="w-5 h-5 text-green-600" />
                        <span className="text-green-800">Microservice created successfully!</span>
                    </div>
                )}

                {/* Error Message */}
                {error && (
                    <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-center space-x-2">
                        <AlertCircle className="w-5 h-5 text-red-600" />
                        <span className="text-red-800">{error}</span>
                    </div>
                )}

                {/* Service Name */}
                <div>
                    <label htmlFor="name" className="flex items-center text-sm font-medium text-gray-700 mb-2">
                        <Database className="w-4 h-4 mr-2" />
                        Service Name *
                    </label>
                    <input
                        type="text"
                        id="name"
                        name="name"
                        value={formData.name}
                        onChange={handleInputChange}
                        placeholder="e.g., AuthService, OrderService, UserService"
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        required
                        disabled={loading}
                    />
                </div>

                {/* Database Connection Details */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {/* Database Name */}
                    <div>
                        <label htmlFor="dbName" className="block text-sm font-medium text-gray-700 mb-1">
                            Database Name *
                        </label>
                        <input
                            type="text"
                            id="dbName"
                            name="dbName"
                            value={formData.dbName}
                            onChange={handleInputChange}
                            placeholder="database_name"
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                            required
                            disabled={loading}
                        />
                    </div>

                    {/* Database User */}
                    <div>
                        <label htmlFor="dbUser" className="block text-sm font-medium text-gray-700 mb-1">
                            Username
                        </label>
                        <input
                            type="text"
                            id="dbUser"
                            name="dbUser"
                            value={formData.dbUser}
                            onChange={handleInputChange}
                            placeholder="postgres"
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                            disabled={loading}
                        />
                    </div>

                    {/* Database Password */}
                    <div>
                        <label htmlFor="dbPassword" className="block text-sm font-medium text-gray-700 mb-1">
                            Password
                        </label>
                        <input
                            type="password"
                            id="dbPassword"
                            name="dbPassword"
                            value={formData.dbPassword}
                            onChange={handleInputChange}
                            placeholder="password"
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                            disabled={loading}
                        />
                    </div>

                    {/* Database Host */}
                    <div>
                        <label htmlFor="dbHost" className="block text-sm font-medium text-gray-700 mb-1">
                            Host
                        </label>
                        <input
                            type="text"
                            id="dbHost"
                            name="dbHost"
                            value={formData.dbHost}
                            onChange={handleInputChange}
                            placeholder="localhost"
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                            disabled={loading}
                        />
                    </div>

                    {/* Database Port */}
                    <div>
                        <label htmlFor="dbPort" className="block text-sm font-medium text-gray-700 mb-1">
                            Port
                        </label>
                        <input
                            type="text"
                            id="dbPort"
                            name="dbPort"
                            value={formData.dbPort}
                            onChange={handleInputChange}
                            placeholder="5432"
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                            disabled={loading}
                        />
                    </div>
                </div>

                {/* Connection String Preview */}
                <div className="bg-gray-50 p-3 rounded-lg border border-gray-200">
                    <p className="text-sm font-medium text-gray-700 mb-1">Connection String:</p>
                <div>
                    <label htmlFor="dbUser" className="block text-sm font-medium text-gray-700 mb-1">
                        Username
                    </label>
                    <input
                        type="text"
                        id="dbUser"
                        name="dbUser"
                        value={formData.dbUser}
                        onChange={handleInputChange}
                        placeholder="postgres"
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        disabled={loading}
                    />
                </div>

                <div className="flex items-center space-x-2">
                    <button
                        type="button"
                        onClick={handleReset}
                        disabled={loading || success}
                        className="px-6 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50"
                    >
                        Reset
                    </button>
                    <button
                        type="button"
                        onClick={handleSubmit}
                        disabled={loading || success}
                        className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 flex items-center space-x-2"
                    >
                        {loading ? (
                            <>
                                <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                                <span>Creating...</span>
                            </>
                        ) : (
                            <>
                                <Save className="w-4 h-4" />
                                <span>Create Microservice</span>
                            </>
                        )}
                    </button>
                </div>
                </div>
            </div>
        </div>
    );
};

export default AddMicroservice;