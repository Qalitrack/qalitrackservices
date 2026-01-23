// Add this to your app to test API connectivity
// Place it in src/components/ApiTest.jsx

import React, { useState } from 'react';
import { Button, Card, Typography, Space, Alert } from 'antd';
import axios from 'axios';

const { Text, Title } = Typography;

export default function ApiTest() {
  const [results, setResults] = useState([]);
  const [testing, setTesting] = useState(false);

  const addResult = (test, status, message, details = null) => {
    setResults(prev => [...prev, { test, status, message, details, time: new Date().toLocaleTimeString() }]);
  };

  const testConnectivity = async () => {
    setResults([]);
    setTesting(true);

    // Test 1: Direct API access (will likely fail due to CORS)
    try {
      addResult('Direct API', 'testing', 'Testing direct API access...');
      const response = await axios.get('https://qalitrack.cseco.co.ke/api/Transaction', {
        timeout: 5000,
      });
      addResult('Direct API', 'success', 'Direct access works!', response.status);
    } catch (err) {
      addResult('Direct API', 'error', `Expected CORS error: ${err.message}`, err.code);
    }

    // Test 2: Proxy - Transaction endpoint
    try {
      addResult('Proxy /Transaction', 'testing', 'Testing proxy...');
      const response = await axios.get('/Transaction', {
        timeout: 5000,
        params: { PageSize: 1 }
      });
      addResult('Proxy /Transaction', 'success', 'Proxy works!', response.status);
    } catch (err) {
      addResult('Proxy /Transaction', 'error', err.message, {
        code: err.code,
        status: err.response?.status,
        data: err.response?.data
      });
    }

    // Test 3: Proxy - MasterData endpoint
    try {
      addResult('Proxy /MasterData', 'testing', 'Testing MasterData proxy...');
      const response = await axios.get('/MasterData/Weighbridges', {
        timeout: 5000,
        params: { PageSize: 1 }
      });
      addResult('Proxy /MasterData', 'success', 'MasterData proxy works!', response.status);
    } catch (err) {
      addResult('Proxy /MasterData', 'error', err.message, {
        code: err.code,
        status: err.response?.status
      });
    }

    // Test 4: Check if backend is reachable from browser
    try {
      addResult('Backend Ping', 'testing', 'Checking backend availability...');
      const response = await fetch('https://qalitrack.cseco.co.ke/api/health', {
        method: 'HEAD',
        mode: 'no-cors',
      });
      addResult('Backend Ping', 'success', 'Backend is reachable');
    } catch (err) {
      addResult('Backend Ping', 'error', 'Backend unreachable', err.message);
    }

    setTesting(false);
  };

  return (
    <Card title="API Connectivity Test" style={{ margin: '20px' }}>
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        <Button 
          type="primary" 
          onClick={testConnectivity} 
          loading={testing}
          size="large"
        >
          Run Connectivity Tests
        </Button>

        {results.length > 0 && (
          <div>
            <Title level={5}>Test Results:</Title>
            {results.map((result, idx) => (
              <Alert
                key={idx}
                message={`${result.time} - ${result.test}`}
                description={
                  <div>
                    <Text>{result.message}</Text>
                    {result.details && (
                      <pre style={{ fontSize: '11px', marginTop: '8px' }}>
                        {JSON.stringify(result.details, null, 2)}
                      </pre>
                    )}
                  </div>
                }
                type={
                  result.status === 'success' ? 'success' :
                  result.status === 'error' ? 'error' : 'info'
                }
                style={{ marginBottom: '10px' }}
              />
            ))}
          </div>
        )}

        <Alert
          message="Debugging Information"
          description={
            <div>
              <p><strong>Current Origin:</strong> {window.location.origin}</p>
              <p><strong>Expected Dev Server:</strong> http://localhost:5173</p>
              <p><strong>Target API:</strong> https://qalitrack.cseco.co.ke/api</p>
            </div>
          }
          type="info"
        />
      </Space>
    </Card>
  );
}