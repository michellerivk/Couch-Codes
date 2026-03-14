using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SliderSettings : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string MusicParam = "MusicVolume";
    private const string SfxParam = "SFXVolume";

    private const string MusicPref = "MusicVolume";
    private const string SfxPref = "SFXVolume";

    private void Start()
    {
        float savedMusic = PlayerPrefs.GetFloat(MusicPref, 0.8f);
        float savedSfx = PlayerPrefs.GetFloat(SfxPref, 0.8f);

        musicSlider.value = savedMusic;
        sfxSlider.value = savedSfx;

        SetMusicVolume(savedMusic);
        SetSfxVolume(savedSfx);

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
    }

    public void SetMusicVolume(float value)
    {
        mixer.SetFloat(MusicParam, LinearToDecibels(value));
        PlayerPrefs.SetFloat(MusicPref, value);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float value)
    {
        mixer.SetFloat(SfxParam, LinearToDecibels(value));
        PlayerPrefs.SetFloat(SfxPref, value);
        PlayerPrefs.Save();
    }

    private float LinearToDecibels(float value)
    {
        return Mathf.Log10(value) * 20f; // More natural feeling than whole values
    }
}
