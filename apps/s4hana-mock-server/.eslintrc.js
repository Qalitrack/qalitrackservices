module.exports = {
  env: {
    node: true,
    es2021: true,
    browser: true
  },
  extends: [
    'eslint:recommended'
  ],
  parserOptions: {
    ecmaVersion: 12,
    sourceType: 'module'
  },
  rules: {
    // Error prevention
    'no-console': 'off', // Allow console.log for server logging
    'no-unused-vars': ['error', { 'argsIgnorePattern': '^_' }],
    'no-undef': 'error',
    
    // Code style
    'indent': ['error', 4],
    'linebreak-style': ['error', 'unix'],
    'quotes': ['error', 'single'],
    'semi': ['error', 'always'],
    
    // Best practices
    'eqeqeq': 'error',
    'no-trailing-spaces': 'error',
    'no-multiple-empty-lines': ['error', { 'max': 2 }],
    'comma-dangle': ['error', 'never']
  },
  ignorePatterns: [
    'node_modules/',
    'dist/',
    'webapp/localService/data/*.json'
  ]
};