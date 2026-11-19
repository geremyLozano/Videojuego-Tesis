using UnityEngine;

public class ChangeBackGroundMusic : MonoBehaviour
{
    public AudioSource backGroundAudioSource;
    private bool changeAudio;
    private bool activate = false;
    public AudioClip audioBackGround;
    public AudioClip newAudio;

    void Start()
    {
        changeAudio = false;
        backGroundAudioSource.clip = audioBackGround;
        backGroundAudioSource.Play();
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!activate)
            {
                activate = true;
                return;
            }
            
            if(!changeAudio)
            {
                backGroundAudioSource.Stop();
            }
            else
            {
                backGroundAudioSource.Play();
                
            }
            activate = false;
            changeAudio = !changeAudio;
        }
    }
}
