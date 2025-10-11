using Godot;
using System;
using System.Collections.Specialized;
using Game.Notes;
using System.Threading;

namespace Game.Generator
{
    public partial class NoteGenerator : Node
    {
        [Export(PropertyHint.File)] string noteScene;
        PackedScene note;
        Conductor conductor;

        public override void _Ready()
        {
            conductor = Conductor.instance;
            conductor.Beat += SpawnNote;
            note = GD.Load<PackedScene>(noteScene);
        }

        public void SpawnNote(int position)
        {
            Note aNote = note.Instantiate<Note>();
            aNote.speed = 50;
            aNote.Position = new Vector2(1000,0);
            GD.Print($"Spawn note {position}");
        }
    }

}

