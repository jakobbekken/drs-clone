using Godot;
using System;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.Linq;
using Game.Notes;
using System.IO;
using System.Net.Security;

public partial class MidiController : Node
{
    [Export] public PackedScene NoteScene { get; set; }
    [Export] public AudioStreamPlayer2D Audio;
    string godotPath;
    private MidiFile _midiFile;
    private TempoMap _tempoMap;
    private List<Note> _notes;
    private List<Note> trueNotes = new();
    private double _songStartTime;
    private int _nextNoteIndex = 0;
    private List<VisualNote> _activeNotes = new();

    public override void _Ready()
    {
        string filePath = ProjectSettings.GlobalizePath(godotPath);
        _midiFile = MidiFile.Read(filePath);
        _tempoMap = _midiFile.GetTempoMap();
        _notes = _midiFile.GetNotes().OrderBy(n => n.Time).ToList();
        long time = -1;
        foreach (var note in _notes)
        {
            if (note.Time == time)
            {
                time = note.Time;
            }
            else
            {
                time = note.Time;
                trueNotes.Add(note);
            }
        }

        _songStartTime = Time.GetTicksMsec() / 1000.0;
        GetTree().CreateTimer(1.7).Timeout += () => Audio.Play();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_nextNoteIndex >= trueNotes.Count)
            return;

        // Current elapsed time (in seconds)
        double elapsed = (Time.GetTicksMsec() / 1000.0) - _songStartTime;

        while (_nextNoteIndex < trueNotes.Count)
        {
            var next = trueNotes[_nextNoteIndex];
            double noteTimeSec = TimeConverter
                .ConvertTo<MetricTimeSpan>(next.Time, _tempoMap)
                .TotalMicroseconds / 1_000_000.0;

            if (elapsed >= noteTimeSec)
            {

                TriggerNoteVisual(next);
                _nextNoteIndex++;
            }
            else break;
        }
    }

    public override void _Process(double delta)
    {
        for (int i = _activeNotes.Count - 1; i >= 0; i--)
        {
            var note = _activeNotes[i];
            if (!IsInstanceValid(note))
            {
                _activeNotes.RemoveAt(i);
                continue; // skip any access to disposed node
            }

            note.Position += new Vector2(0, (float)(200 * delta));

            if (note.Position.Y > 1200)
            {
                note.QueueFree();
                _activeNotes.RemoveAt(i);
            }
        }
    }

    private void TriggerNoteVisual(Note nextNote)
    {
        // Spawn a note visual when the MIDI event hits
        var instance = NoteScene.Instantiate<VisualNote>();
        AddChild(instance);

        // Example: horizontal position based on note pitch
        float x = nextNote.NoteNumber % 12 * 50 + 100;
        instance.Position = new Vector2(x, 0);

        _activeNotes.Add(instance);
        GD.Print($"Triggered {nextNote.NoteName}");
    }
    public void SetSong(string ogg, string mid)
    {
        Audio.Stream = GD.Load<AudioStream>(ogg);
        godotPath = mid;
    }
}