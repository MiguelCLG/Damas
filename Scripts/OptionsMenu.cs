using Godot;
using System;

public class OptionsMenu : Panel
{

    public override void _Ready()
    {
        GetNode<HSlider>("%AudioSlider").Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master"));
    }

    public void OnClose()
    {
        Visible = false;
    }

    public void FlagClicked(string locale)
    {
        GetNode<TranslationHandler>("/root/TranslationHandler").LoadTranslationLocally(locale);
    }

    public void OnAudioSliderChanged(float value)
    {
        if (value < -19f) AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), true);
        else
        {
            AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), false);
        }
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), value);
    }


}
