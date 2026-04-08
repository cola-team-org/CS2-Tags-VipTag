
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CS2MenuManager.API.Menu;
using CS2Tags_VipTag.Models;
using Microsoft.Extensions.Logging;

namespace CS2Tags_VipTag;

internal class CommandManager(
    CS2Tags_VipTag plugin,
    PlayerModelCache playerModelCache)
{
    public void InitializeCommands()
    {
        plugin.AddCommand("css_settag", "Ability for VIP to change their Scoreboard and Chat tag", TagChange);
        plugin.AddCommand("css_tagmenu", "Ability for VIP to change their Scoreboard and Chat tag", TagMenu);
    }

    [CommandHelper(minArgs: 1, usage: "TagName")]
    private void TagChange(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || player.IsBot || player.IsHLTV) return;
        if (!AdminManager.PlayerHasPermissions(player, plugin.Config.VipSetTagFlag))
        {
            player!.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["NoPermissions"]}");
            return;
        }

        //var arg = commandInfo.GetArg(1);

        string arg = "";

        for (int i = 1; i < commandInfo.ArgCount; i++)
        {
            arg += commandInfo.GetArg(i) + " ";
        }

        arg = arg.TrimEnd();

        if (arg.Length > 50)
        {
            player.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["TooLong"]}");
            return;
        }



        var newtag = $"{arg} ";
        try
        {
            var model = playerModelCache.Get(player.GetAuthorizedSteamId());

            if (model is null)
            {
                model = playerModelCache.Set(player.GetAuthorizedSteamId(), new PlayerModel
                {
                    steamid = player.GetAuthorizedSteamId(),
                    tag = arg,
                    tagcolor = null,
                    namecolor = null,
                    chatcolor = null,
                    visibility = true,
                    chatvisibility = true,
                    scorevisibility = true,
                });
            }
            else
            {
                model.tag = arg;
            }

            if (AdminManager.PlayerHasPermissions(player, plugin.Config.VipScoreboardFlag))
            {
                plugin._tagApi?.SetAttribute(player, TagsApi.Tags.TagType.ScoreTag, newtag);
            }
            if (AdminManager.PlayerHasPermissions(player, plugin.Config.VipChatFlag))
            {
                if (model.tagcolor == null)
                {
                    plugin._tagApi?.SetAttribute(player!, TagsApi.Tags.TagType.ChatTag, $"{model.tag} ");
                }
                else
                {
                    plugin._tagApi?.SetAttribute(player, TagsApi.Tags.TagType.ChatTag, $"{{{model.tagcolor}}}{arg} ");
                }
            }

            player.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["TagSet", arg]}");
        }
        catch (Exception ex)
        {
            plugin.Logger.LogInformation($"TagChange: {ex}");
        }

    }

    private void TagMenu(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || player.IsBot || player.IsHLTV) return;
        if (!AdminManager.PlayerHasPermissions(player, plugin.Config.Vip_BaseFlag))
        {
            player!.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["NoPermissions"]}");
            return;
        }
        /*
        if (!_plugin.Players.ContainsKey(player.AuthorizedSteamID!.SteamId64))
        {
            player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["SetupTag"]}");
            return;
        }
        */
        var model = playerModelCache.Get(player.GetAuthorizedSteamId());
        WasdMenu menu = new(plugin.Localizer["VipMenu"], plugin);

        menu?.AddItem($"{plugin.Localizer["ToggleTagMenu"]}", (player, option) =>
            {
                plugin.MenuManager!.CreateDisableMenu(player, menu);
            },
            disableOption: (AdminManager.PlayerHasPermissions(player, plugin.Config.VipToggleMenuFlag) && model is not null)
                ? CS2MenuManager.API.Enum.DisableOption.None
                : CS2MenuManager.API.Enum.DisableOption.DisableHideNumber);
        menu?.AddItem(plugin.Localizer["TagColorMenu"], (player, option) =>
            {
                plugin.MenuManager!.CreateMenuWithColors(player, 1, menu);
            },
            disableOption: (AdminManager.PlayerHasPermissions(player, plugin.Config.VipTagColorFlag) && AdminManager.PlayerHasPermissions(player, plugin.Config.VipChatFlag) && model is not null)
                ? CS2MenuManager.API.Enum.DisableOption.None
                : CS2MenuManager.API.Enum.DisableOption.DisableHideNumber);
        menu?.AddItem(plugin.Localizer["ChatColorMenu"], (player, option) =>
            {
                plugin.MenuManager!.CreateMenuWithColors(player, 2, menu);
            },
            disableOption: AdminManager.PlayerHasPermissions(player, plugin.Config.VipChatColorFlag)
                ? CS2MenuManager.API.Enum.DisableOption.None
                : CS2MenuManager.API.Enum.DisableOption.DisableHideNumber);
        menu?.AddItem(plugin.Localizer["NameColorMenu"], (player, option) =>
            {
                plugin.MenuManager!.CreateMenuWithColors(player, 3, menu);
            },
            disableOption: AdminManager.PlayerHasPermissions(player, plugin.Config.VipNameColorFlag)
                ? CS2MenuManager.API.Enum.DisableOption.None
                : CS2MenuManager.API.Enum.DisableOption.DisableHideNumber);
        menu?.Display(player, 0);
    }

}