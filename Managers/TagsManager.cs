using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

using static TagsApi.Tags;

namespace VipTags.Managers;

public sealed class TagsManager(VipTagsPlugin plugin, PlayerModelCache playerModelCache)
{
    public void SetEverythingTagRelated(CCSPlayerController player, int mode)
    {
        if (player?.AuthorizedSteamID == null)
            return;

        var model = playerModelCache.Get(player.GetAuthorizedSteamId());

        if (model is null) return;

        if (AdminManager.PlayerHasPermissions(player, plugin.Config.VipScoreboardFlag))
        {
            if (mode == 1 || model.ScoreVisibility == true)
                plugin.TagApi?.SetAttribute(player, TagType.ScoreTag, model.Tag);
            else
                plugin.TagApi?.ResetAttribute(player, TagType.ScoreTag);
        }
        else
        {
            plugin.TagApi?.ResetAttribute(player, TagType.ScoreTag);
        }

        // CHAT TAG
        if (AdminManager.PlayerHasPermissions(player, plugin.Config.VipChatFlag))
        {
            if (mode == 1 || model.ChatVisibility == true)
                SetChatTag(player);
            else
                plugin.TagApi?.ResetAttribute(player, TagType.ChatTag);
        }
        else
        {
            plugin.TagApi?.ResetAttribute(player, TagType.ChatTag);
        }

        SetNameColor(player);
        SetChatColor(player);
    }



    public void SetChatTag(CCSPlayerController player)
    {
        if (player.AuthorizedSteamID == null)
            return;

        var model = playerModelCache.Get(player.GetAuthorizedSteamId());

        if (model is null) return;

        if (!AdminManager.PlayerHasPermissions(player, plugin.Config.VipTagColorFlag) || model.TagColor == null)
            plugin.TagApi?.SetAttribute(player, TagType.ChatTag, $"{model.Tag} ");
        else
            plugin.TagApi?.SetAttribute(player, TagType.ChatTag, $"{{{model.TagColor}}}{model.Tag} ");
    }


    private void SetNameColor(CCSPlayerController player)
    {
        if (player.AuthorizedSteamID == null)
            return;

        var model = playerModelCache.Get(player.GetAuthorizedSteamId());

        if (model is null) return;

        if (!AdminManager.PlayerHasPermissions(player, plugin.Config.VipNameColorFlag) || model.NameColor == null)
        {
            plugin.TagApi?.ResetAttribute(player, TagType.NameColor);
            return;
        }

        plugin.TagApi?.SetAttribute(player, TagType.NameColor, $"{{{model.NameColor}}}");
    }

    private void SetChatColor(CCSPlayerController player)
    {
        var model = playerModelCache.Get(player.GetAuthorizedSteamId());

        if (model is null) return;

        if (!AdminManager.PlayerHasPermissions(player, plugin.Config.VipChatColorFlag) || model.ChatColor == null)
        {
            plugin.TagApi?.ResetAttribute(player, TagType.ChatColor);
            return;
        }

        plugin.TagApi?.SetAttribute(player, TagType.ChatColor, $"{{{model.ChatColor}}}");
    }
}