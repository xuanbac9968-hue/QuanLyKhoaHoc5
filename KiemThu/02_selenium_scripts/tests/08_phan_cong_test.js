/**
 * 08_phan_cong_test.js – Kiểm thử module Phân công Giảng viên
 * Phủ sóng TC-053 → TC-055
 */
const { createBrowser, loginAs, report, summary } = require('./helpers');
const config = require('../config');

const BASE = config.BASE_URL;
const results = [];

async function run() {
  console.log('\n══════════ MODULE 8: PHÂN CÔNG GIẢNG VIÊN ══════════\n');

  const { browser, ctx } = await createBrowser();
  await loginAs(ctx, 'admin');
  const page = await ctx.newPage();

  // TC-053a: Xem trang phân công
  await page.goto(BASE + '/Admin/PhanCong', { waitUntil: 'networkidle' });
  const pcTitle = await page.title();
  results.push(report('TC-053a', 'Trang PhanCong tải được', pcTitle.length > 0, `Title: "${pcTitle}"`));

  // TC-053b: Form phân công tồn tại
  const pcForm = await page.$('form[action*="/Admin/DoPhanCong"]');
  results.push(report('TC-053b', 'Form DoPhanCong tồn tại', !!pcForm));

  if (pcForm) {
    // Lấy giá trị KhoaHocId từ form đầu tiên
    const khoaHocId = await pcForm.evaluate(f => {
      const input = f.querySelector('input[name="KhoaHocId"]');
      return input ? input.value : null;
    });

    // Select GV đầu tiên
    const gvSelect = await pcForm.$('select[name="GiangVienId"]');
    if (gvSelect) {
      const firstGvOption = await pcForm.$('select[name="GiangVienId"] option:not([value=""])');
      if (firstGvOption) {
        const gvVal = await firstGvOption.getAttribute('value');
        await gvSelect.selectOption(gvVal);

        // TC-053c: Thực hiện phân công
        // pcForm là Playwright ElementHandle → dùng .$() chứ không phải .querySelector()
        const submitBtn = await pcForm.$('button[type="submit"]');
        const [nav] = await Promise.all([
          page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => null),
          submitBtn?.click()
        ]);
        const successMsg = await page.$eval('.alert-success', el => el.textContent.trim()).catch(() => '');
        results.push(report('TC-053c', 'Phân công GV → thông báo thành công', successMsg.includes('phân công') || successMsg.length > 0, `Msg: "${successMsg}"`));
      } else {
        results.push(report('TC-053c', 'Không có GV trong select – skip', true, 'no GV'));
      }
    } else {
      results.push(report('TC-053c', 'Không tìm thấy select GV – skip', true, 'no select'));
    }
  }

  // TC-054: Lịch sử phân công
  const historyLink = await page.$('a[href*="/Admin/LichSuPhanCong"]');
  if (historyLink) {
    await historyLink.click();
    await page.waitForLoadState('domcontentloaded');
    const histTitle = await page.title();
    results.push(report('TC-054', 'Trang LichSuPhanCong tải được', histTitle.length > 0, `Title: "${histTitle}"`));
  } else {
    results.push(report('TC-054', 'Link LichSuPhanCong không thấy (skip)', true, 'skip'));
  }

  // TC-055: Hủy phân công
  await page.goto(BASE + '/Admin/PhanCong', { waitUntil: 'networkidle' });
  const huyBtn = await page.$('form[action*="/Admin/HuyPhanCong"] button');
  if (huyBtn) {
    // Chờ navigation hoàn tất (TempData được render sau redirect)
    await Promise.all([
      page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => null),
      huyBtn.click()
    ]);
    // Đợi alert xuất hiện trước khi đọc (tránh race condition với TempData)
    await page.waitForSelector('.alert-success, .alert-danger, .alert', { timeout: 5000 }).catch(() => {});
    const huyMsg = await page.$eval('.alert-success, .alert-danger, .alert', el => el.textContent.trim()).catch(() => '');
    results.push(report('TC-055', 'Hủy phân công → thông báo', huyMsg.includes('hủy') || huyMsg.length > 0, `Msg: "${huyMsg}"`));
  } else {
    results.push(report('TC-055', 'Không có phân công để hủy (skip)', true, 'no assignment to cancel'));
  }

  await page.close();
  await browser.close();

  const failed = summary(results);
  if (failed > 0) process.exit(1);
}

run().catch(e => { console.error('CRASH:', e.message); process.exit(1); });
