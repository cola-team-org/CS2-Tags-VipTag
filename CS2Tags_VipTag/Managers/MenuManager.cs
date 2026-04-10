using System.Diagnostics.CodeAnalysis;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Menu;

using VipTags.Models;
using VipTags.Utilities;

namespace VipTags.Managers;

public sealed class MenuManager(
    VipTagsPlugin plugin,
    TagsManager tagsManager,
    PlayerModelCache playerModelCache)
{
    private bool TryGetModel(CCSPlayerController player, [NotNullWhen(true)] out TagSettings? model)
    {
        if (player.AuthorizedSteamID == null || playerModelCache.Get(player.AuthorizedSteamID.SteamId64) is not { } m)
        {
            model = null;
            return false;
        }

        model = m;
        return true;
    }

    // TODO: replace with "reset tag" options
    public void CreateDisableMenu(CCSPlayerController player, WasdMenu? parentMenu)
    {
        if (!TryGetModel(player, out var model))
            return;

        WasdMenu menu = new("Disable menu", plugin)
        {
            PrevMenu = parentMenu
        };
        menu.AddItem(
            $"{plugin.Localizer["ToggleEverythingMenu"]} - [{model.Visibility}]",
            (p, o) =>
            {
                if (!TryGetModel(p, out var m))
                {
                    o.PostSelectAction = PostSelectAction.Close;
                    return;
                }

                m.Visibility = !(m.Visibility ?? false);
                tagsManager.ApplyTags(player, model);

                if (m.Visibility == true)
                {
                    p.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["Toggled"]}");
                }
                else
                {
                    p.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["UnToggled"]}");
                }

                o.PostSelectAction = PostSelectAction.Close;
                Server.NextWorldUpdate(() => CreateDisableMenu(p, parentMenu));
            }
        );

        menu.AddItem(
            $"{plugin.Localizer["ToggleScoreTagMenu"]} - [{model.ScoreVisibility}]",
            (p, o) =>
            {
                if (!TryGetModel(p, out var m))
                {
                    o.PostSelectAction = PostSelectAction.Close;
                    return;
                }

                m.ScoreVisibility = !(m.ScoreVisibility ?? false);
                tagsManager.ApplyTags(player, m);

                if (m.ScoreVisibility == true)
                {
                    p.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["ToggledScoreTag"]}");
                }
                else
                {
                    p.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["UnToggledScoreTag"]}");
                }

                o.PostSelectAction = PostSelectAction.Close;
                Server.NextWorldUpdate(() => CreateDisableMenu(p, parentMenu));
            }
        );

        menu.AddItem(
            $"{plugin.Localizer["ToggleChatMenu"]} - [{model.ChatVisibility}]",
            (p, o) =>
            {
                if (!TryGetModel(p, out var m))
                {
                    o.PostSelectAction = PostSelectAction.Close;
                    return;
                }

                m.ChatVisibility = !(m.ChatVisibility ?? false);
                tagsManager.ApplyTags(player, m);

                if (m.ChatVisibility == true)
                {
                    p.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["ToggledChatTag"]}");
                }
                else
                {
                    p.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["UnToggledChatTag"]}");
                }

                o.PostSelectAction = PostSelectAction.Close;
                Server.NextWorldUpdate(() => CreateDisableMenu(p, parentMenu));
            }
        );

        menu.Display(player, 0);
    }

    public delegate void OnColorSelected(string color, TagSettings tagSettings);

    public void CreateMenuWithColors(
        CCSPlayerController? player,
        string menuTitle,
        OnColorSelected onColorSelected,
        WasdMenu? parentMenu)
    {
        if (player == null) return;
        if (!TryGetModel(player, out _))
            return;

        var menu = new WasdMenu(menuTitle, plugin) { PrevMenu = parentMenu };

        foreach (var color in TagColors.Colors)
        {
            var hex = TagColors.ComputeColorHex(color, player.Team);

            menu.AddItem(
                $"<font color='{hex}'><b>{color}</b></font>",
                (p, o) =>
                {
                    if (!TryGetModel(p, out var m))
                    {
                        o.PostSelectAction = PostSelectAction.Close;
                        return;
                    }

                    onColorSelected(color, m);
                    tagsManager.ApplyTags(player, m);
                    o.PostSelectAction = PostSelectAction.Nothing;
                }
            );
        }

        menu.Display(player, 0);
    }


}