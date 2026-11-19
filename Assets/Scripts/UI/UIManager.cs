using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject inventory;
    public GameObject pauseMenu;
    public GameObject statsPanel;
    public GameObject pauseMenuButton;
    public GameObject questPanel;
    public GameObject diePanel;
    public GameObject storyPanel;
    public GameObject endStoryText;
    public GameObject loadingPanel;
    
    public TMP_Text moneyText;
    public TMP_Text meatText;
    public TMP_Text woodText;
    public TMP_Text healthText;

    public TMP_Text maxHealthText;
    public TMP_Text levelText;
    public TMP_Text attackDamageText;
    public TMP_Text speedText;
    public Slider xpSlider;

    
    public TMP_Text questText;
    public TMP_Text questTitle;

    private Ejercicio ejercicios;
 
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OpenOrCloseInventory()
    {
        if(statsPanel.activeSelf == true)
        {
            statsPanel.GetComponent<Animator>().Play("Close");
        }
        if(inventory.activeSelf == true)
        {
            inventory.GetComponent<Animator>().Play("Close");
        }
        else
        {
            inventory.SetActive(true);
            inventory.GetComponent<Animator>().Play("Open");
        }
        
    }

    public void OpenOrCloseStatsPlayer()
    {
        if(inventory.activeSelf == true)
        {
            inventory.GetComponent<Animator>().Play("Close");
        }
        if(statsPanel.activeSelf == true)
        {
            statsPanel.GetComponent<Animator>().Play("Close");
        }
        else
        {
            statsPanel.SetActive(true);
            statsPanel.GetComponent<Animator>().Play("Open");
        }
        
    }

    public void UpdateMoneyMeetWood(string tag, int value)
    {
        switch (tag)
        {
            case "MoneyBag":
                moneyText.text = value.ToString();
                break;
            case "Meat":
                meatText.text = value.ToString();
                break;
            case "Wood":
                woodText.text = value.ToString();
                break;
        }
    }

    public void UpdateHealth(int hpValue, int maxHealthTextValue)
    {
        healthText.text = hpValue.ToString();
        maxHealthText.text = maxHealthTextValue.ToString();
    }

    public void UpdatePlayerStats(int xpValue, int levelValue,float speedValue,int attackDamageValue)
    {
        xpSlider.value = xpValue;
        levelText.text = levelValue.ToString();
        speedText.text = speedValue.ToString();
        attackDamageText.text = attackDamageValue.ToString();
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        pauseMenuButton.SetActive(false);
        Time.timeScale = 0f;
    }
    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        pauseMenuButton.SetActive(true);
        Time.timeScale = 1f; //SI SE AUMENTA MAS DE UNO PUEDE IR EL JUEGO MAS RAPIDO, SI SE PONE EN CERO SE DETIENE EL JUEGO
    }
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowQuetPanel()
    {
        questPanel.SetActive(true);
    }

    public void HideQuetPanel()
    {
        questPanel.SetActive(false);
    }

    public void UpdateRequiredResourcesQuestAndName(int moneyRequired, int meatRequired, int woodRequired,string questName)
    {
        questText.text = questName;
    }

    public void UpdateQuestText(int id)
    {
        questText.text = DatosPartida.ejercicios[id].enunciado;
    }

    public void UpdateQuestTitle(int id)
    {
        questTitle.text = DatosPartida.temas[id].nombre;
    }
    
    public void DiePanelAnimation()
    {
        diePanel.SetActive(true);
    }


    public void EndStory()
    {
        endStoryText.SetActive(true);
        storyPanel.SetActive(true);
        Invoke("CloseStoryAndGoToMainMenu",10);
    }
    public void CloseStory()
    {
        storyPanel.SetActive(false);
    }
    public void CloseStoryAndGoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void OpenLoadingPanel()
    {
        loadingPanel.SetActive(true);
        Invoke("CloseLoadingPanel",5);
    }

    public void CloseLoadingPanel()
    {
        loadingPanel.SetActive(false);
    }
    
}
