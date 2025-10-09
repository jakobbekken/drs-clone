using Godot;
using System;
namespace Game;
public partial struct InputData
{
    public float foot0Pos, foot1Pos;
    public int foot0Step, foot1Step;
    public InputData(float foot0Pos, float foot1Pos, int foot0Step, int foot1Step)
    {
        this.foot0Pos = foot0Pos;
        this.foot1Pos = foot1Pos;
        this.foot0Step = foot0Step;
        this.foot1Step = foot1Step;
    }
}
