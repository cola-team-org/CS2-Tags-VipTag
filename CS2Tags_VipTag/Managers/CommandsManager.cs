using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

using CS2MenuManager.API.Menu;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using VipTags.Authorization;
using VipTags.Models;

namespace VipTags.Managers;

public sealed class CommandManager(
    VipTagsPlugin plugin,
    AuthorizationComputer authorizationComputer,
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

        var authorizationContext = authorizationComputer.ComputeAuthorizationContext(player);
        if (!authorizationContext.CanSetCustomTag)
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

        try
        {
            var model = playerModelCache.Get(authorizationContext.SteamId);

            if (model is null)
            {
                model = playerModelCache.Set(authorizationContext.SteamId, new TagSettings
                {
                    SteamId = authorizationContext.SteamId,
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

        var authorizationContext = authorizationComputer.ComputeAuthorizationContext(player);
        if (!authorizationContext.CanSetAnything)
        {
            player.PrintToChat($"{localizer["Prefix"]}{localizer["NoPermissions"]}");
            return;
        }

        var model = playerModelCache.Get(authorizationContext.SteamId);

        if (model is null)
        {
            player.PrintToChat($"{localizer["Prefix"]}{localizer["SetupTag"]}");
            return;
        }

        WasdMenu menu = new(localizer["VipMenu"], plugin);

        menu.AddItem($"{localizer["ResetTag"]}", (player, _) => // TODO: add string
            {
                playerModelCache.Clear(authorizationContext.SteamId);
                player.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["TagReset"]}".ReplaceColorTags());
                Task.Run(() => databaseManager.DeleteTags(authorizationContext.SteamId));
            },
            disableOption: authorizationContext.CanSetCustomTag
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
            disableOption: authorizationContext.CanSetTagColor
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
            disableOption: authorizationContext.CanSetChatColor
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
            disableOption: authorizationContext.CanSetNameColor
                ? CS2MenuManager.API.Enum.DisableOption.None
                : CS2MenuManager.API.Enum.DisableOption.DisableHideNumber);
        menu.Display(player, 0);
    }

}