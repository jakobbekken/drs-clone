using Godot;
using System;
namespace Game.VFX;

public partial class NoteHitText : Node2D
{

    [Export] float speed = 60;
    [Export] float fadeTime = 1;
    [Export] Label text;
    float timeSpent = 0;
    public void Setup(int hitLevel)
    {
        timeSpent = 0;
        if (hitLevel == 3)
        {
            text.Text = "PERFECT!!";
            text.Scale = new Vector2(2.5f, 2);

        }
        else if (hitLevel == 2)
        {
            text.Text = "Great!";
            text.Scale = new Vector2(1.75f, 1.5f);
        }
        else
        {
            text.Text = "OK";
        }

    }
    public override void _Process(double delta)
    {
        timeSpent += (float)delta;
        Color c = Modulate;
        c.A = (fadeTime - timeSpent) / fadeTime;
        Modulate = c;
        GlobalPosition = new Vector2(GlobalPosition.X, GlobalPosition.Y - speed * (float)delta);
    }


}
