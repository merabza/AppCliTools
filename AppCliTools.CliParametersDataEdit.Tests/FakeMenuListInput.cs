using System.Collections.Generic;
using System.Linq;
using AppCliTools.CliMenu;

namespace AppCliTools.CliParametersDataEdit.Tests;

//answers as SelectFromMenuListInput (MenuInputer.InputIdFromMenuList) does: Enter gives the CountedId of the item with
//the current name, and the key of an item without a key of its own gives the item's position among those items
internal sealed class FakeMenuListInput
{
    public const string EnterAnswer = "<Enter>";

    //Enter keeps the current name; any other answer is the name of an offered item
    public string Answer { get; set; } = EnterAnswer;

    public List<(string FieldName, CliMenuSet MenuSet, string? CurrentName)> Selections { get; } = [];

    public int InputIdFromMenuList(string fieldName, CliMenuSet menuSet, string? currentName)
    {
        Selections.Add((fieldName, menuSet, currentName));
        if (Answer == EnterAnswer)
        {
            //the real input ignores Enter when there is no current name or the current name is not offered
            Assert.NotNull(currentName);
            CliMenuItem? currentItem = menuSet.GetMenuItemWithName(currentName);
            Assert.NotNull(currentItem);
            return currentItem.CountedId;
        }

        int position = CliMenuTestAccess.GetMenuItems(menuSet).Where(x => x.Key is null).ToList()
            .FindIndex(x => x.MenuItemName == Answer);
        Assert.True(position >= 0, $"{Answer} is not offered for {fieldName}");
        return position;
    }
}
