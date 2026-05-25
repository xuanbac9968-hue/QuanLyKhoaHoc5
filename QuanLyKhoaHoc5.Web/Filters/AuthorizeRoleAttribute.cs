using Microsoft.AspNetCore.Authorization;

namespace QuanLyKhoaHoc5.Web.Filters;

public class AuthorizeRoleAttribute : AuthorizeAttribute
{
    public AuthorizeRoleAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
        AuthenticationSchemes = "CookieAuth";
    }
}
