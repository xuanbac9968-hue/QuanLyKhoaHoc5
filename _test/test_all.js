const { chromium } = require('playwright');

const BASE = 'http://localhost:5299';
const results = [];

async function testPage(page, label, url, checks = []) {
  const errors = [];
  const consoleErrors = [];

  page.on('console', msg => {
    if (msg.type() === 'error') consoleErrors.push(msg.text());
  });

  try {
    const resp = await page.goto(BASE + url, { waitUntil: 'domcontentloaded', timeout: 15000 });
    const statusCode = resp ? resp.status() : 0;
    const finalUrl = page.url();

    // Check if redirected to login (not authenticated)
    if (finalUrl.includes('/Account/Login') && !url.includes('/Account/Login')) {
      results.push({ label, url, status: '⚠️ REDIRECT→LOGIN', statusCode, errors: [], consoleErrors: [] });
      return;
    }

    // Check HTTP error
    if (statusCode >= 400) {
      errors.push(`HTTP ${statusCode}`);
    }

    // Check for exception page
    const bodyText = await page.textContent('body').catch(() => '');
    if (bodyText.includes('An unhandled exception') || bodyText.includes('System.') || bodyText.includes('NullReferenceException')) {
      errors.push('EXCEPTION on page: ' + bodyText.substring(0, 200));
    }

    // Custom checks
    for (const check of checks) {
      const found = await page.$(check.selector).catch(() => null);
      if (check.shouldExist && !found) errors.push(`Missing: ${check.label}`);
      if (!check.shouldExist && found) errors.push(`Unexpected: ${check.label}`);
    }

    const status = errors.length === 0 && consoleErrors.length === 0 ? '✅ OK'
      : errors.length === 0 ? '⚠️ CONSOLE ERRORS'
      : '❌ ERROR';

    results.push({ label, url, status, statusCode, errors, consoleErrors });
  } catch (e) {
    results.push({ label, url, status: '❌ TIMEOUT/CRASH', statusCode: 0, errors: [e.message], consoleErrors });
  }
}

async function login(page, email, password) {
  await page.goto(BASE + '/Account/Login', { waitUntil: 'domcontentloaded' });
  await page.fill('input[name="Email"]', email);
  await page.fill('input[name="MatKhau"]', password);
  await page.click('button[type="submit"]');
  await page.waitForLoadState('domcontentloaded');
  return page.url();
}

async function getSidebarColor(page) {
  try {
    const sidebar = await page.$('.sidebar, nav.sidebar, aside, [class*="sidebar"]');
    if (!sidebar) return 'no-sidebar';
    const bg = await sidebar.evaluate(el => window.getComputedStyle(el).backgroundColor);
    return bg;
  } catch { return 'error'; }
}

(async () => {
  const browser = await chromium.launch({ headless: true });

  // ============================================================
  // ADMIN TESTS
  // ============================================================
  console.log('\n=== TESTING ADMIN ===');
  const adminCtx = await browser.newContext();
  const adminPage = await adminCtx.newPage();

  const adminUrl = await login(adminPage, 'admin@nnl.com', 'Admin@123');
  console.log('Admin login redirected to:', adminUrl);

  const adminRoutes = [
    { label: 'Admin Dashboard', url: '/Admin/Index' },
    { label: 'Admin Dashboard (root)', url: '/Admin' },
    { label: 'Khóa học', url: '/KhoaHoc' },
    { label: 'Khóa học Create', url: '/KhoaHoc/Create' },
    { label: 'Lớp học', url: '/LopHoc' },
    { label: 'Học viên (Admin)', url: '/HocVien' },
    { label: 'Giảng viên', url: '/GiangVien' },
    { label: 'Đăng ký', url: '/DangKy' },
    { label: 'Lịch học (Admin)', url: '/LichHoc' },
    { label: 'Quản lý điểm', url: '/DiemSo' },
    { label: 'Phân công GV', url: '/PhanCongGV' },
    { label: 'Thanh toán (Admin)', url: '/ThanhToan' },
    { label: 'Báo cáo', url: '/BaoCao' },
    { label: 'Quản lý tài khoản', url: '/TaiKhoan' },
    { label: 'Hồ sơ Admin', url: '/Profile' },
    { label: 'Thông báo (Admin)', url: '/ThongBao' },
    { label: 'Đổi mật khẩu (Admin)', url: '/Account/ChangePassword' },
  ];

  for (const r of adminRoutes) {
    const p = await adminCtx.newPage();
    await testPage(p, `[ADMIN] ${r.label}`, r.url);
    await p.close();
  }

  await adminCtx.close();

  // ============================================================
  // GIẢNG VIÊN TESTS
  // ============================================================
  console.log('\n=== TESTING GIẢNG VIÊN ===');
  const gvCtx = await browser.newContext();
  const gvPage = await gvCtx.newPage();

  const gvUrl = await login(gvPage, 'gv01@nnl.com', 'Gv@123');
  console.log('GV login redirected to:', gvUrl);

  const gvRoutes = [
    { label: 'GV Dashboard', url: '/GiangVien/Dashboard' },
    { label: 'Lớp & Học viên (GV)', url: '/LopHoc' },
    { label: 'Nhập điểm (GV)', url: '/DiemSo' },
    { label: 'Lịch dạy (GV)', url: '/GiangVien/LichDay' },
    { label: 'Hồ sơ (GV)', url: '/Profile' },
    { label: 'Thông báo (GV)', url: '/ThongBao' },
    { label: 'Đổi mật khẩu (GV)', url: '/Account/ChangePassword' },
  ];

  for (const r of gvRoutes) {
    const p = await gvCtx.newPage();
    await testPage(p, `[GV] ${r.label}`, r.url);
    await p.close();
  }

  await gvCtx.close();

  // ============================================================
  // HỌC VIÊN TESTS
  // ============================================================
  console.log('\n=== TESTING HỌC VIÊN ===');
  const hvCtx = await browser.newContext();
  const hvPage = await hvCtx.newPage();

  const hvUrl = await login(hvPage, 'hv01@nnl.com', 'Hv@123');
  console.log('HV login redirected to:', hvUrl);

  const hvRoutes = [
    { label: 'HV Dashboard', url: '/HocVien/Dashboard' },
    { label: 'Khóa học (HV)', url: '/KhoaHoc' },
    { label: 'Đăng ký của tôi (HV)', url: '/DangKy/CuaToi' },
    { label: 'Lịch học của tôi (HV)', url: '/LichHoc/CuaToi' },
    { label: 'Điểm của tôi (HV)', url: '/DiemSo/CuaToi' },
    { label: 'Gợi ý AI (HV)', url: '/GoiY' },
    { label: 'Thanh toán (HV)', url: '/ThanhToan/CuaToi' },
    { label: 'Hồ sơ (HV)', url: '/Profile' },
    { label: 'Thông báo (HV)', url: '/ThongBao' },
    { label: 'Đổi mật khẩu (HV)', url: '/Account/ChangePassword' },
  ];

  for (const r of hvRoutes) {
    const p = await hvCtx.newPage();
    await testPage(p, `[HV] ${r.label}`, r.url);
    await p.close();
  }

  await hvCtx.close();

  await browser.close();

  // ============================================================
  // PRINT RESULTS
  // ============================================================
  console.log('\n\n========== KẾT QUẢ TEST ==========\n');

  const byStatus = { ok: [], warn: [], error: [], redirect: [] };
  for (const r of results) {
    if (r.status.includes('✅')) byStatus.ok.push(r);
    else if (r.status.includes('⚠️ CONSOLE')) byStatus.warn.push(r);
    else if (r.status.includes('⚠️ REDIRECT')) byStatus.redirect.push(r);
    else byStatus.error.push(r);
  }

  console.log(`✅ OK: ${byStatus.ok.length} trang`);
  console.log(`⚠️  Console errors: ${byStatus.warn.length} trang`);
  console.log(`⚠️  Redirect login: ${byStatus.redirect.length} trang`);
  console.log(`❌ Lỗi: ${byStatus.error.length} trang`);

  console.log('\n--- CHI TIẾT TỪNG TRANG ---\n');
  for (const r of results) {
    const icon = r.status.split(' ')[0];
    console.log(`${icon} [${r.statusCode || '-'}] ${r.label}  →  ${r.url}`);
    if (r.errors.length) {
      for (const e of r.errors) console.log(`     🔴 ${e.substring(0, 300)}`);
    }
    if (r.consoleErrors.length) {
      for (const e of r.consoleErrors.slice(0,3)) console.log(`     🟡 console: ${e.substring(0, 200)}`);
    }
  }

  if (byStatus.error.length || byStatus.redirect.length) {
    console.log('\n========== DANH SÁCH CẦN SỬA ==========\n');
    const needFix = [...byStatus.error, ...byStatus.redirect];
    needFix.forEach((r, i) => {
      console.log(`${i+1}. ${r.label} (${r.url})`);
      console.log(`   Status: ${r.status}`);
      if (r.errors.length) console.log(`   Lỗi: ${r.errors[0].substring(0, 200)}`);
    });
  }
})();
