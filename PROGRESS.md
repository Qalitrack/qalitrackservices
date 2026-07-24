# Revamp progress notes

Working notes for the ongoing QaliTrack revamp, so a new session (or a
new agent) can pick up where the last one left off without re-deriving
everything from the diff. Update this as you go; delete sections once
they're fully resolved and verified.

Last updated: 2026-07-24. Branch: `main`.

## What shipped this round

- **Analytics dashboard fixes** (`Analytics.jsx`): default time range
  changed from "Today" (empty on a fresh day) to "All Time"; added a
  "Last 60 Days" option; KPI cards restyled from a per-metric-colored
  icon-badge layout to the white/amber-border card style used by
  `ReportAnalytics`'s `MetricCard` and the plain Reports summary cards,
  per explicit request to make all three visually consistent.
- **Reports stat cards unified** across every tab to the same
  white-card/amber-border style: `Reports.jsx` summary cards,
  `CommodityReport`/`CustomerReport`/`SupplierReport`'s shared
  `CompactStat`, `DriverReport`'s two separate KPI blocks (top-level
  summary + drill-down detail — these were previously styled
  differently from each other, now both match), and
  `ReweighedTransactionsReport`'s 4 KPI cards (kept green/red as
  meaningful accents for Approved/Rejected, just on white backgrounds
  now instead of tinted fills). `CrossEntityComparison`'s `StatCard`
  also converted from tinted backgrounds to white + colored border.
- **Off-theme badge cleanup**: `Saccos.jsx` Name column was a dark teal
  gradient "plate" badge (teal isn't in the app's palette at all,
  and that visual style is reserved elsewhere for vehicle plates) —
  now plain bold text like every other table. `Owners.jsx` Type badge
  used 3 different colors per value (blue/purple/green) unlike every
  other categorical tag in the app which is single-color regardless of
  value — now always blue.
- **Analytics header Refresh button** (`Analytics.jsx`): was invisible
  chrome (icon+text floating with no border/shadow) because the
  `cs-ghost-btn` class only sets `border-color`, never `border-width`,
  and the className was missing both a `border` utility and a
  `shadow-sm` fallback that every other ghost-button instance in the
  app has. Added both.
- **Transaction search bug fixed** (`Transaction.jsx`): the free-text
  search box sent `filters.search` to the backend as a `search` query
  param, but `TransactionsController.cs` only matches that param
  against `TransactionNumber` — so searching by driver/vehicle/
  commodity/etc. silently returned nothing. Stopped sending `search` to
  the backend and added a client-side multi-field filter (receipt no,
  plate, driver, commodity, supplier, transporter, customer, origin,
  destination, weighbridge), mirroring the pattern already used in
  `Reports.jsx`'s own search. Also removed a debounce branch that no
  longer does anything now that `search` isn't part of the server
  request's dependency array.
- **Redux selector warning fixed** (`IncompleteTransactionsTable.jsx`):
  `useSelector` was building a new `{transactions, loading,
  serverTotal}` object literal every render, which React-Redux flags
  as "returned a different result when called with the same
  parameters" since it can't tell nothing changed. Split into three
  separate selectors, each returning a stable reference straight from
  the store.
- **Receipt number prefix made configurable**
  (`ReceiptNumberService.cs`): was `private const string Prefix =
  "NCCU"` hardcoded. Now reads `IConfiguration["Transaction:
  ReceiptPrefix"]` (falls back to `"NCCU"`), wired through
  `Transaction__ReceiptPrefix` in `docker-compose.yml` and
  `RECEIPT_PREFIX` in `Deployment/.env` (gitignored, not in this repo)
  + documented in `RUNBOOK.md`. Rebuilt and restarted
  `transaction-service-prod`; confirmed the env var lands inside the
  running container.

## Known gaps / not yet verified in this session

- None of the frontend changes above were clicked through in a real
  browser session by the assistant — verified by reading code, HMR
  applying cleanly with no console errors, and (for the search fix)
  reasoning through the actual data flow. Worth a manual pass: Analytics
  time-range switching, Refresh button visibility, Reports tabs
  (Drivers/Commodities/Customers/Suppliers/Reweighed/Comparison) stat
  card rendering, Transaction search across each field type, Saccos/
  Owners table rendering.
- `ReweighedTransactionsReport.jsx` also has a dark "receipt card" style
  detail view (bg-gray-900 ticket mockup) with its own inner KPI grid
  (`bg-gray-50`/`bg-green-50`/etc. tinted backgrounds) that was
  deliberately left untouched — it reads as an intentional
  physical-receipt visual, not a stray inconsistency, but flag it if a
  future pass wants full consistency with the white/amber-border cards
  everywhere else.
- `.NET SDK` isn't installed on this machine outside of the Docker
  build itself — `ReceiptNumberService.cs` was build-verified only via
  the actual `docker compose build` (which succeeded), not a local
  `dotnet build`.
