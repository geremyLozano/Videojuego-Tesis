using System;
using UnityEngine;

public class ArcadeChangeMusic : MonoBehaviour
{
    public AudioSource audioSource;
    public GameObject panel;
    private bool dentro = false;
    private static AudioSource currentAudio = null;

    void Update()
    {
        ActiveMusic();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            panel.SetActive(true);
            dentro = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        panel.SetActive(false);
        dentro = false;
    }

    private void ActiveMusic()
    {
        if (dentro)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (currentAudio != null && currentAudio != audioSource)
                    currentAudio.Stop();
                audioSource.Play();
                currentAudio = audioSource;
            }
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            audioSource.Stop();
        }
    }
}
