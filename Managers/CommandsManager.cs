using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CS2MenuManager.API.Menu;
using CS2Tags_VipTag.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace CS2Tags_VipTag;

public sealed class CommandManager(
    CS2Tags_VipTag plugin,
    ILogger<CommandManager> logger,
    IStringLocalizer localizer,
    MenuManager menuManager,
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
            player!.PrintToChat($"{localizer["Prefix"]}{localizer["NoPermissions"]}");
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
            player.PrintToChat($"{localizer["Prefix"]}{localizer["TooLong"]}");
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

            player.PrintToChat($"{localizer["Prefix"]}{localizer["TagSet", arg]}");
        }
        catch (Exception ex)
        {
            logger.LogInformation($"TagChange: {ex}");
        }

    }

    private void TagMenu(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || player.IsBot || player.IsHLTV) return;
        if (!AdminManager.PlayerHasPermissions(player, plugin.Config.Vip_BaseFlag))
        {
            player!.PrintToChat($"{localizer["Prefix"]}{localizer["NoPermissions"]}");
            return;
        }
        /*
        if (!_plugin.Players.ContainsKey(player.AuthorizedSteamID!.SteamId64))
        {
            player.PrintToChat($"{_localizer["Prefix"]}{_localizer["SetupTag"]}");
            return;
        }
        */
        var model = playerModelCache.Get(player.GetAuthorizedSteamId());
        WasdMenu menu = new(localizer["VipMenu"], plugin);

        menu?.AddItem($"{localizer["ToggleTagMenu"]}", (player, option) =>
            {
                menuManager.CreateDisableMenu(player, menu);
            },
            disableOption: (AdminManager.PlayerHasPermissions(player, plugin.Config.VipToggleMenuFlag) && model is not null)
                ? CS2MenuManager.API.Enum.DisableOption.None
                : CS2MenuManager.API.Enum.DisableOption.DisableHideNumber);
        menu?.AddItem(localizer["TagColorMenu"], (player, option) =>
            {
                menuManager.CreateMenuWithColors(player, 1, menu);
            },
            disableOption: (AdminManager.PlayerHasPermissions(player, plugin.Config.VipTagColorFlag) && AdminManager.PlayerHasPermissions(player, plugin.Config.VipChatFlag) && model is not null)
                ? CS2MenuManager.API.Enum.DisableOption.None
                : CS2MenuManager.API.Enum.DisableOption.DisableHideNumber);
        menu?.AddItem(localizer["ChatColorMenu"], (player, option) =>
            {
                menuManager.CreateMenuWithColors(player, 2, menu);
            },
            disableOption: AdminManager.PlayerHasPermissions(player, plugin.Config.VipChatColorFlag)
                ? CS2MenuManager.API.Enum.DisableOption.None
                : CS2MenuManager.API.Enum.DisableOption.DisableHideNumber);
        menu?.AddItem(localizer["NameColorMenu"], (player, option) =>
            {
                menuManager.CreateMenuWithColors(player, 3, menu);
            },
            disableOption: AdminManager.PlayerHasPermissions(player, plugin.Config.VipNameColorFlag)
                ? CS2MenuManager.API.Enum.DisableOption.None
                : CS2MenuManager.API.Enum.DisableOption.DisableHideNumber);
        menu?.Display(player, 0);
    }

}