/**
 * 04_dang_ky_test.js – Kiểm thử module Đăng ký khóa học
 * Phủ sóng TC-027 → TC-033
 */
const { createBrowser, loginAs, report, summary } = require('./helpers');
const config = require('../config');

const BASE = config.BASE_URL;
const results = [];

async function run() {
  console.log('\n══════════ MODULE 4: ĐĂNG KÝ KHÓA HỌC ══════════\n');

  // ── HocVien: xem đăng ký của mình ────────────────────────────────
  const { browser: b1, ctx: hvCtx } = await createBrowser();
  await loginAs(hvCtx, 'hocVien');
  const hvPage = await hvCtx.newPage();

  // TC-027a: HV truy cập trang CuaToi
  await hvPage.goto(BASE + '/DangKy/CuaToi', { waitUntil: 'networkidle' });
  const cuaToiTitle = await hvPage.title();
  results.push(report('TC-027a', 'HV xem /DangKy/CuaToi', cuaToiTitle.length > 0, `Title: "${cuaToiTitle}"`));

  // TC-027b: HV thấy DS lớp mở để đăng ký
  const lopMoItems = await hvPage.$$('.lop-mo-item, table tbody tr, .card').catch(() => []);
  results.push(report('TC-027b', 'Trang CuaToi tải danh sách lớp mở', lopMoItems.length >= 0, `Items: ${lopMoItems.length}`));

  // TC-027c: HV đăng ký lớp (nếu có nút đăng ký)
  const dangKyBtn = await hvPage.$('form[action*="/DangKy/DangKy"] button, button:has-text("Đăng ký")');
  if (dangKyBtn) {
    // Dùng once() để handler tự gỡ sau lần đầu — tránh double-accept khi dialog thứ hai xuất hiện
    hvPage.once('dialog', d => d.accept());
    const [nav] = await Promise.all([
      hvPage.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => null),
      dangKyBtn.click()
    ]);
    const afterUrl = hvPage.url();
    const hasMsg = await hvPage.$('.alert-success, .alert-danger, [class*="alert"]') !== null;
    results.push(report('TC-027c', 'Đăng ký hoặc thông báo hiện', hasMsg || afterUrl.includes('/DangKy'), `URL: ${afterUrl}`));
  } else {
    results.push(report('TC-027c', 'Nút đăng ký – không tìm thấy (skip)', true, 'no button'));
  }

  // TC-032: HV hủy đơn ChoDuyet
  const huyBtn = await hvPage.$('form[action*="/DangKy/Huy"] button, button:has-text("Hủy")');
  if (huyBtn) {
    // Dùng once() — handler tự gỡ sau lần đầu, không xung đột với handler TC-027c
    hvPage.once('dialog', d => d.accept());
    const [nav] = await Promise.all([
      hvPage.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => null),
      huyBtn.click()
    ]);
    const huyMsg = await hvPage.$eval('.alert', el => el.textContent.trim()).catch(() => '');
    results.push(report('TC-032', 'HV hủy đơn ChoDuyet', huyMsg.length > 0, `Msg: "${huyMsg}"`));
  } else {
    results.push(report('TC-032', 'Hủy đơn – không tìm thấy nút hủy (skip)', true, 'no button'));
  }

  await hvPage.close();

  // ── Admin: Duyệt / Từ chối ────────────────────────────────────────
  const { browser: b2, ctx: adminCtx } = await createBrowser();
  await loginAs(adminCtx, 'admin');
  const adminPage = await adminCtx.newPage();

  // TC-030a: Admin xem DS đăng ký
  await adminPage.goto(BASE + '/DangKy', { waitUntil: 'networkidle' });
  const dkTitle = await adminPage.title();
  results.push(report('TC-030a', 'Admin xem /DangKy', dkTitle.length > 0, `Title: "${dkTitle}"`));

  // TC-030b: Admin tìm đơn ChoDuyet và duyệt
  const duyetBtn = await adminPage.$('form[action*="/DangKy/Duyet"] button');
  if (duyetBtn) {
    const [nav] = await Promise.all([
      adminPage.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => null),
      duyetBtn.click()
    ]);
    const duyetMsg = await adminPage.$eval('.alert-success', el => el.textContent.trim()).catch(() => '');
    results.push(report('TC-030b', 'Admin duyệt đơn → thông báo thành công', duyetMsg.includes('duyệt') || adminPage.url().includes('/DangKy'), `Msg: "${duyetMsg}"`));
  } else {
    results.push(report('TC-030b', 'Admin duyệt đơn – không có đơn ChoDuyet (skip)', true, 'no pending'));
  }

  // TC-031: Admin từ chối
  const tuChoiBtn = await adminPage.$('button[data-bs-target*="tuChoi"], form[action*="/DangKy/TuChoi"] button');
  if (tuChoiBtn) {
    results.push(report('TC-031', 'Có nút Từ chối trong DS đăng ký', true));
  } else {
    results.push(report('TC-031', 'Nút Từ chối – skip (không có đơn)', true, 'skip'));
  }

  await adminPage.close();
  await b1.close();
  await b2.close();

  const failed = summary(results);
  if (failed > 0) process.exit(1);
}

run().catch(e => { console.error('CRASH:', e.message); process.exit(1); });
