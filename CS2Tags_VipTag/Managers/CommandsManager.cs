using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Menu;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using VipTags.Authorization;
using VipTags.Models;
using VipTags.Utilities;

namespace VipTags.Managers;

public sealed class CommandManager(
    VipTagsPlugin plugin,
    ILogger<CommandManager> logger,
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

        AsyncHelpers.RunWithErrorLogging(logger, async () =>
        {
            await tagsManager.UpdateTag(authorizationContext, arg);
            await player.SafeColoredPrintToChat($"{localizer["Prefix"]}{localizer["TagSet", arg]}");
        });
    }

    private void AddColorMenuOption(
        PlayerMenu menu,
        AuthorizationContext authorizationContext,
        ColorType colorType)
    {
        var colorTypeName = Enum.GetName(colorType) ??
                            throw new ArgumentOutOfRangeException(nameof(colorType), colorType, null);

        menu.AddItem(localizer[$"{colorTypeName}Menu"], (_, _) =>
        {
            DisplayColorSelector(authorizationContext, colorType, menu);
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

        var menu = new PlayerMenu(localizer["TagsMenu"], plugin);

        foreach (var colorType in Enum.GetValues<ColorType>())
        {
            if (!authorizationContext.CanUpdate(colorType)) continue;

            AddColorMenuOption(menu, authorizationContext, colorType);
        }

        if (authorizationContext.CanSetCustomTag)
        {
            if (plugin.Config.CustomTagOnScoreboard)
            {
                var currentVisibility = tagsManager.GetTagVisibilityOnScoreboard(authorizationContext);
                string GetScoreboardVisibilityOptionName() =>
                    localizer[currentVisibility ? "DisableScoreboardVisibility" : "EnableScoreboardVisibility"];

                menu.AddItem(
                    GetScoreboardVisibilityOptionName(),
                    (_, o) =>
                    {
                        currentVisibility = !currentVisibility;
                        o.Text = GetScoreboardVisibilityOptionName();
                        o.PostSelectAction = PostSelectAction.Nothing;

                        AsyncHelpers.RunWithErrorLogging(logger, async () =>
                        {
                            await tagsManager.UpdateTagVisibilityOnScoreboard(authorizationContext, currentVisibility);

                            if (currentVisibility)
                            {
                                await authorizationContext.Player.SafeColoredPrintToChat(
                                    $"{plugin.Localizer["Prefix"]}{plugin.Localizer["ScoreboardVisibilityEnabled"]}");
                            }
                            else
                            {
                                await authorizationContext.Player.SafeColoredPrintToChat(
                                    $"{plugin.Localizer["Prefix"]}{plugin.Localizer["ScoreboardVisibilityDisabled"]}");
                            }
                        });
                    });
            }

            menu.AddItem($"{localizer["ResetCustomTag"]}", (_, o) =>
            {
                o.PostSelectAction = PostSelectAction.Nothing;
                AsyncHelpers.RunWithErrorLogging(logger, async () =>
                {
                    await tagsManager.UpdateTag(authorizationContext, null);
                    await player.SafeColoredPrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["CustomTagReset"]}");
                });
            });
        }

        menu.AddItem($"{localizer["ResetEverything"]}", (_, o) =>
        {
            o.PostSelectAction = PostSelectAction.Nothing;
            AsyncHelpers.RunWithErrorLogging(logger, async () =>
            {
                await tagsManager.DeleteSettings(authorizationContext);
                await player.SafeColoredPrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["EverythingReset"]}");
            });
        });

        menu.Display(player, 0);
    }

    private void DisplayColorSelector(
        AuthorizationContext authorizationContext,
        ColorType colorType,
        PlayerMenu? parentMenu)
    {
        var player = authorizationContext.Player;
        var colorTypeName = Enum.GetName(colorType) ??
                            throw new ArgumentOutOfRangeException(nameof(colorType), colorType, null);

        var menu = new WasdMenu(plugin.Localizer[$"{colorTypeName}Menu"], plugin) { PrevMenu = parentMenu };

        var resetOption = menu.AddItem(plugin.Localizer["ResetColor"], (_, _) => SetColor(null));
        resetOption.PostSelectAction = PostSelectAction.Nothing;

        foreach (var color in TagColors.Colors)
        {
            var hex = TagColors.ComputeColorHex(color, player.Team);

            var option = menu.AddItem(
                $"<font color='{hex}'><b>{color}</b></font>",
                (_, _) => SetColor(color));
            option.PostSelectAction = PostSelectAction.Nothing;
        }

        menu.Display(player, 0);

        void SetColor(string? color)
        {
            AsyncHelpers.RunWithErrorLogging(logger, async () =>
            {
                await tagsManager.UpdateColor(authorizationContext, colorType, color);

                if (color is not null)
                {
                    await player.SafeColoredPrintToChat(
                        $"{plugin.Localizer["Prefix"]}{{{color}}}{plugin.Localizer[$"New{colorTypeName}", color]}");
                }
                else
                {
                    await player.SafeColoredPrintToChat(
                        $"{plugin.Localizer["Prefix"]}{plugin.Localizer[$"Reset{colorTypeName}"]}");
                }
            });

        }
    }
}