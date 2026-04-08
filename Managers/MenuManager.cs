using System.Diagnostics.CodeAnalysis;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;

using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Menu;

using VipTags.Models;
using VipTags.Utilities;

using static TagsApi.Tags;

namespace VipTags.Managers;

public sealed class MenuManager(
    VipTagsPlugin plugin,
    TagsManager tagsManager,
    PlayerModelCache playerModelCache)
{
    private bool TryGetModel(CCSPlayerController player, [NotNullWhen(true)] out PlayerModel? model)
    {
        if (player.AuthorizedSteamID == null || playerModelCache.Get(player.AuthorizedSteamID.SteamId64) is not { } m)
        {
            model = null;
            return false;
        }

        model = m;
        return true;
    }

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
                plugin.TagApi?.SetPlayerVisibility(p, m.Visibility ?? true);

                if (m.Visibility == true)
                {
                    tagsManager.SetEverythingTagRelated(p, 1);
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

                if (m.ScoreVisibility == true)
                {
                    plugin.TagApi?.SetAttribute(p, TagType.ScoreTag, m.Tag);
                    p.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["ToggledScoreTag"]}");
                }
                else
                {
                    plugin.TagApi?.ResetAttribute(p, TagType.ScoreTag);
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

                if (m.ChatVisibility == true)
                {
                    tagsManager.SetChatTag(p);
                    p.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["ToggledChatTag"]}");
                }
                else
                {
                    plugin.TagApi?.ResetAttribute(p, TagType.ChatTag);
                    p.PrintToChat($"{plugin.Localizer["Prefix"]}{plugin.Localizer["UnToggledChatTag"]}");
                }

                o.PostSelectAction = PostSelectAction.Close;
                Server.NextWorldUpdate(() => CreateDisableMenu(p, parentMenu));
            }
        );

        menu.Display(player, 0);
    }

    public void CreateMenuWithColors(CCSPlayerController? player, int type, WasdMenu? parentMenu)
    {
        if (player == null) return;
        if (!TryGetModel(player, out _))
            return;

        WasdMenu menu = type switch
        {
            1 => new(plugin.Localizer["TagColorMenu"], plugin),
            2 => new(plugin.Localizer["ChatColorMenu"], plugin),
            3 => new(plugin.Localizer["NameColorMenu"], plugin),
            _ => new(plugin.Localizer["TagsMenu"], plugin)
        };

        menu.PrevMenu = parentMenu;

        foreach (var color in plugin.Colors)
        {
            var hex = color switch
            {
                "TeamColor" => player.Team switch
                {
                    CsTeam.CounterTerrorist => PluginUtilities.FromNameToHex("CTBlue"),
                    CsTeam.Terrorist => PluginUtilities.FromNameToHex("Orange"),
                    _ => null,
                },
                _ => PluginUtilities.FromNameToHex(color),
            } ?? "#FFFFFF";

            menu.AddItem(
                $"<font color='{hex}'><b>{color}</b></font>",
                (p, o) =>
                {
                    if (!TryGetModel(p, out var m))
                    {
                        o.PostSelectAction = PostSelectAction.Close;
                        return;
                    }

                    switch (type)
                    {
                        case 1:
                            m.TagColor = color;
                            player.PrintToChat($"{plugin.Localizer["Prefix"]}{{{color}}}{plugin.Localizer["NewTagColor", color]}".ReplaceColorTags().Replace("{TeamColor}", ChatColors.ForTeam(player.Team).ToString()));
                            tagsManager.SetChatTag(p);
                            break;

                        case 2:
                            m.ChatColor = color;
                            player.PrintToChat($"{plugin.Localizer["Prefix"]}{{{color}}}{plugin.Localizer["NewChatColor", color]}".ReplaceColorTags().Replace("{TeamColor}", ChatColors.ForTeam(player.Team).ToString()));
                            plugin.TagApi?.SetAttribute(p, TagType.ChatColor, $"{{{color}}}");
                            break;

                        case 3:
                            m.NameColor = color;
                            player.PrintToChat($"{plugin.Localizer["Prefix"]}{{{color}}}{plugin.Localizer["NewNameColor", color]}".ReplaceColorTags().Replace("{TeamColor}", ChatColors.ForTeam(player.Team).ToString()));
                            plugin.TagApi?.SetAttribute(p, TagType.NameColor, $"{{{color}}}");
                            break;
                    }

                    o.PostSelectAction = PostSelectAction.Nothing;
                }
            );
        }

        menu.Display(player!, 0);
    }


}