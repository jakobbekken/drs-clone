using Godot;

namespace Game.Stage;
public partial class Foot : Sprite2D
{
    int currState = 0;
    enum States{
        FootUp,
        FootDown,
        Step
    }
    public void Unstep()
    {
        Frame = (int)States.FootUp;
        currState = Frame;
    }
    public void Step(float stepTime)
    {
        if(currState != (int)States.FootUp) return;

        GetTree().CreateTimer(stepTime).Timeout += FootDown;
        Frame = (int)States.Step;
        currState = Frame;
    }
    private void FootDown()
    {
        Frame = (int)States.FootDown;
        currState = Frame;
    }
}
