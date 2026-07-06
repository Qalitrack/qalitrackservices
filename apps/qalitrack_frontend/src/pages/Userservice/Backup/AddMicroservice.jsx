import React, { useState } from 'react';
import { createMicroservice } from '../../../api/helpers/Backup/Microservice.js';
import { Save, X, AlertCircle, CheckCircle, Database } from 'lucide-react';

const AddMicroservice = ({ onClose, onSuccess }) => {
    const [formData, setFormData] = useState({
        name: '',
        host: '',
        port: '5432',
        dbName: '',
        username: '',
        password: '',
    });
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
        if (error) setError('');
    };

    const buildConnectionString = () =>
        `Host=${formData.host};Port=${formData.port};Database=${formData.dbName};Username=${formData.username};Password=${formData.password};MinPoolSize=50;MaxPoolSize=500;Timeout=30;CommandTimeout=60;ConnectionIdleLifetime=300;ConnectionPruningInterval=10;Pooling=true;`;

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!formData.name.trim() || !formData.dbName.trim() || !formData.host.trim()) {
            setError('Service name, host and database name are required');
            return;
        }

        setLoading(true);
        setError('');
        try {
            const result = await createMicroservice({
                name: formData.name.trim(),
                connectionString: buildConnectionString(),
                lastBackupAt: null,
            });
            setSuccess(true);
            if (onSuccess) onSuccess(result);
            setTimeout(() => onClose?.(), 1500);
        } catch (err) {
            setError(err.message || 'Failed to create microservice');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-lg shadow-xl w-full max-w-lg">
                {/* Header */}
                <div className="bg-amber-500 text-white px-6 py-4 rounded-t-lg flex justify-between items-center">
                    <div className="flex items-center gap-2">
                        <Database size={20} />
                        <h2 className="text-lg font-semibold">Add Backup Target</h2>
                    </div>
                    <button onClick={onClose} disabled={loading} className="hover:text-gray-200">
                        <X size={22} />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                    {error && (
                        <div className="flex items-center gap-2 p-3 bg-red-50 border border-red-200 rounded-md text-sm text-red-700">
                            <AlertCircle size={16} className="shrink-0" />
                            {error}
                        </div>
                    )}
                    {success && (
                        <div className="flex items-center gap-2 p-3 bg-green-50 border border-green-200 rounded-md text-sm text-green-700">
                            <CheckCircle size={16} className="shrink-0" />
                            Backup target created successfully!
                        </div>
                    )}

                    {/* Service Name */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Service Name *</label>
                        <input
                            name="name"
                            value={formData.name}
                            onChange={handleChange}
                            placeholder="e.g. QalitrackDB"
                            disabled={loading}
                            required
                            className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-amber-500 focus:border-amber-500"
                        />
                    </div>

                    {/* Host + Port */}
                    <div className="grid grid-cols-3 gap-3">
                        <div className="col-span-2">
                            <label className="block text-sm font-medium text-gray-700 mb-1">Host *</label>
                            <input
                                name="host"
                                value={formData.host}
                                onChange={handleChange}
                                placeholder="postgres-prod"
                                disabled={loading}
                                required
                                className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-amber-500 focus:border-amber-500"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">Port</label>
                            <input
                                name="port"
                                value={formData.port}
                                onChange={handleChange}
                                disabled={loading}
                                className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-amber-500 focus:border-amber-500"
                            />
                        </div>
                    </div>

                    {/* DB Name + Username + Password */}
                    <div className="grid grid-cols-2 gap-3">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">Database *</label>
                            <input
                                name="dbName"
                                value={formData.dbName}
                                onChange={handleChange}
                                placeholder="qalitrackdb"
                                disabled={loading}
                                required
                                className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-amber-500 focus:border-amber-500"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">Username</label>
                            <input
                                name="username"
                                value={formData.username}
                                onChange={handleChange}
                                placeholder="qalitrack"
                                disabled={loading}
                                className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-amber-500 focus:border-amber-500"
                            />
                        </div>
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
                        <input
                            type="password"
                            name="password"
                            value={formData.password}
                            onChange={handleChange}
                            disabled={loading}
                            className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-amber-500 focus:border-amber-500"
                        />
                    </div>

                    {/* Connection string preview */}
                    {formData.host && formData.dbName && (
                        <div className="bg-gray-50 border border-gray-200 rounded-md p-3">
                            <p className="text-xs text-gray-500 mb-1 font-medium uppercase tracking-wide">Preview</p>
                            <p className="font-mono text-xs text-gray-600 break-all">{buildConnectionString()}</p>
                        </div>
                    )}

                    {/* Actions */}
                    <div className="flex justify-end gap-3 pt-2">
                        <button
                            type="button"
                            onClick={onClose}
                            disabled={loading}
                            className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            disabled={loading || success}
                            className="inline-flex items-center gap-2 px-4 py-2 bg-amber-500 hover:bg-amber-600 text-white rounded-md text-sm font-medium disabled:opacity-50"
                        >
                            {loading ? (
                                <><div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" /> Creating...</>
                            ) : (
                                <><Save size={15} /> Create</>
                            )}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default AddMicroservice;
