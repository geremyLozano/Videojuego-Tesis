using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 7;

    Rigidbody2D rb2d;
    Vector2 movementInput;

    private Animator animator;
    private bool GameIsPaused = false;

    private bool isAttacking = false;
    [HideInInspector]
    public bool canMove = true;

    Vector2 lastMovementDir = Vector2.right;

    Vector2 attackDir;
    public float attackRange = 1.2f;
    public int attackDamage = 1;
    public LayerMask targetLayer;

    private int xp = 0;
    [HideInInspector]
    public int currentLevel = 1;

    [Header("Skin")]
    public RuntimeAnimatorController[] animatorControllers;
    public NPCSkin selectedSkin;
    public enum NPCSkin { Blue, Purple, Red, Yellow }

    public AudioSource audioAttack;


    private BloqueNumero bloqueCercano;
    private BloqueNumero bloqueAgarrado;
    private Vector3 offsetBloque;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        UIManager.Instance.UpdatePlayerStats(xp, currentLevel, speed, attackDamage);
        //ApplySkin();
    }

    void Update()
    {
        if (movementInput != Vector2.zero)
        {
            lastMovementDir = movementInput;
        }

        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        movementInput = movementInput.normalized; // Normalize to prevent faster diagonal movement

        animator.SetFloat("Horizontal", Mathf.Abs(movementInput.x));
        animator.SetFloat("Vertical", Mathf.Abs(movementInput.y));

        CheckFlip();
        OpenCloseInventory();
        OpenCloseStatsPlayer();
        OpenClosePauseMenu();
        OpenCloseQuest();
        AgarrarBloque();
        MoverBloqueAgarrado();
        Attack();
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            rb2d.linearVelocity = movementInput * speed;
        }
    }

    void CheckFlip()
    {
        if (movementInput.x > 0 && transform.localScale.x < 0 || movementInput.x < 0 && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("BloqueNumero"))
        {
            BloqueNumero bloque = collision.GetComponent<BloqueNumero>();
            if (bloque != null)
            {
                bloqueCercano = bloque;
                Debug.Log("Bloque cercano con valor: " + bloqueCercano.valor);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("BloqueNumero"))
        {
            BloqueNumero bloque = collision.GetComponent<BloqueNumero>();
            if (bloque != null && bloque == bloqueCercano)
            {
                bloqueCercano = null;
                Debug.Log("Bloque cercano perdido");
            }
        }
    }

    void OpenCloseInventory()
    {
        if (Input.GetKeyDown(KeyCode.I))
            UIManager.Instance.OpenOrCloseInventory();
    }

    void OpenCloseStatsPlayer()
    {
        if (Input.GetKeyDown(KeyCode.U))
            UIManager.Instance.OpenOrCloseStatsPlayer();
    }

    void OpenClosePauseMenu()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (GameIsPaused)
            {
                UIManager.Instance.ResumeGame();
                GameIsPaused = false;
            }
            else
            {
                UIManager.Instance.PauseGame();
                GameIsPaused = true;
            }
        }
    }

    public void Attack()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking) // Left mouse button
        {
            int dir = GetDirectionIndex(lastMovementDir);
            attackDir = GetAttackInputDirection();
            int attackDirection = GetDirectionIndex(attackDir);

            animator.SetInteger("AttackDirection", attackDirection);

            int randomIndex = Random.Range(0, 2);
            animator.SetInteger("AttackIndex", randomIndex);
            animator.SetTrigger("DoAttack");

            audioAttack.PlayOneShot(audioAttack.clip);
        }
    }

    public void StartAttack()
    {
        isAttacking = true;
        rb2d.linearVelocity = Vector2.zero;
        canMove = false;
    }

    public void EndAttack()
    {
        isAttacking = false;
        canMove = true;
    }

    Vector2 GetAttackInputDirection()
    {
        Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (inputDir != Vector2.zero)
        {
            return inputDir;
        }
        else
        {
            if (transform.localScale.x > 0)
                return Vector2.right;
            else
                return Vector2.left;
        }
    }

    int GetDirectionIndex(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x > 0 ? 0 : 1;
        }
        else
        {
            return dir.y > 0 ? 2 : 3;
        }
    }

    public void DetectAndDamageTargets()
    {
        Vector2 attackPoint = (Vector2)transform.position + attackDir.normalized * attackRange * 0.5f;
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint, attackRange, targetLayer);

        foreach (Collider2D target in hitTargets)
        {
            Vector2 hitDirection = target.transform.position - transform.position;
            GameObject obj = target.gameObject;
            int layer = obj.layer;

            if (layer == LayerMask.NameToLayer("Enemy"))
            {
                obj.GetComponent<DamageReciver>().ApplyDamage(attackDamage, true, false, hitDirection);
            }
            else if (layer == LayerMask.NameToLayer("Sheep"))
            {
                obj.GetComponent<DamageReciver>().ApplyDamage(attackDamage, true, false, hitDirection);
            }
            else if (layer == LayerMask.NameToLayer("Tree"))
            {
                obj.GetComponent<DamageReciver>().ApplyDamage(attackDamage, false, true, hitDirection);
            }
            else
            {

            }
        }
    }
    void OnEnable()
    {
        DamageReciver.OnTargetKilled += AddExp;
    }

    void OnDisable()
    {
        DamageReciver.OnTargetKilled -= AddExp;

    }

    public void AddExp(int xpAmount)
    {
        xp += xpAmount;
        if (xp > 100)
        {
            xp -= 100;
            LevelUp();
        }
        UIManager.Instance.UpdatePlayerStats(xp, currentLevel, speed, attackDamage);
    }

    private void LevelUp()
    {
        currentLevel += 1;

        switch (currentLevel)
        {
            case 2:
                speed += 1;
                attackDamage += 1;
                GetComponent<DamageReciverPlayer>().GainHealth(1);
                break;
            case 3:
                speed += 1;
                attackDamage += 1;
                GetComponent<DamageReciverPlayer>().GainHealth(1);
                break;
            default:
                break;
        }
    }
    public void ApplySkin()
    {
        string savedSkinName = PlayerPrefs.GetString("MainPlayerSkin", "Blue");

        if (System.Enum.TryParse(savedSkinName, out NPCSkin savedSkin)) //Pilla la skin guardada en PlayerPrefs y la aplica al jugador, si no hay ninguna guardada, aplica la skin por defecto (Blue)
        {
            selectedSkin = savedSkin;
        }
        else
        {
            selectedSkin = NPCSkin.Blue;
        }

        if (animatorControllers != null && animatorControllers.Length > 0)
        {
            int skinIndex = (int)selectedSkin;
            if (animator != null && skinIndex < animatorControllers.Length)
            {
                animator.runtimeAnimatorController = animatorControllers[skinIndex];
            }
        }
    }

    public void OpenCloseQuest()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (UIManager.Instance.questPanel.activeSelf)
            {
                UIManager.Instance.HideQuetPanel();
            }
            else
            {
                UIManager.Instance.ShowQuetPanel();
            }
        }
    }

    private void AgarrarBloque()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (bloqueAgarrado == null && bloqueCercano != null && !bloqueCercano.bloqueado)
            {
                bloqueAgarrado = bloqueCercano;
                bloqueAgarrado.Agarrar();

                Debug.Log("Dirección: " + bloqueAgarrado.direccionAgarrar);
            }
            else if (bloqueAgarrado != null)
            {
                ZonaColocacion zona = bloqueAgarrado.zonaActual;

                if (zona != null)
                {
                    bool correcto = zona.ValidarBloque();

                    if (correcto)
                    {
                        Debug.Log("Bloque colocado correctamente");
                    }
                }

                bloqueAgarrado.Soltar();
                bloqueAgarrado = null;
            }
        }
    }

    /*private void MoverBloqueAgarrado()
    {
        if (bloqueAgarrado != null)
        {
            bloqueAgarrado.transform.position = transform.position + offsetBloque;
        }
    }*/

    private void MoverBloqueAgarrado()
    {
        if (bloqueAgarrado != null)
        {
            Vector3 posicionJugador = transform.position;

            Vector3 direccion = new Vector3(
                bloqueAgarrado.direccionAgarrar.x,
                bloqueAgarrado.direccionAgarrar.y,
                0
            );

            bloqueAgarrado.transform.position =
                posicionJugador - direccion * 2f;
        }
    }

}

