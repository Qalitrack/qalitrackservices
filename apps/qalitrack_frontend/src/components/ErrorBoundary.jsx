import React from 'react';
import { AlertTriangle, RefreshCw, LogIn, ArrowLeft } from 'lucide-react';

function FullScreenError({ error, onReset, onGoBack, onGoToLogin }) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100 p-6">
      <div className="w-full max-w-md bg-white rounded-2xl shadow-xl overflow-hidden">
        <div className="bg-amber-500 px-6 py-8 text-center">
          <AlertTriangle size={48} className="text-white mx-auto mb-3" />
          <h1 className="text-xl font-bold text-white">Something Went Wrong</h1>
          <p className="text-amber-100 text-sm mt-1">
            The app ran into an unexpected problem.
          </p>
        </div>

        <div className="px-6 py-6 space-y-3">
          {error?.message && (
            <p className="text-xs text-gray-500 bg-gray-50 rounded-lg p-3 font-mono break-words">
              {error.message}
            </p>
          )}

          <p className="text-sm text-gray-600 text-center">
            You can try going back, reloading this page, or returning to login.
          </p>

          <button
            onClick={onReset}
            className="w-full flex items-center justify-center gap-2 py-2.5 px-4 rounded-lg font-medium bg-amber-500 text-white hover:bg-amber-600 transition-colors"
          >
            <RefreshCw size={16} />
            Try Again
          </button>

          <button
            onClick={onGoBack}
            className="w-full flex items-center justify-center gap-2 py-2.5 px-4 rounded-lg font-medium border border-gray-300 text-gray-700 hover:bg-gray-50 transition-colors"
          >
            <ArrowLeft size={16} />
            Go Back
          </button>

          <button
            onClick={onGoToLogin}
            className="w-full flex items-center justify-center gap-2 py-2 px-4 text-sm text-gray-500 hover:text-gray-700 transition-colors"
          >
            <LogIn size={14} />
            Return to Login
          </button>
        </div>

        <div className="px-6 pb-5 text-center">
          <p className="text-xs text-gray-400">
            Powered by <span className="font-semibold text-gray-500">Qalibrated Systems</span>
          </p>
        </div>
      </div>
    </div>
  );
}

function InlineError({ error, onReset, onGoBack, onGoToLogin }) {
  return (
    <div className="flex items-center justify-center h-full min-h-[300px] p-6">
      <div className="w-full max-w-sm bg-white rounded-xl shadow-md overflow-hidden">
        <div className="bg-amber-500 px-5 py-5 text-center">
          <AlertTriangle size={36} className="text-white mx-auto mb-2" />
          <h2 className="text-base font-bold text-white">Page Error</h2>
          <p className="text-amber-100 text-xs mt-0.5">
            This page couldn't load correctly.
          </p>
        </div>

        <div className="px-5 py-5 space-y-3">
          {error?.message && (
            <p className="text-xs text-gray-500 bg-gray-50 rounded-lg p-2.5 font-mono break-words">
              {error.message}
            </p>
          )}

          <p className="text-sm text-gray-600 text-center">
            Try reloading the page or go back to continue working.
          </p>

          <button
            onClick={onReset}
            className="w-full flex items-center justify-center gap-2 py-2 px-4 rounded-lg font-medium bg-amber-500 text-white hover:bg-amber-600 transition-colors text-sm"
          >
            <RefreshCw size={14} />
            Try Again
          </button>

          <div className="flex gap-2">
            <button
              onClick={onGoBack}
              className="flex-1 flex items-center justify-center gap-1.5 py-2 px-3 rounded-lg text-sm border border-gray-300 text-gray-700 hover:bg-gray-50 transition-colors"
            >
              <ArrowLeft size={13} />
              Go Back
            </button>
            <button
              onClick={onGoToLogin}
              className="flex-1 flex items-center justify-center gap-1.5 py-2 px-3 rounded-lg text-sm border border-gray-300 text-gray-700 hover:bg-gray-50 transition-colors"
            >
              <LogIn size={13} />
              Login
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

export default class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error) {
    return { hasError: true, error };
  }

  componentDidCatch(error, info) {
  }

  handleReset = () => {
    this.setState({ hasError: false, error: null });
    if (this.props.onReset) this.props.onReset();
  };

  handleGoBack = () => {
    this.setState({ hasError: false, error: null });
    window.history.back();
  };

  handleGoToLogin = () => {
    this.setState({ hasError: false, error: null });
    window.location.hash = '#/login';
  };

  render() {
    if (!this.state.hasError) return this.props.children;

    const props = {
      error: this.state.error,
      onReset: this.handleReset,
      onGoBack: this.handleGoBack,
      onGoToLogin: this.handleGoToLogin,
    };

    return this.props.variant === 'inline'
      ? <InlineError {...props} />
      : <FullScreenError {...props} />;
  }
}
