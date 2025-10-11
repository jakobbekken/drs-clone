using Godot;
using System;


namespace Game
{
    public partial class Main : Node2D
    {
        Conductor conductor;
        public override void _Ready()
        {
            conductor = Conductor.instance;
            conductor.BeatOffset(0);
        }

    }

}
