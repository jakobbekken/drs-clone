using Godot;
using System;

public partial class ToSceneButton : Button
{
    [Export(PropertyHint.File)] string scene;
    public override void _Ready()
    {
        this.Pressed += () => GetTree().ChangeSceneToFile(scene);
    }
}
