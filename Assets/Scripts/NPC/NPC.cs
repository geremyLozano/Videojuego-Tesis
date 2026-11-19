using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    protected NavMeshAgent navMeshAgent;
    protected Animator animator;
    protected Transform playerTransform;

    [Header("movement Type")]
    public MovementType movementType;
    public enum MovementType { Static, Path, RandomMovement }

    [Header("Skin")]
    public RuntimeAnimatorController[] animatorControllers;
    public NPCSkin selectedSkin;
    public enum NPCSkin { Blue, Purple, Red, Yellow }

    [Header("Path Movement")]
    public Transform[] pathPoints;
    public float waitTimeAtPoint = 3;
    private int indexPath = 0;


    [Header("Random Movement")]
    public float movementRadius = 5f;
    public float waitTimeRandom = 4f;

    [Header("Player Chase")]
    public bool canChasePlayer = false;
    public float chaseRadius = 6f;
    public float stopDistanceFromPlayer = 1.5f;
    protected bool isChasingPlayer = false;

    [Header("Flee behaivor")]
    public bool canFlee = false;
    public float fleeRange = 4f;
    public float fleeDistance = 5f;
    private bool isFleeing = false;
    private MovementType previousMovementType;

    [Header("Return To Origin")]
    public bool returnToOrigin = false;
    public float maxDistanceFromOrigin = 1;
    protected Vector3 originPosition;

    private Coroutine currentMovementRoutine;

    protected virtual void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;

        animator = GetComponent<Animator>();
        ApplySkin();

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        originPosition = transform.position;

        ResumeOriginalBehaivor();
    }

    protected virtual void Update()
    {
        AdjustAnimationAndRotation();
        HandleChaseLogic();
        HandleFleeLogic();
        HandleReturnToOrigin();
    }

    public void AdjustAnimationAndRotation()
    {
        bool isMoving = navMeshAgent.velocity.sqrMagnitude > 0.01f;
        animator.SetBool("IsRunning", isMoving);
        if (navMeshAgent.desiredVelocity.x > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        if (navMeshAgent.desiredVelocity.x < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

    }

    public void ApplySkin()
    {
        if (animatorControllers != null && animatorControllers.Length > 0)
        {
            int skinIndex = (int)selectedSkin;
            if (animator != null && skinIndex < animatorControllers.Length)
            {
                animator.runtimeAnimatorController = animatorControllers[skinIndex];
            }
        }


    }

    public virtual void ResumeOriginalBehaivor()
    {
        ResumenMovementBehaivor(movementType);
    }

    private void ResumenMovementBehaivor(MovementType type)
    {
        StopCurrentRoutine();

        switch (type)
        {
            case MovementType.Static:
                navMeshAgent.ResetPath();
                break;
            case MovementType.Path:
                StartPathRoutine();
                break;
            case MovementType.RandomMovement:
                StartRandomRoutine();
                break;
            default:
                break;
        }
    }

    protected void StartPathRoutine()
    {
        StopAllCoroutines();
        currentMovementRoutine = StartCoroutine(FollowPath());
    }

    protected void StartRandomRoutine()
    {
        StopAllCoroutines();
        currentMovementRoutine = StartCoroutine(RandomMovement());
    }

    void StopCurrentRoutine()
    {
        if (currentMovementRoutine != null)
        {
            StopCoroutine(currentMovementRoutine);
            currentMovementRoutine = null;
        }
    }

    protected IEnumerator FollowPath()
    {
        while (true)
        {
            if (pathPoints.Length > 0)
            {
                navMeshAgent.SetDestination(pathPoints[indexPath].position);
                yield return WaitUntilDestinationReached();
                yield return new WaitForSeconds(waitTimeAtPoint);
                indexPath = (indexPath + 1) % pathPoints.Length;
            }

        }
    }

    protected IEnumerator RandomMovement()
    {
        while (true)
        {
            Vector3 randomPos = GetRandomNavMeshPosition();
            navMeshAgent.SetDestination(randomPos);
            yield return WaitUntilDestionReached();
            yield return new WaitForSeconds(waitTimeRandom);
        }
    }

    private IEnumerator WaitUntilDestionReached()
    {
        while (!navMeshAgent.pathPending && navMeshAgent.remainingDistance > 0.05f)
        {
            yield return null;
        }
    }

    private Vector3 GetRandomNavMeshPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere *movementRadius + transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, movementRadius,NavMesh.AllAreas))
        {
            return hit.position;
        }
        return transform.position;
    }


    private IEnumerator WaitUntilDestinationReached()
    {
        while (!navMeshAgent.pathPending && navMeshAgent.remainingDistance > 0.5f)
        {
            yield return null;
        }
    }

    protected void HandleChaseLogic()
    {
        if (!canChasePlayer || playerTransform == null || isFleeing)
        {
            return;
        }
        float distance = Vector3.Distance(transform.position,playerTransform.position);
        if(distance <= chaseRadius)
        {
            if (!isChasingPlayer)
            {
                StopCurrentRoutine();
                isChasingPlayer = true;
            }
            if(distance > stopDistanceFromPlayer)
            {
                navMeshAgent.stoppingDistance = stopDistanceFromPlayer;
                navMeshAgent.SetDestination(playerTransform.position);
            }
            else
            {
                navMeshAgent.ResetPath();
            }
        }
        else if(isChasingPlayer)
        {
            navMeshAgent.ResetPath();
            isChasingPlayer = false;
            ResumeOriginalBehaivor();
        }
    }

    protected void HandleFleeLogic()
    {
        if (!canFlee || playerTransform == null)
        {
            return;
        }
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer < fleeRange  && !isFleeing)
        {
            isFleeing = true;
            previousMovementType = movementType;
            StopCurrentRoutine();            
            Vector3 fleeDirection = (transform.position - playerTransform.position).normalized;
            Vector3 fleeTarget = transform.position + fleeDirection * fleeDistance;

            if (NavMesh.SamplePosition(fleeTarget,out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }
        }
        else if(distanceToPlayer >= fleeRange && isFleeing)
        {
            isFleeing = false;
            ResumenMovementBehaivor(previousMovementType);
        }
    }

    protected void HandleReturnToOrigin()
    {
        if (!returnToOrigin || isFleeing)
        {
            return;
        }
        float distance = Vector3.Distance(transform.position,originPosition);
        if(distance > maxDistanceFromOrigin)
        {
            StopCurrentRoutine();
            navMeshAgent.SetDestination(originPosition);
        }
    }

}
