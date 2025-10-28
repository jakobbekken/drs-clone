using Game.VFX;
using Godot;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
namespace Game.Stage
{
    public partial class Stage : Sprite2D
    {
        int score = 0;
        [Export(PropertyHint.File)] string noteHitTextPath;
        [Export] float footXTolerance = 15f;
        [Export] float stepTime = 0.2f;
        [Export] float keyboardMovementSpeed = 250f;
        [Export] Foot foot0;
        [Export] Foot foot1;
        [Export] Area2D hitbox;
        [Export] WebSocket _socket;
        float sceneWidth;
        List<Note.Note> notes = new();

        double controllerDelay = 0.1;

        public override void _Ready()
        {
            hitbox.AreaEntered += AddActiveNote;
            _socket.DataReceived += MoveFeet;
            sceneWidth = Texture.GetWidth() / 2f * Scale.X;
        }


        private void AddActiveNote(Node2D body)
        {
            if (body is Note.Note note) notes.Add(note);
        }

        public void MoveFeet(float leftX, float rightX, string leftState, string rightState)
        {
            foot0.Position = new Vector2(keyboardMovementSpeed * -leftX, foot0.Position.Y);
            foot1.Position = new Vector2(keyboardMovementSpeed * -rightX, foot1.Position.Y);

            if (leftState == "Press") { Step(foot0, stepTime); }
            else if (leftState == "Up") { Unstep(foot0); }

            if (rightState == "Press") { Step(foot1, stepTime); }
            else if (rightState == "Up") { Unstep(foot1); }
        }

        public override void _Process(double delta)
        {
            foreach (Note.Note note in notes)
            {
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
            foreach (Note.Note note in notes)
            {
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
                case var _ when yDistance < 10f:  //perfect
                    return 3;
                case var _ when yDistance < 25f: //good
                    return 2;
                case var _ when yDistance < 45f: // ok
                    return 1;
                default:                         // miss
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
