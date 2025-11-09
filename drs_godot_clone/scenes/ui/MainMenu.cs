using Godot;
using System;

namespace Game.UI;

public partial class MainMenu : Node2D
{
    [Export(PropertyHint.File)] string startPath;
    [Export(PropertyHint.File)] string optionsPath;
    [Export] Button startButton;
    [Export] Button optionsButton;
    [Export] Button quitButton;

    public override void _Ready()
    {
        startButton.Pressed += StartGame;
        optionsButton.Pressed += EnterOptions;
        quitButton.Pressed += QuitGame;
    }

    private void StartGame()
    {
        GetTree().ChangeSceneToFile(startPath);
    }
    private void EnterOptions()
    {
        GetTree().ChangeSceneToFile(optionsPath);
    }
    private void QuitGame()
    {
        GetTree().Quit();
    }

}
