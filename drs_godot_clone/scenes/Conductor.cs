using Godot;
using System;
namespace Game
{
    public partial class Conductor : AudioStreamPlayer2D
    {
        [Export] float BPM = 120f;
        [Export] int measures = 4;
        [Export] private Timer offsetTimer;

        public static Conductor instance { get; private set; }

        double songPosition = 0.0d;
        int songPositionInBeats = 0;
        float crotchet;
        double lastBeatTime = 0.0d;
        int lastReportedBeat = 0;
        int beatsBeforeStart = 0;
        int measure = 1;


        [Signal]
        public delegate void BeatEventHandler(int position);
        [Signal]
        public delegate void MeasureEventHandler(int position);

        public override void _Ready()
        {
            offsetTimer.Timeout += OnOffsetTimerTimeout;
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                QueueFree();
            }
            this.crotchet = 60 / BPM;
            Console.WriteLine("Ready");
        }

        public override void _PhysicsProcess(double delta)
        {
            if (Playing)
            {
                songPosition += GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - songPosition;
                songPosition -= AudioServer.GetOutputLatency();
                songPositionInBeats = (int)Math.Floor(songPosition / crotchet) + beatsBeforeStart;
                
                ReportBeat();
            }
        }

        public void ReportBeat()
        {
            if (songPosition > lastBeatTime + crotchet)
            {
                if (measure > measures)
                {
                    measure = 1;
                }
                
                EmitSignal(SignalName.Beat, songPositionInBeats);
                EmitSignal(SignalName.Measure, measure);
                lastBeatTime += crotchet;
                measure += 1;
                
            }
            
        }

        public void BeatOffset(int num)
        {
            this.beatsBeforeStart = num;
            offsetTimer.WaitTime = crotchet;
            offsetTimer.Start();
            Console.WriteLine("BeatOffset function");
        }

        public void OnOffsetTimerTimeout()
        {
            this.songPositionInBeats += 1;
            if (this.songPositionInBeats < this.beatsBeforeStart - 1)
            {
                offsetTimer.Start();
            }
            else if (this.songPositionInBeats == this.beatsBeforeStart - 1)
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
            Console.WriteLine("OnOffsetTimerTimeout");
            ReportBeat();
        }
    }
}
