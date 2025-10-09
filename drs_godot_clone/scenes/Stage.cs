using Godot;
using System.Collections.Generic;
namespace Game.Stage
{
    public partial class Stage : Sprite2D
    {
        int score = 0;
        [Export] float footXTolerance = 15f;
        [Export] float stepTime = 0.2f;
        [Export] float keyboardMovementSpeed = 69f;
        [Export] Foot foot0;
        [Export] Foot foot1;
        [Export] Area2D hitbox;

        List<Note.Note> notes = new();
        public override void _Ready()
        {
            hitbox.AreaEntered += AddActiveNote;
        }
        

        private void AddActiveNote(Node2D body)
        {
            if (body is Note.Note note) notes.Add(note);
        }

        public void MoveFeet(InputData data)
        {
            if (data.foot0Step == 1f) { Step(foot0, stepTime); }
            else if (data.foot0Step == -1f) { Unstep(foot0); }

            if (data.foot1Step == 1) Step(foot1, stepTime);
            else if (data.foot1Step == -1) { Unstep(foot1); }

            foot0.Position = new Vector2(data.foot0Pos, foot0.Position.Y);
            foot1.Position = new Vector2(data.foot1Pos, foot1.Position.Y);
        }

        public override void _Process(double delta)
        {
            KeyboardControls(delta);
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
        Vector2 ClampedPos(float pos)
        {
            return new Vector2(Mathf.Min(Mathf.Max(pos, -216), 216),0);
        }

        private void Step(Foot foot, float time)
        {
            foot.Step(time);
            foreach (Note.Note note in notes)
            {
                if (Mathf.Abs(note.GlobalPosition.X - foot.GlobalPosition.X) < footXTolerance)
                {
                    
                    float yDistance = Mathf.Abs(note.GlobalPosition.Y - GlobalPosition.Y);
                    int timing = TimingRating(yDistance);
                    if (timing > 0) 
                    {
                        GivePoints(timing);
                        NoteVFX(timing);
                        notes.Remove(note);
                        note.QueueFree();
                    }
                    
                }
            }

        }
#region NoteHit
        private void NoteVFX(int timing)
        {
            GD.Print("Wow, pretty effects!");
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
            GD.Print("Score: "+ score);
        }

        private int TimingRating(float yDistance)
        {
            int howGood = 3;
            switch (yDistance)
            {
                case var _ when yDistance < 10f:  //perfect
                    howGood = 3;
                    break;
                case var _ when yDistance < 25f: //good
                    howGood = 2;
                    break;
                case var _ when yDistance < 45f: // ok
                    howGood = 1;
                    break;
                default:                         // near miss
                    break;
            }
            return howGood;
        }
#endregion NoteHit
        private void Unstep(Foot foot)
        {
            foot.Unstep();
        }
    }
}
