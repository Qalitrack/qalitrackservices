import React, { Suspense } from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { routes } from "./App/routes";

export default function App() {
  const loadingFallback = (
    <div style={{ padding: "2rem", textAlign: "center" }}>
      Loading...
    </div>
  );

  const renderRoutes = (routesArray) => {
    return routesArray.map((route, index) => (
      <Route key={index} path={route.path} element={<route.component />}>
        {route.children && renderRoutes(route.children)}
      </Route>
    ));
  };

  return (
    <BrowserRouter
      future={{
        v7_startTransition: true,   // ✅ enable startTransition ahead of v7
        v7_relativeSplatPath: true, // ✅ enable new relative splat resolution
      }}
    >
      <Suspense fallback={loadingFallback}>
        <Routes>{renderRoutes(routes)}</Routes>
      </Suspense>
    </BrowserRouter>
  );
}
