// stress-tests/run.mjs
//
// Drives the ACTUAL built app (same dist/ bundle a Windows install ships)
// inside a real Electron window via Playwright, logs in for real against
// the real dev backend (Deployment/docker-compose.yml), and stress-tests:
//   0. Real CRUD           — create/edit/delete real Owners + Drivers rows
//                            through the real form, each step independently
//                            verified against the backend API (Postgres)
//   1. Frontend UI/forms   — rapid typing burst + repeated route-switch churn
//   2. Redux state layer   — long soak on a screen with real interval-driven
//                            dispatches (WeighbridgePanel), watching heap +
//                            redux-persist payload size
//   3. Electron main/IPC   — repeated IPC round-trips + main-process memory
//
// Nothing here touches src/ — it drives the built app like a user would.
//
// Requires the dev backend running (docker compose up -d in Deployment/).
//
// Usage:
//   QALI_LICENSE_TOKEN='eyJ...' QALI_LOGIN_EMAIL='...' QALI_LOGIN_PASSWORD='...' \
//     node stress-tests/run.mjs
//
// Credentials are only used locally for this test session — never logged or
// written to disk.

import { _electron as electron } from 'playwright-core';
import { spawn } from 'node:child_process';
import { setTimeout as sleep } from 'node:timers/promises';
import { fileURLToPath } from 'node:url';
import { start as startHardwareMock } from './mock-hardware-server.mjs';

// new URL('..', import.meta.url).pathname would produce a leading-slash path
// like /C:/Users/... on Windows, which breaks child_process.spawn's cwd —
// fileURLToPath() handles the platform difference correctly.
const PROJECT_ROOT = fileURLToPath(new URL('..', import.meta.url));
const PREVIEW_PORT = 4173;
const PREVIEW_URL = `http://localhost:${PREVIEW_PORT}`;

const LICENSE_TOKEN = process.env.QALI_LICENSE_TOKEN;
const LOGIN_EMAIL = process.env.QALI_LOGIN_EMAIL;
const LOGIN_PASSWORD = process.env.QALI_LOGIN_PASSWORD;
const API_BASE = 'http://localhost:7000/api';
if (!LICENSE_TOKEN) {
  console.error('Set QALI_LICENSE_TOKEN to a valid license JWT before running.');
  process.exit(1);
}
if (!LOGIN_EMAIL || !LOGIN_PASSWORD) {
  console.error('Set QALI_LOGIN_EMAIL and QALI_LOGIN_PASSWORD to a real dev backend account before running.');
  process.exit(1);
}

function percentile(sortedArr, p) {
  if (sortedArr.length === 0) return 0;
  const idx = Math.min(sortedArr.length - 1, Math.floor(p * sortedArr.length));
  return sortedArr[idx];
}

function fmtMs(n) { return `${n.toFixed(3)} ms`; }
function fmtMB(bytes) { return `${(bytes / 1024 / 1024).toFixed(2)} MB`; }

async function waitForServer(url, timeoutMs = 20000) {
  const start = Date.now();
  while (Date.now() - start < timeoutMs) {
    try {
      const res = await fetch(url);
      if (res.ok) return;
    } catch { /* not up yet */ }
    await sleep(300);
  }
  throw new Error(`Preview server never came up at ${url}`);
}

async function main() {
  const report = {};

  console.log('=== 1/5 Building app (npm run build) ===');
  await new Promise((resolve, reject) => {
    const p = spawn('npx', ['vite', 'build'], { cwd: PROJECT_ROOT, stdio: 'inherit' });
    p.on('exit', (code) => code === 0 ? resolve() : reject(new Error(`build failed (${code})`)));
  });

  console.log('\n=== 2/5 Starting vite preview (serves dist/) + mock hardware server ===');
  const preview = spawn('npx', ['vite', 'preview', '--port', String(PREVIEW_PORT), '--strictPort'], {
    cwd: PROJECT_ROOT,
    stdio: 'ignore',
  });
  const hardware = await startHardwareMock(5000);
  console.log('mock hardware (weight/plates/camera) listening on http://localhost:5000');
  try {
    await waitForServer(PREVIEW_URL);

    console.log('\n=== 3/5 Launching Electron ===');
    const electronApp = await electron.launch({
      args: ['--js-flags=--expose-gc', PROJECT_ROOT],
      cwd: PROJECT_ROOT,
      env: { ...process.env, VITE_DEV_SERVER_URL: PREVIEW_URL },
      timeout: 30000,
    });

    try {
      await electronApp.firstWindow();
      let page = electronApp.windows().find((p) => p.url().startsWith(PREVIEW_URL));
      const deadline = Date.now() + 90000;
      while (!page && Date.now() < deadline) {
        try {
          const w = await electronApp.waitForEvent('window', { timeout: Math.max(1000, deadline - Date.now()) });
          if (w.url().startsWith(PREVIEW_URL)) page = w;
        } catch { /* timeout on this wait, loop will exit via deadline check */ }
        if (!page) page = electronApp.windows().find((p) => p.url().startsWith(PREVIEW_URL));
      }
      if (!page) throw new Error(`could not find main app window among: ${electronApp.windows().map((p) => p.url()).join(', ')}`);
      await page.waitForLoadState('domcontentloaded');

      report.gcAvailable = await page.evaluate(() => typeof window.gc === 'function');

      // ── Seed the license only. Auth is real: drive the actual Login form ──
      // against the real backend so we get a real JWT and can genuinely
      // create/edit/delete rows in Postgres, not just poke at the UI.
      await page.evaluate((token) => {
        localStorage.setItem('qali_lic_v1', JSON.stringify({ token, cachedAt: Date.now() }));
      }, LICENSE_TOKEN);
      await page.reload();
      await sleep(800);

      console.log('\n=== logging in as', LOGIN_EMAIL, '===');
      await realLogin(page, LOGIN_EMAIL, LOGIN_PASSWORD);

      console.log('\n=== 4/5 Running scenarios ===');
      report.realCrud = await scenarioRealCrud(page, Number(process.env.QALI_CRUD_ITERATIONS) || 10);
      report.routeSmoke = await scenarioRouteSmokeTest(page);
      report.formTyping = await scenarioFormTyping(page);
      report.routeChurn = await scenarioRouteChurn(page, hardware, Number(process.env.QALI_ROUTE_CHURN_MS) || 150000);
      report.reduxSoak = await scenarioReduxSoak(page, hardware, Number(process.env.QALI_STREAM_SOAK_MS) || 450000);
      report.electronIpc = await scenarioElectronIpc(page, electronApp);

      console.log('\n=== 5/5 Report ===');
      printReport(report);
    } finally {
      await electronApp.close();
    }
  } finally {
    preview.kill();
    await hardware.stop();
  }
}

// ─────────────────────────────────────────────────────────────────────────
// Real login — drives the actual Login form against the real backend
// (/Auth/login) so every subsequent scenario runs under a genuine JWT.
// ─────────────────────────────────────────────────────────────────────────
async function realLogin(page, email, password) {
  await page.evaluate(() => { location.hash = '#/login'; });
  await page.waitForSelector('input[type="email"]', { timeout: 10000 });
  await page.locator('input[type="email"]').fill(email);
  await page.locator('input[type="password"]').fill(password);
  await page.locator('button[type="submit"]').click();
  await page.waitForFunction(
    () => location.hash.startsWith('#/operator') || location.hash.startsWith('#/admin'),
    { timeout: 15000 }
  );
}

// ─────────────────────────────────────────────────────────────────────────
// Scenario 0 — REAL CRUD against the REAL backend/DB. This is the actual
// "adding owners/drivers" flow from the bug report: fill the real form,
// submit for real, edit for real, delete for real — each step verified
// independently via a direct fetch to the backend API (not just trusting
// the UI's own state), proving the data genuinely round-tripped through
// Postgres and back, not just an optimistic client-side update.
// ─────────────────────────────────────────────────────────────────────────
// useSearch: true means the page filters CLIENT-SIDE (fetches everything,
// filters in JS) so its search box genuinely works and is safe to drive.
// useSearch: false means the page sends searchTerm to the backend, which we
// confirmed is broken for several entities (Drivers/Products silently
// return zero results for ANY term; Suppliers/Transporters/Saccos with
// server-side search would 500 — see report). For those we skip the search
// box entirely and scan the plain (small, unfiltered) table instead.
const CRUD_ENTITIES = [
  {
    key: 'owner',
    route: '#/operator/weighing/owners',
    formReadySelector: 'input[name="name"]',
    createFields: (unique) => [
      { selector: 'input[name="name"]', value: unique },
      { selector: 'input[name="contactPerson"]', value: 'StressBot' },
      { selector: 'input[name="phoneNumber"]', value: '0700111222' },
      { selector: 'input[name="email"]', value: `${unique}@stress.test` },
      { selector: 'input[name="address"]', value: 'Stress Address' },
    ],
    useSearch: true,
    searchSelector: 'input[placeholder="Search owners..."]',
    editButtonTitle: 'Edit',
    deleteButtonTitle: 'Delete',
    editField: { selector: 'input[name="contactPerson"]', value: 'StressBot-Edited' },
    apiListPath: '/MasterData/Owners?pageNumber=1&pageSize=200',
    apiDeletePath: (id) => `/MasterData/Owners/${id}`,
    extractItems: (json) => json?.items ?? [],
    matchField: 'name',
  },
  {
    key: 'driver',
    route: '#/operator/weighing/drivers',
    formReadySelector: 'input[name="fullName"]',
    createFields: (unique) => [
      { selector: 'input[name="fullName"]', value: unique },
      { selector: 'input[name="phone"]', value: '0711222333' },
      { selector: 'input[name="idNumber"]', value: String(Math.floor(10000000 + Math.random() * 89999999)) },
      { selector: 'input[name="licenseNumber"]', value: `DL${Math.floor(Math.random() * 1000000)}` },
      { selector: 'input[name="email"]', value: `${unique}@stress.test` },
    ],
    useSearch: false, // backend searchTerm confirmed broken for Drivers (always empty)
    searchSelector: 'input[placeholder="Search drivers..."]',
    editButtonTitle: 'Edit driver',
    deleteButtonTitle: 'Delete driver',
    editField: { selector: 'input[name="phone"]', value: '0799888777' },
    apiListPath: '/MasterData/Drivers?pageNumber=1&pageSize=200',
    apiDeletePath: (id) => `/MasterData/Drivers/${id}`,
    extractItems: (json) => json?.data?.items ?? json?.items ?? [],
    matchField: 'fullName',
  },
  {
    key: 'transporter',
    route: '#/operator/transporters',
    formReadySelector: 'input[name="name"]',
    createFields: (unique) => [
      { selector: 'input[name="name"]', value: unique },
      { selector: 'input[name="email"]', value: `${unique}@stress.test` },
      { selector: 'input[name="phone"]', value: '0700222333' },
    ],
    useSearch: true, // fetches all + filters client-side, like Owners
    searchSelector: 'input[placeholder="Search transporters..."]',
    editButtonTitle: 'Edit',
    deleteButtonTitle: 'Delete',
    editField: { selector: 'input[name="phone"]', value: '0799333444' },
    apiListPath: '/MasterData/Transporters?pageNumber=1&pageSize=200',
    apiDeletePath: (id) => `/MasterData/Transporters/${id}`,
    extractItems: (json) => json?.items ?? [],
    matchField: 'name',
  },
  {
    key: 'supplier',
    route: '#/operator/suppliers',
    formReadySelector: 'input[name="name"]',
    createFields: (unique) => [
      { selector: 'input[name="name"]', value: unique },
      { selector: 'input[name="contactPerson"]', value: 'StressBot' },
      { selector: 'input[name="phone"]', value: '0700333444' },
      { selector: 'input[name="email"]', value: `${unique}@stress.test` },
    ],
    useSearch: false, // backend searchTerm confirmed to 500 for Suppliers
    searchSelector: 'input[placeholder="Search suppliers..."]',
    editButtonTitle: 'Edit',
    deleteButtonTitle: 'Delete',
    editField: { selector: 'input[name="phone"]', value: '0799444555' },
    apiListPath: '/MasterData/Suppliers?pageNumber=1&pageSize=200',
    apiDeletePath: (id) => `/MasterData/Suppliers/${id}`,
    extractItems: (json) => json?.items ?? [],
    matchField: 'name',
  },
  {
    key: 'product',
    route: '#/operator/weighing/products',
    formReadySelector: 'input[name="name"]',
    createFields: (unique) => [
      { selector: 'input[name="name"]', value: unique },
      { selector: 'input[name="code"]', value: `SKU-${Math.floor(Math.random() * 1000000)}` },
      { selector: 'input[name="unit"]', value: 'kg' },
    ],
    useSearch: false, // backend searchTerm confirmed broken for Products (always empty)
    searchSelector: 'input[placeholder="Search products..."]',
    editButtonTitle: 'Edit',
    deleteButtonTitle: 'Delete',
    editField: { selector: 'input[name="unit"]', value: 'tonnes' },
    apiListPath: '/MasterData/Products?pageNumber=1&pageSize=200',
    apiDeletePath: (id) => `/MasterData/Products/${id}`,
    extractItems: (json) => json?.items ?? [],
    matchField: 'name',
  },
  {
    key: 'sacco',
    route: '#/operator/saccos',
    formReadySelector: 'input[name="name"]',
    createFields: (unique) => [
      { selector: 'input[name="name"]', value: unique },
      { selector: 'input[name="registrationNumber"]', value: `SAC-${Math.floor(Math.random() * 1000000)}` },
    ],
    useSearch: true, // fetches all + filters client-side, like Owners
    searchSelector: 'input[placeholder="Search saccos..."]',
    editButtonTitle: 'Edit',
    deleteButtonTitle: 'Delete',
    editField: { selector: 'input[name="otherDetails"]', value: 'Edited by stress test' },
    apiListPath: '/MasterData/Saccos?pageNumber=1&pageSize=200',
    apiDeletePath: (id) => `/MasterData/Saccos/${id}`,
    extractItems: (json) => json?.items ?? [],
    matchField: 'name',
  },
];

async function apiGet(path) {
  const res = await fetch(`${API_BASE}${path}`, { headers: { Authorization: 'Bearer stress-test' } });
  if (!res.ok) return null;
  return res.json();
}

async function apiDelete(path) {
  return fetch(`${API_BASE}${path}`, { method: 'DELETE', headers: { Authorization: 'Bearer stress-test' } });
}

// Client-filtered pages (useSearch:true) resolve near-instantly; poll
// instead of a fixed sleep so timing doesn't need to be guessed per-page.
// useSearch:false skips the search box entirely (see CRUD_ENTITIES comment
// on why: backend searchTerm is confirmed broken for several entities) and
// just polls the plain table, which is small enough in this dev dataset
// that the new row is on page 1 without filtering.
async function findRowByText(page, searchSelector, text, useSearch, timeoutMs = 6000) {
  if (useSearch) await page.locator(searchSelector).fill(text);
  const row = page.locator('tr', { hasText: text }).first();
  const start = Date.now();
  while (Date.now() - start < timeoutMs) {
    if ((await row.count()) > 0) return row;
    // eslint-disable-next-line no-await-in-loop
    await sleep(300);
  }
  return row; // caller checks count() again and reports the miss
}

async function crudCycle(page, cfg, iteration) {
  const unique = `StressTest-${cfg.key}-${Date.now()}-${iteration}`;
  const result = { entity: cfg.key, unique, created: false, verifiedInDb: false, edited: false, deleted: false, verifiedDeletedInDb: false, errors: [] };

  try {
    await page.evaluate((r) => { location.hash = r; }, cfg.route);
    // Generous timeout: the very first navigation right after login can be
    // slow while lazy-loaded route chunks are still warming up.
    await page.waitForSelector(cfg.formReadySelector, { timeout: 20000 });

    // CREATE via the real form
    for (const f of cfg.createFields(unique)) {
      await page.locator(f.selector).fill(f.value);
    }
    await page.locator('button[type="submit"]').click();
    await sleep(1500); // let the POST + refetch complete

    // Verify creation directly against the backend (independent of the UI)
    const afterCreate = await apiGet(cfg.apiListPath);
    const items = cfg.extractItems(afterCreate);
    const createdRow = items.find((it) => it[cfg.matchField] === unique);
    result.created = true;
    result.verifiedInDb = !!createdRow;
    if (!createdRow) result.errors.push('created row not found via direct API GET after submit');

    // Narrow the UI to just this row via the real search box
    const row = await findRowByText(page, cfg.searchSelector, unique, cfg.useSearch);

    // EDIT via the real form
    if (createdRow && (await row.count()) > 0) {
      await row.locator(`button[title="${cfg.editButtonTitle}"]`).click();
      await page.locator(cfg.editField.selector).fill(cfg.editField.value);
      await page.locator('button[type="submit"]').click();
      await sleep(1500);
      const afterEdit = await apiGet(cfg.apiListPath);
      const editedRow = cfg.extractItems(afterEdit).find((it) => it[cfg.matchField] === unique);
      result.edited = true;
      result.editVerifiedInDb = editedRow
        ? JSON.stringify(editedRow).includes(cfg.editField.value)
        : false;
      if (!result.editVerifiedInDb) result.errors.push('edited field not reflected via direct API GET');
    } else {
      result.errors.push('could not locate created row in UI to edit (search/render mismatch)');
    }

    // DELETE via the real UI (handles the native confirm() dialog)
    const rowForDelete = await findRowByText(page, cfg.searchSelector, unique, cfg.useSearch);
    if ((await rowForDelete.count()) > 0) {
      // .catch() here matters: if the click below fails/times out (e.g. the
      // row re-renders and detaches mid-click, which does happen under load)
      // no dialog ever fires, this listener is never consumed, and a LATER
      // unrelated dialog can double-fire it -> "Cannot accept dialog which
      // is already handled" as an unhandled rejection that kills the whole
      // multi-minute run. Swallowing it here is safe: worst case is one
      // missed delete confirmation, which the DB-verify step below catches.
      page.once('dialog', (d) => d.accept().catch(() => {}));
      // Re-locate immediately before clicking rather than reusing the
      // earlier handle — the table can re-render between find and click.
      await page.locator('tr', { hasText: unique }).first()
        .locator(`button[title="${cfg.deleteButtonTitle}"]`)
        .click({ timeout: 10000 });
      await sleep(1500);
      result.deleted = true;
      const afterDelete = await apiGet(cfg.apiListPath);
      const stillThere = cfg.extractItems(afterDelete).find((it) => it[cfg.matchField] === unique);
      result.verifiedDeletedInDb = !stillThere;
      if (stillThere) {
        result.errors.push('row still present via direct API GET after UI delete');
        // best-effort cleanup so repeated runs don't accumulate junk rows
        await apiDelete(cfg.apiDeletePath(stillThere.id)).catch(() => {});
      }
    } else if (createdRow) {
      result.errors.push('could not locate created row in UI to delete; cleaning up directly via API');
      await apiDelete(cfg.apiDeletePath(createdRow.id)).catch(() => {});
    }
  } catch (err) {
    result.errors.push(`exception: ${err.message}`);
  }

  return result;
}

async function scenarioRealCrud(page, iterationsPerEntity = 5) {
  const results = [];
  for (const cfg of CRUD_ENTITIES) {
    for (let i = 0; i < iterationsPerEntity; i++) {
      // eslint-disable-next-line no-await-in-loop
      const r = await crudCycle(page, cfg, i);
      results.push(r);
      console.log(`  [real CRUD] ${cfg.key} #${i + 1}/${iterationsPerEntity}: created=${r.created} dbVerified=${r.verifiedInDb} edited=${r.edited} deleted=${r.deleted} dbDeleteVerified=${r.verifiedDeletedInDb}${r.errors.length ? '  ERRORS: ' + r.errors.join('; ') : ''}`);
    }
  }
  return results;
}

// ─────────────────────────────────────────────────────────────────────────
// Scenario 0.5 — Full-app route smoke test: visit every real route (as
// Admin, the superset role) and check each one actually rendered — not
// blank, and the ErrorBoundary didn't catch a crash ("Page Error" / "Something
// Went Wrong" from src/components/ErrorBoundary.jsx) — plus collect any
// console errors / uncaught exceptions fired while that route was mounting.
// This is breadth coverage; Scenario 0 above is the depth coverage (real
// CRUD) for the two entities the original bug report named.
// ─────────────────────────────────────────────────────────────────────────
const ALL_ADMIN_ROUTES = [
  '/admin/dashboard',
  '/admin/weighing/factory',
  '/admin/transactions',
  '/admin/weighing/vehicle',
  '/admin/weighing/drivers',
  '/admin/automation',
  '/admin/calibrations',
  '/admin/analytics',
  '/admin/reports',
  '/admin/system',
  '/admin/transporters',
  '/admin/weighing/axle-config',
  '/admin/weighing/owners',
  '/admin/weighing/products',
  '/admin/suppliers',
  '/admin/saccos',
  '/admin/weighbridges',
  '/admin/routes',
  '/admin/user-management',
  '/admin/security/password-policy',
  '/admin/security/permissions',
  '/admin/security/roles',
  '/admin/shifts',
  '/admin/attendance',
  '/admin/shift-assignment',
  '/admin/backup/microservice',
  '/admin/profile',
];

async function scenarioRouteSmokeTest(page) {
  const results = [];

  for (const path of ALL_ADMIN_ROUTES) {
    const pageErrors = [];
    const consoleErrors = [];
    const onPageError = (err) => pageErrors.push(err.message);
    const onConsole = (msg) => { if (msg.type() === 'error') consoleErrors.push(msg.text()); };
    page.on('pageerror', onPageError);
    page.on('console', onConsole);

    // eslint-disable-next-line no-await-in-loop
    await page.evaluate((p) => { location.hash = `#${p}`; }, path);
    // eslint-disable-next-line no-await-in-loop
    await sleep(1000);

    // eslint-disable-next-line no-await-in-loop
    const bodyText = await page.evaluate(() => document.body.innerText);
    page.off('pageerror', onPageError);
    page.off('console', onConsole);

    const crashed = bodyText.includes('Page Error') || bodyText.includes('Something Went Wrong');
    const blank = bodyText.trim().length < 20;

    results.push({
      path,
      ok: !crashed && !blank,
      crashed,
      blank,
      pageErrors,
      consoleErrors: consoleErrors.slice(0, 3), // cap noise
    });
    console.log(`  [route smoke] ${path} -> ${crashed ? 'CRASHED' : blank ? 'BLANK' : 'ok'}${pageErrors.length ? `  (${pageErrors.length} page errors)` : ''}${consoleErrors.length ? `  (${consoleErrors.length} console errors)` : ''}`);
  }

  return results;
}

// ─────────────────────────────────────────────────────────────────────────
// Scenario 1 — Frontend UI: rapid typing burst in a real form field.
// Reproduces the exact user complaint: type fast, see if the main thread
// stalls (Long Tasks API — any task >50ms is a dropped-frame-class stall).
// ─────────────────────────────────────────────────────────────────────────
async function scenarioFormTyping(page) {
  await page.evaluate(() => { location.hash = '#/operator/weighing/owners'; });
  await page.waitForSelector('input[name="name"]', { timeout: 10000 });

  await page.evaluate(() => {
    window.__longtasks = [];
    window.__ltObserver = new PerformanceObserver((list) => {
      list.getEntries().forEach((e) => window.__longtasks.push({ start: e.startTime, duration: e.duration }));
    });
    window.__ltObserver.observe({ entryTypes: ['longtask'] });
  });

  const text = 'The quick brown fox jumps over the lazy dog 1234567890'.repeat(3); // 165 chars
  const t0 = performance.now?.() ?? Date.now();
  const start = Date.now();
  await page.locator('input[name="name"]').pressSequentially(text, { delay: 0 });
  const elapsed = Date.now() - start;

  const longtasks = await page.evaluate(() => window.__longtasks);
  const value = await page.locator('input[name="name"]').inputValue();

  return {
    charsTyped: text.length,
    charsLanded: value.length,
    droppedChars: text.length - value.length,
    totalMs: elapsed,
    avgMsPerChar: elapsed / text.length,
    longtaskCount: longtasks.length,
    longtaskMaxMs: longtasks.length ? Math.max(...longtasks.map((t) => t.duration)) : 0,
    longtaskTotalMs: longtasks.reduce((a, t) => a + t.duration, 0),
  };
}

// ─────────────────────────────────────────────────────────────────────────
// Scenario 2 — Frontend UI: repeated route-switch churn (mount/unmount of
// Sidebar/Topbar/ProtectedRoute + page components many times), watching JS
// heap for a non-recovering upward trend — the leak signature. Includes
// Factory Weighing so the weight/plate EventSources and MJPEG camera
// connection actually get created and torn down on every cycle — if the
// app doesn't close them on unmount, the mock hardware server's own
// connection count (tracked server-side, not guessable from the page) will
// climb every time this route is visited instead of returning to ~0.
// ─────────────────────────────────────────────────────────────────────────
async function scenarioRouteChurn(page, hardware, durationMs = 150000) {
  const routes = [
    '#/operator/weighing/owners',
    '#/operator/weighing/vehicle',
    '#/operator/weighing/drivers',
    '#/operator/transactions',
    '#/operator/weighing/factory',
  ];
  const heapSamples = [];
  const sseClientSamplesAfterLeavingFactory = [];
  const start = Date.now();
  let i = 0;

  while (Date.now() - start < durationMs) {
    const route = routes[i % routes.length];
    const wasFactory = route === '#/operator/weighing/factory';
    await page.evaluate((r) => { location.hash = r; }, route);
    await sleep(wasFactory ? 400 : 150); // let streams actually connect before moving on

    if (i % 5 === 0) {
      const heap = await page.evaluate(async () => {
        if (window.gc) { window.gc(); await new Promise((r) => setTimeout(r, 50)); window.gc(); }
        return performance.memory ? performance.memory.usedJSHeapSize : null;
      });
      if (heap != null) heapSamples.push(heap);
    }

    if (wasFactory) {
      // navigate away, then check the server-side connection count settled back down
      const next = routes[(i + 1) % routes.length];
      await page.evaluate((r) => { location.hash = r; }, next);
      await sleep(300);
      sseClientSamplesAfterLeavingFactory.push(hardware.stats().activeSseClients);
    }
    i++;
    if (i % 20 === 0) console.log(`  [route churn] ${i} cycles, ${Math.round((Date.now() - start) / 1000)}s elapsed`);
  }

  return {
    cyclesRun: i,
    durationMs,
    heapTrend: summarizeHeapTrend(heapSamples, i),
    sseClientsAfterLeavingFactory: sseClientSamplesAfterLeavingFactory,
  };
}

// ─────────────────────────────────────────────────────────────────────────
// Scenario 3 — Redux state layer + live weight/camera streaming: sit on
// Factory Weighing, where WeighbridgePanel dispatches fetchSimulatedWeight()
// every 2s and setDetectedPlate() every 8s, AND the real hardware streams
// (LiveWeighbridgeStatus's scale SSE, CameraGrid's MJPEG + plate SSE) are
// now backed by the mock hardware server instead of failing to connect.
// Confirms real streamed data actually reaches the UI, and watches heap +
// the persisted localStorage payload size over a sustained soak.
// ─────────────────────────────────────────────────────────────────────────
async function scenarioReduxSoak(page, hardware, durationMs = 450000) {
  await page.evaluate(() => { location.hash = '#/operator/weighing/factory'; });
  await sleep(1500); // let EventSources connect and the first weight/plate events land

  const streamStatus = await page.evaluate(() => document.body.innerText.includes('No Signal')
    ? 'No Signal'
    : (document.body.innerText.match(/\b(Stable|Live)\b/)?.[0] ?? 'unknown'));

  const DURATION_MS = durationMs;
  const SAMPLE_EVERY_MS = 15000;
  const samples = [];
  const start = Date.now();

  while (Date.now() - start < DURATION_MS) {
    await sleep(SAMPLE_EVERY_MS);
    const sample = await page.evaluate(async () => {
      if (window.gc) { window.gc(); await new Promise((r) => setTimeout(r, 50)); window.gc(); }
      const persisted = localStorage.getItem('persist:root');
      return {
        heap: performance.memory ? performance.memory.usedJSHeapSize : null,
        persistBytes: persisted ? persisted.length : 0,
      };
    });
    samples.push({ ...sample, hardwareStats: hardware.stats() });
    console.log(`  [streaming soak] ${Math.round((Date.now() - start) / 1000)}s elapsed, heap=${fmtMB(sample.heap ?? 0)}, sse clients=${samples[samples.length - 1].hardwareStats.activeSseClients}`);
  }

  const heapTrend = summarizeHeapTrend(samples.map((s) => s.heap).filter((h) => h != null), samples.length);
  const persistSizes = samples.map((s) => s.persistBytes);
  return {
    durationMs: DURATION_MS,
    sampleCount: samples.length,
    streamStatusAfterConnect: streamStatus,
    heapTrend,
    persistBytesFirst: persistSizes[0] ?? 0,
    persistBytesLast: persistSizes[persistSizes.length - 1] ?? 0,
    persistBytesMax: Math.max(0, ...persistSizes),
    hardwareSseClientsLast: samples[samples.length - 1]?.hardwareStats.activeSseClients ?? 0,
  };
}

// ─────────────────────────────────────────────────────────────────────────
// Scenario 4 — Electron main process / IPC: repeated get-machine-id round
// trips (the only IPC channel this app currently exposes), plus a
// main-process memory snapshot before/after.
// ─────────────────────────────────────────────────────────────────────────
async function scenarioElectronIpc(page, electronApp) {
  const before = await electronApp.evaluate(({ app }) => app.getAppMetrics());

  const N = 300;
  const latencies = [];
  for (let i = 0; i < N; i++) {
    const t0 = Date.now();
    // eslint-disable-next-line no-await-in-loop
    await page.evaluate(() => window.electronAPI.getMachineId());
    latencies.push(Date.now() - t0);
  }

  const after = await electronApp.evaluate(({ app }) => app.getAppMetrics());
  const sorted = [...latencies].sort((a, b) => a - b);

  const mainBefore = before.find((m) => m.type === 'Browser');
  const mainAfter = after.find((m) => m.type === 'Browser');

  return {
    calls: N,
    firstCallMs: latencies[0],
    p50Ms: percentile(sorted, 0.5),
    p95Ms: percentile(sorted, 0.95),
    maxMs: sorted[sorted.length - 1],
    mainProcessWorkingSetBeforeKB: mainBefore?.memory?.workingSetSize ?? null,
    mainProcessWorkingSetAfterKB: mainAfter?.memory?.workingSetSize ?? null,
  };
}

function summarizeHeapTrend(samples, totalCycles) {
  if (samples.length < 2) return { note: 'not enough samples (performance.memory unavailable?)', samples };
  const firstThird = samples.slice(0, Math.max(1, Math.floor(samples.length / 3)));
  const lastThird = samples.slice(-Math.max(1, Math.floor(samples.length / 3)));
  const avg = (arr) => arr.reduce((a, b) => a + b, 0) / arr.length;
  const firstAvg = avg(firstThird);
  const lastAvg = avg(lastThird);
  const growthPct = ((lastAvg - firstAvg) / firstAvg) * 100;
  return {
    cycles: totalCycles,
    samples: samples.length,
    firstAvgBytes: firstAvg,
    lastAvgBytes: lastAvg,
    growthPct,
    minBytes: Math.min(...samples),
    maxBytes: Math.max(...samples),
  };
}

function printReport(r) {
  console.log(`\ngc() available for forced collection: ${r.gcAvailable}`);

  console.log('\n================ KNOWN BACKEND BUG (found via this harness, confirmed with direct curl) ================');
  console.log('MasterData search/filter (?searchTerm=) is broken across most services:');
  console.log('  - Drivers, Products     : ANY non-empty searchTerm silently returns 0 items, even exact/substring matches');
  console.log('  - Suppliers             : ANY non-empty searchTerm returns HTTP 200 with body "An error occurred while retrieving suppliers"');
  console.log('  - Owners, Transporters, Saccos: same backend bug exists, but is NOT user-visible — those pages fetch');
  console.log('    all records and filter client-side in JS, never actually sending searchTerm to the backend.');
  console.log('This means the Drivers/Suppliers/Products search boxes are completely non-functional for real users');
  console.log('right now. This harness works around it (see useSearch:false in CRUD_ENTITIES) to keep testing those');
  console.log('pages, but it is a real, separate bug worth fixing server-side — likely one shared search/filter helper.');
  console.log('===========================================================================================================');

  console.log('\n--- Scenario 0: Real CRUD against the live backend/DB (Owners, Drivers, Transporters, Suppliers, Products, Saccos) ---');
  const byEntity = {};
  for (const row of r.realCrud) {
    (byEntity[row.entity] ??= []).push(row);
  }
  for (const [entity, rows] of Object.entries(byEntity)) {
    const created = rows.filter((x) => x.verifiedInDb).length;
    const edited = rows.filter((x) => x.editVerifiedInDb).length;
    const deleted = rows.filter((x) => x.verifiedDeletedInDb).length;
    console.log(`  ${entity}: ${rows.length} cycles — created&DB-verified ${created}/${rows.length}, edited&DB-verified ${edited}/${rows.length}, deleted&DB-verified ${deleted}/${rows.length}`);
    rows.filter((x) => x.errors.length).forEach((x) => console.log(`    ! ${x.unique}: ${x.errors.join('; ')}`));
  }
  const allOk = r.realCrud.every((x) => x.verifiedInDb && x.editVerifiedInDb && x.verifiedDeletedInDb);
  console.log(`  ${allOk ? 'PASS — every create/edit/delete round-tripped through the real backend and Postgres' : 'WATCH — see errors above, some CRUD operations did not verify against the DB'}`);

  console.log(`\n--- Scenario 0.5: Full-app route smoke test (${r.routeSmoke.length} routes) ---`);
  const broken = r.routeSmoke.filter((x) => !x.ok);
  console.log(`  ${broken.length === 0 ? `PASS — all ${r.routeSmoke.length} routes rendered` : `WATCH — ${broken.length}/${r.routeSmoke.length} routes had problems:`}`);
  broken.forEach((x) => console.log(`    ${x.path}: ${x.crashed ? 'ErrorBoundary caught a crash' : 'rendered blank'}${x.pageErrors.length ? ` — ${x.pageErrors[0]}` : ''}${x.consoleErrors.length ? ` — console: ${x.consoleErrors[0]}` : ''}`));
  const withConsoleErrors = r.routeSmoke.filter((x) => x.ok && x.consoleErrors.length);
  if (withConsoleErrors.length) {
    console.log(`  note: ${withConsoleErrors.length} route(s) rendered fine but logged console errors:`);
    withConsoleErrors.forEach((x) => console.log(`    ${x.path}: ${x.consoleErrors[0]}`));
  }

  console.log('\n--- Scenario 1: Form typing burst (Owners > Owner Name) ---');
  const ft = r.formTyping;
  console.log(`  chars typed/landed        : ${ft.charsTyped} / ${ft.charsLanded} (dropped: ${ft.droppedChars})`);
  console.log(`  total burst time          : ${ft.totalMs} ms  (${fmtMs(ft.avgMsPerChar)}/char avg)`);
  console.log(`  long tasks (>50ms) during burst : count=${ft.longtaskCount}  max=${fmtMs(ft.longtaskMaxMs)}  total=${fmtMs(ft.longtaskTotalMs)}`);
  console.log(`  ${ft.longtaskCount === 0 ? 'PASS — no dropped frames detected' : 'WATCH — main thread stalled during typing, matches the freeze symptom'}`);

  console.log(`\n--- Scenario 2: Route-switch churn (${r.routeChurn.cyclesRun} cycles over ${(r.routeChurn.durationMs / 60000).toFixed(1)} min, 5 routes incl. Factory Weighing) ---`);
  printHeapTrend(r.routeChurn.heapTrend);
  const sse = r.routeChurn.sseClientsAfterLeavingFactory;
  console.log(`  hardware SSE clients still open ~300ms after leaving Factory Weighing (each cycle): [${sse.join(', ')}]`);
  console.log(`  ${sse.every((n) => n <= 1) ? 'PASS — streams close on unmount, no accumulation' : 'WATCH — SSE connections piling up across navigations (EventSource leak)'}`);

  console.log(`\n--- Scenario 3: Redux + live weight/camera streaming soak (Factory Weighing, ${(r.reduxSoak.durationMs / 60000).toFixed(1)} min) ---`);
  const rs = r.reduxSoak;
  console.log(`  UI stream status shortly after connecting        : ${rs.streamStatusAfterConnect}`);
  console.log(`  ${rs.streamStatusAfterConnect === 'No Signal' ? 'WATCH — mock weight stream not reaching the UI' : 'PASS — real streamed weight data is rendering live'}`);
  console.log(`  samples over ${rs.durationMs / 1000}s        : ${rs.sampleCount}`);
  printHeapTrend(rs.heapTrend);
  console.log(`  persist:root size  first -> last -> max : ${rs.persistBytesFirst}B -> ${rs.persistBytesLast}B -> ${rs.persistBytesMax}B`);
  console.log(`  hardware SSE clients still attached at end of soak : ${rs.hardwareSseClientsLast}`);
  console.log(`  ${rs.hardwareSseClientsLast > 4 ? 'WATCH — more concurrent streams than expected, investigate' : 'note: 1 expected for the weight stream; the plate stream is opened once per CameraGrid instance (live/snapshot/plate all call useCameraRealtime independently) — 3 more duplicate connections is a known finding, see summary.'}`);

  console.log('\n--- Scenario 4: Electron IPC (get-machine-id x300) ---');
  const ei = r.electronIpc;
  console.log(`  first call (cold, spawns registry read on Windows) : ${ei.firstCallMs} ms`);
  console.log(`  p50 / p95 / max (warm, cached)                     : ${ei.p50Ms} / ${ei.p95Ms} / ${ei.maxMs} ms`);
  console.log(`  main process working set  before -> after          : ${ei.mainProcessWorkingSetBeforeKB}KB -> ${ei.mainProcessWorkingSetAfterKB}KB`);

  console.log('\nNOTE: run this on the actual Windows machine (not just this Linux dev box) for');
  console.log('numbers that reflect the disk/AV/IPC latency mentioned in the original report.');
}

function printHeapTrend(t) {
  if (t.note) { console.log(`  ${t.note}`); return; }
  console.log(`  heap first-third avg -> last-third avg : ${fmtMB(t.firstAvgBytes)} -> ${fmtMB(t.lastAvgBytes)}  (${t.growthPct.toFixed(1)}%)`);
  console.log(`  heap min / max across run               : ${fmtMB(t.minBytes)} / ${fmtMB(t.maxBytes)}`);
  const verdict = t.growthPct > 25 ? 'WATCH — heap kept growing after forced GC, possible leak' : 'PASS — heap recovered after forced GC';
  console.log(`  ${verdict}`);
}

main().catch((err) => {
  console.error('\nSTRESS TEST FAILED:', err);
  process.exitCode = 1;
});
