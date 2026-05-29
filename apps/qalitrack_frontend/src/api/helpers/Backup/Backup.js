import { backupApiClient } from '../BackupApiclient.js';

class BackupAPI {
    constructor() {
        this.baseEndpoint = '/Backup';
    }

    /**
     * Create a new backup
     * @param {Object} backupData - The backup configuration
     * @param {string} backupData.microservice - Name of the microservice to backup
     * @param {string} backupData.saveLocation - Location to save the backup
     * @param {string} [backupData.cronSchedule] - Optional cron schedule for recurring backups
     * @returns {Promise<Object>} The created backup details
     */
    async createBackup(backupData) {
        try {
            const payload = {
                microservice: backupData.microservice,
                type: 0, // Full backup as specified
                saveLocation: backupData.saveLocation,
                cronSchedule: backupData.cronSchedule || ''
            };

            const response = await backupApiClient.post(`${this.baseEndpoint}/create`, payload);
            return response.data;
        } catch (error) {
            throw this._handleError(error);
        }
    }

    /**
     * Get all scheduled backups
     * @returns {Promise<Array>} List of scheduled backups
     */
    async getScheduledBackups() {
        try {
            const response = await backupApiClient.get(`${this.baseEndpoint}/scheduled`);
            return response.data;
        } catch (error) {
            throw this._handleError(error);
        }
    }

    /**
     * Get available backups for a specific microservice
     * @param {string} microservice - The name of the microservice to get backups for
     * @returns {Promise<Array>} List of available backup files
     */
    async getAvailableBackups(microservice) {
        try {
            const response = await backupApiClient.get(`${this.baseEndpoint}/available`, {
                params: { microservice }
            });
            return response.data;
        } catch (error) {
            throw this._handleError(error);
        }
    }

    /**
     * Restore a backup
     * @param {Object} restoreData - The restore configuration
     * @param {string} restoreData.microservice - Name of the microservice to restore to
     * @param {string} restoreData.backupSourcePath - Path to the backup file
     * @param {string} restoreData.backupId - ID of the backup to restore
     * @returns {Promise<Object>} The restore operation result
     */
    async restoreBackup(restoreData) {
        try {
            const payload = {
                microservice: restoreData.microservice,
                backupSourcePath: restoreData.backupSourcePath,
                backupId: restoreData.backupId
            };

            const response = await backupApiClient.post(`${this.baseEndpoint}/restore`, payload);
            return response.data;
        } catch (error) {
            throw this._handleError(error);
        }
    }

    /**
     * Restore a backup by ID
     * @param {string} backupId - The ID of the backup to restore
     * @param {string} microservice - The microservice to restore to
     * @returns {Promise<Object>} The restore operation result
     */
    async restoreBackupById(backupId, microservice) {
        try {
            const payload = {
                microservice,
                backupId
            };


            const response = await backupApiClient.post(
                `${this.baseEndpoint}/restore`,
                payload,
                {
                    headers: {
                        'accept': 'text/plain',
                        'Content-Type': 'application/json'
                    }
                }
            );

            return response.data;
        } catch (error) {
            throw this._handleError(error);
        }
    }

    /**
     * Unschedule a backup
     * @param {string} microservice - Name of the microservice
     * @param {number} backupType - Type of backup (0 = Full, 1 = Incremental)
     * @returns {Promise<Object>} The operation result
     */
    async unscheduleBackup(microservice, backupType) {
        try {
            const response = await backupApiClient.delete(`${this.baseEndpoint}/unschedule/${encodeURIComponent(microservice)}/${backupType}`);
            return response.data;
        } catch (error) {
            throw this._handleError(error);
        }
    }

    /**
     * Handle API errors
     * @private
     */
    _handleError(error) {
        if (error.response) {
            // The request was made and the server responded with a status code
            // that falls out of the range of 2xx
            const { status, data } = error.response;

            if (status === 400) {
                return new Error(data.message || 'Invalid request data');
            } else if (status === 404) {
                return new Error('Backup service not found');
            } else if (status === 500) {
                return new Error('Internal server error');
            }
        } else if (error.request) {
            // The request was made but no response was received
            return new Error('No response from server. Please check your connection.');
        }

        // Something happened in setting up the request that triggered an Error
        return error;
    }
}

// Create and export a singleton instance
const backupAPI = new BackupAPI();
export { BackupAPI, backupAPI };
