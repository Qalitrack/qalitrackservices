using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace Qalitrack.Services;

public static class PrivilegeChecker
{
    /// <summary>
    /// Checks if the application is running with administrator/root privileges
    /// </summary>
    public static bool IsRunningAsAdministrator()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return IsWindowsAdministrator();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
                 RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return IsUnixRoot();
        }
        
        return false;
    }

    /// <summary>
    /// Checks if running as Windows Administrator
    /// </summary>
    [SupportedOSPlatform("windows")]
    private static bool IsWindowsAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if running as Unix root (UID = 0)
    /// </summary>
    private static bool IsUnixRoot()
    {
        try
        {
            // On Unix systems, root user has UID 0
            var uid = GetUserId();
            return uid == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the current Unix user ID
    /// </summary>
    private static int GetUserId()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return (int)Syscall.getuid();
        }
        return -1;
    }

    /// <summary>
    /// Gets a user-friendly message about current privilege level
    /// </summary>
    public static string GetPrivilegeStatus()
    {
        var isAdmin = IsRunningAsAdministrator();
        var platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : "Linux/Unix";
        
        if (isAdmin)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "✓ Running as Windows Administrator (LocalSystem)";
            }
            else
            {
                return $"✓ Running as root (UID: {GetUserId()})";
            }
        }
        else
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "✗ NOT running as Administrator - some features may not work";
            }
            else
            {
                return $"✗ NOT running as root (UID: {GetUserId()}) - some features may not work";
            }
        }
    }
}

/// <summary>
/// P/Invoke declarations for Unix system calls
/// </summary>
internal static class Syscall
{
    [DllImport("libc", SetLastError = true)]
    public static extern uint getuid();

    [DllImport("libc", SetLastError = true)]
    public static extern uint geteuid();
}

// Extension: Add this to your Program.cs startup
public static class PrivilegeExtensions
{
    public static void LogPrivilegeStatus(this ILogger logger)
    {
        var isAdmin = PrivilegeChecker.IsRunningAsAdministrator();
        var status = PrivilegeChecker.GetPrivilegeStatus();
        
        if (isAdmin)
        {
            logger.LogInformation(status);
        }
        else
        {
            logger.LogWarning(status);
            logger.LogWarning("Serial port access and some system features may be restricted!");
        }
    }
}