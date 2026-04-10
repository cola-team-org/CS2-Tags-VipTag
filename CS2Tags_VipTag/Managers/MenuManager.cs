using System.Diagnostics.CodeAnalysis;

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