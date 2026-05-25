using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;

namespace QuanLyKhoaHoc5.Tests.Helpers;

/// <summary>
/// Helper dùng để cấu hình ControllerContext giả (fake HttpContext, User, TempData)
/// cho unit test mà không cần khởi động toàn bộ ASP.NET Core pipeline.
/// </summary>
public static class ControllerHelper
{
    /// <summary>
    /// Tạo ControllerContext với HttpContext giả, có thể tùy chỉnh user và IAuthenticationService.
    /// Đăng ký đầy đủ các service mà Controller sử dụng nội bộ:
    ///   IAuthenticationService, ITempDataDictionaryFactory, IUrlHelperFactory.
    /// </summary>
    public static ControllerContext CreateContext(
        ClaimsPrincipal? user = null,
        Mock<IAuthenticationService>? authService = null)
    {
        // ── IAuthenticationService ──────────────────────────────────────────────
        var mockAuth = authService ?? new Mock<IAuthenticationService>();
        mockAuth
            .Setup(a => a.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string?>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties?>()))
            .Returns(Task.CompletedTask);
        mockAuth
            .Setup(a => a.SignOutAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string?>(),
                It.IsAny<AuthenticationProperties?>()))
            .Returns(Task.CompletedTask);

        // ── ITempDataDictionaryFactory ──────────────────────────────────────────
        // Controller.View() / Controller.TempData nội bộ yêu cầu factory này.
        var mockTempDataProvider = new Mock<ITempDataProvider>();
        mockTempDataProvider
            .Setup(p => p.LoadTempData(It.IsAny<HttpContext>()))
            .Returns(new Dictionary<string, object?>());
        var tempData = new TempDataDictionary(new DefaultHttpContext(), mockTempDataProvider.Object);

        var mockTempDataFactory = new Mock<ITempDataDictionaryFactory>();
        mockTempDataFactory
            .Setup(f => f.GetTempData(It.IsAny<HttpContext>()))
            .Returns(tempData);

        // ── IUrlHelperFactory ───────────────────────────────────────────────────
        // RedirectToAction / Url.IsLocalUrl yêu cầu IUrlHelperFactory.
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(u => u.IsLocalUrl(It.IsAny<string?>()))
            .Returns(false);
        mockUrlHelper
            .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
            .Returns("/");

        var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
        mockUrlHelperFactory
            .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(mockUrlHelper.Object);

        // ── IServiceProvider tổng hợp ───────────────────────────────────────────
        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(IAuthenticationService))).Returns(mockAuth.Object);
        sp.Setup(s => s.GetService(typeof(ITempDataDictionaryFactory))).Returns(mockTempDataFactory.Object);
        sp.Setup(s => s.GetService(typeof(IUrlHelperFactory))).Returns(mockUrlHelperFactory.Object);

        var httpContext = new DefaultHttpContext { RequestServices = sp.Object };
        if (user != null)
            httpContext.User = user;

        return new ControllerContext { HttpContext = httpContext };
    }

    /// <summary>
    /// Tạo ClaimsPrincipal giả đã xác thực với scheme "CookieAuth".
    /// </summary>
    public static ClaimsPrincipal CreateUser(int id, string email, string name, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "CookieAuth"));
    }

    /// <summary>
    /// Tạo TempDataDictionary rỗng (dùng mock provider) để test các action dùng TempData.
    /// </summary>
    public static TempDataDictionary CreateTempData()
    {
        var mockProvider = new Mock<ITempDataProvider>();
        mockProvider
            .Setup(p => p.LoadTempData(It.IsAny<HttpContext>()))
            .Returns(new Dictionary<string, object?>());
        return new TempDataDictionary(new DefaultHttpContext(), mockProvider.Object);
    }
}
