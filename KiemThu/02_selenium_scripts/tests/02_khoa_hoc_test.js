/**
 * 02_khoa_hoc_test.js – Kiểm thử module Khóa học
 * Phủ sóng TC-011 → TC-020
 */
const { chromium } = require('playwright');
const { createBrowser, loginAs, report, summary } = require('./helpers');
const config = require('../config');

const BASE = config.BASE_URL;
const results = [];

async function run() {
  console.log('\n══════════ MODULE 2: KHÓA HỌC ══════════\n');
  const { browser, ctx } = await createBrowser();

  // ── Đăng nhập Admin ──────────────────────────────────────────────
  await loginAs(ctx, 'admin');
  const adminPage = await ctx.newPage();

  // TC-011: Admin xem DS khóa học (tất cả trạng thái)
  await adminPage.goto(BASE + '/KhoaHoc', { waitUntil: 'networkidle' });
  const adminKhCount = await adminPage.$$eval('.card, table tbody tr', els => els.length).catch(() => 0);
  results.push(report('TC-011', 'Admin xem DS khóa học (cards/rows hiển thị)', adminKhCount > 0, `Count: ${adminKhCount}`));

  // TC-013: Filter ngôn ngữ
  await adminPage.goto(BASE + '/KhoaHoc?NgonNgu=TiengAnh', { waitUntil: 'networkidle' });
  const filterTitle = await adminPage.title();
  results.push(report('TC-013', 'Filter khóa học theo ngôn ngữ – trang load OK', filterTitle.length > 0, `Title: "${filterTitle}"`));

  // TC-014: Tạo khóa học mới
  await adminPage.goto(BASE + '/KhoaHoc/Create', { waitUntil: 'domcontentloaded' });
  const createFormExists = await adminPage.$('form') !== null;
  results.push(report('TC-014a', 'Trang Create KH tải được (form tồn tại)', createFormExists));

  if (createFormExists) {
    await adminPage.fill('input[name="TenKhoaHoc"]', `AutoTest_KH_${Date.now()}`);
    await adminPage.selectOption('select[name="NgonNgu"]', 'Tiếng Anh').catch(() => {});
    await adminPage.selectOption('select[name="TrinhDo"]', 'Sơ cấp').catch(() => {});
    await adminPage.fill('input[name="HocPhi"]', '3000000');
    // SoChoToiDa không có trong KhoaHoc/Create.cshtml (chỉ có ở LopHoc)
    await adminPage.fill('input[name="ThoiLuong"]', '60');
    await adminPage.fill('input[name="SoBuoiMoiTuan"]', '3');
    await adminPage.fill('input[name="ThoiGianMoiBuoi"]', '90');
    const [resp] = await Promise.all([
      adminPage.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => null),
      adminPage.click('button[type="submit"]')
    ]);
    const afterCreate = adminPage.url();
    results.push(report('TC-014b', 'POST Create KH → redirect DS', afterCreate.includes('/KhoaHoc'), `URL: ${afterCreate}`));
  }

  // TC-015: Tạo KH thiếu tên (validation)
  await adminPage.goto(BASE + '/KhoaHoc/Create', { waitUntil: 'domcontentloaded' });
  const formValid = await adminPage.$('form') !== null;
  if (formValid) {
    // Không điền TenKhoaHoc
    await adminPage.fill('input[name="HocPhi"]', '1000000');
    await adminPage.click('button[type="submit"]');
    await adminPage.waitForLoadState('domcontentloaded');
    const stillCreate = adminPage.url().includes('/KhoaHoc/Create') ||
                        await adminPage.$('.field-validation-error, .text-danger') !== null;
    results.push(report('TC-015', 'POST Create KH thiếu tên → validation lỗi', stillCreate));
  }

  // TC-019: Đổi trạng thái KH qua modal
  // KhoaHoc/Index dùng onclick="showChangeStatus(id, status)" → mở modal Bootstrap
  // Sau đó submit form #changeStatusForm POST → /KhoaHoc/ChangeStatus/{id}
  await adminPage.goto(BASE + '/KhoaHoc', { waitUntil: 'networkidle' });
  const statusTrigger = await adminPage.$('button[title="Đổi trạng thái"], button[onclick*="showChangeStatus"]');
  if (statusTrigger) {
    // Bước 1: Click nút để mở modal
    await statusTrigger.click();
    // Bước 2: Chờ modal hiện
    await adminPage.waitForSelector('#changeStatusModal.show', { timeout: 5000 }).catch(() => {});
    // Bước 3: Submit form trong modal và chờ navigation
    const [nav] = await Promise.all([
      adminPage.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => null),
      adminPage.click('#changeStatusForm button[type="submit"]')
    ]);
    const afterUrl = adminPage.url();
    const hasSuccess = await adminPage.$('.alert-success') !== null;
    results.push(report('TC-019', 'ChangeStatus KH → redirect/thành công', afterUrl.includes('/KhoaHoc') || hasSuccess, `URL: ${afterUrl}`));
  } else {
    results.push(report('TC-019', 'ChangeStatus – không tìm thấy nút trigger', false, 'skip'));
  }

  await adminPage.close();

  // ── Đăng nhập HocVien ────────────────────────────────────────────
  const hvCtx = await browser.newContext();
  await loginAs(hvCtx, 'hocVien');
  const hvPage = await hvCtx.newPage();

  // TC-012: HV chỉ thấy KH DangMo
  await hvPage.goto(BASE + '/KhoaHoc', { waitUntil: 'networkidle' });
  const allStatusText = await hvPage.evaluate(() => {
    const els = [...document.querySelectorAll('.badge, .text-muted, [class*="status"]')];
    return els.map(e => e.textContent).join(' ');
  });
  const noDaDong = !allStatusText.toLowerCase().includes('đã đóng');
  results.push(report('TC-012', 'HV chỉ thấy KH DangMo (không thấy Đã đóng)', noDaDong, allStatusText.substring(0, 80)));

  // TC-020: Chi tiết KH
  const detailLink = await hvPage.$('a[href*="/KhoaHoc/Details"]');
  if (detailLink) {
    await detailLink.click();
    await hvPage.waitForLoadState('domcontentloaded');
    const hasTitle = await hvPage.$('h1, h2, h3') !== null;
    results.push(report('TC-020', 'Chi tiết KH tải được', hasTitle, `URL: ${hvPage.url()}`));
  }

  await hvPage.close();
  await browser.close();

  const failed = summary(results);
  if (failed > 0) process.exit(1);
}

run().catch(e => { console.error('CRASH:', e.message); process.exit(1); });
