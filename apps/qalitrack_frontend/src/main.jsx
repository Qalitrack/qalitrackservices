import React from "react";
import ReactDOM from "react-dom/client";
import { Provider } from "react-redux";
import { PersistGate } from "redux-persist/integration/react";
import App from "./App";
import { AuthProvider } from "./components/Context/authContext.jsx";
import { store, persistor } from "./store";
import ErrorBoundary from "./components/ErrorBoundary";
import "./index.css";
import "antd/dist/reset.css";

window.addEventListener("error", (event) => {
  console.error("Window Error:", event.error);
});

window.addEventListener("unhandledrejection", (event) => {
  console.error("Unhandled Promise:", event.reason);
});

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <ErrorBoundary>
      <Provider store={store}>
        <PersistGate loading={null} persistor={persistor}>
          <AuthProvider>
            <App />
          </AuthProvider>
        </PersistGate>
      </Provider>
    </ErrorBoundary>
  </React.StrictMode>
);
