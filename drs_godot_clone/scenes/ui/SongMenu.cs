using System;
using Godot;
using Godot.Collections;

namespace Game.UI;

public partial class SongMenu : Node2D
{
    [Export(PropertyHint.File)] string gameScene;
    [Export(PropertyHint.File)] string songEntryFile;
    [Export(PropertyHint.Dir)] string jsonDir;
    [Export] VBoxContainer songList;
    Dictionary<string, Dictionary<string, string>> songs;
    public override void _Ready()
    {
        LoadJsonFile();
        foreach (var song in songs.Values)
        {
            AddSongElement(song);
        }
    }

    private void AddSongElement(Dictionary<string, string> song)
    {
        var entry = GD.Load<PackedScene>(songEntryFile).Instantiate<SongEntry>();
        entry.SetSongVariables(song);
        entry.OnStartButtonPressed(SwitchSceneToSong);
        songList.AddChild(entry);
    }

    void SwitchSceneToSong(string midiFile, string oggFile, int channel)
    {
        PackedScene scene = GD.Load<PackedScene>(gameScene);
        var thing = scene.Instantiate<GameHead>();
        thing.SetSong(oggFile, midiFile, channel);
        AddSibling(thing);
        QueueFree();
    }

    void LoadJsonFile()
    {
        var file = FileAccess.Open(jsonDir + "/songs.json", FileAccess.ModeFlags.Read);
        songs = Json.ParseString(file.GetAsText()).AsGodotDictionary<string, Dictionary<string, string>>();
    }
}