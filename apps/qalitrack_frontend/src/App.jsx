// App.jsx - Clean version that uses routes from separate file
import React, { Suspense } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { routes } from './App/routes.jsx'; // Import routes configuration

// Loading component
const Loading = () => (
    <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-amber-500"></div>
    </div>
);

// Recursive function to render routes from configuration
const renderRoute = (route, index) => {
    const { path, element, children, ...props } = route;

    if (children && children.length > 0) {
        return (
            <Route key={index} path={path} element={element} {...props}>
                {children.map((child, childIndex) => renderRoute(child, `${index}-${childIndex}`))}
            </Route>
        );
    }

    return <Route key={index} path={path} element={element} {...props} />;
};

function App() {
    return (
        <BrowserRouter
            future={{
                v7_startTransition: true,
                v7_relativeSplatPath: true,
            }}
        >
            <Suspense fallback={<Loading />}>
                <Routes>
                    {routes.map((route, index) => renderRoute(route, index))}
                </Routes>
            </Suspense>
        </BrowserRouter>
    );
}

export default App;