using UnityEngine;

[CreateAssetMenu(menuName = "Secret Agent/Operative Profile")]
public class OperativeProfile : ScriptableObject
{
    [Header("Identity")]
    public string displayName = "Velvet Grunt";
    public EnemyRank rank = EnemyRank.Grunt;
    public OperativeSpecialization specialization = OperativeSpecialization.HandToHand;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float sedationThreshold = 100f;
    public float sedationResistance = 0f;
    public int recruitmentValue = 100;

    [Header("Recruitment")]
    public GameObject companionPrefab;
    public string[] outfitNames = { "Field Suit", "Stealth Suit", "Armored Suit" };
}

