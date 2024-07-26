namespace CliMenu;

public enum EMenuAction
{
    Exit,
    LevelUp,
    LoadSubMenu,
    Reload,
    ReloadWithoutPause,//გამოიყენება ProcessMonitoringCliMenuCommand-ში
    GoToMenuLink,
    Nothing
}