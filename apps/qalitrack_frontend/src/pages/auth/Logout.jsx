import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import  useAuth from '../../helpers/auth.js';
import { apiClient } from '../../helpers/apiClients';

const Logout = () => {
    const { logout } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        const performLogout = async () => {
            try {
                // Call the server-side logout endpoint
                await apiClient.post('/Auth/logout', {}, {
                    headers: {
                        'Authorization': `Bearer ${localStorage.getItem('authToken')}`
                    }
                });
            } catch (error) {
                console.error('Logout error:', error);
            } finally {
                logout();

                navigate('/login', { replace: true });
            }
        };
        performLogout();
    }, [logout, navigate]);

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-100">
            <div className="text-center">
                <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-500 mx-auto mb-4"></div>
                <p className="text-gray-700">Logging out...</p>
            </div>
        </div>
    );
};

export default Logout;