using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using static TagsApi.Tags;

namespace CS2Tags_VipTag;

public sealed class TagsManager(CS2Tags_VipTag plugin, PlayerModelCache playerModelCache)
{
    public void SetEverythingTagRelated(CCSPlayerController player, int mode)
    {
        if (player?.AuthorizedSteamID == null)
            return;

        var model = playerModelCache.Get(player.GetAuthorizedSteamId());

        if (model is null) return;

        if (AdminManager.PlayerHasPermissions(player, plugin.Config.VipScoreboardFlag))
        {
            if (mode == 1 || model.scorevisibility == true)
                plugin._tagApi?.SetAttribute(player, TagType.ScoreTag, model.tag);
            else
                plugin._tagApi?.ResetAttribute(player, TagType.ScoreTag);
        }
        else
        {
            plugin._tagApi?.ResetAttribute(player, TagType.ScoreTag);
        }

        // CHAT TAG
        if (AdminManager.PlayerHasPermissions(player, plugin.Config.VipChatFlag))
        {
            if (mode == 1 || model.chatvisibility == true)
                SetChatTag(player);
            else
                plugin._tagApi?.ResetAttribute(player, TagType.ChatTag);
        }
        else
        {
            plugin._tagApi?.ResetAttribute(player, TagType.ChatTag);
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

        if (!AdminManager.PlayerHasPermissions(player, plugin.Config.VipTagColorFlag) || model.tagcolor == null)
            plugin._tagApi?.SetAttribute(player, TagType.ChatTag, $"{model.tag} ");
        else
            plugin._tagApi?.SetAttribute(player, TagType.ChatTag, $"{{{model.tagcolor}}}{model.tag} ");
    }


    private void SetNameColor(CCSPlayerController player)
    {
        if (player.AuthorizedSteamID == null)
            return;
        
        var model = playerModelCache.Get(player.GetAuthorizedSteamId());

        if (model is null) return;

        if (!AdminManager.PlayerHasPermissions(player, plugin.Config.VipNameColorFlag) || model.namecolor == null)
        {
            plugin._tagApi?.ResetAttribute(player, TagType.NameColor);
            return;
        }

        plugin._tagApi?.SetAttribute(player, TagType.NameColor, $"{{{model.namecolor}}}");
    }

    private void SetChatColor(CCSPlayerController player)
    {
        var model = playerModelCache.Get(player.GetAuthorizedSteamId());

        if (model is null) return;

        if (!AdminManager.PlayerHasPermissions(player, plugin.Config.VipChatColorFlag) || model.chatcolor == null)
        {
            plugin._tagApi?.ResetAttribute(player, TagType.ChatColor);
            return;
        }

        plugin._tagApi?.SetAttribute(player, TagType.ChatColor, $"{{{model.chatcolor}}}");
    }
}