using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

using CS2MenuManager.API.Menu;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using VipTags.Models;

namespace VipTags.Managers;

public sealed class CommandManager(
    VipTagsPlugin plugin,
    ILogger<CommandManager> logger,
    DatabaseManager databaseManager,
    IStringLocalizer localizer,
    MenuManager menuManager,
    TagsManager tagsManager,
    PlayerModelCache playerModelCache)
{
    public void InitializeCommands()
    {
        // TODO: clear tag and not just wipe all? Perhaps a reset for each color too.
        plugin.AddCommand("css_settag", "Ability for VIP to change their Scoreboard and Chat tag", TagChange);
        plugin.AddCommand("css_tagmenu", "Ability for VIP to change their Scoreboard and Chat tag", TagMenu);
    }

    [CommandHelper(minArgs: 1, usage: "TagName")]
    private void TagChange(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || player.IsBot || player.IsHLTV) return;
        if (!AdminManager.PlayerHasPermissions(player, plugin.Config.VipSetTagFlag))
        {
            player.PrintToChat($"{localizer["Prefix"]}{localizer["NoPermissions"]}");
            return;
        }

        var arg = commandInfo.ArgString.Trim();

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
                model = playerModelCache.Set(player.GetAuthorizedSteamId(), new TagSettings
                {
                    SteamId = player.GetAuthorizedSteamId(),
                    Tag = arg,
                    TagColor = null,
                    NameColor = null,
                    ChatColor = null,
                });
            }
            else
            {
                model.Tag = arg;
            }

            tagsManager.ApplyTags(player, model);
            player.PrintToChat($"{localizer["Prefix"]}{localizer["TagSet", arg]}");
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "TagChange failed");
        }

    }

    private void TagMenu(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || player.IsBot || player.IsHLTV) return;
        if (!AdminManager.PlayerHasPermissions(player, plugin.Config.VipBaseFlag))
        {
            player.PrintToChat($"{localizer["Prefix"]}{localizer["NoPermissions"]}");
            return;
        }

        var model = playerModelCache.Get(player.GetAuthorizedSteamId());

        if (model is null)
        {
            player.PrintToChat($"{localizer["Prefix"]}{localizer["SetupTag"]}");
            return;
        }

        WasdMenu menu = new(localizer["VipMenu"], plugin);

        menu.AddItem($"{localizer["ResetTag"]}", (player, _) => // TODO: add string
            {
                playerModelCache.Clear(player.GetAuthorizedSteamId());
                player.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["TagReset"]}".ReplaceColorTags());
                var steamId = player.GetAuthorizedSteamId();
                Task.Run(() => databaseManager.DeleteTags(steamId));
            },
            disableOption: (AdminManager.PlayerHasPermissions(player, plugin.Config.VipToggleMenuFlag) && model is not null)
                ? CS2MenuManager.API.Enum.DisableOption.None
                : CS2MenuManager.API.Enum.DisableOption.DisableHideNumber);
        menu.AddItem(localizer["TagColorMenu"], (player, _) =>
            {
                menuManager.CreateMenuWithColors(
                    player,
                    plugin.Localizer["TagColorMenu"],
                    (color, settings) =>
                    {
                        settings.TagColor = color;
                        player.PrintToChat(
                            $"{plugin.Localizer["Prefix"]}{{{color}}}{plugin.Localizer["NewTagColor", color]}"
                                .ReplaceColorTags().Replace("{TeamColor}", ChatColors.ForTeam(player.Team).ToString()));
                    },
                    menu);
            },
            disableOption: (AdminManager.PlayerHasPermissions(player, plugin.Config.VipTagColorFlag) && AdminManager.PlayerHasPermissions(player, plugin.Config.VipChatFlag) && model is not null)
                ? CS2MenuManager.API.Enum.DisableOption.None
                : CS2MenuManager.API.Enum.DisableOption.DisableHideNumber);
        menu.AddItem(localizer["ChatColorMenu"], (player, _) =>
            {
                menuManager.CreateMenuWithColors(
                    player,
                    plugin.Localizer["ChatColorMenu"],
                    (color, settings) =>
                    {
                        settings.ChatColor = color;
                        player.PrintToChat(
                            $"{plugin.Localizer["Prefix"]}{{{color}}}{plugin.Localizer["NewChatColor", color]}"
                                .ReplaceColorTags().Replace("{TeamColor}", ChatColors.ForTeam(player.Team).ToString()));
                    }, menu);
            },
            disableOption: AdminManager.PlayerHasPermissions(player, plugin.Config.VipChatColorFlag)
                ? CS2MenuManager.API.Enum.DisableOption.None
                : CS2MenuManager.API.Enum.DisableOption.DisableHideNumber);
        menu.AddItem(localizer["NameColorMenu"], (player, _) =>
            {
                menuManager.CreateMenuWithColors(
                    player,
                    plugin.Localizer["NameColorMenu"],
                    (color, settings) =>
                    {
                        settings.NameColor = color;
                        player.PrintToChat(
                            $"{plugin.Localizer["Prefix"]}{{{color}}}{plugin.Localizer["NewNameColor", color]}"
                                .ReplaceColorTags().Replace("{TeamColor}", ChatColors.ForTeam(player.Team).ToString()));
                    },
                    menu);
            },
            disableOption: AdminManager.PlayerHasPermissions(player, plugin.Config.VipNameColorFlag)
                ? CS2MenuManager.API.Enum.DisableOption.None
                : CS2MenuManager.API.Enum.DisableOption.DisableHideNumber);
        menu.Display(player, 0);
    }

}