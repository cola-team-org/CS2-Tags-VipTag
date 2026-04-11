using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Menu;

using Microsoft.Extensions.Localization;

using VipTags.Authorization;
using VipTags.Utilities;

namespace VipTags.Managers;

public sealed class CommandManager(
    VipTagsPlugin plugin,
    AuthorizationComputer authorizationComputer,
    IStringLocalizer localizer,
    TagsManager tagsManager)
{
    public void InitializeCommands()
    {
        plugin.AddCommand("css_settag", "Ability for VIP to change their Scoreboard and Chat tag", TagChange);
        plugin.AddCommand("css_tagmenu", "Ability for VIP to change their Scoreboard and Chat tag", TagMenu);
    }

    [CommandHelper(minArgs: 1, usage: "TagName")]
    private void TagChange(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!player.IsRealAuthorizedPerson()) return;

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
            await player.SafePrintToChat($"{localizer["Prefix"]}{localizer["TagSet", arg]}");
        });

    }

    private void TagMenu(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!player.IsRealAuthorizedPerson()) return;

        var authorizationContext = authorizationComputer.ComputeAuthorizationContext(player);
        if (!authorizationContext.CanSetAnything)
        {
            player.PrintToChat($"{localizer["Prefix"]}{localizer["NoPermissions"]}");
            return;
        }

        var menu = new WasdMenu(localizer["TagsMenu"], plugin);

        menu.AddItem(localizer["TagColorMenu"], (player, _) =>
            {
                CreateMenuWithColors(
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
                ? DisableOption.None
                : DisableOption.DisableHideNumber);
        menu.AddItem(localizer["ChatColorMenu"], (player, _) =>
            {
                CreateMenuWithColors(
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
                ? DisableOption.None
                : DisableOption.DisableHideNumber);
        menu.AddItem(localizer["NameColorMenu"], (player, _) =>
            {
                CreateMenuWithColors(
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
                ? DisableOption.None
                : DisableOption.DisableHideNumber);

        menu.AddItem($"{localizer["ResetTag"]}", (player, _) =>
        {
            Task.Run(async () =>
            {
                await tagsManager.DeleteSettings(authorizationContext);
                await player.SafePrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["TagReset"]}".ReplaceColorTags());
            });
        });

        menu.Display(player, 0);
    }

    private void CreateMenuWithColors(
        CCSPlayerController? player,
        string menuTitle,
        Func<string, Task> onColorSelected,
        WasdMenu? parentMenu)
    {
        if (player == null) return;

        var menu = new WasdMenu(menuTitle, plugin) { PrevMenu = parentMenu };

        foreach (var color in TagColors.Colors)
        {
            var hex = TagColors.ComputeColorHex(color, player.Team);

            var option = menu.AddItem(
                $"<font color='{hex}'><b>{color}</b></font>",
                (p, o) =>
                {
                    Task.Run(() => onColorSelected(color));
                }
            );
            option.PostSelectAction = PostSelectAction.Nothing;
        }

        menu.Display(player, 0);
    }
}