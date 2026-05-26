import puppeteer from 'puppeteer';
import { readFile, writeFile } from 'fs/promises';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';
import { existsSync } from 'fs';

const __dirname = dirname(fileURLToPath(import.meta.url));
const siteDir = join(__dirname, '_site');
const outputPdf = join(siteDir, 'UserService-Docs.pdf');

// Recursively collect hrefs from a TOC in order
function collectTocHrefs(items, prefix = '') {
  const hrefs = [];
  for (const item of items || []) {
    if (item.href && !item.href.startsWith('http') && !item.href.endsWith('toc.html')) {
      hrefs.push(prefix + item.href);
    }
    if (item.items) hrefs.push(...collectTocHrefs(item.items, prefix));
  }
  return hrefs;
}

async function readToc(tocPath) {
  const raw = await readFile(tocPath, 'utf8');
  return JSON.parse(raw).items || [];
}

async function buildPageList() {
  const pages = [];
  const seen = new Set();

  const add = (rel) => {
    const full = join(siteDir, rel.replace(/\//g, '/'));
    if (!seen.has(full) && existsSync(full)) {
      seen.add(full);
      pages.push(full);
    }
  };

  // 1. Cover / home
  add('index.html');
  add('README.html');

  // 2. Docs section — follow docs/toc.json order
  const docsToc = await readToc(join(siteDir, 'docs', 'toc.json'));
  for (const href of collectTocHrefs(docsToc)) {
    add(`docs/${href}`);
  }

  // 3. API section — follow api/toc.json order, then expand members alphabetically
  const apiToc = await readToc(join(siteDir, 'api', 'toc.json'));
  const topLevelHrefs = collectTocHrefs(apiToc);

  // Collect all api html files so we can include members not listed in top-level TOC
  const { readdirSync } = await import('fs');
  const allApiFiles = readdirSync(join(siteDir, 'api'))
    .filter(f => f.endsWith('.html') && f !== 'toc.html')
    .sort();

  // Add top-level namespace pages first, then their members (files that start with the same prefix)
  for (const href of topLevelHrefs) {
    add(`api/${href}`);
    const prefix = href.replace('.html', '.');
    for (const f of allApiFiles) {
      if (f.startsWith(prefix)) add(`api/${f}`);
    }
  }

  // Catch any remaining API files not covered above
  for (const f of allApiFiles) add(`api/${f}`);

  return pages;
}

async function main() {
  console.log('Building page list from TOC...');
  const pages = await buildPageList();
  console.log(`Rendering ${pages.length} pages in structured order`);

  const isWindows = process.platform === 'win32';
  const launchOptions = {
    headless: true,
    args: ['--no-sandbox', '--disable-setuid-sandbox']
  };
  if (isWindows) {
    launchOptions.executablePath = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
  }

  const browser = await puppeteer.launch(launchOptions);
  const page = await browser.newPage();
  const buffers = [];

  for (let i = 0; i < pages.length; i++) {
    const file = pages[i];
    const label = file.replace(siteDir, '').replace(/\\/g, '/');
    const url = `file:///${file.replace(/\\/g, '/')}`;
    console.log(`[${i + 1}/${pages.length}] ${label}`);
    try {
      await page.goto(url, { waitUntil: 'networkidle0', timeout: 15000 });
      const buf = await page.pdf({
        format: 'A4',
        printBackground: true,
        margin: { top: '20mm', bottom: '20mm', left: '15mm', right: '15mm' }
      });
      buffers.push(buf);
    } catch (e) {
      console.warn(`  skipped: ${e.message}`);
    }
  }

  await browser.close();

  console.log('\nMerging pages...');
  const { PDFDocument } = await import('pdf-lib');
  const merged = await PDFDocument.create();
  for (const buf of buffers) {
    try {
      const doc = await PDFDocument.load(buf);
      const copied = await merged.copyPages(doc, doc.getPageIndices());
      copied.forEach(p => merged.addPage(p));
    } catch {}
  }

  const mergedBytes = await merged.save();
  await writeFile(outputPdf, mergedBytes);
  console.log(`PDF saved: ${outputPdf} (${(mergedBytes.length / 1024 / 1024).toFixed(1)} MB)`);
}

main().catch(e => { console.error(e); process.exit(1); });
