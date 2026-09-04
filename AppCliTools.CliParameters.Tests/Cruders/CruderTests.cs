using System.Collections.Generic;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.Cruders;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters.Tests.Cruders;

//CheckRecordKeyChanged: ველის რედაქტორის შემდეგ გასაღების ცვლილების აღმოჩენა და სიის მენიუს ხელახლა აწყობა
public sealed class CruderTests
{
    [Fact]
    public void CheckRecordKeyChanged_WhenKeyIsNotFromItem_ReturnsFalse()
    {
        // Arrange
        var cruder = new TestCruder(false);
        var item = new KeyedItemData("NewKey");

        // Act
        bool result = cruder.CheckRecordKeyChanged("OldKey", item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CheckRecordKeyChanged_WhenItemHasNoKey_ReturnsFalse()
    {
        // Arrange
        var cruder = new TestCruder(true);
        var item = new ItemData();

        // Act
        bool result = cruder.CheckRecordKeyChanged("OldKey", item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CheckRecordKeyChanged_WhenKeyIsUnchanged_ReturnsFalseAndKeepsListMenu()
    {
        // Arrange
        var cruder = new TestCruder(true);
        var item = new KeyedItemData("SameKey");
        CliMenuSet listMenuBefore = cruder.GetListMenu();

        // Act
        bool result = cruder.CheckRecordKeyChanged("SameKey", item);

        // Assert
        Assert.False(result);
        Assert.Same(listMenuBefore, cruder.GetListMenu());
    }

    [Fact]
    public void CheckRecordKeyChanged_WhenKeyChanged_ReturnsTrueAndRebuildsListMenu()
    {
        // Arrange
        var cruder = new TestCruder(true);
        var item = new KeyedItemData("NewKey");
        CliMenuSet listMenuBefore = cruder.GetListMenu();

        // Act
        bool result = cruder.CheckRecordKeyChanged("OldKey", item);

        // Assert
        Assert.True(result);
        Assert.NotSame(listMenuBefore, cruder.GetListMenu());
    }

    private sealed class KeyedItemData : ItemData
    {
        private readonly string _key;

        public KeyedItemData(string key)
        {
            _key = key;
        }

        public override string GetItemKey()
        {
            return _key;
        }
    }

    private sealed class TestCruder : Cruder
    {
        public TestCruder(bool fieldKeyFromItem) : base("Item", "Items", fieldKeyFromItem)
        {
        }

        protected override Dictionary<string, ItemData> GetCrudersDictionary()
        {
            return [];
        }
    }
}
