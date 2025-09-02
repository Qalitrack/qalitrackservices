import React, { Suspense } from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { routes } from "./App/routes";

export default function App() {
  // A simple loading fallback for lazy-loaded components
  const loadingFallback = (
    <div style={{ padding: "2rem", textAlign: "center" }}>
      Loading...
    </div>
  );

  // Function to render routes recursively, including nested routes
  const renderRoutes = (routesArray) => {
    return routesArray.map((route, index) => (
      <Route
        key={index}
        path={route.path}
        element={<route.component />}
      >
        {/* Render nested routes if they exist */}
        {route.children && renderRoutes(route.children)}
      </Route>
    ));
  };

  return (
    <BrowserRouter>
      {/* Suspense is required for lazy-loaded components */}
      <Suspense fallback={loadingFallback}>
        <Routes>
          {renderRoutes(routes)}
        </Routes>
      </Suspense>
    </BrowserRouter>
  );
}
