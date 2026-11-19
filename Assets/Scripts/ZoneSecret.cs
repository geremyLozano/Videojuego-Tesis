using UnityEngine;

public class SecretZone : MonoBehaviour
{
    private float currentTime = 0;
    void Start()
    {

    }

    void Update()
    {
        currentTime += Time.deltaTime;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(currentTime > 120f && currentTime <= 180f)
            {
                UIManager.Instance.OpenLoadingPanel();
            }
        }
    }
}
