using Godot;
using System;

public partial class SettingsMenu : Node2D
{
    [Export] Slider activeSceneArea;
    [Export] Slider noteSpeed;
    [Export] Slider difficulty;

    public override void _Ready()
    {
        activeSceneArea.Value = Settings.ActiveSceneArea;
        noteSpeed.Value = Settings.NoteSpeed;
        difficulty.Value = Settings.Difficulty;
    }

    public override void _Process(double delta)
    {
        Settings.ActiveSceneArea = (float)activeSceneArea.Value;
        Settings.NoteSpeed = (float)noteSpeed.Value;
        Settings.Difficulty = (int)difficulty.Value;
    }


}
