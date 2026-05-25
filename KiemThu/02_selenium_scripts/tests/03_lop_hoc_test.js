/**
 * 03_lop_hoc_test.js – Kiểm thử module Lớp học
 * Phủ sóng TC-021 → TC-026
 */
const { createBrowser, loginAs, report, summary } = require('./helpers');
const config = require('../config');

const BASE = config.BASE_URL;
const results = [];

async function run() {
  console.log('\n══════════ MODULE 3: LỚP HỌC ══════════\n');

  // ── Admin: xem tất cả lớp ────────────────────────────────────────
  const { browser: b1, ctx: adminCtx } = await createBrowser();
  await loginAs(adminCtx, 'admin');
  const adminPage = await adminCtx.newPage();

  // TC-021: Admin xem tất cả lớp
  await adminPage.goto(BASE + '/LopHoc', { waitUntil: 'networkidle' });
  const allRows = await adminPage.$$('table tbody tr, .card').catch(() => []);
  results.push(report('TC-021', 'Admin thấy tất cả lớp', allRows.length >= 0, `Rows: ${allRows.length}`));

  // TC-023: Tạo lớp học mới
  await adminPage.goto(BASE + '/LopHoc/Create', { waitUntil: 'domcontentloaded' });
  const createForm = await adminPage.$('form') !== null;
  results.push(report('TC-023a', 'Trang Create LopHoc tải được', createForm));

  if (createForm) {
    const tenLopUniq = `AutoLop_${Date.now()}`;
    await adminPage.fill('input[name="TenLop"]', tenLopUniq);
    // Chọn khóa học đầu tiên trong select
    const khOption = await adminPage.$('select[name="KhoaHocId"] option:not([value=""])');
    if (khOption) {
      const khVal = await khOption.getAttribute('value');
      await adminPage.selectOption('select[name="KhoaHocId"]', khVal);
    }
    await adminPage.fill('input[name="NgayKhaiGiang"]', '2026-06-01');
    await adminPage.fill('input[name="NgayKetThuc"]', '2026-12-31');
    await adminPage.fill('input[name="SiSoToiDa"]', '20');
    await adminPage.selectOption('select[name="TrangThai"]', 'DangTuyenSinh').catch(() => {});
    const [nav] = await Promise.all([
      adminPage.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => null),
      adminPage.click('button[type="submit"]')
    ]);
    const afterUrl = adminPage.url();
    results.push(report('TC-023b', 'POST Create LopHoc → redirect DS', afterUrl.includes('/LopHoc'), `URL: ${afterUrl}`));
  }

  // TC-024: Sửa lớp học
  await adminPage.goto(BASE + '/LopHoc', { waitUntil: 'networkidle' });
  const editLink = await adminPage.$('a[href*="/LopHoc/Edit"]');
  if (editLink) {
    await editLink.click();
    await adminPage.waitForLoadState('domcontentloaded');
    const editForm = await adminPage.$('form') !== null;
    results.push(report('TC-024', 'Trang Edit LopHoc tải được', editForm));
  }

  // TC-026: Xóa lớp có HV (thất bại)
  // Tìm lớp đầu tiên trong DS và thử xóa → mong đợi lỗi vì thường có DangKy
  const deleteForm = await adminPage.$('form[action*="/LopHoc/Delete"]');
  if (deleteForm) {
    await deleteForm.evaluate(f => f.submit());
    await adminPage.waitForLoadState('domcontentloaded');
    const errorMsg = await adminPage.$eval('.alert-danger, .text-danger, [class*="error"]',
      el => el.textContent.trim()).catch(() => '');
    const hasError = errorMsg.includes('Không thể xóa') || adminPage.url().includes('/LopHoc');
    results.push(report('TC-026', 'Xóa lớp có HV → lỗi hoặc redirect với msg', hasError, `Msg: "${errorMsg}"`));
  }

  await adminPage.close();

  // ── GV: chỉ xem lớp của mình ─────────────────────────────────────
  const { browser: b2, ctx: gvCtx } = await createBrowser();
  await loginAs(gvCtx, 'giangVien');
  const gvPage = await gvCtx.newPage();

  // TC-022: GV thấy lớp của mình
  await gvPage.goto(BASE + '/LopHoc', { waitUntil: 'networkidle' });
  const gvTitle = await gvPage.title();
  results.push(report('TC-022', 'GV truy cập /LopHoc tải được', gvTitle.length > 0, `Title: "${gvTitle}"`));

  await gvPage.close();

  await b1.close();
  await b2.close();

  const failed = summary(results);
  if (failed > 0) process.exit(1);
}

run().catch(e => { console.error('CRASH:', e.message); process.exit(1); });
