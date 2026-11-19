using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public Quest[] quests;
    private int currentQuestIndex = 0;

    private bool questActive = false;
    private PlayerResourcesCollector player;

    public MecanicaManager mecanicaManager;
    public int ejercicioIndex;

    public GameObject door;

    public BoxCollider2D spawnZonaColocacion;
    public BoxCollider2D spawnBloque;
    public BoxCollider2D spawnRespuesta;
    public BoxCollider2D spawnCuadrillaMCM;
    public BoxCollider2D spawnProcedimientoMCM;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerResourcesCollector>();
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            UIManager.Instance.ShowQuetPanel();

            if (!questActive)
            {
                questActive = true; 
                ShowQuest();
                mecanicaManager.CargarMecanica(ejercicioIndex, spawnZonaColocacion, spawnBloque,spawnRespuesta,spawnCuadrillaMCM,spawnProcedimientoMCM, door);
                mecanicaManager.IniciarTiempoEjercicio(ejercicioIndex);
            }
            else
            {
                /*Quest currentQuest = quests[currentQuestIndex];
                if(currentQuest.IsCompleted(player.GetMoney(),player.GetMeet(),player.GetWood()))
                {
                    AdvanceQuest();
                }
                else
                {
                    ShowQuest();
                }*/
                ShowQuest();
            }
        }
    }
    /*public void ShowQuest(Quest quest)
    {
        UIManager.Instance.UpdateRequiredResourcesQuestAndName
        (quest.moneyRequired, quest.meatRequired, quest.woodRequired,quest.questName);
    }*/

    public void ShowQuest()
    {
        UIManager.Instance.UpdateQuestText(ejercicioIndex);
        UIManager.Instance.UpdateQuestTitle(ejercicioIndex);
    }

    public void AdvanceQuest()
    {
        Debug.Log("Quest completed!, moving to next quest");
        player.SetMoney(player.GetMoney() - quests[currentQuestIndex].moneyRequired);
        player.SetMeet(player.GetMeet() - quests[currentQuestIndex].meatRequired);
        player.SetWood(player.GetWood() - quests[currentQuestIndex].woodRequired);

        player.UpdateAllResources();
        
        if(currentQuestIndex < quests.Length - 1)
        {
            currentQuestIndex++;
            ShowQuest();
        }
        else
        {
            Debug.Log("All quests completed!");
            UIManager.Instance.EndStory();
        }
    }
    
    
}
