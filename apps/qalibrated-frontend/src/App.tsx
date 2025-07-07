import React from 'react';
import { Button, Card } from '@qalibrated/shared-components';
import './App.css';

function App() {
  return (
    <div className="App">
      <header className="App-header">
        <Card title="Qalibrated Frontend">
          <p>Welcome to the Qalibrated frontend application!</p>
          <Button onClick={() => alert('Button clicked!')}>
            Click Me
          </Button>
        </Card>
      </header>
    </div>
  );
}

export default App;
