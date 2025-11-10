using Godot;
using Godot.Collections;

public partial class Settings : Node
{
    public static float ActiveSceneArea { get { return instance.activeSceneArea; } set { instance.activeSceneArea = value; } }
    public static float NoteSpeed { get { return instance.noteSpeed; } set { instance.noteSpeed = value; } }
    public static int Difficulty { get { return instance.difficulty; } set { instance.difficulty = value; } }
    public static Dictionary<string, int> HighScores { get { return instance.highScores; } set { instance.SetHighScore(value); } }
    public static string activeSong = "";



    static Settings instance;
    float activeSceneArea = 60;
    float noteSpeed = 100;
    int difficulty = 1;
    Dictionary<string, int> highScores;
    string highScorePath = "res://highScore.json";
    public override void _Ready()
    {
        if (instance == null)
        {
            instance = this;
            var file = FileAccess.Open(ProjectSettings.GlobalizePath(instance.highScorePath), FileAccess.ModeFlags.Read);
            highScores = Json.ParseString(file.GetAsText()).AsGodotDictionary<string, int>();
        }
        else
        {
            QueueFree();
        }
    }
    private void SetHighScore(Dictionary<string, int> value)
    {
        foreach (var (k, v) in value)
            instance.highScores[k] = v;

        string data = Json.Stringify(instance.highScores);

        var file = FileAccess.Open(ProjectSettings.GlobalizePath(instance.highScorePath), FileAccess.ModeFlags.Write);
        file.StoreString(data);
    }
}
