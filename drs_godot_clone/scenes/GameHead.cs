using Godot;
using System;

public partial class GameHead : Node2D
{
    [Export] MidiController controller;

    public void SetSong(string ogg, string mid, int channel)
    {
        controller.SetSong(ogg, mid, channel);
    }
}
