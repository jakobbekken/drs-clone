using Godot;
using System;

namespace Game.UI;

public partial class MainMenu : Node2D
{
    [Export(PropertyHint.File)] string startPath;
    [Export(PropertyHint.File)] string optionsEditPath;
    [Export] Button startButton;
    [Export] Button optionsButton;
    [Export] Button quitButton;
    [Export(PropertyHint.File)] string optionsElementFile;

    public override void _Ready()
    {
        startButton.Pressed += StartGame;
        optionsButton.Pressed += EnterOptions;
        quitButton.Pressed += QuitGame;
        GetTree().CreateTimer(0.05).Timeout += () =>
        {
            AddSibling(GD.Load<PackedScene>(optionsElementFile).Instantiate());
        };
    }

    private void StartGame()
    {
        GetTree().ChangeSceneToFile(startPath);
    }
    private void EnterOptions()
    {
        GetTree().ChangeSceneToFile(optionsEditPath);
    }
    private void QuitGame()
    {
        GetTree().Quit();
    }

}
