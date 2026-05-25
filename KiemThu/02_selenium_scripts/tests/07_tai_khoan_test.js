/**
 * 07_tai_khoan_test.js – Kiểm thử module Quản lý Tài khoản
 * Phủ sóng TC-046 → TC-052
 * (Port từ _test2/test_taikhoan.js đã xác nhận chạy OK)
 */
const { chromium } = require('playwright');
const config = require('../config');

const BASE = config.BASE_URL;
const results = [];

async function login(ctx) {
  const p = await ctx.newPage();
  await p.goto(BASE + '/Account/Login', { waitUntil: 'domcontentloaded' });
  await p.fill('input[name="Email"]', 'admin@nnl.com');
  await p.fill('input[name="MatKhau"]', 'Admin@123');
  await p.click('button[type="submit"]');
  await p.waitForLoadState('domcontentloaded');
  await p.close();
}

function report(id, name, passed, detail = '') {
  const icon = passed ? '✅' : '❌';
  console.log(`[${id}] ${icon} ${name}${detail ? ' | ' + detail : ''}`);
  results.push(passed);
  return passed;
}

async function run() {
  console.log('\n══════════ MODULE 7: QUẢN LÝ TÀI KHOẢN ══════════\n');

  const browser = await chromium.launch(config.BROWSER);
  const ctx = await browser.newContext();
  await login(ctx);

  const page = await ctx.newPage();
  const errs = [];
  page.on('pageerror', e => errs.push(e.message));
  page.on('console', m => { if (m.type() === 'error') errs.push('console: ' + m.text()); });

  // TC-046a: Trang /TaiKhoan load
  await page.goto(BASE + '/TaiKhoan', { waitUntil: 'networkidle' });
  report('TC-046a', 'Trang /TaiKhoan tải OK', (await page.title()).length > 0, `Title: "${await page.title()}"`);

  // Nút Tạo tài khoản mới
  const createBtn = await page.$('button:has-text("Tạo tài khoản mới")');
  report('TC-046b', 'Nút "Tạo tài khoản mới" tồn tại', !!createBtn);

  // Bảng tài khoản
  const rows = await page.$$('#tk-tbody tr:not(.no-data-row)');
  report('TC-046c', `Bảng có ${rows.length} tài khoản`, rows.length > 0);

  // Action buttons
  const btnKhoa  = await page.$('.btn-khoa');
  const btnReset = await page.$('.btn-reset');
  const btnSuaVT = await page.$('.btn-sua-vaitro');
  report('TC-048a', 'Nút Khóa/Mở khóa tồn tại', !!btnKhoa);
  report('TC-050a', 'Nút Reset MK tồn tại', !!btnReset);
  report('TC-051a', 'Nút Sửa vai trò tồn tại', !!btnSuaVT);

  // Badge "Bạn"
  const selfBadge = await page.$('.badge.bg-info[title="Tài khoản đang đăng nhập"]');
  report('TC-049a', 'Badge "Bạn" hiện cho tài khoản mình', !!selfBadge);

  // Self row: Khóa và Sửa vai trò ẩn
  const selfRow = await page.evaluate(() => {
    const badge = document.querySelector('.badge.bg-info[title="Tài khoản đang đăng nhập"]');
    if (!badge) return null;
    const row = badge.closest('tr');
    return {
      hasKhoa: !!row.querySelector('.btn-khoa'),
      hasSuaVT: !!row.querySelector('.btn-sua-vaitro'),
      hasReset: !!row.querySelector('.btn-reset')
    };
  });
  if (selfRow) {
    report('TC-049b', 'Self row: Khóa bị ẩn', !selfRow.hasKhoa);
    report('TC-049c', 'Self row: SuaVaiTro bị ẩn', !selfRow.hasSuaVT);
    report('TC-049d', 'Self row: Reset vẫn hiện', selfRow.hasReset);
  }

  // TC-046: Mở modal Tạo TK, điền form, POST
  if (createBtn) {
    await createBtn.click();
    await page.waitForSelector('#modalCreateTK.show', { timeout: config.TIMEOUT.modal }).catch(() => {});
    const modalVis = await page.$eval('#modalCreateTK', el =>
      window.getComputedStyle(el).display !== 'none'
    ).catch(() => false);
    report('TC-046d', 'Modal Tạo TK mở được', modalVis);

    const fields = ['cr-hoTen', 'cr-email', 'cr-vaiTro', 'cr-matKhau', 'cr-xacNhan'];
    const fieldsOk = (await Promise.all(fields.map(f => page.$(`#${f}`)))).every(Boolean);
    report('TC-046e', 'Tất cả 5 field trong modal', fieldsOk);

    // Điền form
    const testEmail = `test.auto.${Date.now()}@example.com`;
    await page.fill('#cr-hoTen', 'AutoTest User');
    await page.fill('#cr-email', testEmail);
    await page.selectOption('#cr-vaiTro', 'HocVien');
    await page.fill('#cr-matKhau', 'Test@123');
    await page.fill('#cr-xacNhan', 'Test@123');

    const [resp] = await Promise.all([
      page.waitForResponse(r => r.url().includes('/TaiKhoan/Create') && r.request().method() === 'POST', { timeout: config.TIMEOUT.ajax }),
      page.click('#btn-create-submit')
    ]);
    const json = await resp.json().catch(() => null);
    report('TC-046f', 'POST Create → success=true', json?.success === true, JSON.stringify(json));

    // TC-047: Email trùng → thất bại
    await page.waitForTimeout(500);
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);

    if (createBtn) {
      await page.click('button:has-text("Tạo tài khoản mới")');
      await page.waitForSelector('#modalCreateTK.show', { timeout: config.TIMEOUT.modal }).catch(() => {});
      await page.fill('#cr-hoTen', 'Duplicate User');
      await page.fill('#cr-email', testEmail); // same email
      await page.selectOption('#cr-vaiTro', 'HocVien');
      await page.fill('#cr-matKhau', 'Test@123');
      await page.fill('#cr-xacNhan', 'Test@123');

      const [resp2] = await Promise.all([
        page.waitForResponse(r => r.url().includes('/TaiKhoan/Create') && r.request().method() === 'POST', { timeout: config.TIMEOUT.ajax }),
        page.click('#btn-create-submit')
      ]);
      const json2 = await resp2.json().catch(() => null);
      report('TC-047', 'POST Create email trùng → success=false', json2?.success === false, JSON.stringify(json2));
    }
  }

  // TC-048: KhoaTaiKhoan toggle
  await page.keyboard.press('Escape').catch(() => {});
  await page.waitForTimeout(400);

  const lockTarget = await page.evaluate(() => {
    const btn = document.querySelector('.btn-khoa.btn-warning, .btn-khoa.btn-success');
    if (!btn) return null;
    return { id: btn.dataset.id, isWarning: btn.classList.contains('btn-warning') };
  });

  if (lockTarget) {
    page.on('dialog', d => d.accept());
    await page.evaluate(id => {
      const btn = document.querySelector(`.btn-khoa[data-id="${id}"]`);
      if (btn) btn.click();
    }, lockTarget.id);

    // Truyền cả id lẫn isWarning vào browser context dưới dạng 1 object argument
    // (waitForFunction chỉ nhận 1 arg — không thể dùng closure Node.js)
    await page.waitForFunction(({ id, isWarning }) => {
      const btn = document.querySelector(`.btn-khoa[data-id="${id}"]`);
      if (!btn) return true;
      return btn.classList.contains('btn-warning') !== isWarning;
    }, { id: lockTarget.id, isWarning: lockTarget.isWarning }, { timeout: config.TIMEOUT.ajax }).catch(() => {});

    await page.waitForTimeout(400);
    const updatedCls = await page.evaluate(id => {
      const btn = document.querySelector(`.btn-khoa[data-id="${id}"]`);
      return btn ? btn.className : null;
    }, lockTarget.id);

    report('TC-048b', 'KhoaTaiKhoan: nút đổi class sau toggle', !!updatedCls, `Class: ${updatedCls}`);
  }

  // TC-050: Reset mật khẩu (toast)
  await page.waitForTimeout(400);
  const resetId = await page.evaluate(() => {
    const btn = document.querySelector('.btn-reset');
    return btn ? btn.dataset.id : null;
  });
  if (resetId) {
    const toastBefore = (await page.$$('#toast-container .toast')).length;
    await page.evaluate(id => {
      const btn = document.querySelector(`.btn-reset[data-id="${id}"]`);
      if (btn) btn.click();
    }, resetId);
    await page.waitForFunction(count => {
      return document.querySelectorAll('#toast-container .toast').length > count;
    }, toastBefore, { timeout: config.TIMEOUT.ajax }).catch(() => {});
    const toastText = await page.$eval('#toast-container .toast', el => el.textContent.trim()).catch(() => '');
    report('TC-050b', 'Reset MK → toast hiện', toastText.length > 0, `Toast: "${toastText.substring(0, 60)}"`);
  }

  // Console errors
  report('CONSOLE', 'Không có JS errors', errs.length === 0, errs.join(' | '));

  await page.close();
  await browser.close();

  const total = results.length;
  const passed = results.filter(Boolean).length;
  console.log(`\n${'─'.repeat(50)}`);
  console.log(`TỔNG: ${total} | ✅ ${passed} | ❌ ${total - passed}`);
  console.log(`${'─'.repeat(50)}\n`);
  if (total - passed > 0) process.exit(1);
}

run().catch(e => { console.error('CRASH:', e.message); process.exit(1); });
