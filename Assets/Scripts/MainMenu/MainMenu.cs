using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsPanel;
    public GameObject buttonsPanel;

    public TMP_Dropdown qualityDropdown;

    public GameObject  mainPlayerSkin;
    public Sprite[] spritesSkins;
    public string[] skinNames = { "Blue", "Purple", "Red", "Yellow"};

    public void Start()
    {
        PlayerPrefs.SetString("MainPlayerSkin",skinNames[0]); //PILLAR EL NOMBRE DE LA SKIN GUARDADA, SI NO HAY NINGUNA GUARDADA, PILLAR LA PRIMERA SKIN
        int savedQualityIndex = PlayerPrefs.GetInt("QualityLevel",-1); //PILLAR EL VALOR DE LA CALIDAD GUARDADA, SI NO HAY NINGUNA GUARDADA, PILLAR -1

        if(savedQualityIndex == -1)
        {
            savedQualityIndex = QualitySettings.GetQualityLevel(); //PILLAR LA CALIDAD QUE SE TENGA
        }
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names)); //PILLAR LOS NOMBRES DE LAS CALIDADES
        qualityDropdown.value = savedQualityIndex; //PILLAR EL VALOR DE LA CALIDAD GUARDADA
        qualityDropdown.onValueChanged.AddListener(OnQuialityChanged); //CUANDO CAMBIE EL VALOR DE LA CALIDAD, LLAMAR A LA FUNCION
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Mapa1");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        buttonsPanel.SetActive(false);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        buttonsPanel.SetActive(true);
    }

    public void OnQuialityChanged(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex,true);
        PlayerPrefs.SetInt("QualityLevel",qualityIndex); //GUARDAR INFORMACION CUANDO SE REINICIA EL JUEGO
    }

    public void SelectSkin(int index)
    {
        mainPlayerSkin.GetComponent<Image>().sprite = spritesSkins[index];
        PlayerPrefs.SetString("MainPlayerSkin", skinNames[index]);
    }

}
