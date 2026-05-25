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

async function checkSidebarAndContent(ctx, role, label, url) {
  const page = await ctx.newPage();
  const consoleMsgs = [];
  page.on('console', m => { if(m.type()==='error') consoleMsgs.push(m.text()); });
  page.on('pageerror', e => consoleMsgs.push('JS: '+e.message));

  await page.goto(BASE + url, { waitUntil: 'networkidle', timeout: 20000 });
  const finalUrl = page.url();

  // Sidebar check
  const sidebarBg = await page.evaluate(() => {
    const s = document.querySelector('.sidebar');
    return s ? window.getComputedStyle(s).backgroundImage || window.getComputedStyle(s).background : 'no-sidebar';
  });

  // Check role indicator (badge in topbar)
  const roleBadge = await page.evaluate(() => {
    const b = document.querySelector('.badge.bg-primary, .badge.bg-success, .badge.bg-danger, .badge.bg-info');
    return b ? b.textContent.trim() : 'no-badge';
  });

  // Check page has meaningful content (not error/empty)
  const bodyText = await page.evaluate(() => document.body.innerText);
  const hasException = bodyText.includes('An unhandled exception') || bodyText.includes('NullReferenceException');
  const isAccessDenied = finalUrl.includes('AccessDenied') || bodyText.includes('Không có quyền');

  // Count sidebar nav links
  const navLinks = await page.$$('.sidebar .nav-link');

  // Get page heading
  const heading = await page.$eval('h1,h2,h3', el => el.textContent?.trim()).catch(() => '(no heading)');

  // Get key content indicators
  const cards = (await page.$$('.card')).length;
  const tables = (await page.$$('table')).length;
  const forms = (await page.$$('form')).length;

  const issues = [];
  if (isAccessDenied) issues.push('REDIRECT to AccessDenied!');
  if (hasException) issues.push('SERVER EXCEPTION!');
  if (consoleMsgs.length > 0) issues.push('Console errors: ' + consoleMsgs.slice(0,2).join('; '));

  const icon = issues.length === 0 ? '✅' : '❌';

  console.log(`${icon} [${role}] ${label}`);
  console.log(`   URL: ${url} → ${finalUrl}`);
  console.log(`   Sidebar: ${sidebarBg.substring(0, 80)}`);
  console.log(`   Role badge: "${roleBadge}"`);
  console.log(`   Heading: "${heading}" | Cards:${cards} Tables:${tables} Forms:${forms} NavLinks:${navLinks.length}`);
  if (issues.length) console.log(`   ❗ Issues: ${issues.join(' | ')}`);

  await page.close();
  return { ok: issues.length === 0, label, url };
}

(async () => {
  const browser = await chromium.launch({ headless: true });

  // ================================================================
  // ADMIN - exact sidebar links
  // ================================================================
  console.log('\n═══ ADMIN SIDEBAR URLS ═══');
  const adminCtx = await browser.newContext();
  await login(adminCtx, 'admin@nnl.com', 'Admin@123');

  // Let me read admin sidebar from the actual rendered page
  const adminNavPage = await adminCtx.newPage();
  await adminNavPage.goto(BASE + '/Admin', { waitUntil: 'domcontentloaded' });
  const adminLinks = await adminNavPage.evaluate(() => {
    return Array.from(document.querySelectorAll('.sidebar .nav-link[href]')).map(a => ({
      text: a.textContent.trim(),
      href: a.getAttribute('href')
    }));
  });
  console.log('\nActual Admin sidebar links:');
  adminLinks.forEach(l => console.log(`  • "${l.text}" → ${l.href}`));
  await adminNavPage.close();

  const adminTests = [
    { label: 'Dashboard', url: '/Admin' },
    { label: 'Khóa học', url: '/KhoaHoc' },
    { label: 'Lớp học', url: '/LopHoc' },
    { label: 'Học viên', url: '/HocVien' },
    { label: 'Giảng viên', url: '/GiangVien' },
    { label: 'Đăng ký', url: '/DangKy' },
    { label: 'Lịch học', url: '/LichHoc' },
    { label: 'Quản lý điểm', url: '/DiemSo' },
    { label: 'Phân công GV', url: '/PhanCongGV' },
    { label: 'Thanh toán', url: '/ThanhToan' },
    { label: 'Báo cáo', url: '/BaoCao' },
    { label: 'Quản lý tài khoản', url: '/TaiKhoan' },
    { label: 'Hồ sơ', url: '/Profile' },
    { label: 'Thông báo', url: '/ThongBao' },
    { label: 'Đổi mật khẩu', url: '/Account/ChangePassword' },
  ];
  for (const t of adminTests) await checkSidebarAndContent(adminCtx, 'ADMIN', t.label, t.url);
  await adminCtx.close();

  // ================================================================
  // GIẢNG VIÊN - exact sidebar links
  // ================================================================
  console.log('\n═══ GIẢNG VIÊN SIDEBAR URLS ═══');
  const gvCtx = await browser.newContext();
  await login(gvCtx, 'gv01@nnl.com', 'Gv@123');

  const gvNavPage = await gvCtx.newPage();
  await gvNavPage.goto(BASE + '/GiangVien/Dashboard', { waitUntil: 'domcontentloaded' });
  const gvLinks = await gvNavPage.evaluate(() => {
    return Array.from(document.querySelectorAll('.sidebar .nav-link[href]')).map(a => ({
      text: a.textContent.trim(),
      href: a.getAttribute('href')
    }));
  });
  console.log('\nActual GV sidebar links:');
  gvLinks.forEach(l => console.log(`  • "${l.text}" → ${l.href}`));
  await gvNavPage.close();

  const gvTests = gvLinks.filter(l => l.href && !l.href.includes('Logout'));
  for (const t of gvTests) await checkSidebarAndContent(gvCtx, 'GV', t.text, t.href);
  await gvCtx.close();

  // ================================================================
  // HỌC VIÊN - exact sidebar links
  // ================================================================
  console.log('\n═══ HỌC VIÊN SIDEBAR URLS ═══');
  const hvCtx = await browser.newContext();
  await login(hvCtx, 'hv01@nnl.com', 'Hv@123');

  const hvNavPage = await hvCtx.newPage();
  await hvNavPage.goto(BASE + '/HocVien/Dashboard', { waitUntil: 'domcontentloaded' });
  const hvLinks = await hvNavPage.evaluate(() => {
    return Array.from(document.querySelectorAll('.sidebar .nav-link[href]')).map(a => ({
      text: a.textContent.trim(),
      href: a.getAttribute('href')
    }));
  });
  console.log('\nActual HV sidebar links:');
  hvLinks.forEach(l => console.log(`  • "${l.text}" → ${l.href}`));
  await hvNavPage.close();

  const hvTests = hvLinks.filter(l => l.href && !l.href.includes('Logout'));
  for (const t of hvTests) await checkSidebarAndContent(hvCtx, 'HV', t.text, t.href);
  await hvCtx.close();

  await browser.close();

  console.log('\n══ Done ══');
})();
