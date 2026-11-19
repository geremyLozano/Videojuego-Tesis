using UnityEngine;
using UnityEngine.SceneManagement;

public class DamageReciverPlayer : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 5;
    private int currentHealth;

    private Rigidbody2D rb2D;
    private Animator animator;
    public float forceImpulse = 5;

    public AudioSource audioDie;
    public AudioSource audioDamageRecive;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;    
        UIManager.Instance.UpdateHealth(currentHealth,maxHealth);
    }


    public void ApplyDamage(int amount, bool applyForceOrNot, bool applyHitAnimation, Vector2 hitDirection)
    {
        currentHealth -= amount;
        UIManager.Instance.UpdateHealth(currentHealth,maxHealth);
        audioDamageRecive.Play();
        if (applyForceOrNot)
        {
            GetComponent<Player>().canMove = false;
            rb2D.AddForce(hitDirection.normalized*forceImpulse,ForceMode2D.Impulse);
            Invoke("ResetMovement",0.1f);
        }
        if (applyHitAnimation)
        {
            animator.SetTrigger("Hit");
        }
        if(currentHealth <= 0)
        {
           
            Die();
        }
    }

    private void ResetMovement()
    {
        GetComponent<Player>().canMove = true;
    }

    public void GainHealth(int healthAmount)
    {
        maxHealth += healthAmount;
        currentHealth = maxHealth;
        UIManager.Instance.UpdateHealth(currentHealth,maxHealth);
    }

    void Die()
    {
        animator.SetTrigger("Die");
        UIManager.Instance.DiePanelAnimation();
        audioDie.Play();
        GetComponent<Player>().enabled = false;
        Invoke("ResetLevel",2f);
    }

    public void ResetLevel()
    {
        Scene currenScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currenScene.name);
    }
}
