using CLIPWEB.Application.Authorization;
using CLIPWEB.Application.Campaigns;
using Discord;
using Discord.Interactions;

namespace CLIPWEB.Application.Commands;

/// <summary>Brand management commands (<c>/brand …</c>).</summary>
[Group("brand", "Manage brands")]
public class BrandModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ICampaignService _campaigns;

    public BrandModule(ICampaignService campaigns) => _campaigns = campaigns;

    [SlashCommand("create", "Create a new brand.")]
    [RequireManager]
    public async Task CreateAsync(
        [Summary("name", "The brand's name")] string name,
        [Summary("website", "Brand website URL")] string? website = null,
        [Summary("email", "Contact email")] string? email = null)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            await RespondAsync("A brand name is required.", ephemeral: true);
            return;
        }

        var brand = await _campaigns.CreateBrandAsync(
            name, NullIfBlank(website), NullIfBlank(email));

        var embed = new EmbedBuilder()
            .WithTitle("Brand created")
            .WithColor(Color.Green)
            .AddField("Name", brand.Name)
            .AddField("Website", brand.WebsiteUrl ?? "—", inline: true)
            .AddField("Contact", brand.ContactEmail ?? "—", inline: true)
            .WithFooter($"Brand id: {brand.Id}")
            .Build();

        await RespondAsync(embed: embed, ephemeral: true);
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
