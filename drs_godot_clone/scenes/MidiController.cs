using Godot;
using System;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.Linq;
using Game.Notes;
using Game.Stage;

public partial class MidiController : Node
{
    // Scenes
    [Export] public PackedScene NoteScene { get; set; }
    [Export] public AudioStreamPlayer2D Audio;

    // Stage
    [Export] public Stage _stage;
    public float _stageSize;
    public float _stagePosY;
    Timer noteTimer = new Timer();

    // Notes
    [Export] public float _noteSpawnHight = 0; // 0 is at y = 0
    [Export] public double _noteSpeed;
    public double _addBpmChange = 400;
    private List<Note> _notes;
    private List<Note> trueNotes = new();
    private List<VisualNote> _activeNotes = new();
    private int _nextNoteIndex;


    //Midi file and song
    string godotPath = "";
    private MidiFile _midiFile;
    private TempoMap _tempoMap;
    private double _songStartTime;
    private double _BPM;
    private bool _canSpawnNote = true;
    private int _leadChannel;

    public Dictionary<int, List<string>> groups = new Dictionary<int, List<string>>
    {
        { 1, new List<string> { "C", "CSharp", "D" } },
        { 2, new List<string> { "DSharp", "E", "F" } },
        { 3, new List<string> { "FSharp", "G", "GSharp" } },
        { 4, new List<string> { "A", "ASharp", "B" } }
    };

    public override void _Ready()
    {
        _stagePosY = _stage.Position.Y;
        _stageSize = _stage.Texture.GetSize().X * _stage.Scale.X;

        string filePath = ProjectSettings.GlobalizePath(godotPath);
        _midiFile = MidiFile.Read(filePath);

        _tempoMap = _midiFile.GetTempoMap();
        _notes = _midiFile.GetNotes().OrderBy(n => n.Time).ToList();
        _noteSpeed = _BPM + _addBpmChange;
        _noteSpawnHight = _stagePosY - 1200;

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

        float travelDistance = _noteSpawnHight - _stagePosY;
        double travelTime = travelDistance / _noteSpeed; // in seconds

        _songStartTime = Time.GetTicksMsec() / 1000.0;

        double firstNoteTimeSec = TimeConverter
            .ConvertTo<MetricTimeSpan>(trueNotes[0].Time, _tempoMap)
            .TotalMicroseconds / 1_000_000.0;
        double audioDelay = Math.Max(0, firstNoteTimeSec - travelTime);

        GD.Print(GetStaticBPM());

        GetTree().CreateTimer(audioDelay).Timeout += () => Audio.Play();
        AddChild(noteTimer); // Add the timer to the current node's children
        noteTimer.WaitTime = 0.01;
        noteTimer.OneShot = true; // Set to true for a single timeout
        noteTimer.Timeout += SetNoteSpawn;
        //noteTimer.Start(); // Enable this to prevent notes from spawning too close 
    }

    private void SetNoteSpawn()
    {
        _canSpawnNote = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_nextNoteIndex >= trueNotes.Count)
            return;
        if (timeSinceLastSpawnedNote > 0)
        {
            timeSinceLastSpawnedNote -= delta;
        }

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
                GetDynamicBPM(next);
                if(_nextNoteIndex % 1==0 && _canSpawnNote && next.Channel == _leadChannel)
                {
                    TriggerNoteVisual(next);
                    //_canSpawnNote = false; // Enable this to prevent notes from spawning too close
                    //noteTimer.Start();
                }
                _nextNoteIndex++;
            }
            else break;
        }
    }
    private void TriggerNoteVisual(Note nextNote)
    {
        if (timeSinceLastSpawnedNote > 0) return;
        // Spawn a note visual when the MIDI event hits
        var instance = NoteScene.Instantiate<VisualNote>();
        AddChild(instance);
        timeSinceLastSpawnedNote = noteDelay;
        // Example: horizontal position based on note pitch
        float columnWidth = _stageSize / 4f;
        float stageCenterX = _stage.GlobalPosition.X;
        float x = stageCenterX - _stageSize / 2f + columnWidth * (nextNote.NoteNumber % 4 + 0.5f);
        instance.Position = new Vector2(x, _noteSpawnHight);
        instance.speed = _noteSpeed;

        _activeNotes.Add(instance);
        GD.Print($"Triggered {nextNote.NoteName}");
    }

    private string GetStaticBPM()
    {
        var tempoChanges = _tempoMap.GetTempoChanges();

        // Collect all BPMs
        List<double> bpms = tempoChanges.Select(t => t.Value.BeatsPerMinute).ToList();

        if (bpms.Count == 0)
        {
            return "Unknown BPM";
        }


        double minBpm = bpms.Min();
        double maxBpm = bpms.Max();

        if (minBpm == maxBpm)
        {
            return $"BPM: {maxBpm:F2}";
        }
        else
        {
            return $"BPM: {minBpm:F2} - {maxBpm:F2}";
        }
    }

    private void GetDynamicBPM(Note nextNote)
    {
        var tempoAtNote = _tempoMap.GetTempoAtTime(new MidiTimeSpan(nextNote.Time));
        double bpmAtNote = tempoAtNote.BeatsPerMinute;

        _BPM = bpmAtNote;
    }
    public void SetSong(string ogg, string mid, int channel)
    {
        Audio.Stream = GD.Load<AudioStream>(ogg);
        godotPath = mid;
        _leadChannel = channel;
    }
}



