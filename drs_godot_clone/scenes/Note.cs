using Godot;
using System;
using System.Text.Json.Serialization.Metadata;

namespace Game.Note
{
    public partial class Note : Area2D
    {
        [Export] int speed;
        [Export] string color;
        

        //[Signal]
        //public delegate void DestroyedEventHandler(Note note);
        public void Move(int speed, double delta)
        {
            Position += new Vector2(0, speed * (float)delta);
        }

        bool hasFreezed = false;
        double freezeTime;
        
        public override void _Process(double delta)
        {
            if (freezeTime > 0) freezeTime -= delta;
            else Move(speed, delta);
        }

        public void Freeze(double time)
        {
            if (hasFreezed) return;
            freezeTime = time;
            hasFreezed = true;
        }

        public void DestroyedNote()
        {
            //EmitSignal(SignalName.Destroyed, this);
            this.QueueFree();
        }

    }
}


