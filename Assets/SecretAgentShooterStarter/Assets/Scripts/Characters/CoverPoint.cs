using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A spot an enemy can take cover at and shoot from: beside a window inside a
/// house, behind a ruined wall, or behind sandbags. Only one enemy holds a
/// point at a time.
///
/// Enemies walk in straight lines, so a point inside a building lists the
/// spots to walk through on the way in (outside the door, then inside the
/// door). The enemy follows that path before settling at the point.
/// </summary>
public class CoverPoint : MonoBehaviour
{
    private static readonly List<CoverPoint> active = new List<CoverPoint>();

    [Tooltip("World positions to walk through, in order, before reaching the point.")]
    public Vector3[] approachPath = new Vector3[0];

    [Tooltip("Height of the eyes when firing from here (a window sill needs standing height).")]
    public float firingHeight = 1.3f;

    public EnemyCombatController Holder { get; private set; }

    public static IReadOnlyList<CoverPoint> Active => active;

    private void OnEnable()
    {
        if (!active.Contains(this)) active.Add(this);
    }

    private void OnDisable()
    {
        active.Remove(this);
        Holder = null;
    }

    public bool IsFreeFor(EnemyCombatController enemy)
    {
        return Holder == null || Holder == enemy || !Holder.isActiveAndEnabled;
    }

    public bool TryClaim(EnemyCombatController enemy)
    {
        if (!IsFreeFor(enemy)) return false;
        Holder = enemy;
        return true;
    }

    public void Release(EnemyCombatController enemy)
    {
        if (Holder == enemy) Holder = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.55f, 0.1f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.3f, 0.3f);
        Vector3 previous = transform.position;
        for (int i = approachPath.Length - 1; i >= 0; i--)
        {
            Gizmos.DrawLine(previous, approachPath[i]);
            previous = approachPath[i];
        }
    }
}
