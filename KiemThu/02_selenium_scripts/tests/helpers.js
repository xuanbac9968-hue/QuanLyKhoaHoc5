/**
 * helpers.js – Các hàm tiện ích dùng chung cho tất cả test
 */
const { chromium } = require('playwright');
const config = require('../config');

/**
 * Khởi tạo browser + context mới
 */
async function createBrowser() {
  const browser = await chromium.launch(config.BROWSER);
  const ctx = await browser.newContext();
  return { browser, ctx };
}

/**
 * Đăng nhập với role cụ thể và trả về page đã auth
 */
async function loginAs(ctx, role = 'admin') {
  const user = config.USERS[role] || config.USERS.admin;
  const p = await ctx.newPage();
  await p.goto(config.BASE_URL + '/Account/Login', { waitUntil: 'domcontentloaded' });
  await p.fill('input[name="Email"]', user.email);
  await p.fill('input[name="MatKhau"]', user.password);
  await p.click('button[type="submit"]');
  await p.waitForLoadState('domcontentloaded');
  await p.close();
}

/**
 * POST qua fetch AJAX (lấy CSRF từ trang hiện tại)
 */
async function ajaxPost(page, url, body = {}) {
  const token = await page.$eval(
    'input[name="__RequestVerificationToken"]',
    el => el.value
  ).catch(() => '');
  body.__RequestVerificationToken = token;
  const resp = await page.evaluate(async ({ url, body }) => {
    const r = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: new URLSearchParams(body)
    });
    return { status: r.status, json: await r.json().catch(() => null) };
  }, { url, body });
  return resp;
}

/**
 * Ghi kết quả test ra console
 */
function report(id, name, passed, detail = '') {
  const icon = passed ? '✅' : '❌';
  const line = `[${id}] ${icon} ${name}${detail ? ' | ' + detail : ''}`;
  console.log(line);
  return passed;
}

/**
 * Tổng kết cuối mỗi file test
 */
function summary(results) {
  const total = results.length;
  const passed = results.filter(Boolean).length;
  const failed = total - passed;
  console.log(`\n${'─'.repeat(50)}`);
  console.log(`TỔNG: ${total} | ✅ ${passed} | ❌ ${failed}`);
  console.log(`${'─'.repeat(50)}\n`);
  return failed;
}

module.exports = { createBrowser, loginAs, ajaxPost, report, summary };
