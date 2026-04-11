using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

using CS2MenuManager.API.Menu;

using Microsoft.Extensions.Localization;

using VipTags.Authorization;

namespace VipTags.Managers;

public sealed class CommandManager(
    VipTagsPlugin plugin,
    AuthorizationComputer authorizationComputer,
    IStringLocalizer localizer,
    MenuManager menuManager,
    TagsManager tagsManager)
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

        Task.Run(async () =>
        {
            await tagsManager.UpdateTag(authorizationContext, arg);
            await Server.NextFrameAsync(() => player.PrintToChat($"{localizer["Prefix"]}{localizer["TagSet", arg]}"));
        });

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

        // TODO: Remove SetupTag string

        var menu = new WasdMenu(localizer["VipMenu"], plugin);

        menu.AddItem($"{localizer["ResetTag"]}", (player, _) => // TODO: add string
            {
                Task.Run(async () =>
                {
                    await tagsManager.DeleteSettings(authorizationContext);
                    await player.SafePrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["TagReset"]}".ReplaceColorTags());
                });
            },
            disableOption: authorizationContext.CanSetCustomTag
                ? CS2MenuManager.API.Enum.DisableOption.None
                : CS2MenuManager.API.Enum.DisableOption.DisableHideNumber);
        menu.AddItem(localizer["TagColorMenu"], (player, _) =>
            {
                menuManager.CreateMenuWithColors(
                    player,
                    plugin.Localizer["TagColorMenu"], async color =>
                    {
                        await tagsManager.UpdateTagColor(authorizationContext, color);
                        await player.SafePrintToChat(
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
                    plugin.Localizer["ChatColorMenu"], async color =>
                    {
                        await tagsManager.UpdateChatColor(authorizationContext, color);
                        await player.SafePrintToChat(
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
                    plugin.Localizer["NameColorMenu"], async color =>
                    {
                        await tagsManager.UpdateNameColor(authorizationContext, color);
                        await player.SafePrintToChat(
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