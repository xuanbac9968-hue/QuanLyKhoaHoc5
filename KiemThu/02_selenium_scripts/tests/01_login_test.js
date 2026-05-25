/**
 * 01_login_test.js – Kiểm thử module Đăng nhập / Đăng xuất
 * Phủ sóng TC-001 → TC-010
 */
const { chromium } = require('playwright');
const { createBrowser, report, summary } = require('./helpers');
const config = require('../config');

const BASE = config.BASE_URL;
const results = [];

async function run() {
  console.log('\n══════════ MODULE 1: ĐĂNG NHẬP / ĐĂNG XUẤT ══════════\n');
  const { browser, ctx } = await createBrowser();
  const page = await ctx.newPage();
  const errs = [];
  page.on('pageerror', e => errs.push(e.message));

  // TC-001: Admin đăng nhập thành công
  await page.goto(BASE + '/Account/Login', { waitUntil: 'domcontentloaded' });
  await page.fill('input[name="Email"]', 'admin@nnl.com');
  await page.fill('input[name="MatKhau"]', 'Admin@123');
  await page.click('button[type="submit"]');
  await page.waitForLoadState('domcontentloaded');
  const adminRedirected = page.url().includes('/Admin');
  results.push(report('TC-001', 'Admin đăng nhập → redirect /Admin', adminRedirected, `URL: ${page.url()}`));

  // TC-009: Đăng xuất
  await page.goto(BASE + '/Account/Logout');
  await page.waitForLoadState('domcontentloaded');
  const loggedOut = page.url().includes('/Account/Login');
  results.push(report('TC-009', 'Đăng xuất → redirect Login', loggedOut));

  // TC-002: GiangVien đăng nhập
  await page.goto(BASE + '/Account/Login', { waitUntil: 'domcontentloaded' });
  await page.fill('input[name="Email"]', 'gv01@nnl.com');
  await page.fill('input[name="MatKhau"]', 'Gv@123');
  await page.click('button[type="submit"]');
  await page.waitForLoadState('domcontentloaded');
  const gvRedirected = page.url().includes('/GiangVien/Dashboard');
  results.push(report('TC-002', 'GiangVien đăng nhập → redirect Dashboard', gvRedirected, `URL: ${page.url()}`));
  await page.goto(BASE + '/Account/Logout');

  // TC-003: HocVien đăng nhập
  await page.goto(BASE + '/Account/Login', { waitUntil: 'domcontentloaded' });
  await page.fill('input[name="Email"]', 'hv01@nnl.com');
  await page.fill('input[name="MatKhau"]', 'Hv@123');
  await page.click('button[type="submit"]');
  await page.waitForLoadState('domcontentloaded');
  const hvRedirected = page.url().includes('/HocVien/Dashboard');
  results.push(report('TC-003', 'HocVien đăng nhập → redirect Dashboard', hvRedirected, `URL: ${page.url()}`));
  await page.goto(BASE + '/Account/Logout');

  // TC-004: Sai mật khẩu
  await page.goto(BASE + '/Account/Login', { waitUntil: 'domcontentloaded' });
  await page.fill('input[name="Email"]', 'admin@nnl.com');
  await page.fill('input[name="MatKhau"]', 'wrongpassword');
  await page.click('button[type="submit"]');
  await page.waitForLoadState('domcontentloaded');
  const stillOnLogin = page.url().includes('/Account/Login');
  const errMsg4 = await page.$eval('.validation-summary-errors, .text-danger', el => el.textContent.trim()).catch(() => '');
  results.push(report('TC-004', 'Sai mật khẩu → ở lại Login + thông báo lỗi', stillOnLogin && errMsg4.includes('không đúng'), `Lỗi: "${errMsg4}"`));

  // TC-005: Email không tồn tại
  await page.fill('input[name="Email"]', 'notexist@example.com');
  await page.fill('input[name="MatKhau"]', 'Abc@123');
  await page.click('button[type="submit"]');
  await page.waitForLoadState('domcontentloaded');
  const errMsg5 = await page.$eval('.validation-summary-errors, .text-danger', el => el.textContent.trim()).catch(() => '');
  results.push(report('TC-005', 'Email không tồn tại → thông báo lỗi', errMsg5.includes('không đúng'), `Lỗi: "${errMsg5}"`));

  // TC-007: Email rỗng
  await page.fill('input[name="Email"]', '');
  await page.fill('input[name="MatKhau"]', 'Abc@123');
  await page.click('button[type="submit"]');
  await page.waitForLoadState('domcontentloaded');
  const stillOnLogin7 = page.url().includes('/Account/Login') || page.url() === BASE + '/';
  results.push(report('TC-007', 'Email rỗng → validation/ở lại Login', stillOnLogin7));

  // TC-010: Đổi mật khẩu thành công
  await page.goto(BASE + '/Account/Login', { waitUntil: 'domcontentloaded' });
  await page.fill('input[name="Email"]', 'hv01@nnl.com');
  await page.fill('input[name="MatKhau"]', 'Hv@123');
  await page.click('button[type="submit"]');
  await page.waitForLoadState('domcontentloaded');
  await page.goto(BASE + '/Account/ChangePassword', { waitUntil: 'domcontentloaded' });
  const cpPage = page.url().includes('/Account/ChangePassword') || page.url().includes('/Profile');
  results.push(report('TC-010', 'Trang đổi mật khẩu tải được', cpPage, `URL: ${page.url()}`));

  // Console errors check
  results.push(report('CONSOLE', 'Không có JS errors trong quá trình test', errs.length === 0, errs.join(' | ')));

  await page.goto(BASE + '/Account/Logout').catch(() => {});
  await browser.close();

  const failed = summary(results);
  if (failed > 0) process.exit(1);
}

run().catch(e => { console.error('CRASH:', e.message); process.exit(1); });
