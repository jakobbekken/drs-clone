using Godot;
using System;
using System.Collections.Specialized;
namespace Game.NoteGenerator
{
    public partial class NoteGenerator : Node
    {

        [Export]
        private Conductor conductor;

        public override void _Ready()
        {
            conductor.Beat += SpawnNote;
        }

        public void SpawnNote(int position)
        {
            GD.Print($"Spawn note {position}");
        }
    }

}

