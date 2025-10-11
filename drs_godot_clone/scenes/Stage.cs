using Godot;
namespace Game.Stage
{
    public partial class Stage : Sprite2D
    {
        [Export] Foot foot0;
        [Export] Foot foot1;
        [Export] float stepTime;
        [Export] float speed = 69f;

        Conductor conductor;

        public override void _Ready()
        {
            conductor = Conductor.instance;
            
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

            foot0.Position += ClampedPos(foot0Movement * (float)delta * speed);
            foot1.Position += ClampedPos(foot1Movement * (float)delta * speed);
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

        }
        private void Unstep(Foot foot)
        {
            foot.Unstep();
        }
    }
}
