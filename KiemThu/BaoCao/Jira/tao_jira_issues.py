"""
tao_jira_issues.py
Kết nối Jira Cloud REST API v3 và tạo issues từ SonarQube bugs
Jira: https://xuanbac9968.atlassian.net | Project: NNL (QuanLyKhoaHoc5)
Email: xuanbac9968@gmail.com

Cách dùng:
  python tao_jira_issues.py                    # sẽ đọc token từ JIRA_TOKEN.txt hoặc biến môi trường
  python tao_jira_issues.py --token YOUR_TOKEN # truyền token trực tiếp
  python tao_jira_issues.py --list             # chỉ liệt kê issues hiện có, không tạo mới
  python tao_jira_issues.py --export-html      # xuất báo cáo HTML
"""

import requests
import json
import os
import sys
import argparse
from base64 import b64encode
from datetime import datetime

# ── CONFIG ────────────────────────────────────────────────────────────────────
JIRA_BASE    = "https://xuanbac9968.atlassian.net"
JIRA_EMAIL   = "xuanbac9968@gmail.com"
PROJECT_KEY  = "NNL"
ISSUE_TYPES  = {"Bug": "Bug", "Vulnerability": "Bug", "Code Smell": "Task"}
PRIORITY_MAP = {"High": "High", "Medium": "Medium", "Low": "Low"}

TOKEN_FILE   = os.path.join(os.path.dirname(__file__), "JIRA_TOKEN.txt")

# ── BUGS từ SonarQube 26.5.0 ─────────────────────────────────────────────────
SONAR_BUGS = [
    {
        "sonar_id": "NNL-001",
        "type": "Bug",
        "priority": "High",
        "severity": "CRITICAL",
        "summary": "[SonarQube] NullReferenceException khi cookie xác thực hết hạn giữa request API chat",
        "description": """*Phát hiện bởi:* SonarQube 26.5.0 | Rule: csharpsquid:S2259

*Component:* ChatController.cs:47

*Mô tả:*
ChatController.SendMessage() không kiểm tra null cho User.FindFirst(ClaimTypes.NameIdentifier) trước khi dùng giá trị.
Nếu cookie xác thực bị hết hạn giữa request, sẽ ném NullReferenceException và crash ứng dụng.

*Cách tái hiện:*
1. Đăng nhập và mở trang Chat
2. Để session hết hạn (xóa cookie)
3. Gửi một tin nhắn
4. Quan sát: HTTP 500 Internal Server Error

*Fix đề xuất:*
{{code:csharp}}
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
if (userId == null) return Unauthorized();
{{code}}

*Effort:* 30 phút | *Sprint:* Current""",
        "labels": ["sonarqube", "null-check", "security", "chat"],
        "component": "ChatController.cs"
    },
    {
        "sonar_id": "NNL-002",
        "type": "Bug",
        "priority": "High",
        "severity": "MAJOR",
        "summary": "[SonarQube] Potential SQL Injection qua tham số tìm kiếm khóa học chưa sanitize",
        "description": """*Phát hiện bởi:* SonarQube 26.5.0 | Rule: csharpsquid:S2078

*Component:* KhoaHocController.cs:89

*Mô tả:*
Tham số 'tuKhoa' từ query string có thể không được parameterized đúng cách trong một số nhánh code.

*Fix đề xuất:* Sử dụng LINQ thay vì string concatenation, ensure EF Core parameterizes all queries.

*Effort:* 1 giờ""",
        "labels": ["sonarqube", "sql-injection", "security"],
        "component": "KhoaHocController.cs"
    },
    {
        "sonar_id": "NNL-004",
        "type": "Vulnerability",
        "priority": "High",
        "severity": "CRITICAL",
        "summary": "[SonarQube] Missing CSRF protection (ValidateAntiForgeryToken) trên POST /ThanhToan/Duyet",
        "description": """*Phát hiện bởi:* SonarQube 26.5.0 | Rule: csharpsquid:S4502

*Component:* ThanhToanController.cs:143

*Mô tả:*
POST /ThanhToan/Duyet không có [ValidateAntiForgeryToken] attribute.
Kẻ tấn công có thể gửi request giả mạo để phê duyệt thanh toán.

*Fix:* Thêm [ValidateAntiForgeryToken] vào action Duyet()
*Effort:* 15 phút""",
        "labels": ["sonarqube", "csrf", "security", "vulnerability"],
        "component": "ThanhToanController.cs"
    },
    {
        "sonar_id": "NNL-005",
        "type": "Vulnerability",
        "priority": "High",
        "severity": "MAJOR",
        "summary": "[SonarQube] Hard-coded credentials trong SeedData.cs",
        "description": """*Phát hiện bởi:* SonarQube 26.5.0 | Rule: csharpsquid:S2068

*Component:* SeedData.cs:23

*Mô tả:*
Mật khẩu hard-coded 'Admin@123', 'Gv@123', 'Hv@123' trong source code.
Theo OWASP A07:2021 – Identification and Authentication Failures.

*Fix:* Đọc credentials từ environment variables hoặc User Secrets.
*Effort:* 1 giờ""",
        "labels": ["sonarqube", "hard-coded-credentials", "owasp"],
        "component": "SeedData.cs"
    },
    {
        "sonar_id": "NNL-009",
        "type": "Vulnerability",
        "priority": "Medium",
        "severity": "MAJOR",
        "summary": "[ZAP/SonarQube] Missing Content-Security-Policy header – nguy cơ XSS",
        "description": """*Phát hiện bởi:* SonarQube 26.5.0 + OWASP ZAP | Rule: csharpsquid:S5122

*Component:* Program.cs:45

*Mô tả:*
Ứng dụng không set Content-Security-Policy header.
ZAP Active Scan: Medium Risk alert.

*Fix:*
{{code:csharp}}
app.Use(async (context, next) => {
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline' cdn.jsdelivr.net");
    await next();
});
{{code}}
*Effort:* 1 giờ""",
        "labels": ["sonarqube", "zap", "csp", "xss", "headers"],
        "component": "Program.cs"
    },
    {
        "sonar_id": "NNL-011",
        "type": "Vulnerability",
        "priority": "High",
        "severity": "CRITICAL",
        "summary": "[SonarQube] File upload chỉ check extension – bypass bằng rename malware",
        "description": """*Phát hiện bởi:* SonarQube 26.5.0 | Rule: csharpsquid:S2083

*Component:* ThanhToanController.cs:198

*Mô tả:*
ThanhToanController.TaoYeuCau() chỉ kiểm tra extension file (.jpg, .png) nhưng không verify MIME type thực.
Kẻ tấn công có thể rename file .exe thành .jpg để upload.

*Fix:* Đọc magic bytes để verify MIME type thực sự.
*Effort:* 2 giờ""",
        "labels": ["sonarqube", "file-upload", "security", "mime-type"],
        "component": "ThanhToanController.cs"
    },
    {
        "sonar_id": "NNL-014",
        "type": "Vulnerability",
        "priority": "Medium",
        "severity": "MAJOR",
        "summary": "[ZAP] Anti-clickjacking header (X-Frame-Options) chưa được set",
        "description": """*Phát hiện bởi:* OWASP ZAP 2.16.0 + SonarQube

*Component:* Program.cs:45

*Mô tả:*
Response headers không có X-Frame-Options hoặc CSP frame-ancestors.
Ứng dụng có thể bị nhúng iframe (clickjacking attack).

*Fix:* context.Response.Headers.Add("X-Frame-Options", "DENY");
*Effort:* 15 phút""",
        "labels": ["zap", "clickjacking", "headers", "security"],
        "component": "Program.cs"
    },
    {
        "sonar_id": "NNL-018",
        "type": "Vulnerability",
        "priority": "Medium",
        "severity": "MAJOR",
        "summary": "[ZAP] CDN scripts thiếu Subresource Integrity (SRI)",
        "description": """*Phát hiện bởi:* OWASP ZAP 2.16.0

*Component:* Views/Shared/_Layout.cshtml:12

*Mô tả:*
Bootstrap, Chart.js, jQuery từ CDN không có integrity và crossorigin attributes.
Nếu CDN bị compromise, kẻ tấn công inject malicious code.

*Fix:* Thêm integrity hash cho mỗi CDN resource.
*Effort:* 2 giờ""",
        "labels": ["zap", "sri", "cdn", "xss"],
        "component": "_Layout.cshtml"
    },
]


def get_token(args):
    """Lấy Jira API token từ args, env, hoặc file."""
    if hasattr(args, 'token') and args.token:
        return args.token
    token = os.environ.get("JIRA_TOKEN")
    if token:
        print(f"✅ Dùng token từ biến môi trường JIRA_TOKEN")
        return token
    if os.path.exists(TOKEN_FILE):
        token = open(TOKEN_FILE).read().strip()
        if token and token != "YOUR_JIRA_API_TOKEN_HERE":
            print(f"✅ Dùng token từ {TOKEN_FILE}")
            return token
    print("\n" + "="*60)
    print("⚠️  CHƯA CÓ JIRA API TOKEN")
    print("="*60)
    print("Bước 1: Tạo API token tại:")
    print("  https://id.atlassian.com/manage-profile/security/api-tokens")
    print("Bước 2: Lưu vào file JIRA_TOKEN.txt (cùng thư mục)")
    print("   HOẶC set biến môi trường: set JIRA_TOKEN=your_token_here")
    print("Bước 3: Chạy lại script này")
    print("="*60)
    token = input("\nNhập API token bây giờ (Enter để bỏ qua): ").strip()
    if token:
        with open(TOKEN_FILE, "w") as f:
            f.write(token)
        print(f"✅ Token đã lưu vào {TOKEN_FILE}")
        return token
    return None


def get_auth_header(token):
    creds = f"{JIRA_EMAIL}:{token}"
    b64 = b64encode(creds.encode()).decode()
    return {"Authorization": f"Basic {b64}", "Content-Type": "application/json", "Accept": "application/json"}


def test_connection(headers):
    """Test kết nối Jira."""
    try:
        r = requests.get(f"{JIRA_BASE}/rest/api/3/myself", headers=headers, timeout=10)
        if r.status_code == 200:
            user = r.json()
            print(f"✅ Kết nối Jira thành công!")
            print(f"   User: {user.get('displayName')} ({user.get('emailAddress')})")
            return True
        else:
            print(f"❌ Lỗi xác thực: HTTP {r.status_code}")
            if r.status_code == 401:
                print("   Token không hợp lệ hoặc hết hạn.")
            return False
    except Exception as e:
        print(f"❌ Không thể kết nối Jira: {e}")
        return False


def get_or_create_project(headers):
    """Kiểm tra project NNL tồn tại."""
    r = requests.get(f"{JIRA_BASE}/rest/api/3/project/{PROJECT_KEY}", headers=headers, timeout=10)
    if r.status_code == 200:
        proj = r.json()
        print(f"✅ Project: {proj['name']} ({proj['key']})")
        return proj
    elif r.status_code == 404:
        print(f"⚠️  Project '{PROJECT_KEY}' chưa tồn tại → tạo mới...")
        return create_project(headers)
    else:
        print(f"❌ Lỗi kiểm tra project: {r.status_code} – {r.text}")
        return None


def create_project(headers):
    """Tạo Jira project mới."""
    # Lấy account ID của current user
    me = requests.get(f"{JIRA_BASE}/rest/api/3/myself", headers=headers, timeout=10).json()
    account_id = me.get("accountId")

    payload = {
        "key": PROJECT_KEY,
        "name": "QuanLyKhoaHoc5 – NNL Language Center",
        "description": "Hệ thống quản lý khóa học trung tâm ngoại ngữ NNL (ASP.NET Core MVC)",
        "projectTypeKey": "software",
        "projectTemplateKey": "com.pyxis.greenhopper.jira:gh-scrum-template",
        "leadAccountId": account_id,
        "assigneeType": "PROJECT_LEAD"
    }
    r = requests.post(f"{JIRA_BASE}/rest/api/3/project", headers=headers, json=payload, timeout=15)
    if r.status_code in (200, 201):
        proj = r.json()
        print(f"✅ Đã tạo project: {proj['name']} (key: {proj['key']})")
        return proj
    else:
        print(f"❌ Không thể tạo project: {r.status_code}")
        print(f"   Response: {r.text[:300]}")
        return None


def create_issue(headers, bug):
    """Tạo một Jira issue từ bug data."""
    issue_type = ISSUE_TYPES.get(bug["type"], "Bug")
    priority = PRIORITY_MAP.get(bug["priority"], "Medium")

    payload = {
        "fields": {
            "project": {"key": PROJECT_KEY},
            "issuetype": {"name": issue_type},
            "priority": {"name": priority},
            "summary": bug["summary"],
            "description": {
                "type": "doc",
                "version": 1,
                "content": [
                    {
                        "type": "paragraph",
                        "content": [{"type": "text", "text": bug["description"]}]
                    }
                ]
            },
            "labels": bug.get("labels", []),
        }
    }

    r = requests.post(f"{JIRA_BASE}/rest/api/3/issue", headers=headers, json=payload, timeout=15)

    if r.status_code in (200, 201):
        issue = r.json()
        print(f"  ✅ Tạo thành công: {issue['key']} – {bug['summary'][:60]}...")
        return issue
    else:
        print(f"  ❌ Lỗi tạo issue '{bug['sonar_id']}': HTTP {r.status_code}")
        print(f"     {r.text[:200]}")
        return None


def list_issues(headers):
    """Liệt kê tất cả issues trong project."""
    jql = f"project = {PROJECT_KEY} ORDER BY created DESC"
    r = requests.get(f"{JIRA_BASE}/rest/api/3/search",
                     headers=headers,
                     params={"jql": jql, "maxResults": 50, "fields": "summary,status,priority,issuetype"},
                     timeout=15)
    if r.status_code == 200:
        data = r.json()
        issues = data.get("issues", [])
        print(f"\n📋 Tổng số issues trong project {PROJECT_KEY}: {data.get('total', 0)}")
        for issue in issues:
            f = issue["fields"]
            print(f"  [{issue['key']}] [{f['issuetype']['name']}] [{f['priority']['name']}] [{f['status']['name']}] {f['summary'][:60]}")
        return issues
    else:
        print(f"❌ Không thể lấy issues: {r.status_code}")
        return []


def export_html_report(issues_created, output_dir):
    """Xuất báo cáo HTML của issues đã tạo."""
    html_file = os.path.join(output_dir, "jira_export_report.html")
    rows = ""
    for i in issues_created:
        if i and "key" in i:
            rows += f"<tr><td><a href='{JIRA_BASE}/browse/{i['key']}' target='_blank'>{i['key']}</a></td><td>Bug</td><td>Tạo mới</td></tr>\n"

    html = f"""<!DOCTYPE html><html><head><meta charset="UTF-8"><title>Jira Export</title>
<style>body{{font-family:Arial,sans-serif;padding:20px}}table{{border-collapse:collapse;width:100%}}
th,td{{border:1px solid #ddd;padding:8px;text-align:left}}th{{background:#0052cc;color:#fff}}
</style></head><body>
<h1>Jira Issues Export – QuanLyKhoaHoc5</h1>
<p>Exported: {datetime.now().strftime('%Y-%m-%d %H:%M')} | Project: {PROJECT_KEY} | {JIRA_BASE}</p>
<table><thead><tr><th>Issue Key</th><th>Type</th><th>Action</th></tr></thead>
<tbody>{rows}</tbody></table>
</body></html>"""
    with open(html_file, "w", encoding="utf-8") as f:
        f.write(html)
    print(f"✅ Xuất HTML report: {html_file}")


def main():
    parser = argparse.ArgumentParser(description="Tạo Jira issues từ SonarQube bugs")
    parser.add_argument("--token", help="Jira API token")
    parser.add_argument("--list", action="store_true", help="Chỉ liệt kê issues")
    parser.add_argument("--export-html", action="store_true", help="Xuất HTML report")
    parser.add_argument("--dry-run", action="store_true", help="Không tạo thật, chỉ hiển thị")
    args = parser.parse_args()

    print("\n" + "="*60)
    print("  🔗 JIRA INTEGRATION – QuanLyKhoaHoc5")
    print(f"  URL: {JIRA_BASE}")
    print(f"  Email: {JIRA_EMAIL}")
    print(f"  Project: {PROJECT_KEY}")
    print("="*60)

    token = get_token(args)
    if not token:
        print("\n❌ Không có token. Thoát.")
        sys.exit(1)

    headers = get_auth_header(token)

    # Test connection
    if not test_connection(headers):
        sys.exit(1)

    if args.list:
        list_issues(headers)
        return

    # Check/create project
    proj = get_or_create_project(headers)

    if args.dry_run:
        print("\n🔍 DRY RUN – Danh sách issues sẽ được tạo:")
        for bug in SONAR_BUGS:
            print(f"  [{bug['type']}] [{bug['priority']}] {bug['summary'][:70]}")
        return

    # Create issues
    print(f"\n📝 Tạo {len(SONAR_BUGS)} issues từ SonarQube...")
    created = []
    for bug in SONAR_BUGS:
        issue = create_issue(headers, bug)
        if issue:
            created.append(issue)

    print(f"\n✅ Đã tạo {len(created)}/{len(SONAR_BUGS)} issues thành công!")
    print(f"   Xem tại: {JIRA_BASE}/jira/software/projects/{PROJECT_KEY}/boards")

    if args.export_html or created:
        export_html_report(created, os.path.dirname(__file__))

    # List all issues
    print("\n📋 Danh sách issues sau khi tạo:")
    list_issues(headers)


if __name__ == "__main__":
    main()
