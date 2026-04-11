using CounterStrikeSharp.API.Core;

using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Menu;

using VipTags.Utilities;

namespace VipTags.Managers;

public sealed class MenuManager(VipTagsPlugin plugin)
{

    public void CreateMenuWithColors(
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

            menu.AddItem(
                $"<font color='{hex}'><b>{color}</b></font>",
                (p, o) =>
                {
                    // TODO: check auth?
                    Task.Run(() => onColorSelected(color));
                    // TODO: move somewhere sane
                    o.PostSelectAction = PostSelectAction.Nothing;
                }
            );
        }

        menu.Display(player, 0);
    }


}