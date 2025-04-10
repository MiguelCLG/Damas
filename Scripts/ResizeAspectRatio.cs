using Godot;
using System;

public class ResizeAspectRatio : AspectRatioContainer
{
    private Timer resizeDebounceTimer;

    [Export] public float MinWidth = 800f;
    [Export] public float MaxWidth = 1600f;
    [Export] public float MinScale = 1f;
    [Export] public float MaxScale = 1.2f;

    [Export] public float PortraitScaleMultiplier = 0.9f; // Scale down in portrait if needed
    public float ScaleLerpSpeed = 5f; // How fast to interpolate (higher = faster)

    private Vector2 targetScale;

    public override void _Ready()
    {
        GetViewport().Connect("size_changed", this, nameof(OnViewportChanged));

        resizeDebounceTimer = new Timer();
        resizeDebounceTimer.WaitTime = 0.2f;
        resizeDebounceTimer.OneShot = true;
        resizeDebounceTimer.Connect("timeout", this, nameof(CalculateTargetScale));
        AddChild(resizeDebounceTimer);

        CalculateTargetScale();
    }

    public override void _Process(float delta)
    {
        // Smoothly lerp towards the target scale
        RectScale = RectScale.LinearInterpolate(targetScale, delta * ScaleLerpSpeed);
    }

    private void OnViewportChanged()
    {
        if (!resizeDebounceTimer.IsStopped())
            resizeDebounceTimer.Stop();

        resizeDebounceTimer.Start();
    }

    public void CalculateTargetScale()
    {
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;

        //RectPivotOffset = new Vector2(RectSize.x / 2, RectSize.y / 2);

        float width = viewportSize.x;
        float height = viewportSize.y;

        bool isPortrait = height > width;

        float t = InverseLerp(MinWidth, MaxWidth, width);
        float baseScale = Mathf.Lerp(MinScale, MaxScale, t);

        if (isPortrait)
        {
            baseScale *= PortraitScaleMultiplier;
        }

        targetScale = new Vector2(baseScale, baseScale);
    }

    private float InverseLerp(float a, float b, float v)
    {
        if (Mathf.Abs(a - b) < 0.00001f)
            return 0f;

        return Mathf.Clamp((v - a) / (b - a), 0f, 1f);
    }
}
