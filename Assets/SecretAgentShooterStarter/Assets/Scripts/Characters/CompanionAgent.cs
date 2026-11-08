using UnityEngine;
using UnityEngine.AI;

public class CompanionAgent : MonoBehaviour
{
    public Transform followTarget;
    public float followDistance = 3f;
    public float moveSpeed = 5f;
    public float scanRadius = 18f;
    public float attackRange = 12f;
    public float sedationPerPulse = 25f;
    public float pulseCooldown = 1.25f;

    private NavMeshAgent agent;
    private float nextPulseTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        FollowPlayer();
        SupportAgainstNearestEnemy();
    }

    public void Initialize(Transform target, RecruitedOperativeData recruit = null)
    {
        followTarget = target;
        if (recruit == null) return;

        sedationPerPulse += (int)recruit.rank * 5f;
        switch (recruit.specialization)
        {
            case OperativeSpecialization.Marksman:
                attackRange = 19f;
                pulseCooldown = 1.6f;
                sedationPerPulse += 15f;
                break;
            case OperativeSpecialization.HandToHand:
                attackRange = 3f;
                pulseCooldown = 0.65f;
                moveSpeed = 6f;
                break;
            case OperativeSpecialization.Tank:
                attackRange = 5f;
                sedationPerPulse += 10f;
                break;
            case OperativeSpecialization.Stealth:
                moveSpeed = 6.5f;
                pulseCooldown = 0.9f;
                break;
        }
    }

    private void FollowPlayer()
    {
        if (followTarget == null) return;

        float distance = Vector3.Distance(transform.position, followTarget.position);
        if (distance <= followDistance) return;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(followTarget.position);
            return;
        }

        Vector3 target = followTarget.position - followTarget.forward * 2f + followTarget.right * 1.25f;
        target.y = transform.position.y;
        Vector3 direction = target - transform.position;
        if (direction.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction.normalized), Time.deltaTime * 7f);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    private void SupportAgainstNearestEnemy()
    {
        if (Time.time < nextPulseTime) return;

        EnemyOperative target = FindNearestEnemy();
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > attackRange) return;

        target.ApplySedation(sedationPerPulse);
        nextPulseTime = Time.time + pulseCooldown;
        SpawnSupportTrace(target.transform.position + Vector3.up);
    }

    private EnemyOperative FindNearestEnemy()
    {
        EnemyOperative[] enemies = FindObjectsByType<EnemyOperative>();
        EnemyOperative best = null;
        float bestDistance = float.MaxValue;

        foreach (EnemyOperative enemy in enemies)
        {
            if (enemy == null || enemy.isDead || enemy.isKnockedOut || enemy.isRecruited) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < bestDistance && distance <= scanRadius)
            {
                best = enemy;
                bestDistance = distance;
            }
        }

        return best;
    }

    private void SpawnSupportTrace(Vector3 end)
    {
        GameObject traceObject = new GameObject("Companion Dart Trace");
        LineRenderer trace = traceObject.AddComponent<LineRenderer>();
        trace.positionCount = 2;
        trace.SetPosition(0, transform.position + Vector3.up * 1.1f);
        trace.SetPosition(1, end);
        trace.startWidth = 0.045f;
        trace.endWidth = 0.012f;
        Color color = new Color(0.2f, 0.95f, 0.72f);
        trace.startColor = color;
        trace.endColor = color;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null)
        {
            trace.material = new Material(shader);
            trace.material.color = color;
            Destroy(trace.material, 0.2f);
        }
        Destroy(traceObject, 0.16f);
    }
}
