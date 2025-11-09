using Godot;
using System;
using Game.Stage;
public partial class ScoreDisplay : Control
{
    [Export] Stage stage;
    [Export] Label score;
    [Export] Label highScore;

    public override void _Ready()
    {
        GetHighScore();
    }

    public override void _Process(double delta)
    {
        int num = stage.score;
        score.Text = num.ToString();
        if (num > highScore.Text.ToInt())
        {
            highScore.Text = num.ToString();
        }
    }
    private void GetHighScore()
    {
        highScore.Text = "9001";
    }
}
