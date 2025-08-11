/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./App.{js,jsx,ts,tsx}",
    "./src/**/*.{js,jsx,ts,tsx}"
  ],
  theme: {
    extend: {
      colors: {
        primary: '#2563EB',   // blue-600
        secondary: '#FBBF24', // amber-400
        background: '#F3F4F6', // gray-100
      },
    },
  },
  plugins: [],
};
