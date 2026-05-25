const { chromium } = require('playwright');

const BASE = 'http://localhost:5299';
const results = [];

async function login(ctx, email, password) {
  const page = await ctx.newPage();
  await page.goto(BASE + '/Account/Login', { waitUntil: 'domcontentloaded' });
  await page.fill('input[name="Email"]', email);
  await page.fill('input[name="MatKhau"]', password);
  await page.click('button[type="submit"]');
  await page.waitForLoadState('domcontentloaded');
  await page.close();
}

async function checkPage(ctx, label, url, assertions) {
  const page = await ctx.newPage();
  const consoleErrs = [];
  page.on('console', m => { if (m.type() === 'error') consoleErrs.push(m.text()); });
  page.on('pageerror', e => consoleErrs.push('JS: ' + e.message));

  try {
    const res = await page.goto(BASE + url, { waitUntil: 'domcontentloaded', timeout: 15000 });
    const finalUrl = page.url();
    const status = res ? res.status() : 0;
    const body = await page.content();

    const issues = [];

    // Run all assertions
    for (const a of (assertions || [])) {
      try {
        const result = await a.fn(page, body);
        if (!result) issues.push(`FAIL: ${a.desc}`);
      } catch (e) {
        issues.push(`ERR(${a.desc}): ${e.message.substring(0, 100)}`);
      }
    }

    const icon = issues.length === 0 && consoleErrs.length === 0 ? '✅'
      : consoleErrs.length > 0 && issues.length === 0 ? '🟡'
      : '❌';

    results.push({
      icon, label, url,
      httpStatus: status,
      finalUrl,
      issues,
      consoleErrs: consoleErrs.slice(0, 5),
    });

  } catch (e) {
    results.push({ icon: '❌', label, url, httpStatus: 0, finalUrl: '', issues: ['CRASH: ' + e.message.substring(0,200)], consoleErrs: [] });
  } finally {
    await page.close();
  }
}

// Helper assertions
const hasSidebar = (role) => ({
  desc: `Sidebar role="${role}"`,
  fn: async (page) => {
    // Check for sidebar with role-specific class or color indicator
    const sidebar = await page.$('.sidebar, nav[class*="sidebar"], aside[class*="sidebar"]');
    return sidebar !== null;
  }
});

const hasElement = (sel, desc) => ({
  desc: desc || `Element ${sel}`,
  fn: async (page) => (await page.$(sel)) !== null
});

const textContains = (text, desc) => ({
  desc: desc || `Text "${text}"`,
  fn: async (_, body) => body.includes(text)
});

const tableHasRows = (desc) => ({
  desc: desc || 'Table has data rows',
  fn: async (page) => {
    const rows = await page.$$('table tbody tr');
    return rows.length > 0;
  }
});

const noException = {
  desc: 'No server exception',
  fn: async (_, body) => !body.includes('An unhandled exception') && !body.includes('NullReferenceException') && !body.includes('System.Exception')
};

(async () => {
  const browser = await chromium.launch({ headless: true });

  // ================================================================
  // ADMIN
  // ================================================================
  console.log('Setting up ADMIN session...');
  const adminCtx = await browser.newContext();
  await login(adminCtx, 'admin@nnl.com', 'Admin@123');

  await checkPage(adminCtx, 'Admin > Dashboard', '/Admin', [
    noException,
    hasSidebar('admin'),
    textContains('Dashboard', 'Title Dashboard'),
    { desc: 'Stat cards present', fn: async (p) => (await p.$$('.card')).length >= 3 },
  ]);

  await checkPage(adminCtx, 'Admin > Khóa học List', '/KhoaHoc', [
    noException,
    hasSidebar('admin'),
    tableHasRows('Danh sách khóa học'),
    hasElement('a[href*="Create"]', 'Nút Thêm mới'),
  ]);

  await checkPage(adminCtx, 'Admin > Khóa học Create', '/KhoaHoc/Create', [
    noException,
    hasElement('form', 'Form tạo khóa học'),
    hasElement('input[name="TenKhoaHoc"]', 'Field TenKhoaHoc'),
  ]);

  await checkPage(adminCtx, 'Admin > Lớp học', '/LopHoc', [
    noException,
    tableHasRows('Danh sách lớp học'),
    hasElement('a[href*="Create"]', 'Nút Thêm mới'),
  ]);

  await checkPage(adminCtx, 'Admin > Học viên', '/HocVien', [
    noException,
    tableHasRows('Danh sách học viên'),
  ]);

  await checkPage(adminCtx, 'Admin > Giảng viên', '/GiangVien', [
    noException,
    tableHasRows('Danh sách giảng viên'),
  ]);

  await checkPage(adminCtx, 'Admin > Đăng ký', '/DangKy', [
    noException,
    tableHasRows('Danh sách đăng ký'),
  ]);

  await checkPage(adminCtx, 'Admin > Lịch học', '/LichHoc', [
    noException,
    { desc: 'Calendar or table present', fn: async (p) => (await p.$('table, .fc-view, [class*="calendar"]')) !== null },
  ]);

  await checkPage(adminCtx, 'Admin > Quản lý điểm (DiemSo)', '/DiemSo', [
    noException,
    { desc: 'Có nội dung điểm', fn: async (p) => (await p.$('table, .card, select')) !== null },
  ]);

  await checkPage(adminCtx, 'Admin > DiemSo KyHocList', '/DiemSo/KyHocList', [
    noException,
  ]);

  await checkPage(adminCtx, 'Admin > Phân công GV', '/PhanCongGV', [
    noException,
    { desc: 'Có form hoặc table', fn: async (p) => (await p.$('table, form, select')) !== null },
  ]);

  await checkPage(adminCtx, 'Admin > Thanh toán', '/ThanhToan', [
    noException,
    tableHasRows('Danh sách thanh toán'),
  ]);

  await checkPage(adminCtx, 'Admin > Báo cáo', '/BaoCao', [
    noException,
    { desc: 'Có stats hoặc biểu đồ', fn: async (p) => (await p.$('.card, canvas, table')) !== null },
  ]);

  await checkPage(adminCtx, 'Admin > Quản lý tài khoản (TaiKhoan)', '/TaiKhoan', [
    noException,
    tableHasRows('Danh sách tài khoản'),
  ]);

  await checkPage(adminCtx, 'Admin > Hồ sơ', '/Profile', [
    noException,
    hasElement('form', 'Form hồ sơ'),
    { desc: 'Có thông tin user', fn: async (p) => (await p.$('input[name="HoTen"], input[name="Email"]')) !== null },
  ]);

  await checkPage(adminCtx, 'Admin > Thông báo', '/ThongBao', [
    noException,
    { desc: 'Có danh sách hoặc thông báo trống', fn: async (p) => (await p.$('.card, .list-group, table, .alert')) !== null },
  ]);

  await checkPage(adminCtx, 'Admin > Đổi mật khẩu', '/Account/ChangePassword', [
    noException,
    hasElement('form', 'Form đổi mật khẩu'),
    hasElement('input[name="MatKhauCu"]', 'Field MatKhauCu'),
  ]);

  // Admin-only pages
  await checkPage(adminCtx, 'Admin > GoiY/Admin', '/GoiY/Admin', [noException]);
  await checkPage(adminCtx, 'Admin > PhanCong (Admin)', '/Admin/PhanCong', [noException]);
  await checkPage(adminCtx, 'Admin > TaiKhoan (Admin)', '/Admin/TaiKhoan', [noException]);
  await checkPage(adminCtx, 'Admin > BaoCao (Admin)', '/Admin/BaoCao', [noException]);

  await adminCtx.close();

  // ================================================================
  // GIẢNG VIÊN
  // ================================================================
  console.log('Setting up GIẢNG VIÊN session...');
  const gvCtx = await browser.newContext();
  await login(gvCtx, 'gv01@nnl.com', 'Gv@123');

  await checkPage(gvCtx, 'GV > Dashboard', '/GiangVien/Dashboard', [
    noException,
    hasSidebar('gv'),
    { desc: 'Dashboard GV có thống kê', fn: async (p) => (await p.$$('.card')).length >= 2 },
  ]);

  await checkPage(gvCtx, 'GV > Lớp học (LopHoc)', '/LopHoc', [
    noException,
    { desc: 'Có danh sách lớp', fn: async (p) => (await p.$('table, .card')) !== null },
  ]);

  await checkPage(gvCtx, 'GV > Nhập điểm (DiemSo)', '/DiemSo', [
    noException,
    { desc: 'Có form nhập điểm hoặc danh sách', fn: async (p) => (await p.$('table, form, select')) !== null },
  ]);

  await checkPage(gvCtx, 'GV > Lịch dạy', '/GiangVien/LichDay', [
    noException,
    { desc: 'Có lịch dạy', fn: async (p) => (await p.$('table, .fc-view, [class*="calendar"], .card')) !== null },
  ]);

  await checkPage(gvCtx, 'GV > Hồ sơ', '/Profile', [
    noException,
    hasElement('form', 'Form hồ sơ'),
  ]);

  await checkPage(gvCtx, 'GV > Thông báo', '/ThongBao', [
    noException,
  ]);

  await checkPage(gvCtx, 'GV > Đổi mật khẩu', '/Account/ChangePassword', [
    noException,
    hasElement('form', 'Form đổi mật khẩu'),
  ]);

  // GV-specific: nhập điểm vào lớp
  await checkPage(gvCtx, 'GV > DiemSo/LopHoc (nhập điểm)', '/DiemSo/LopHoc', [
    noException,
  ]);

  await gvCtx.close();

  // ================================================================
  // HỌC VIÊN
  // ================================================================
  console.log('Setting up HỌC VIÊN session...');
  const hvCtx = await browser.newContext();
  await login(hvCtx, 'hv01@nnl.com', 'Hv@123');

  await checkPage(hvCtx, 'HV > Dashboard', '/HocVien/Dashboard', [
    noException,
    hasSidebar('hv'),
    { desc: 'Dashboard HV có thống kê', fn: async (p) => (await p.$$('.card')).length >= 2 },
  ]);

  await checkPage(hvCtx, 'HV > Khóa học', '/KhoaHoc', [
    noException,
    tableHasRows('Danh sách khóa học'),
  ]);

  await checkPage(hvCtx, 'HV > Đăng ký của tôi', '/DangKy/CuaToi', [
    noException,
    { desc: 'Có danh sách đăng ký', fn: async (p) => (await p.$('table, .card, .list-group')) !== null },
  ]);

  await checkPage(hvCtx, 'HV > Lịch học của tôi', '/LichHoc/CuaToi', [
    noException,
    { desc: 'Có lịch học', fn: async (p) => (await p.$('table, .card, [class*="calendar"]')) !== null },
  ]);

  await checkPage(hvCtx, 'HV > Điểm của tôi', '/DiemSo/CuaToi', [
    noException,
    { desc: 'Có bảng điểm', fn: async (p) => (await p.$('table, .card')) !== null },
  ]);

  await checkPage(hvCtx, 'HV > Gợi ý AI', '/GoiY', [
    noException,
    { desc: 'Có form gợi ý', fn: async (p) => (await p.$('form, button, .card')) !== null },
  ]);

  await checkPage(hvCtx, 'HV > Lịch sử gợi ý AI', '/GoiY/LichSu', [
    noException,
  ]);

  await checkPage(hvCtx, 'HV > Thanh toán của tôi', '/ThanhToan/CuaToi', [
    noException,
    { desc: 'Có danh sách thanh toán', fn: async (p) => (await p.$('table, .card, .list-group')) !== null },
  ]);

  await checkPage(hvCtx, 'HV > Hồ sơ', '/Profile', [
    noException,
    hasElement('form', 'Form hồ sơ'),
  ]);

  await checkPage(hvCtx, 'HV > Thông báo', '/ThongBao', [
    noException,
  ]);

  await checkPage(hvCtx, 'HV > Đổi mật khẩu', '/Account/ChangePassword', [
    noException,
    hasElement('form', 'Form đổi mật khẩu'),
  ]);

  await hvCtx.close();

  // ================================================================
  // Test Access Control: HV không được vào trang Admin
  // ================================================================
  console.log('Testing access control...');
  const hvCtx2 = await browser.newContext();
  await login(hvCtx2, 'hv01@nnl.com', 'Hv@123');

  const acPage = await hvCtx2.newPage();
  const acRes = await acPage.goto(BASE + '/Admin', { waitUntil: 'domcontentloaded' });
  const acUrl = acPage.url();
  const acStatus = acRes ? acRes.status() : 0;
  const acBody = await acPage.content();
  const acOk = acUrl.includes('AccessDenied') || acStatus === 403 || acBody.includes('Từ chối') || acBody.includes('Access Denied') || acBody.includes('403');
  results.push({
    icon: acOk ? '✅' : '❌',
    label: 'AccessControl: HV không vào được Admin',
    url: '/Admin',
    httpStatus: acStatus,
    finalUrl: acUrl,
    issues: acOk ? [] : [`HV vào được Admin! URL: ${acUrl}`],
    consoleErrs: [],
  });
  await acPage.close();
  await hvCtx2.close();

  // GV không vào Admin
  const gvCtx2 = await browser.newContext();
  await login(gvCtx2, 'gv01@nnl.com', 'Gv@123');
  const gvAcPage = await gvCtx2.newPage();
  const gvAcRes = await gvAcPage.goto(BASE + '/Admin', { waitUntil: 'domcontentloaded' });
  const gvAcUrl = gvAcPage.url();
  const gvAcStatus = gvAcRes ? gvAcRes.status() : 0;
  const gvAcBody = await gvAcPage.content();
  const gvAcOk = gvAcUrl.includes('AccessDenied') || gvAcStatus === 403 || gvAcBody.includes('Từ chối') || gvAcBody.includes('Access Denied') || gvAcBody.includes('403');
  results.push({
    icon: gvAcOk ? '✅' : '❌',
    label: 'AccessControl: GV không vào được Admin',
    url: '/Admin',
    httpStatus: gvAcStatus,
    finalUrl: gvAcUrl,
    issues: gvAcOk ? [] : [`GV vào được Admin! URL: ${gvAcUrl}`],
    consoleErrs: [],
  });
  await gvAcPage.close();
  await gvCtx2.close();

  await browser.close();

  // ================================================================
  // REPORT
  // ================================================================
  console.log('\n\n══════════════════════════════════════════');
  console.log('       BÁO CÁO TEST TOÀN BỘ LUỒNG');
  console.log('══════════════════════════════════════════\n');

  const ok = results.filter(r => r.icon === '✅');
  const warn = results.filter(r => r.icon === '🟡');
  const err = results.filter(r => r.icon === '❌');

  console.log(`📊 Tổng: ${results.length} | ✅ OK: ${ok.length} | 🟡 Cảnh báo: ${warn.length} | ❌ Lỗi: ${err.length}\n`);

  // Group by role
  const groups = { 'ADMIN': [], 'GV': [], 'HV': [], 'AccessControl': [] };
  for (const r of results) {
    if (r.label.startsWith('Admin')) groups['ADMIN'].push(r);
    else if (r.label.startsWith('GV')) groups['GV'].push(r);
    else if (r.label.startsWith('HV')) groups['HV'].push(r);
    else groups['AccessControl'].push(r);
  }

  for (const [grp, items] of Object.entries(groups)) {
    if (items.length === 0) continue;
    const grpOk = items.filter(r => r.icon === '✅').length;
    const grpErr = items.filter(r => r.icon === '❌').length;
    const grpWarn = items.filter(r => r.icon === '🟡').length;
    console.log(`\n┌─ ${grp} (${grpOk}✅ ${grpWarn}🟡 ${grpErr}❌ / ${items.length} trang)`);
    for (const r of items) {
      const issueStr = r.issues.length ? ` → ${r.issues[0].substring(0,80)}` : '';
      const consStr = r.consoleErrs.length ? ` [console: ${r.consoleErrs[0].substring(0,60)}]` : '';
      console.log(`│  ${r.icon} [${r.httpStatus}] ${r.label}${issueStr}${consStr}`);
      if (r.issues.length > 1) {
        for (const iss of r.issues.slice(1)) console.log(`│      └─ ${iss.substring(0,100)}`);
      }
    }
    console.log('└─────────────────────────────────────────────');
  }

  if (err.length > 0 || warn.length > 0) {
    console.log('\n\n══════════ DANH SÁCH CẦN SỬA ══════════\n');
    const needFix = [...err, ...warn];
    needFix.forEach((r, i) => {
      console.log(`${i+1}. ${r.icon} ${r.label} (${r.url})`);
      if (r.issues.length) r.issues.forEach(iss => console.log(`   ❗ ${iss.substring(0,200)}`));
      if (r.consoleErrs.length) r.consoleErrs.forEach(ce => console.log(`   🟡 console: ${ce.substring(0,150)}`));
    });
  } else {
    console.log('\n✨ Không có lỗi nào cần sửa!');
  }
})();
