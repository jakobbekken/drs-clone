using Godot;
using System;
using System.Text.Json.Serialization.Metadata;

namespace Game.Notes
{
    public partial class VisualNote : Area2D
    {
        public double speed = 0f; // pixels per second
        public bool isHit = false;

        public override void _Process(double delta)
        {
            Position += new Vector2(0, (float)(speed * delta)); // move note

            // remove note if it passes hit zone
            if (Position.Y > 1200 && !isHit)
            {
                QueueFree();
            }
        }

    }
}


