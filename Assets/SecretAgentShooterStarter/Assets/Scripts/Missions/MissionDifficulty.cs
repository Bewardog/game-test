using System;

public enum MissionDifficulty
{
    FieldAgent,
    SecretAgent,
    BlackOps
}

[Serializable]
public class MissionDifficultyProfile
{
    public MissionDifficulty difficulty;
    public string displayName;
    public string description;
    public float enemyDurabilityMultiplier;
    public float enemySedationThresholdMultiplier;
    public float enemyDamageMultiplier;
    public float enemyAwarenessMultiplier;
    public float rewardMultiplier;
    public int checkpointRecoveriesPerSector;
    public int performanceScoreModifier;
}

public static class MissionDifficultyCatalog
{
    private static readonly MissionDifficultyProfile FieldAgent = new MissionDifficultyProfile
    {
        difficulty = MissionDifficulty.FieldAgent,
        displayName = "FIELD AGENT",
        description = "Reduced enemy response and two field recoveries per secured checkpoint.",
        enemyDurabilityMultiplier = 0.85f,
        enemySedationThresholdMultiplier = 0.86f,
        enemyDamageMultiplier = 0.75f,
        enemyAwarenessMultiplier = 0.82f,
        rewardMultiplier = 0.85f,
        checkpointRecoveriesPerSector = 2,
        performanceScoreModifier = -5
    };

    private static readonly MissionDifficultyProfile SecretAgent = new MissionDifficultyProfile
    {
        difficulty = MissionDifficulty.SecretAgent,
        displayName = "SECRET AGENT",
        description = "Standard syndicate response and one field recovery per secured checkpoint.",
        enemyDurabilityMultiplier = 1f,
        enemySedationThresholdMultiplier = 1f,
        enemyDamageMultiplier = 1f,
        enemyAwarenessMultiplier = 1f,
        rewardMultiplier = 1f,
        checkpointRecoveriesPerSector = 1,
        performanceScoreModifier = 0
    };

    private static readonly MissionDifficultyProfile BlackOps = new MissionDifficultyProfile
    {
        difficulty = MissionDifficulty.BlackOps,
        displayName = "BLACK OPS",
        description = "Elite response, stronger resistance, higher rewards, and no field recovery.",
        enemyDurabilityMultiplier = 1.32f,
        enemySedationThresholdMultiplier = 1.2f,
        enemyDamageMultiplier = 1.3f,
        enemyAwarenessMultiplier = 1.2f,
        rewardMultiplier = 1.5f,
        checkpointRecoveriesPerSector = 0,
        performanceScoreModifier = 8
    };

    public static MissionDifficultyProfile Get(MissionDifficulty difficulty)
    {
        switch (difficulty)
        {
            case MissionDifficulty.FieldAgent: return FieldAgent;
            case MissionDifficulty.BlackOps: return BlackOps;
            default: return SecretAgent;
        }
    }
}
