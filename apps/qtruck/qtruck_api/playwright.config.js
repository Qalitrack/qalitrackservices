// @ts-check
const { defineConfig, devices } = require('@playwright/test');

module.exports = defineConfig({
  testDir: './tests/e2e',
  fullyParallel: false, // Run tests sequentially for API testing
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: 1, // Single worker for API tests
  reporter: [
    ['html', { outputFolder: 'playwright-report' }],
    ['list', { printSteps: true }], // Show test steps in console
    ['junit', { outputFile: 'test-results/results.xml' }]
  ],
  maxFailures: 1, // Stop on first failure (fail-fast)
  use: {
    baseURL: 'http://localhost:8000',
    extraHTTPHeaders: {
      'Content-Type': 'application/json',
    },
  },

  projects: [
    {
      name: 'qtruck-api-tests',
      testDir: './tests/e2e',
    },
  ],
});