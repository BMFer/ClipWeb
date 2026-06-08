namespace CLIPWEB.Application.Authorization;

/// <summary>
/// Optional role mapping for the spec's management roles, bound from the
/// "Roles" configuration section. When a role id is set, holding that role
/// grants access to manager commands even without the Discord "Manage Server"
/// permission. 0 = unmapped.
/// </summary>
public class RolesOptions
{
    public const string SectionName = "Roles";

    /// <summary>Role treated as CLIPWEB Admin.</summary>
    public ulong AdminRoleId { get; set; }

    /// <summary>Role treated as CLIPWEB Network Manager.</summary>
    public ulong NetworkManagerRoleId { get; set; }
}
