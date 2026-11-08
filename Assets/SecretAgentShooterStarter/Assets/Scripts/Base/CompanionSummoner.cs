using UnityEngine;

public class CompanionSummoner : MonoBehaviour
{
    public RecruitRoster roster;
    public Transform player;
    public Transform summonPoint;
    public CompanionAgent activeCompanion;

    private void Awake()
    {
        if (roster == null) roster = FindAnyObjectByType<RecruitRoster>();
        if (player == null) player = transform;
    }

    public bool SummonByIndex(int recruitIndex)
    {
        if (roster == null) roster = FindAnyObjectByType<RecruitRoster>();
        if (roster == null) return false;

        RecruitedOperativeData recruit = roster.GetRecruit(recruitIndex);
        if (recruit == null) return false;

        DismissCurrent();

        Vector3 position = summonPoint != null
            ? summonPoint.position
            : player.position + player.right * 1.8f - player.forward * 0.8f;
        GameObject companionObject;
        if (recruit.companionPrefab != null)
        {
            companionObject = Instantiate(recruit.companionPrefab, position, player.rotation);
        }
        else
        {
            companionObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            companionObject.transform.position = position + Vector3.up;
            companionObject.transform.rotation = player.rotation;
            Renderer renderer = companionObject.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = GetSpecializationColor(recruit.specialization);
        }

        companionObject.name = $"Field Recruit - {recruit.displayName}";
        activeCompanion = companionObject.GetComponent<CompanionAgent>();
        if (activeCompanion == null) activeCompanion = companionObject.AddComponent<CompanionAgent>();
        activeCompanion.Initialize(player, recruit);
        return true;
    }

    public void DismissCurrent()
    {
        if (activeCompanion == null) return;
        Destroy(activeCompanion.gameObject);
        activeCompanion = null;
    }

    private static Color GetSpecializationColor(OperativeSpecialization specialization)
    {
        switch (specialization)
        {
            case OperativeSpecialization.Stealth: return new Color(0.45f, 0.2f, 0.72f);
            case OperativeSpecialization.Tank: return new Color(0.72f, 0.28f, 0.18f);
            case OperativeSpecialization.Marksman: return new Color(0.18f, 0.5f, 0.68f);
            case OperativeSpecialization.Hacker: return new Color(0.85f, 0.68f, 0.18f);
            case OperativeSpecialization.Medic: return new Color(0.75f, 0.8f, 0.82f);
            default: return new Color(0.75f, 0.15f, 0.2f);
        }
    }
}
