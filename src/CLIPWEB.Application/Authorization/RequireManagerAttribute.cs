using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CLIPWEB.Application.Authorization;

/// <summary>
/// Allows a command only for users who can manage the network: anyone with the
/// Discord "Manage Server" permission, OR anyone holding a configured
/// Admin / Network Manager role (see <see cref="RolesOptions"/>).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireManagerAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(
        IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        if (context.User is not IGuildUser guildUser)
            return Task.FromResult(
                PreconditionResult.FromError("This command can only be used in a server."));

        if (guildUser.GuildPermissions.ManageGuild)
            return Task.FromResult(PreconditionResult.FromSuccess());

        var roles = services.GetService<IOptions<RolesOptions>>()?.Value;
        if (roles is not null &&
            ((roles.AdminRoleId != 0 && guildUser.RoleIds.Contains(roles.AdminRoleId)) ||
             (roles.NetworkManagerRoleId != 0 && guildUser.RoleIds.Contains(roles.NetworkManagerRoleId))))
        {
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        return Task.FromResult(PreconditionResult.FromError(
            "You need the **Manage Server** permission or a manager role to use this command."));
    }
}
