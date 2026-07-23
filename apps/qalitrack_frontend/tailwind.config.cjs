/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./index.html','./src/**/*.{js,jsx}'],
  theme: {
    extend: {
      colors: {
        primary: { DEFAULT: '#f59e0b'}, // amber-500
        // amber-* resolves through CSS variables (set in index.css) instead of
        // static hex, so EVERY Tailwind utility that uses it — bg/text/border/
        // ring/divide/gradient stops, hover:/focus: variants, all of it — follows
        // the active color scheme automatically. No more hand-listing which
        // amber-* classes happen to be covered; the scheme switch just works.
        amber: {
          50:  'var(--cs-50)',
          100: 'var(--cs-100)',
          200: 'var(--cs-200)',
          300: 'var(--cs-300)',
          400: 'var(--cs-400)',
          500: 'var(--cs-500)',
          600: 'var(--cs-600)',
          700: 'var(--cs-700)',
          800: 'var(--cs-800)',
          900: 'var(--cs-900)',
          950: 'var(--cs-950)',
        },
      }
    }
  },
  plugins: []
};
