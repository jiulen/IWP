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

    private void Start()
    {
        SetVolumeSliders();
    }

    private void OnDisable()
    {
        SaveVolumes();
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
        masterVolume = PlayerPrefs.GetFloat("MasterVol", 1);
        masterSlider.value = masterVolume;
        masterSliderText.text = ((int)(masterVolume * 100)).ToString();

        musicVolume = PlayerPrefs.GetFloat("MusicVol", 1);
        musicSlider.value = musicVolume;
        musicSliderText.text = ((int)(musicVolume * 100)).ToString();

        soundEffectsVolume = PlayerPrefs.GetFloat("FXVol", 1);
        soundEffectsSlider.value = soundEffectsVolume;
        soundEffectsSliderText.text = ((int)(soundEffectsVolume * 100)).ToString();

        AudioManager.Instance.UpdateMixerVolume();

        Debug.Log("master vol: " + masterVolume);
        Debug.Log("music vol: " + musicVolume);
        Debug.Log("fx vol: " + soundEffectsVolume);
    }

    public void SaveVolumes()
    {
        PlayerPrefs.SetFloat("MasterVol", masterVolume);
        PlayerPrefs.SetFloat("MusicVol", musicVolume);
        PlayerPrefs.SetFloat("FXVol", soundEffectsVolume);
    }
}
