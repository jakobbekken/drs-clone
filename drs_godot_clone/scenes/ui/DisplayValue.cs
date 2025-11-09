using Godot;
using System;

public partial class DisplayValue : Label
{
    [Export] Slider slider;
    public override void _Process(double delta)
    {
        Text = slider.Value.ToString();
    }

}
