using Godot;

[Tool]
public partial class AudioOptionsResource : Resource
{
    [Export] public AudioStream AudioStream { get; set; }
    [Export] public string BusName { get; set; } = "Master";
    [Export] public float VolumeDb { get; set; } = -10.0f;
    [Export] public float PitchScale { get; set; } = 1.0f;
    [Export] public float StartOffset { get; set; } = 0.0f;
    [Export] public float FadeInDuration { get; set; } = 0.0f;
    [Export] public float FadeOutDuration { get; set; } = 0.0f;
    [Export] public bool Mute { get; set; } = false;
    [Export] public bool Is3D { get; set; } = false;
    [Export] public Vector3 Position { get; set; } = Vector3.Zero;
    [Export] public int Priority { get; set; } = 0;
}
