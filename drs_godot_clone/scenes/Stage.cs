using Godot;
using System.Collections.Generic;
using Game.VFX;
using Game.Notes;
using System;
using System.Runtime.CompilerServices;

namespace Game.Stage
{
    public partial class Stage : Sprite2D
    {
        [Export(PropertyHint.File)] string noteHitTextPath;
        [Export] float footXTolerance = 140f;
        [Export] float stepTime = 0.2f;
        [Export] float keyboardMovementSpeed = 250f;
        [Export] Foot foot0;
        [Export] Foot foot1;
        [Export] Area2D hitbox;
        [Export] WebSocket _socket;
        [Export] float maxStageRange = 0.6f;
        [Export] float minStageRange = 0.2f;
        public float halfSize = 1.0f;
        public float unit = 1.0f;
        public int score = 0;

        float sceneWidth;

        List<VisualNote> notes = new();
        double controllerDelay = 0.05;

        public override void _Ready()
        {
            halfSize = this.Texture.GetSize().X / 2f;
            unit = halfSize / 100;
            hitbox.AreaEntered += AddActiveNote;
            hitbox.AreaExited += RemoveActiveNote;
            _socket.DataReceived += MoveFeet;
            sceneWidth = Texture.GetWidth() / 2f * Scale.X;
        }

        private void RemoveActiveNote(Area2D area)
        {
            if (area is VisualNote note) notes.Remove(note);
        }

        private void AddActiveNote(Area2D area)
        {
            if (area is VisualNote note) notes.Add(note);
        }

        private float Normalize(float input, float max, float min)
        {
            return Mathf.Clamp((input - min) / (max - min),0f,1f); // Return a value between 0, and 1 
        }

        public void MoveFeet(float leftX, float rightX, string leftState, string rightState)
        {
            float maxStage = halfSize - foot0.Texture.GetSize().X / 2 * foot0.Transform.Scale.X; // We multiply by scale in case the foot is scaled in game
            float minStage = -halfSize + foot1.Texture.GetSize().X / 2 * foot1.Transform.Scale.X;
            float normLeft = -Normalize(leftX, maxStageRange, minStageRange);
            float normRight = -Normalize(rightX, maxStageRange, minStageRange);

            foot0.Position = new Vector2(Mathf.Clamp((normLeft + 0.5f) * 2f * halfSize, minStage, maxStage), foot0.Position.Y);
            foot1.Position = new Vector2(Mathf.Clamp((normRight + 0.5f) * 2f * halfSize, minStage, maxStage), foot1.Position.Y);

            if (leftState == "Press") { Step(foot0, stepTime); }
            else if (leftState == "Up") { Unstep(foot0); }

            if (rightState == "Press") { Step(foot1, stepTime); }
            else if (rightState == "Up") { Unstep(foot1); }
        }

        public override void _Process(double delta)
        {
            foreach (VisualNote note in notes)
            {
                if (note == null)
                {
                    notes.Remove(note);
                    continue;
                }
                float yDistance = note.GlobalPosition.Y - hitbox.GlobalPosition.Y;
                if (yDistance > 0) note.Freeze(controllerDelay);
            }
            //KeyboardControls(delta);
        }

        Vector2 ClampedPos(float pos)
        {
            return new Vector2(Mathf.Min(Mathf.Max(pos, -216), 216), 0);
        }

        private void Step(Foot foot, float time)
        {
            foot.Step(time);
            foreach (VisualNote note in notes.ToArray())
            {
                if (!IsInstanceValid(note)) continue;
                if (Mathf.Abs(note.GlobalPosition.X - foot.GlobalPosition.X) < footXTolerance)
                {

                    float yDistance = Mathf.Abs(note.GlobalPosition.Y - hitbox.GlobalPosition.Y);
                    int timing = TimingRating(yDistance);
                    if (timing > 0)
                    {
                        GivePoints(timing);
                        NoteVFX(foot, timing);
                        notes.Remove(note);
                        note.QueueFree();
                        break;
                    }

                }
            }

        }
        #region NoteHit
        private void NoteVFX(Foot foot, int timing)
        {
            PackedScene hitTextScene = GD.Load<PackedScene>(noteHitTextPath);
            NoteHitText text = hitTextScene.Instantiate<NoteHitText>();
            text.Setup(timing);
            text.GlobalPosition = foot.GlobalPosition - Vector2.Up * 15;
            AddSibling(text);
        }

        private void GivePoints(int timing)
        {
            score += timing * 10;
            switch (timing)
            {
                case 3:
                    GD.Print("PERFECT!!");
                    break;
                case 2:
                    GD.Print("Great!");
                    break;
                default:
                    GD.Print("OK");
                    break;
            }
            GD.Print("Score: " + score);
        }

        private int TimingRating(float yDistance)
        {
            switch (yDistance)
            {
                case var _ when yDistance < 10f: //perfect
                    return 3;
                case var _ when yDistance < 25f: //good
                    return 2;
                case var _ when yDistance < 45f: //ok
                    return 1;
                default:                         //miss
                    return 0;
            }
        }
        #endregion NoteHit
        private void Unstep(Foot foot)
        {
            foot.Unstep();
        }
        private void KeyboardControls(double delta)
        {
            float foot0Movement = Input.GetActionStrength("wasd_right") - Input.GetActionStrength("wasd_left");
            float foot1Movement = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");

            foot0.Position += ClampedPos(foot0Movement * (float)delta * keyboardMovementSpeed);
            foot1.Position += ClampedPos(foot1Movement * (float)delta * keyboardMovementSpeed);
            if (Input.GetActionStrength("wasd_up") > 0f)
            {
                Step(foot0, stepTime);
            }
            else
            {
                Unstep(foot0);
            }
            if (Input.GetActionStrength("ui_up") > 0f)
            {
                Step(foot1, stepTime);
            }
            else
            {
                Unstep(foot1);
            }
        }
    }
}
