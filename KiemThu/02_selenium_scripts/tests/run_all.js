/**
 * run_all.js – Chạy toàn bộ test suite tuần tự
 * Usage: node tests/run_all.js
 */
const { execSync } = require('child_process');
const path = require('path');

const tests = [
  '01_login_test.js',
  '02_khoa_hoc_test.js',
  '03_lop_hoc_test.js',
  '04_dang_ky_test.js',
  '05_diem_test.js',
  '06_thanh_toan_test.js',
  '07_tai_khoan_test.js',
  '08_phan_cong_test.js'
];

let totalFailed = 0;
const startTime = Date.now();

console.log('╔══════════════════════════════════════════════════════════╗');
console.log('║   QuanLyKhoaHoc5 – Playwright Automated Test Suite       ║');
console.log(`║   Started: ${new Date().toLocaleString('vi-VN')}                         ║`);
console.log('╚══════════════════════════════════════════════════════════╝\n');

for (const test of tests) {
  const testPath = path.join(__dirname, test);
  console.log(`\n▶ Running: ${test}`);
  try {
    execSync(`node "${testPath}"`, { stdio: 'inherit', timeout: 120000 });
    console.log(`✅ PASSED: ${test}`);
  } catch (err) {
    console.error(`❌ FAILED: ${test} (exit code ${err.status})`);
    totalFailed++;
  }
}

const elapsed = ((Date.now() - startTime) / 1000).toFixed(1);
console.log('\n╔══════════════════════════════════════════════════════════╗');
console.log(`║  FINAL RESULT: ${totalFailed === 0 ? '✅ ALL PASSED' : `❌ ${totalFailed} file(s) FAILED`}${''.padEnd(34 - (totalFailed === 0 ? 13 : 10) - String(totalFailed).length)}║`);
console.log(`║  Total time: ${elapsed}s${''.padEnd(44 - elapsed.length)}║`);
console.log('╚══════════════════════════════════════════════════════════╝\n');

process.exit(totalFailed > 0 ? 1 : 0);
