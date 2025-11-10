using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Text;

namespace checkpoint_web.Pages.Account
{
 [AllowAnonymous] // Allow everyone to debug
 public class CookieDiagnosticsModel : PageModel
 {
 private readonly ILogger<CookieDiagnosticsModel> _logger;
 public CookieDiagnosticsModel(ILogger<CookieDiagnosticsModel> logger) => _logger = logger;

 public string DiagnosticInfo { get; private set; } = string.Empty;
 public bool IsAuthenticated { get; private set; }
 public string? UserName { get; private set; }
 public Dictionary<string, string> CookieDetails { get; private set; } = new();

 public void OnGet()
 {
 var sb = new StringBuilder();

 IsAuthenticated = User?.Identity?.IsAuthenticated ?? false;
 UserName = User?.Identity?.Name;

 sb.AppendLine("=== CHECKPOINT COOKIE DIAGNOSTICS ===\n");
 sb.AppendLine($"?? Authenticated: {IsAuthenticated}");
 sb.AppendLine($"?? User: {UserName ?? "Anonymous"}");
 sb.AppendLine($"?? Server Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

 sb.AppendLine("--- ?? Request Cookies (Received from Browser) ---");
 if (Request.Cookies.Any())
 {
 foreach (var cookie in Request.Cookies)
 {
 var value = cookie.Value.Length > 80
 ? cookie.Value.Substring(0, 80) + "..."
 : cookie.Value;
 sb.AppendLine($"  • {cookie.Key}");
 sb.AppendLine($"    Value: {value}");
 sb.AppendLine($"    Length: {cookie.Value.Length} bytes");
 CookieDetails[cookie.Key] = $"{cookie.Value.Length} bytes";
 }
 }
 else
 {
 sb.AppendLine("  ??  No cookies found in request");
 }

 sb.AppendLine("\n--- ?? Response Set-Cookie Headers (Being Sent to Browser) ---");
 if (Response.Headers.ContainsKey("Set-Cookie"))
 {
 foreach (var header in Response.Headers["Set-Cookie"])
 {
 sb.AppendLine($"  • {header}");

 // Analyze the cookie
 if (header.ToString().Contains("expires="))
 {
 sb.AppendLine("    ??  WARNING: Cookie has 'expires' attribute (PERSISTENT)");
 }
 else if (header.ToString().Contains("max-age="))
 {
 sb.AppendLine("    ??  WARNING: Cookie has 'max-age' attribute (PERSISTENT)");
 }
 else
 {
 sb.AppendLine("    ? Cookie appears to be session-only (no expires/max-age)");
 }
 }
 }
 else
 {
 sb.AppendLine("  ??  No Set-Cookie headers in this response");
 }

 sb.AppendLine("\n--- ?? How to Verify in Browser ---");
 sb.AppendLine("1. Press F12 to open DevTools");
 sb.AppendLine("2. Go to 'Application' tab ? 'Cookies' ? 'https://localhost:7088'");
 sb.AppendLine("3. Look at 'Checkpoint.Auth' cookie:");
 sb.AppendLine(" ? CORRECT: Expires/Max-Age = 'Session'");
 sb.AppendLine("   ? WRONG: Expires/Max-Age = specific date/time");
 sb.AppendLine("\n--- ?? Test Procedure ---");
 sb.AppendLine("1. Login to the application");
 sb.AppendLine("2. Verify cookie in DevTools shows 'Session'");
 sb.AppendLine("3. Close ALL browser windows (not just tab)");
 sb.AppendLine("4. Reopen browser and go to https://localhost:7088");
 sb.AppendLine("5. You should be redirected to login (session expired)");

 DiagnosticInfo = sb.ToString();

 _logger.LogInformation("CookieDiagnostics: Auth={auth}, User={user}, CookieCount={count}",
 IsAuthenticated, UserName ?? "Anonymous", Request.Cookies.Count);
 }
 }
}
