namespace AppCliTools.CliMenu;

public enum EMenuAction
{
    Exit,
    LevelUp,
    PageUp,
    PageDown,
    LoadSubMenu,
    Reload,
    ReloadWithoutPause, //გამოიყენება ProcessMonitoringCliMenuCommand-ში
    GoToMenuLink,
    Nothing
}
