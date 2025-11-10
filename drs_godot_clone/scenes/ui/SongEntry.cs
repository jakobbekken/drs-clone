using Godot;
using System;
using Godot.Collections;

namespace Game.UI;

public partial class SongEntry : Control
{
    [Export] Button selectButton;
    [Export] Button startButton;

    [Export] TextureRect image;
    [Export] Label name;
    [Export] Label artist;
    [Export] Label difficulty;

    static SongEntry selectedSong;

    string key;
    string songMid;
    string songOgg;
    int channel;

    public override void _Ready()
    {
        startButton.Visible = false;
        selectButton.Pressed += SelectSong;
    }

    public override void _Process(double delta)
    {
        if (selectedSong == this)
        {
            startButton.Visible = true;
        }
        else
        {
            startButton.Visible = false;
        }
    }

    public void OnStartButtonPressed(Action<string> action)
    {
        startButton.Pressed += () => { action.Invoke(key); };
    }


    public void SetSongVariables(string key, Dictionary<string, string> song_entry)
    {
        this.key = key;
        name.Text = song_entry["name"];
        artist.Text = song_entry["artist"];
        difficulty.Text = song_entry["difficulty"];
        var imgFile = GD.Load<Texture2D>(song_entry["image"]);
        image.Texture = imgFile;
        songOgg = song_entry[".ogg"];
        songMid = song_entry[".mid"];
        channel = int.Parse(song_entry["channel"]);
    }


    private void SelectSong()
    {
        selectedSong = this;
    }
}
