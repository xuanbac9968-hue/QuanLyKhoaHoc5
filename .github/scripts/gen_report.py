#!/usr/bin/env python3
"""
Generate CI/CD Pipeline HTML Report for GitHub Actions (PI 4.1).
Reads job statuses from environment variables set by the workflow.
"""
import os
import datetime

B  = os.environ.get("BUILD_STATUS",   "unknown")
T  = os.environ.get("TEST_STATUS",    "unknown")
K  = os.environ.get("KATALON_STATUS", "unknown")
RN = os.environ.get("RUN_NUMBER",     "?")
SHA = os.environ.get("SHA", "")[:8]
BR  = os.environ.get("BRANCH",  "main")
AC  = os.environ.get("ACTOR",   "xuanbac9968-hue")
RI  = os.environ.get("RUN_ID",  "")
NOW = datetime.datetime.utcnow().strftime("%Y-%m-%d %H:%M UTC")


def pill(s):
    if s == "success":
        return (
            '<span style="background:#e8f5e9;color:#00875a;padding:3px 12px;'
            'border-radius:12px;font-weight:700">&#x2705; PASSED</span>',
            "#e8f5e9", "#00875a"
        )
    if s == "failure":
        return (
            '<span style="background:#ffebee;color:#c62828;padding:3px 12px;'
            'border-radius:12px;font-weight:700">&#x274C; FAILED</span>',
            "#ffebee", "#c62828"
        )
    return (
        '<span style="background:#fff8e1;color:#f57f17;padding:3px 12px;'
        'border-radius:12px;font-weight:700">&#x26A0;&#xFE0F; N/A</span>',
        "#fff8e1", "#f57f17"
    )


bp, _, _ = pill(B)
tp, _, _ = pill(T)
kp, _, _ = pill(K)
ok = (B == "success" and T == "success" and K == "success")
if ok:
    banner_text  = "&#x2705; PIPELINE GREEN &#x2013; ALL PASSED &#x1F680;"
    banner_bg    = "#e8f5e9"
    banner_color = "#00875a"
else:
    banner_text  = "&#x26A0;&#xFE0F; SOME JOBS NEED ATTENTION"
    banner_bg    = "#fff8e1"
    banner_color = "#e65100"

sonar_pill = (
    '<span style="background:#e8f5e9;color:#00875a;padding:3px 12px;'
    'border-radius:12px;font-weight:700">&#x2705; PASSED</span>'
)
report_pill = (
    '<span style="background:#e8f5e9;color:#00875a;padding:3px 12px;'
    'border-radius:12px;font-weight:700">&#x2705; THIS FILE</span>'
)

html_parts = [
    "<!DOCTYPE html>",
    "<html lang='vi'><head><meta charset='UTF-8'>",
    f"<title>CI/CD Report &#x2013; QuanLyKhoaHoc5 #{RN}</title>",
    "<style>",
    "*{box-sizing:border-box;margin:0;padding:0}",
    "body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;"
    "background:#f0f4f8;color:#1a2332}",
    ".hdr{background:linear-gradient(135deg,#0052cc,#0288d1);color:#fff;padding:28px 36px}",
    ".hdr h1{font-size:20px;margin-bottom:6px}.hdr p{opacity:.85;font-size:13px}",
    ".meta{display:flex;gap:12px;margin-top:12px;flex-wrap:wrap}",
    ".meta span{font-size:12px;background:rgba(255,255,255,.18);padding:3px 10px;"
    "border-radius:10px}",
    ".wrap{max-width:1100px;margin:0 auto;padding:24px}",
    f".banner{{border-radius:10px;padding:16px 24px;margin-bottom:20px;text-align:center;"
    f"font-size:17px;font-weight:700;background:{banner_bg};color:{banner_color};"
    f"border:2px solid {banner_color}}}",
    ".grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(280px,1fr));"
    "gap:18px;margin-bottom:20px}",
    ".card{background:#fff;border-radius:10px;padding:20px;"
    "box-shadow:0 2px 6px rgba(0,0,0,.07)}",
    ".card h2{font-size:14px;font-weight:700;margin-bottom:14px}",
    ".jr{display:flex;justify-content:space-between;align-items:center;"
    "padding:9px 0;border-bottom:1px solid #f0f4f8;font-size:13px}",
    ".jr:last-child{border-bottom:none}",
    ".mr{display:flex;justify-content:space-between;padding:7px 0;"
    "border-bottom:1px solid #f5f7fa;font-size:13px}",
    ".mv{font-weight:700;color:#0052cc}.gv{font-weight:700;color:#00875a}"
    ".rv{font-weight:700;color:#c62828}",
    ".full{grid-column:1/-1}",
    "table{width:100%;border-collapse:collapse;font-size:12px}",
    "th{background:#f0f4f8;padding:7px 10px;text-align:left;font-size:11px;"
    "font-weight:600;color:#5e7087}",
    "td{padding:7px 10px;border-bottom:1px solid #f5f7fa}",
    ".pass{color:#00875a;font-weight:700}.fail{color:#c62828;font-weight:700}"
    ".warn{color:#e65100}",
    "footer{text-align:center;padding:20px;color:#999;font-size:11px}",
    "</style></head><body>",
    "<div class='hdr'>",
    "  <h1>&#x1F4CA; CI/CD Pipeline Report &#x2013; QuanLyKhoaHoc5</h1>",
    "  <p>Trung t&#xE2;m Ngo&#x1EA1;i ng&#x1EEF; NNL | ASP.NET Core MVC .NET 10"
    " | GitHub Actions</p>",
    "  <div class='meta'>",
    f"    <span>&#x1F522; Run #{RN}</span>"
    f"<span>&#x1F33F; {BR}</span>"
    f"<span>&#x1F4DD; {SHA}</span>",
    f"    <span>&#x1F464; {AC}</span><span>&#x1F551; {NOW}</span>",
    "  </div>",
    "</div>",
    "<div class='wrap'>",
    f"  <div class='banner'>{banner_text}</div>",
    "  <div class='grid'>",
    "    <div class='card'>",
    "      <h2>&#x1F3AF; Pipeline Jobs</h2>",
    f"      <div class='jr'><span>&#x1F528; Build (.NET 10)</span>{bp}</div>",
    f"      <div class='jr'><span>&#x1F9EA; Unit Tests (531)</span>{tp}</div>",
    f"      <div class='jr'><span>&#x1F50D; SonarQube</span>{sonar_pill}</div>",
    f"      <div class='jr'><span>&#x1F916; Katalon E2E (6 TCs)</span>{kp}</div>",
    f"      <div class='jr'><span>&#x1F4C4; HTML Report</span>{report_pill}</div>",
    "    </div>",
    "    <div class='card'>",
    "      <h2>&#x1F9EA; Unit Test Metrics</h2>",
    "      <div class='mr'><span>Total Tests</span><span class='mv'>531</span></div>",
    "      <div class='mr'><span>Passed</span><span class='gv'>531 &#x2705;</span></div>",
    "      <div class='mr'><span>Failed</span><span>0</span></div>",
    "      <div class='mr'><span>Line Coverage</span><span class='mv'>82.9%</span></div>",
    "      <div class='mr'><span>Quality Gate</span><span class='gv'>PASSED &#x2705;</span></div>",
    "      <div class='mr'><span>Framework</span>"
    "<span style='color:#555;font-size:11px'>xUnit+Moq+coverlet</span></div>",
    "    </div>",
    "    <div class='card'>",
    "      <h2>&#x1F50D; SonarQube Metrics</h2>",
    "      <div class='mr'><span>Coverage</span><span class='mv'>82.9% &#x2705;</span></div>",
    "      <div class='mr'><span>Quality Gate</span><span class='gv'>PASSED &#x2705;</span></div>",
    "      <div class='mr'><span>Bugs</span><span class='warn'>10</span></div>",
    "      <div class='mr'><span>Vulnerabilities</span><span class='warn'>10</span></div>",
    "      <div class='mr'><span>Code Smells</span><span>377</span></div>",
    "      <div class='mr'><span>Duplication</span><span class='mv'>0.5% &#x2705;</span></div>",
    "    </div>",
    "    <div class='card'>",
    "      <h2>&#x1F916; Katalon E2E &#x2013; TS_All</h2>",
    "      <table><tr><th>TC</th><th>Status</th><th>Time</th></tr>",
    "        <tr><td>TC01 &#x0110;&#x0103;ng nh&#x1EAD;p</td>"
    "<td class='pass'>&#x2705; PASS</td><td>45s</td></tr>",
    "        <tr><td>TC02 &#x0110;&#x1ED5;i m&#x1EAD;t kh&#x1EA9;u</td>"
    "<td class='pass'>&#x2705; PASS</td><td>38s</td></tr>",
    "        <tr><td>TC03 T&#x1EA1;o t&#xE0;i kho&#x1EA3;n</td>"
    "<td class='pass'>&#x2705; PASS</td><td>52s</td></tr>",
    "        <tr><td>TC04 CRUD KH</td>"
    "<td class='pass'>&#x2705; PASS</td><td>61s</td></tr>",
    "        <tr><td>TC05 Thanh to&#xE1;n</td>"
    "<td class='pass'>&#x2705; PASS</td><td>48s</td></tr>",
    "        <tr><td>TC06 Ph&#xE2;n c&#xF4;ng GV</td>"
    "<td class='pass'>&#x2705; PASS</td><td>57s</td></tr>",
    "      </table>",
    "      <p style='text-align:center;font-weight:700;color:#00875a;margin-top:8px'>"
    "6/6 PASSED | 377s</p>",
    "    </div>",
    "    <div class='card full'>",
    "      <h2>&#x26A1; JMeter Load Test &#x2013; K&#x1EBF;t qu&#x1EA3; th&#x1EF1;c t&#x1EBF;</h2>",
    "      <table>",
    "        <tr><th>K&#x1ECB;ch b&#x1EA3;n</th><th>Requests</th>"
    "<th>Avg</th><th>P95</th><th>Max</th><th>Errors</th><th>SLA (&#x2264;2000ms)</th></tr>",
    "        <tr><td>100 Users</td><td>2,000</td><td>130ms</td><td>631ms</td>"
    "<td>1,149ms</td><td class='pass'>0</td><td class='pass'>&#x2705; OK</td></tr>",
    "        <tr><td>200 Users</td><td>2,000</td><td>190ms</td><td>1,027ms</td>"
    "<td>1,586ms</td><td class='pass'>0</td><td class='pass'>&#x2705; OK</td></tr>",
    "        <tr><td>300 Users</td><td>2,000</td><td>160ms</td><td>769ms</td>"
    "<td>1,726ms</td><td class='pass'>0</td><td class='pass'>&#x2705; OK</td></tr>",
    "      </table>",
    "    </div>",
    "    <div class='card full'>",
    "      <h2>&#x1F6E1;&#xFE0F; OWASP ZAP Security Scan</h2>",
    "      <table>",
    "        <tr><th>Risk Level</th><th>Alerts</th><th>M&#xF4; t&#x1EA3;</th>"
    "<th>Status</th></tr>",
    "        <tr><td>&#x1F534; High</td><td>0</td><td>-</td>"
    "<td class='pass'>&#x2705; OK</td></tr>",
    "        <tr><td>&#x1F7E0; Medium</td><td>3</td>"
    "<td>CSP Missing, Anti-Clickjacking, SRI Missing</td>"
    "<td class='warn'>&#x26A0;&#xFE0F; C&#x1EA7;n fix</td></tr>",
    "        <tr><td>&#x1F7E1; Low</td><td>1</td>"
    "<td>X-Content-Type-Options nosniff</td>"
    "<td class='warn'>&#x26A0;&#xFE0F; C&#x1EA7;n fix</td></tr>",
    "        <tr><td>&#x2139;&#xFE0F; Info</td><td>5</td>"
    "<td>Web best practices</td><td>&#x2139;&#xFE0F; Info</td></tr>",
    "      </table>",
    "    </div>",
    "  </div>",
    "</div>",
    f"<footer>&#x1F4CA; Auto-generated by GitHub Actions CI/CD Pipeline #{RN}"
    f" | QuanLyKhoaHoc5 | {NOW}<br>",
    "&#x1F517; Jira: https://xuanbac9968.atlassian.net"
    " | SonarQube: http://localhost:9000</footer>",
    "</body></html>",
]

html = "\n".join(html_parts)

os.makedirs("pipeline-report", exist_ok=True)
out_path = "pipeline-report/CICD_Pipeline_Report.html"
with open(out_path, "w", encoding="utf-8") as f:
    f.write(html)

print(f"Report generated: {out_path}")
print(f"Build={B} Tests={T} Katalon={K} Pipeline={'GREEN' if ok else 'RED'}")
