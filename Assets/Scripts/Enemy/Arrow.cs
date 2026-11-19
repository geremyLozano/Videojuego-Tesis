using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float alturaMaxArrow = 5f; 
    private bool isShooting = false;
    public float attackRange = 1.5f;
    public bool isArrowActive = true;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Arrow arrow;
    public LayerMask targetLayer;
    private float flightTime;
    private float elapsedTime;

    private Vector2 playerDirection;

    void Start()
    {
        arrow = GetComponent<Arrow>();
    }

     void Update()
    {
        
        if(isShooting)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flightTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            if(t >= 1f)
            {
                arrow.gameObject.SetActive(false);
                transform.position = startPosition;
                isShooting = false;
            }
        }

    }

    public void Shoot(Transform targetPosition,Transform initialPositionArcher, float speed)
    {
        startPosition = initialPositionArcher.position;
        this.targetPosition = targetPosition.position;

        playerDirection = targetPosition.position - transform.position;

        Vector2 direction = (targetPosition.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        float distance = Vector3.Distance(transform.position, targetPosition.position);
        float time = distance / speed;

        flightTime = time;
        elapsedTime = 0;
        isShooting = true;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isArrowActive)
        {
            isArrowActive = true;
            return;
        }
        if(collision.CompareTag("Player"))
        {
            DetectAndDamageTargets();
            isArrowActive = false;
        }
    }


    public void DetectAndDamageTargets()
    {
        Vector2 attackPoint = (Vector2)transform.position + playerDirection.normalized*attackRange*0.5f;
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint,attackRange,targetLayer);
        
        HashSet<GameObject> damageTargets = new HashSet<GameObject>();

        foreach (Collider2D target in hitTargets)
        {
            
            GameObject obj = target.gameObject;

            if(damageTargets.Contains(obj))
                continue;

            int layer = obj.layer;

            if (layer == LayerMask.NameToLayer("Player"))
            {
                Vector2 hitDirection = target.transform.position - transform.position;
                obj.GetComponent<DamageReciverPlayer>().ApplyDamage(1,true,false,hitDirection); 
                damageTargets.Add(obj);
            }         
        }
    }
    
}
