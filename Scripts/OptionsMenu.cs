using Godot;
using System;

public class OptionsMenu : Panel
{
    public override void _Ready()
    {
        GetNode<HSlider>("%MusicVolumeSlider").Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Music"));
        GetNode<HSlider>("%UIEffectsVolumeSlider").Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("UI_Effects"));
        GetNode<HSlider>("%SoundEffectsVolumeSlider").Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Sound_Effects"));
        GetNode<HSlider>("%MasterVolumeSlider").Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master"));
    }

    public void OnClose()
    {
        Visible = false;
    }

    public void FlagClicked(string locale)
    {
        GetNode<TranslationHandler>("/root/TranslationHandler").ChangeTranslation(locale);
    }

    public void OnAudioSliderChanged(float value, string busName)
    {
        if (value < -19f) AudioServer.SetBusMute(AudioServer.GetBusIndex(busName), true);
        else
        {
            AudioServer.SetBusMute(AudioServer.GetBusIndex(busName), false);
        }
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(busName), value);

    }


}
