const { chromium } = require('playwright');
const BASE = 'http://localhost:5299';

async function login(ctx, email, password) {
  const page = await ctx.newPage();
  await page.goto(BASE + '/Account/Login', { waitUntil: 'domcontentloaded' });
  await page.fill('input[name="Email"]', email);
  await page.fill('input[name="MatKhau"]', password);
  await page.click('button[type="submit"]');
  await page.waitForLoadState('domcontentloaded');
  await page.close();
}

async function diagnose(ctx, label, url) {
  const page = await ctx.newPage();
  const consoleMsgs = [];
  page.on('console', m => consoleMsgs.push(`[${m.type()}] ${m.text()}`));
  page.on('pageerror', e => consoleMsgs.push(`[pageerror] ${e.message}`));
  page.on('requestfailed', r => consoleMsgs.push(`[net-fail] ${r.url()} → ${r.failure()?.errorText}`));

  await page.goto(BASE + url, { waitUntil: 'networkidle', timeout: 20000 });

  console.log(`\n${'='.repeat(60)}`);
  console.log(`PAGE: ${label} (${url})`);
  console.log(`Final URL: ${page.url()}`);

  // Check for table
  const tables = await page.$$('table');
  console.log(`Tables found: ${tables.length}`);
  for (let i = 0; i < tables.length; i++) {
    const rows = await tables[i].$$('tbody tr');
    const headers = await tables[i].$$('thead th');
    const html = await tables[i].innerHTML();
    console.log(`  Table[${i}]: ${headers.length} cols, ${rows.length} body rows`);
    console.log(`  Table HTML preview: ${html.substring(0, 300).replace(/\s+/g, ' ')}`);
  }

  // Check for cards
  const cards = await page.$$('.card');
  console.log(`Cards: ${cards.length}`);

  // Check for forms
  const forms = await page.$$('form');
  console.log(`Forms: ${forms.length}`);

  // Check for select elements
  const selects = await page.$$('select');
  console.log(`Selects: ${selects.length}`);

  // Sidebar color
  const sidebarEl = await page.$('.sidebar, [class*="sidebar"], nav.bg-dark, nav[style*="background"]');
  if (sidebarEl) {
    const cls = await sidebarEl.getAttribute('class');
    const style = await sidebarEl.getAttribute('style');
    const bg = await sidebarEl.evaluate(el => window.getComputedStyle(el).backgroundColor);
    console.log(`Sidebar class: ${cls}`);
    console.log(`Sidebar bg: ${bg}`);
    console.log(`Sidebar style: ${style}`);
  } else {
    console.log(`No sidebar found with standard selectors`);
    // Try any nav
    const nav = await page.$('nav');
    if (nav) {
      const cls = await nav.getAttribute('class');
      const bg = await nav.evaluate(el => window.getComputedStyle(el).backgroundColor);
      console.log(`  nav class: ${cls}, bg: ${bg}`);
    }
  }

  // Page title/heading
  const h1 = await page.$('h1, h2, h3');
  if (h1) {
    const headingText = await h1.textContent();
    console.log(`Main heading: "${headingText?.trim()}"`);
  }

  // Body text summary (first 500 chars of visible text)
  const bodyText = await page.evaluate(() => {
    return Array.from(document.querySelectorAll('h1,h2,h3,h4,p,th,td,label,.card-title'))
      .map(e => e.textContent?.trim())
      .filter(t => t && t.length > 1)
      .slice(0, 20)
      .join(' | ');
  });
  console.log(`Content preview: ${bodyText.substring(0, 400)}`);

  // Console messages
  if (consoleMsgs.length > 0) {
    console.log(`Console (${consoleMsgs.length}):`);
    consoleMsgs.slice(0, 10).forEach(m => console.log(`  ${m}`));
  }

  await page.close();
}

(async () => {
  const browser = await chromium.launch({ headless: true });

  // === ADMIN: KhoaHoc list ===
  const adminCtx = await browser.newContext();
  await login(adminCtx, 'admin@nnl.com', 'Admin@123');
  await diagnose(adminCtx, 'Admin > Khóa học List', '/KhoaHoc');
  await adminCtx.close();

  // === GV: DiemSo ===
  const gvCtx = await browser.newContext();
  await login(gvCtx, 'gv01@nnl.com', 'Gv@123');
  await diagnose(gvCtx, 'GV > DiemSo', '/DiemSo');
  await diagnose(gvCtx, 'GV > DiemSo/LopHoc', '/DiemSo/LopHoc');
  await gvCtx.close();

  // === HV: KhoaHoc ===
  const hvCtx = await browser.newContext();
  await login(hvCtx, 'hv01@nnl.com', 'Hv@123');
  await diagnose(hvCtx, 'HV > Khóa học', '/KhoaHoc');
  await hvCtx.close();

  await browser.close();
})();
