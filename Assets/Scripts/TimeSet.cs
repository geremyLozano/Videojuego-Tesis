using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TimeSet : MonoBehaviour
{
    private Light2D lightSet;
    private float currentTimer;
    void Start()
    {
        lightSet = GetComponent<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTimer += Time.deltaTime;
        if (currentTimer <= 60f)
        {
            lightSet.color = new Color32(255, 250, 240, 255);
            lightSet.intensity = 1f;
        }
        else if (currentTimer > 60f && currentTimer <= 120f)
        {
            lightSet.color = new Color32(255, 190, 120, 255);
            lightSet.intensity = 0.75f;
        }
        else if (currentTimer > 120f && currentTimer <= 180f)
        {
            lightSet.color = new Color32(60, 80, 150, 255);
            lightSet.intensity = 0.35f;
        }else if(currentTimer > 180f)
        {
            currentTimer = 0f;
        }
    }
}
