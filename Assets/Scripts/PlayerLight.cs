using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerLight : MonoBehaviour
{
    private Light2D playerLight;
    private float currentTimer = 0;

    void Start()
    {
        playerLight = GetComponent<Light2D>();    
    }

    // Update is called once per frame
    void Update()
    {
        currentTimer += Time.deltaTime;
        if (currentTimer > 120f && currentTimer <= 180f)
        {
            playerLight.color = new Color32(255, 250, 240, 255);
            playerLight.intensity = 4f;
        }else if(currentTimer > 180f)
        {
            currentTimer = 0f;
        }
    }
}
