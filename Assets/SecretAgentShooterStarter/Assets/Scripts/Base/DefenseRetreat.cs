using UnityEngine;

/// <summary>Walks a defeated attacker away into the distance, then removes it.</summary>
public class DefenseRetreat : MonoBehaviour
{
    public Vector3 direction = Vector3.back;
    public float speed = 4f;
    public float lifetime = 14f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (direction.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 6f);
        transform.position += direction * speed * Time.deltaTime;
    }
}
