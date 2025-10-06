using Godot;
using System.Collections.Generic;
namespace Game.Stage
{
    public partial class Stage : Sprite2D
    {
        [Export] Sprite2D foot0;
        [Export] Sprite2D foot1;
        [Export] float stepSize;
        [Export] float speed = 20f;
        public void MoveFeet(Dictionary<string, float> foot0Dict, Dictionary<string, float> foot1Dict)
        {
            float foot0pos = foot0Dict["pos"];
            float foot1pos = foot1Dict["pos"];

            if (foot0Dict["state"] == 1f) { Step(foot0, stepSize); }
            else if (foot0Dict["state"] == -1f) { Unstep(foot0); }

            if (foot1Dict["state"] == 1) Step(foot1, stepSize);
            else if (foot1Dict["state"]==-1) { Unstep(foot1); }

            foot0.Position = new Vector2(foot0pos, foot0.Position.Y);
            foot1.Position = new Vector2(foot1pos, foot1.Position.Y);
        }
        public override void _Process(double delta)
        {
            KeyboardControls(delta);
        }
        private void KeyboardControls(double delta)
        {
            float foot0Movement = Input.GetActionStrength("wasd_right") - Input.GetActionStrength("wasd_left");
            float foot1Movement = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");

            foot0.Position += new Vector2(foot0Movement * (float)delta * speed, 0);
            foot1.Position += new Vector2(foot1Movement * (float)delta * speed, 0);
            if (Input.GetActionStrength("wasd_up") > 0f)
            {
                Step(foot0, stepSize);
            }
            else
            {
                Unstep(foot0);
            }
            if (Input.GetActionStrength("ui_up") > 0f)
            {
                Step(foot1, stepSize);
            }
            else
            {
                Unstep(foot1);
            }
        }


        private void Step(Sprite2D foot, float distance)
        {
            foot.Position = new Vector2(foot.Position.X, 0-distance);
        }
        private void Unstep(Sprite2D foot)
        {
            if (foot.Position.Y == 0) return;
            foot.Position = new Vector2(foot.Position.X, 0);
        }
    }
}
