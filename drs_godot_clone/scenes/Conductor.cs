using Godot;
using System;

public partial class Conductor : AudioStreamPlayer2D
{
    [Export] int BPM = 120;
    [Export] int measures = 4;
    [Export] private Timer offsetTimer;

    float songPosition = 0.0f;
    int songPositionInBeats = 1;
    int crotchet;
    int lastReportedBeat = 0;
    int beatsBeforeStart = 0;
    int measure = 1;


    [Signal]
    public delegate void BeatEventHandler(int position);
    [Signal]
    public delegate void MeasureEventHandler(int position);

    public override void _Ready()
    {
        this.crotchet = 60 / BPM;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Playing)
        {
            songPosition = GetPlaybackPosition() + (float)AudioServer.GetTimeSinceLastMix() - (float)AudioServer.GetOutputLatency();
            songPositionInBeats = (int)Math.Floor(songPosition / crotchet) + beatsBeforeStart;
            ReportBeat();
        }
    }

    public void ReportBeat()
    {
        if (lastReportedBeat < songPositionInBeats)
        {
            if (measure > measures)
            {
                measure = 1;
            }
            EmitSignal(SignalName.Beat, songPositionInBeats);
            EmitSignal(SignalName.Measure, measure);
            lastReportedBeat = songPositionInBeats;
            measure += 1;
        }
    }

    public void BeatOffset(int num)
    {
        this.beatsBeforeStart = num;
        offsetTimer.WaitTime = crotchet;
        offsetTimer.Start();
    }

    public void OnOffsetTimerTimeout()
    {
        this.songPositionInBeats += 1;
        if (this.songPositionInBeats < this.beatsBeforeStart - 1)
        {
            offsetTimer.Start();
        }
        else if (this.songPositionInBeats == this.beatsBeforeStart)
        {
            offsetTimer.WaitTime = offsetTimer.WaitTime -
                                    (AudioServer.GetTimeToNextMix() + AudioServer.GetOutputLatency());
            offsetTimer.Start();
        }
        else
        {
            Play();
            offsetTimer.Stop();
        }
        ReportBeat();
    }


}
