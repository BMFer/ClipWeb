using Discord.Interactions;

namespace CLIPWEB.Application.Commands;

/// <summary>Platform choices offered by <c>/submit post</c>.</summary>
public enum SocialPlatform
{
    [ChoiceDisplay("TikTok")] TikTok,
    [ChoiceDisplay("YouTube Shorts")] YouTubeShorts,
    [ChoiceDisplay("Instagram Reels")] InstagramReels,
    [ChoiceDisplay("X")] X,
    [ChoiceDisplay("Facebook")] Facebook,
    [ChoiceDisplay("Other")] Other
}

public static class SocialPlatformExtensions
{
    /// <summary>Friendly name stored on the published post.</summary>
    public static string ToDisplayName(this SocialPlatform platform) => platform switch
    {
        SocialPlatform.TikTok => "TikTok",
        SocialPlatform.YouTubeShorts => "YouTube Shorts",
        SocialPlatform.InstagramReels => "Instagram Reels",
        SocialPlatform.X => "X",
        SocialPlatform.Facebook => "Facebook",
        _ => "Other"
    };
}
