// App.jsx
import React, { Suspense } from 'react';
import { ConfigProvider } from 'antd';
import { HashRouter as BrowserRouter, Routes, Route } from 'react-router-dom';
import { routes } from './App/routes.jsx';
import { SidebarSettingsProvider } from './components/Context/Sidebarsettingscontext';
import { ColorSchemeProvider, useColorScheme } from './components/Context/ColorSchemeContext';
import AppLicenseGate from './components/AppLicenseGate';
import ErrorBoundary from './components/ErrorBoundary';

// Makes every antd component (Button, Select, DatePicker, table sorters, etc.)
// follow the active color scheme instead of antd's stock default blue.
function ThemedAntConfig({ children }) {
    const { colorScheme, COLOR_SCHEMES } = useColorScheme();
    const scheme = COLOR_SCHEMES[colorScheme] ?? COLOR_SCHEMES.midnight;
    return (
        <ConfigProvider theme={{ token: { colorPrimary: scheme.primary } }}>
            {children}
        </ConfigProvider>
    );
}

const Loading = () => (
    <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-amber-500"></div>
    </div>
);

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
        <ColorSchemeProvider>
            <ThemedAntConfig>
                <AppLicenseGate>
                    <BrowserRouter>
                        <SidebarSettingsProvider>
                            <ErrorBoundary>
                                <Suspense fallback={<Loading />}>
                                    <Routes>
                                        {routes.map((route, index) => renderRoute(route, index))}
                                    </Routes>
                                </Suspense>
                            </ErrorBoundary>
                        </SidebarSettingsProvider>
                    </BrowserRouter>
                </AppLicenseGate>
            </ThemedAntConfig>
        </ColorSchemeProvider>
    );
}

export default App;