using Godot;
using System;
namespace Game.VFX;
public partial class NoteHitText : Node2D
{
    [Export] float speed;
    [Export] float fadeTime;
    [Export] Label text;
    
    public NoteHitText(int hitLevel)
    {
        if (hitLevel == 3)
        {
            text.Text = "PERFECT!!!";
            text.Scale = new Vector2(2.5f,2);
            
        }
        if (hitLevel == 2)
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
        float dt = (float)delta;
        Color c = Modulate;
        c.A -= dt/fadeTime;
        Modulate = c;
        GlobalPosition = new Vector2(GlobalPosition.X, GlobalPosition.Y - speed * dt);
    }


}
