# CLIPWEB Discord Bot Specification

## 1. Project Overview

**Bot Name:** CLIPWEB
**Language:** C#
**Platform:** Discord
**Purpose:** CLIPWEB tracks community-generated clips for brand campaigns launched through clipping networks.

The bot helps manage:

* Brands
* Campaigns
* Editors / Clippers
* Clip submissions
* Published post links
* View tracking
* Optional engagement metrics
* Editor performance
* Campaign totals
* Community onboarding survey

CLIPWEB is designed to become the operational layer for a Discord-based clipping network.

---

## 2. Core Roles

### Admin

Full control over the bot.

Can:

* Configure server settings
* Create brands
* Create campaigns
* Approve or reject submissions
* Manage users
* View all reports

### Network Manager

Manages campaigns and editors.

Can:

* Create campaigns
* Review submissions
* View campaign analytics
* Assign editors to campaigns

### Editor / Clipper

Community member creating clips.

Can:

* Complete welcome survey
* View active campaigns
* Submit clips
* Submit published post links
* View their own stats

### Brand Viewer

Optional read-only role for brand clients.

Can:

* View campaign performance summaries
* View approved clips
* View campaign totals

---

## 3. Main Data Model

### Brand

```csharp
public class Brand
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? ContactEmail { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
```

### Campaign

```csharp
public class Campaign
{
    public Guid Id { get; set; }
    public Guid BrandId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? SourceContentUrl { get; set; }
    public string? StyleGuideUrl { get; set; }
    public DateTime StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public bool IsActive { get; set; }
}
```

### EditorProfile

```csharp
public class EditorProfile
{
    public Guid Id { get; set; }
    public ulong DiscordUserId { get; set; }
    public string DiscordUsername { get; set; }
    public string? PreferredName { get; set; }
    public string? TimeZone { get; set; }
    public string? PrimaryPlatform { get; set; }
    public bool SurveyCompleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
```

### ClipSubmission

```csharp
public class ClipSubmission
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid EditorProfileId { get; set; }
    public string ClipUrl { get; set; }
    public string? Notes { get; set; }
    public SubmissionStatus Status { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
}
```

```csharp
public enum SubmissionStatus
{
    Pending,
    Approved,
    Rejected,
    NeedsRevision
}
```

### PublishedPost

```csharp
public class PublishedPost
{
    public Guid Id { get; set; }
    public Guid ClipSubmissionId { get; set; }
    public string Platform { get; set; }
    public string PostUrl { get; set; }
    public long Views { get; set; }
    public long? Likes { get; set; }
    public long? Comments { get; set; }
    public long? Shares { get; set; }
    public DateTime PostedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }
}
```

---

## 4. Required Bot Commands

### Public Commands

#### `/welcome`

Shows the official CLIPWEB welcome message.

#### `/survey`

Starts the editor onboarding survey.

#### `/campaigns`

Lists active campaigns.

#### `/campaign details`

Shows campaign instructions, source links, due dates, and payout notes.

#### `/submit clip`

Submits a clip for review.

Required fields:

* Campaign
* Clip URL
* Notes

#### `/submit post`

Adds a published social post URL to an approved clip.

Required fields:

* Clip submission
* Platform
* Post URL
* Views
* Optional likes
* Optional comments
* Optional shares

#### `/mystats`

Shows the editor’s stats.

Stats include:

* Clips submitted
* Clips approved
* Posts published
* Total views generated
* Average views per post

---

## 5. Admin Commands

#### `/brand create`

Creates a brand.

#### `/campaign create`

Creates a campaign.

#### `/campaign close`

Closes a campaign.

#### `/submission review`

Lists pending submissions.

#### `/submission approve`

Approves a clip submission.

#### `/submission reject`

Rejects a clip submission.

#### `/submission revision`

Marks a submission as needing revision.

#### `/report campaign`

Shows campaign totals.

#### `/report editor`

Shows editor performance.

---

## 6. Welcome Message

CLIPWEB should post the following official welcome message when a user joins or runs `/welcome`.

```text
Welcome to CLIPWEB.

This is where brands, campaigns, clipping networks, and editors connect.

Your job here is simple:

Find active campaigns.
Create strong clips.
Submit your work.
Publish approved content.
Track the views you generate.

CLIPWEB keeps score so your work does not disappear into the noise.

Every clip you submit, every post you publish, and every view you generate builds your editor profile inside the network.

Start by completing the onboarding survey with /survey.

After that, use /campaigns to see what is active.

Welcome to the web.
Now start clipping.
```

---

## 7. Onboarding Survey

The survey should collect:

1. Preferred name
2. Time zone
3. Main editing platform
4. Main posting platform
5. Experience level
6. What type of content they clip best
7. How many clips they can make per week
8. Whether they can post on their own accounts
9. Portfolio link
10. Payment/contact preference

### Survey Questions

```text
1. What name should we call you?

2. What time zone are you in?

3. What do you edit with?
Examples: CapCut, Premiere Pro, DaVinci Resolve, Final Cut, mobile apps, other.

4. What platforms do you mainly post on?
Examples: TikTok, YouTube Shorts, Instagram Reels, X, Facebook.

5. What is your experience level?
Beginner, Intermediate, Advanced, Professional.

6. What content do you clip best?
Examples: podcasts, gaming, business, crypto, fitness, comedy, drama, education.

7. How many clips can you realistically create per week?

8. Can you publish clips on your own social accounts?
Yes / No / Sometimes

9. Drop a portfolio link or example post if you have one.

10. What is the best way for campaign managers to contact you?
```

---

## 8. Tracking Priorities

### Must Track

* Campaign
* Editor
* Clip submission
* Approval status
* Published post URL
* Platform
* Views
* Date submitted
* Date posted

### Good To Have

* Likes
* Comments
* Shares
* Saves
* Follower growth

Likes and comments are not primary success metrics. They should be stored when available, but the main dashboard should prioritize views, clips published, and editor output.

---

## 9. Dashboard Metrics

### Campaign Metrics

* Total clips submitted
* Total clips approved
* Total posts published
* Total views
* Average views per post
* Top editor
* Top post
* Active editors

### Editor Metrics

* Total submissions
* Approval rate
* Total published posts
* Total views generated
* Average views per post
* Best performing post
* Active campaigns worked

---

## 10. Suggested Tech Stack

* C#
* .NET 8 or newer
* Discord.Net or DSharpPlus
* Entity Framework Core
* SQL Server, PostgreSQL, or SQLite for local development
* Background worker service for metric updates
* Serilog for logging
* JSON config for server/channel/role settings

---

## 11. Suggested Project Structure

```text
/src
  /CLIPWEB.Bot
    Program.cs
    appsettings.json

  /CLIPWEB.Core
    Entities
    Enums
    Interfaces
    Services

  /CLIPWEB.Infrastructure
    Data
    Repositories
    Discord
    Logging

  /CLIPWEB.Application
    Commands
    Surveys
    Reports
    Campaigns
    Submissions

/tests
  /CLIPWEB.Tests

CLIPWEB_SPEC.md
README.md
```

---

## 12. MVP Build Order

### Phase 1: Foundation

* Create Discord bot
* Add slash command support
* Add configuration
* Add database
* Add basic logging

### Phase 2: Onboarding

* Welcome message
* Survey command
* Store editor profiles
* Assign editor role after survey completion

### Phase 3: Campaigns

* Brand creation
* Campaign creation
* List active campaigns
* Campaign details command

### Phase 4: Submissions

* Submit clip
* Review pending clips
* Approve/reject/revision workflow

### Phase 5: Published Posts

* Submit post URL
* Track platform
* Track views
* Store optional likes/comments/shares

### Phase 6: Reporting

* Editor stats
* Campaign stats
* Admin reports
* Brand-facing summary

---

## 13. MVP Success Definition

CLIPWEB is successful when a Discord community can:

1. Welcome new editors
2. Survey and register editors
3. Show active campaigns
4. Accept clip submissions
5. Approve or reject submissions
6. Track published post URLs
7. Track views generated
8. Report campaign and editor performance

The first version does not need automated scraping. Manual metric entry is acceptable for MVP.

---

## 14. Future Enhancements

* Automated TikTok/YouTube/Instagram metric collection
* AI clip quality scoring
* Campaign recommendation engine
* Editor leaderboard
* Payout calculations
* Brand dashboard
* Fraud detection
* Duplicate URL detection
* Clip style tagging
* Viral pattern analysis
* Discord role rewards
* Web admin portal

---

## 15. Product Principle

CLIPWEB should make one thing obvious:

```text
Who created what, where it was posted, and how many views it generated.
```

That is the core value of the system.
