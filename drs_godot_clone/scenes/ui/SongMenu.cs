using System;
using System.Linq;
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
        foreach (var (key, song) in songs)
        {
            AddSongElement(key, song);
        }
    }

    private void AddSongElement(string key, Dictionary<string, string> song)
    {
        var entry = GD.Load<PackedScene>(songEntryFile).Instantiate<SongEntry>();
        entry.SetSongVariables(key, song);
        entry.OnStartButtonPressed(SwitchSceneToSong);
        songList.AddChild(entry);
    }

    void SwitchSceneToSong(string key)
    {
        Settings.activeSong = key;
        var song = songs[key];
        GD.Print(song);
        PackedScene scene = GD.Load<PackedScene>(this.gameScene);
        var gameScene = scene.Instantiate<GameHead>();
        gameScene.SetSong(song[".ogg"], song[".mid"], song["channel"].ToInt());
        AddSibling(gameScene);
        QueueFree();
    }

    void LoadJsonFile()
    {
        var file = FileAccess.Open(jsonDir + "/songs.json", FileAccess.ModeFlags.Read);
        songs = Json.ParseString(file.GetAsText()).AsGodotDictionary<string, Dictionary<string, string>>();
    }
}