using UnityEngine;

public class ArcadeInstruction : MonoBehaviour
{
    public GameObject arcadeInstrucPanel;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            arcadeInstrucPanel.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            arcadeInstrucPanel.SetActive(false);
        }
    }
}
