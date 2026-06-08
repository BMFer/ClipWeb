using System.Reflection;
using CLIPWEB.Application.Commands;
using Discord.Interactions;

namespace CLIPWEB.Tests;

public class PingModuleTests
{
    [Fact]
    public void PingModule_is_an_interaction_module()
    {
        Assert.True(
            typeof(InteractionModuleBase<SocketInteractionContext>)
                .IsAssignableFrom(typeof(PingModule)),
            "PingModule must derive from InteractionModuleBase so the InteractionService discovers it.");
    }

    [Fact]
    public void PingModule_exposes_a_ping_slash_command()
    {
        var command = typeof(PingModule)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(m => m.GetCustomAttribute<SlashCommandAttribute>())
            .FirstOrDefault(a => a is not null && a.Name == "ping");

        Assert.NotNull(command);
        Assert.False(string.IsNullOrWhiteSpace(command!.Description));
    }
}
