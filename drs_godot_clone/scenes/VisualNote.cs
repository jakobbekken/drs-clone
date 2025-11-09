using Godot;
using System;
using System.Text.Json.Serialization.Metadata;

namespace Game.Notes
{
    public partial class VisualNote : Area2D
    {
        public double speed = 0f; // pixels per second

        bool hasFreezed = false;
        double freezeTime;

        public override void _Process(double delta)
        {
            if (freezeTime > 0) freezeTime -= delta;
            else Move(delta);
        }
        private void Move(double delta)
        {
            Position += new Vector2(0, (float)(speed * delta)); // move down

            // remove note if it passes hit zone
            if (Position.Y > 1200)
            {
                QueueFree();
            }
        }

        public void Freeze(double time)
        {
            if (hasFreezed) return;
            freezeTime = time;
            hasFreezed = true;
        }

    }
}


