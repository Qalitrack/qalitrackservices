import React from 'react';
import { CheckCircleIcon, ClockIcon, InformationCircleIcon } from '@heroicons/react/24/outline';

const RestoreSuccess = ({ result, onClose }) => {
  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleString();
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-2xl">
        <div className="flex items-start">
          <div className="flex-shrink-0">
            <CheckCircleIcon className="h-10 w-10 text-green-500" />
          </div>
          <div className="ml-4">
            <h3 className="text-lg font-medium text-gray-900">Restore Completed Successfully</h3>
            <div className="mt-2 text-sm text-gray-600">
              <p>{result.message}</p>
            </div>
            
            <div className="mt-6 space-y-4">
              <div className="flex items-start">
                <div className="flex-shrink-0">
                  <InformationCircleIcon className="h-5 w-5 text-gray-400" />
                </div>
                <div className="ml-3">
                  <p className="text-sm text-gray-700">
                    <span className="font-medium">Backup ID:</span> {result.backupId}
                  </p>
                  <p className="text-sm text-gray-700">
                    <span className="font-medium">Service:</span> {result.serviceName}
                  </p>
                  <p className="text-sm text-gray-700">
                    <span className="font-medium">Started at:</span> {formatDate(result.startedAt)}
                  </p>
                  <p className="text-sm text-gray-700">
                    <span className="font-medium">Completed at:</span> {formatDate(result.completedAt)}
                  </p>
                  <p className="text-sm text-gray-700">
                    <span className="font-medium">Full backup used:</span> {result.fullBackupUsed}
                  </p>
                </div>
              </div>

              {result.incrementalBackupsUsed?.length > 0 && (
                <div className="flex items-start">
                  <div className="flex-shrink-0">
                    <ClockIcon className="h-5 w-5 text-gray-400" />
                  </div>
                  <div className="ml-3">
                    <p className="text-sm font-medium text-gray-700">Incremental backups applied:</p>
                    <ul className="list-disc pl-5 text-sm text-gray-700">
                      {result.incrementalBackupsUsed.map((backup, idx) => (
                        <li key={idx}>{backup}</li>
                      ))}
                    </ul>
                  </div>
                </div>
              )}
            </div>

            <div className="mt-6">
              <button
                type="button"
                onClick={onClose}
                className="px-4 py-2 text-sm font-medium text-white bg-amber-600 border border-transparent rounded-md shadow-sm hover:bg-amber-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-amber-500"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default RestoreSuccess;
