using Godot;

public class ResizeAspectRatio : AspectRatioContainer
{
    public override void _Ready()
    {
        GetViewport().Connect("size_changed", this, "OnViewPortChanged");
        OnViewPortChanged();
    }

    public async void OnViewPortChanged()
    {
        await ToSignal(GetTree(), "idle_frame");
        Vector2 viewportSize = GetViewport().Size;
        RectPivotOffset = new Vector2(RectSize.x / 2, RectSize.y / 2);
        if (viewportSize.x > 1400)
        {
            RectScale = new Vector2(1.3f, 1.3f);
        }
        else if (viewportSize.x > 1280)
        {
            RectScale = new Vector2(1.1f, 1.1f);
        }
        else
        {
            RectScale = new Vector2(1, 1);
        }
    }
}
