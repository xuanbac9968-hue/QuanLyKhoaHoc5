/**
 * config.js – Cấu hình chung cho toàn bộ Playwright test suite
 * QuanLyKhoaHoc5 Automated Test Suite v1.0
 */

module.exports = {
  BASE_URL: 'http://localhost:5299',

  // ── Tài khoản seed ─────────────────────────────────────────────────
  USERS: {
    admin:     { email: 'admin@nnl.com',  password: 'Admin@123', role: 'Admin' },
    giangVien: { email: 'gv01@nnl.com',   password: 'Gv@123',    role: 'GiangVien' },
    hocVien:   { email: 'hv01@nnl.com',   password: 'Hv@123',    role: 'HocVien' }
  },

  // ── Timeout chuẩn ──────────────────────────────────────────────────
  TIMEOUT: {
    navigation:  15000,
    element:      5000,
    ajax:         8000,
    modal:        3000
  },

  // ── Browser options ────────────────────────────────────────────────
  BROWSER: {
    headless: true,
    slowMo: 0   // tăng lên 100-200 để debug visual
  },

  // ── Màu sidebar mong đợi ───────────────────────────────────────────
  SIDEBAR_COLORS: {
    Admin:      'rgb(13, 27, 62)',    // navy blue
    GiangVien:  'rgb(21, 128, 61)',   // green
    HocVien:    'rgb(88, 28, 135)'    // purple
  },

  // ── Helpers ────────────────────────────────────────────────────────
  DEFAULT_PASSWORD: 'Abc@12345',
  TEST_PREFIX: `auto_${Date.now()}`
};
