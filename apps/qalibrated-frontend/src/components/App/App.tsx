// App.tsx
import React, { Suspense } from 'react';
import { BrowserRouter as Router } from 'react-router-dom';
import Login from '../pages/auth/login';


const App: React.FC = () => {
  return (
    <Router>
      <Suspense fallback={<div className="p-8 text-center">Loading...</div>}>
        <Login />
      </Suspense>
    </Router>
  );
};

export default App;
