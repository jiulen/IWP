using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioOptionsManager : MonoBehaviour
{
    public static float masterVolume { get; private set; }
    public static float musicVolume { get; private set; }
    public static float soundEffectsVolume { get; private set; }

    [SerializeField] TMP_Text masterSliderText, musicSliderText, soundEffectsSliderText;

    [SerializeField] Slider masterSlider, musicSlider, soundEffectsSlider;

    private void OnEnable()
    {
        SetVolumeSliders();
    }

    public void OnMasterSliderValueChange(float value)
    {
        masterVolume = value;
        masterSliderText.text = ((int)(value * 100)).ToString();

        AudioManager.Instance.UpdateMixerVolume();
    }

    public void OnMusicSliderValueChange(float value)
    {
        musicVolume = value;
        musicSliderText.text = ((int)(value * 100)).ToString();

        AudioManager.Instance.UpdateMixerVolume();
    }

    public void OnFXSliderValueChange(float value)
    {
        soundEffectsVolume = value;
        soundEffectsSliderText.text = ((int)(value * 100)).ToString();

        AudioManager.Instance.UpdateMixerVolume();
    }

    public void SetVolumeSliders()
    {
        AudioManager.Instance.musicMixerGroup.audioMixer.GetFloat("MasterVolume", out float masterVolume);
        masterSlider.value = masterVolume;
        masterSliderText.text = ((int)(masterVolume * 100)).ToString();

        AudioManager.Instance.musicMixerGroup.audioMixer.GetFloat("MusicVolume", out float musicVolume);
        musicSliderText.text = ((int)(musicVolume * 100)).ToString();
        musicSlider.value = musicVolume;

        AudioManager.Instance.musicMixerGroup.audioMixer.GetFloat("SoundEffectsVolume", out float soundEffectsVolume);
        soundEffectsSliderText.text = ((int)(soundEffectsVolume * 100)).ToString();
        soundEffectsSlider.value = soundEffectsVolume;
    }
}
