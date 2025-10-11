using Godot;
using System;
using System.Net;
using System.Text.Json.Serialization.Metadata;

namespace Game.Notes
{
    public partial class Note : Area2D
    {
        [Export] public int speed = 0;
        [Export] string color;
        [Export] Timer deathTimer;

        //[Signal]
        //public delegate void DestroyedEventHandler(Note note);

        public override void _PhysicsProcess(double delta)
        {
            Position += new Vector2(0, speed * (float)delta);
        }

        public void HitNote(int hitValue)
        {
            deathTimer.Start();
            DeleteNote();
        }

        public void DeleteNote()
        {
            QueueFree();
        }

    }
}


