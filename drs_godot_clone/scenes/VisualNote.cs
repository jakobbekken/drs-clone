using Godot;
using System;
using System.Text.Json.Serialization.Metadata;

namespace Game.Notes
{
    public partial class VisualNote : Area2D
    {
        [Export] public float speed = 400f; // pixels per second
        public bool isHit = false;

        public override void _Process(double delta)
        {
            Position += new Vector2(0, (float)(speed * delta)); // move down

            // optional: remove note if it passes hit zone
            if (Position.Y > 1200 && !isHit)
            {
                QueueFree();
            }
        }

    }
}


