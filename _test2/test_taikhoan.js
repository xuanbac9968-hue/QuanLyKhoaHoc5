const { chromium } = require('playwright');
const BASE = 'http://localhost:5299';

async function login(ctx) {
  const p = await ctx.newPage();
  await p.goto(BASE + '/Account/Login', { waitUntil: 'domcontentloaded' });
  await p.fill('input[name="Email"]', 'admin@nnl.com');
  await p.fill('input[name="MatKhau"]', 'Admin@123');
  await p.click('button[type="submit"]');
  await p.waitForLoadState('domcontentloaded');
  await p.close();
}

async function run() {
  const browser = await chromium.launch({ headless: true });
  const ctx     = await browser.newContext();
  await login(ctx);

  const page = await ctx.newPage();
  const errs = [];
  page.on('console',   m => { if (m.type() === 'error') errs.push('console: ' + m.text()); });
  page.on('pageerror', e => errs.push('JS error: ' + e.message));

  console.log('\n══════ TEST /TaiKhoan ══════\n');

  // ── 1. Page load ─────────────────────────────────────────────────
  await page.goto(BASE + '/TaiKhoan', { waitUntil: 'networkidle' });
  const title = await page.title();
  console.log(`[1] Page title: "${title}"`);

  // ── 2. Header nút "Tạo tài khoản mới" ────────────────────────────
  const createBtn = await page.$('button:has-text("Tạo tài khoản mới")');
  console.log('[2] Nút "Tạo tài khoản mới":', createBtn ? '✅ Có' : '❌ Thiếu');

  // ── 3. Bảng tài khoản ────────────────────────────────────────────
  const rows = await page.$$('#tk-tbody tr:not(.no-data-row)');
  console.log(`[3] Số dòng trong bảng: ${rows.length}`);

  // ── 4. Nút có text trong cột Thao tác ─────────────────────────────
  const btnKhoa    = await page.$('.btn-khoa');
  const btnReset   = await page.$('.btn-reset');
  const btnSuaVT   = await page.$('.btn-sua-vaitro');
  console.log('[4a] Nút Khóa/Mở khóa:', btnKhoa  ? '✅ Có' : '❌ Thiếu');
  console.log('[4b] Nút Reset MK:',      btnReset ? '✅ Có' : '❌ Thiếu');
  console.log('[4c] Nút Sửa vai trò:',   btnSuaVT ? '✅ Có' : '❌ Thiếu');

  if (btnKhoa) {
    const txt = (await btnKhoa.textContent()).trim();
    console.log(`     Text nút Khóa: "${txt}"`);
    const cls = await btnKhoa.getAttribute('class');
    const isWarningOrSuccess = cls.includes('btn-warning') || cls.includes('btn-success');
    console.log('     Class đúng (warning/success):', isWarningOrSuccess ? '✅' : '❌ ' + cls);
  }

  // ── 5. Badge "Bạn" cho tài khoản hiện tại ────────────────────────
  const selfBadge = await page.$('.badge.bg-info[title="Tài khoản đang đăng nhập"]');
  console.log('[5] Badge "Bạn" trên row chính mình:', selfBadge ? '✅ Có' : '❌ Thiếu');

  // ── 6. Self row KHÔNG có nút Khóa ─────────────────────────────────
  const selfRow = await page.evaluate(() => {
    const badge = document.querySelector('.badge.bg-info[title="Tài khoản đang đăng nhập"]');
    if (!badge) return null;
    const row = badge.closest('tr');
    return {
      hasKhoa: !!row.querySelector('.btn-khoa'),
      hasSuaVT: !!row.querySelector('.btn-sua-vaitro'),
      hasReset: !!row.querySelector('.btn-reset'),
    };
  });
  if (selfRow) {
    console.log('[6] Row của mình - Khóa bị ẩn:', !selfRow.hasKhoa ? '✅ Đúng' : '❌ Vẫn hiện');
    console.log('[6] Row của mình - Sửa vai trò ẩn:', !selfRow.hasSuaVT ? '✅ Đúng' : '❌ Vẫn hiện');
    console.log('[6] Row của mình - Reset vẫn có:', selfRow.hasReset ? '✅ Đúng' : '❌ Mất');
  }

  // ── 7. Mở modal Tạo tài khoản ────────────────────────────────────
  await page.click('button:has-text("Tạo tài khoản mới")');
  await page.waitForSelector('#modalCreateTK.show', { timeout: 3000 }).catch(() => {});
  const modalCreate = await page.$('#modalCreateTK');
  const isVisible   = await modalCreate?.evaluate(el => window.getComputedStyle(el).display !== 'none').catch(() => false);
  console.log('[7] Modal Tạo tài khoản mở được:', isVisible ? '✅' : '❌');

  // Check form fields
  const fields = ['cr-hoTen', 'cr-email', 'cr-vaiTro', 'cr-matKhau', 'cr-xacNhan'];
  const fieldOk = (await Promise.all(fields.map(f => page.$(`#${f}`)))).every(Boolean);
  console.log('[7] Tất cả 5 field trong modal:', fieldOk ? '✅' : '❌');

  // ── 8. AJAX tạo tài khoản mới ────────────────────────────────────
  await page.fill('#cr-hoTen', 'Test User Playwright');
  await page.fill('#cr-email', `test.pw.${Date.now()}@example.com`);
  await page.selectOption('#cr-vaiTro', 'HocVien');
  await page.fill('#cr-matKhau', 'Test@123');
  await page.fill('#cr-xacNhan', 'Test@123');

  const [resp] = await Promise.all([
    page.waitForResponse(r => r.url().includes('/TaiKhoan/Create') && r.request().method() === 'POST', { timeout: 8000 }),
    page.click('#btn-create-submit')
  ]);
  const json = await resp.json().catch(() => null);
  console.log('[8] POST /TaiKhoan/Create response:', json?.success ? '✅ success=true' : '❌ ' + JSON.stringify(json));

  // New row should appear
  await page.waitForTimeout(800);
  const newRow = await page.$('tr:has(td:has-text("Test User Playwright"))');
  console.log('[8] Dòng mới hiện trong bảng (không reload):', newRow ? '✅' : '❌');

  const newCount = await page.$$('#tk-tbody tr:not(.no-data-row)');
  console.log(`[8] Số dòng sau khi thêm: ${newCount.length} (trước: ${rows.length})`);

  // ── 9. Mở modal Sửa vai trò ───────────────────────────────────────
  // Close create modal first
  await page.keyboard.press('Escape');
  await page.waitForTimeout(400);

  const svtBtn2 = await page.$('.btn-sua-vaitro');
  if (svtBtn2) {
    await svtBtn2.click();
    await page.waitForSelector('#modalSuaVaiTro.show', { timeout: 3000 }).catch(() => {});
    const svtModal = await page.$('#modalSuaVaiTro');
    const svtVis   = await svtModal?.evaluate(el => window.getComputedStyle(el).display !== 'none').catch(() => false);
    console.log('[9] Modal Sửa vai trò mở được:', svtVis ? '✅' : '❌');
    const svtHoTen = await page.$eval('#svt-hoten', el => el.textContent.trim()).catch(() => '');
    console.log(`[9] Tên tài khoản trong modal: "${svtHoTen}" ${svtHoTen ? '✅' : '❌'}`);
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);
  }

  // ── 10. AJAX Khóa tài khoản ───────────────────────────────────────
  // Find any non-self lock btn (use evaluate to get id directly)
  const lockTarget = await page.evaluate(() => {
    const btn = document.querySelector('.btn-khoa.btn-warning, .btn-khoa.btn-success');
    if (!btn) return null;
    return { id: btn.dataset.id, isWarning: btn.classList.contains('btn-warning') };
  });

  if (lockTarget) {
    // Accept confirm dialogs automatically for this test
    page.on('dialog', d => d.accept());

    // click via evaluate to avoid stale element
    await page.evaluate((id) => {
      const btn = document.querySelector(`.btn-khoa[data-id="${id}"]`);
      if (btn) btn.click();
    }, lockTarget.id);

    // Wait for DOM to update (button class changes after AJAX)
    const toggled = await page.waitForFunction((id) => {
      const btn = document.querySelector(`.btn-khoa[data-id="${id}"]`);
      if (!btn) return true; // removed = also fine
      return btn.classList.contains('btn-warning') !== true ||
             btn.classList.contains('btn-success');
    }, lockTarget.id, { timeout: 8000 }).catch(() => null);

    await page.waitForTimeout(500);
    const updatedInfo = await page.evaluate((id) => {
      const btn = document.querySelector(`.btn-khoa[data-id="${id}"]`);
      if (!btn) return null;
      return { cls: btn.className, txt: btn.textContent.trim() };
    }, lockTarget.id);

    if (updatedInfo) {
      const togOk = lockTarget.isWarning
        ? updatedInfo.cls.includes('btn-success')   // was Hoat dong → now Khoa → success btn
        : updatedInfo.cls.includes('btn-warning');  // was Khoa → now Mo → warning btn
      console.log('[10] KhoaTaiKhoan toggle:', togOk ? '✅ Nút đã đổi class' : '⚠️ Chưa đổi - ' + updatedInfo.cls);
      console.log(`     Text: "${updatedInfo.txt}" | Class: ${updatedInfo.cls.includes('btn-success') ? 'success' : 'warning'}`);
    } else {
      console.log('[10] KhoaTaiKhoan: ✅ (nút đã thay đổi)');
    }
  } else {
    console.log('[10] KhoaTaiKhoan: ⚠️ Không tìm thấy nút để test');
  }

  // ── 11. AJAX Reset mật khẩu ──────────────────────────────────────
  await page.waitForTimeout(400);
  const resetId = await page.evaluate(() => {
    const btn = document.querySelector('.btn-reset');
    return btn ? btn.dataset.id : null;
  });
  if (resetId) {
    const toastBefore = await page.$$('#toast-container .toast');
    await page.evaluate((id) => {
      const btn = document.querySelector(`.btn-reset[data-id="${id}"]`);
      if (btn) btn.click();
    }, resetId);
    // Wait for new toast to appear (indicates AJAX complete)
    const toastAppeared = await page.waitForFunction((prevCount) => {
      return document.querySelectorAll('#toast-container .toast').length > prevCount;
    }, toastBefore.length, { timeout: 8000 }).catch(() => null);
    await page.waitForTimeout(300);
    const toastText = await page.$eval('#toast-container .toast', el => el.textContent.trim()).catch(() => '');
    console.log('[11] POST ResetMatKhau:', toastText ? '✅ Toast: "' + toastText.substring(0,60) + '"' : '⚠️ Chưa thấy toast');
  }

  // ── 12. Console errors ────────────────────────────────────────────
  console.log('\n[12] Console errors:', errs.length === 0 ? '✅ Không có' : '❌ ' + errs.join(' | '));

  await page.close();
  await browser.close();
  console.log('\n══════ DONE ══════\n');
}

run().catch(e => { console.error('TEST CRASH:', e.message); process.exit(1); });
