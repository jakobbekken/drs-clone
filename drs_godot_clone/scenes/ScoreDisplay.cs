using Godot;
using System;
using Game.Stage;
using Godot.Collections;
public partial class ScoreDisplay : Control
{
    [Export] Stage stage;
    [Export] Label scoreText;
    [Export] Label highScoreText;

    int score = 0;
    Dictionary<string, int> highscores;

    public override void _Ready()
    {
        highscores = Settings.HighScores;
        GetHighScore();
    }

    public override void _Process(double delta)
    {
        score = stage.score;
        scoreText.Text = score.ToString();

        if (score > highScoreText.Text.ToInt())
        {
            highScoreText.Text = score.ToString();
            highscores[Settings.activeSong] = score;
        }
    }
    private void GetHighScore()
    {
        int highScore = highscores.TryGetValue(Settings.activeSong, out int e) ? highscores[Settings.activeSong] : 1;
        highScoreText.Text = highScore.ToString();
        highscores[Settings.activeSong] = highScore;
    }
    public override void _ExitTree()
    {
        Settings.HighScores = highscores;
    }
}
