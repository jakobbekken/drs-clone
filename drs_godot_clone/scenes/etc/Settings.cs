using Godot;
using System;

public partial class Settings : Node
{
    public static float ActiveSceneArea { get { return instance.activeSceneArea; } set { instance.activeSceneArea = value; } }
    public static float NoteSpeed { get { return instance.noteSpeed; } set { instance.noteSpeed = value; } }
    public static int Difficulty { get { return instance.difficulty; } set { instance.difficulty = value; } }

    static Settings instance;
    float activeSceneArea = 60;
    float noteSpeed = 100;
    int difficulty = 1;
    public override void _Ready()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            QueueFree();
        }
    }


}
