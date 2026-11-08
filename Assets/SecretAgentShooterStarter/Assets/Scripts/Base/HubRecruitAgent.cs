using UnityEngine;

public class HubRecruitAgent : MonoBehaviour
{
    public Vector3 homePosition;
    public float wanderRadius = 2.5f;
    public float moveSpeed = 0.75f;
    public float turnSpeed = 4f;

    private Vector3 destination;
    private float waitUntil;

    private void Start()
    {
        homePosition = transform.position;
        PickDestination();
    }

    private void Update()
    {
        if (Time.time < waitUntil) return;

        Vector3 toDestination = destination - transform.position;
        toDestination.y = 0f;
        if (toDestination.sqrMagnitude < 0.08f)
        {
            waitUntil = Time.time + Random.Range(1.5f, 4f);
            PickDestination();
            return;
        }

        Vector3 direction = toDestination.normalized;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            turnSpeed * Time.deltaTime);

        float step = Mathf.Min(moveSpeed * Time.deltaTime, toDestination.magnitude);
        if (!Physics.SphereCast(transform.position + Vector3.up, 0.3f, direction, out _, step + 0.1f, ~0, QueryTriggerInteraction.Ignore))
        {
            transform.position += direction * step;
        }
        else
        {
            PickDestination();
        }
    }

    private void PickDestination()
    {
        Vector2 offset = Random.insideUnitCircle * wanderRadius;
        destination = homePosition + new Vector3(offset.x, 0f, offset.y);
    }
}
