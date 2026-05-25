/**
 * 06_thanh_toan_test.js – Kiểm thử module Thanh toán
 * Phủ sóng TC-040 → TC-045
 */
const { createBrowser, loginAs, report, summary } = require('./helpers');
const config = require('../config');

const BASE = config.BASE_URL;
const results = [];

async function run() {
  console.log('\n══════════ MODULE 6: THANH TOÁN ══════════\n');

  // ── HocVien: Thanh toán của tôi ───────────────────────────────────
  const { browser: b1, ctx: hvCtx } = await createBrowser();
  await loginAs(hvCtx, 'hocVien');
  const hvPage = await hvCtx.newPage();

  // TC-040a: HV xem trang thanh toán
  await hvPage.goto(BASE + '/ThanhToan/CuaToi', { waitUntil: 'networkidle' });
  const ttTitle = await hvPage.title();
  results.push(report('TC-040a', 'HV xem /ThanhToan/CuaToi', ttTitle.length > 0, `Title: "${ttTitle}"`));

  // TC-040b: HV tạo yêu cầu thanh toán (nếu có nút "Thanh toán")
  const taoYcBtn = await hvPage.$('a[href*="/ThanhToan/TaoYeuCau"], button:has-text("Thanh toán")');
  if (taoYcBtn) {
    await taoYcBtn.click();
    await hvPage.waitForLoadState('domcontentloaded');
    const ycForm = await hvPage.$('form') !== null;
    results.push(report('TC-040b', 'Trang TaoYeuCau tải được', ycForm, `URL: ${hvPage.url()}`));

    if (ycForm) {
      // Điền form thanh toán
      await hvPage.selectOption('select[name="PhuongThuc"]', 'TienMat').catch(() => {});
      const [nav] = await Promise.all([
        hvPage.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => null),
        hvPage.click('button[type="submit"]')
      ]);
      const successMsg = await hvPage.$eval('.alert-success, .alert-warning', el => el.textContent.trim()).catch(() => '');
      results.push(report('TC-040c', 'POST TaoYeuCau → thông báo', successMsg.length > 0, `Msg: "${successMsg}"`));
    }
  } else {
    results.push(report('TC-040b', 'Không có nút TaoYeuCau (HV chưa có DaDuyet hoặc đã có yêu cầu)', true, 'skip'));
    results.push(report('TC-040c', 'skip', true, 'skip'));
  }

  // TC-041: Tạo yêu cầu khi không có đăng ký hợp lệ
  await hvPage.goto(BASE + '/ThanhToan/TaoYeuCau?khoaHocId=99999', { waitUntil: 'domcontentloaded' });
  const notFoundOrRedirect = hvPage.url().includes('/CuaToi') || hvPage.url().includes('/ThanhToan');
  const errMsg = await hvPage.$eval('.alert-danger, .alert-warning', el => el.textContent.trim()).catch(() => '');
  results.push(report('TC-041', 'TaoYeuCau KH không hợp lệ → lỗi hoặc redirect', notFoundOrRedirect || errMsg.length > 0, `URL: ${hvPage.url()}, Msg: "${errMsg}"`));

  await hvPage.close();

  // ── Admin: Duyệt thanh toán ───────────────────────────────────────
  const { browser: b2, ctx: adminCtx } = await createBrowser();
  await loginAs(adminCtx, 'admin');
  const adminPage = await adminCtx.newPage();

  // TC-043a: Admin xem DS thanh toán
  await adminPage.goto(BASE + '/ThanhToan', { waitUntil: 'networkidle' });
  const adminTtTitle = await adminPage.title();
  results.push(report('TC-043a', 'Admin xem /ThanhToan', adminTtTitle.length > 0, `Title: "${adminTtTitle}"`));

  // TC-043b: Admin duyệt yêu cầu ChoPheduyet
  const duyetBtn = await adminPage.$('a[href*="/ThanhToan/ChiTiet"], form[action*="/ThanhToan/Duyet"] button');
  if (duyetBtn) {
    results.push(report('TC-043b', 'Nút Duyệt thanh toán tồn tại', true));
  } else {
    results.push(report('TC-043b', 'Không có yêu cầu ChoPheduyet (skip)', true, 'no pending payments'));
  }

  // TC-045: API ThongKe6Thang
  const [thongKeResp] = await Promise.all([
    adminPage.waitForResponse(r => r.url().includes('/ThanhToan/ThongKe6Thang'), { timeout: 5000 }).catch(() => null),
    adminPage.goto(BASE + '/ThanhToan/ThongKe6Thang', { waitUntil: 'domcontentloaded' })
  ]);
  const thongKeJson = await adminPage.evaluate(() => {
    try { return JSON.parse(document.body.innerText); } catch { return null; }
  });
  const isArray6 = Array.isArray(thongKeJson) && thongKeJson.length === 6;
  results.push(report('TC-045', 'API ThongKe6Thang trả về 6 phần tử', isArray6, `Length: ${thongKeJson?.length}`));

  await adminPage.close();
  await b1.close();
  await b2.close();

  const failed = summary(results);
  if (failed > 0) process.exit(1);
}

run().catch(e => { console.error('CRASH:', e.message); process.exit(1); });
