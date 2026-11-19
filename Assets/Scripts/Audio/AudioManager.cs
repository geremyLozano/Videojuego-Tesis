using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioMixer miAudioMixer;
    public Slider masterSlider;

    void Start()
    {
        if(PlayerPrefs.HasKey("MasterVolumen"))
        {
            LoadVolumen();
        }
        else
        {
            SetMasterVolumen();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMasterVolumen()
    {
        if(masterSlider == null)
        {
            return;
        }

        float volumen = masterSlider.value;
        miAudioMixer.SetFloat("MasterVolumenParameter", Mathf.Log10(volumen) * 20);
        PlayerPrefs.SetFloat("MasterVolumen", volumen);
    }

    private void LoadVolumen()
    {
        float savedVolumen = PlayerPrefs.GetFloat("MasterVolumen");
        if(masterSlider != null)
        {
            masterSlider.value = savedVolumen;
        }
        SetMasterVolumen();

    }
}
