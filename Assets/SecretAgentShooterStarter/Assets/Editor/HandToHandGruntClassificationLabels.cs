#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class HandToHandGruntClassificationLabels
{
    private const string PlannedClassLabel = "PlannedClass_HandToHandGrunt";
    private const string CombatRoleLabel = "CombatRole_Melee";

    static HandToHandGruntClassificationLabels()
    {
        EditorApplication.delayCall += ApplyLabelsSilently;
    }

    [MenuItem("Secret Agent/Apply Planned Character Labels")]
    public static void ApplyLabelsFromMenu()
    {
        int changedAssets = ApplyLabels();
        Debug.Log(
            $"Hand-to-hand roster labels applied. Updated {changedAssets} model assets; " +
            "this is editor metadata only and does not create an enemy class.");
    }

    private static void ApplyLabelsSilently()
    {
        ApplyLabels();
    }

    private static int ApplyLabels()
    {
        int changedAssets = 0;
        for (int variant = 1; variant <= 10; variant++)
        {
            string paddedVariant = variant.ToString("00");
            changedAssets += ApplyLabelsToMatches($"HandToHandGrunt{paddedVariant}_Animated t:Model");
            changedAssets += ApplyLabelsToMatches($"HandToHandGrunt_{paddedVariant}");
        }

        if (changedAssets > 0)
        {
            AssetDatabase.SaveAssets();
        }
        return changedAssets;
    }

    private static int ApplyLabelsToMatches(string searchQuery)
    {
        int changedAssets = 0;
        foreach (string guid in AssetDatabase.FindAssets(searchQuery))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            bool packageModel = path.Contains("/SecretAgentShooterStarter/") &&
                (path.Contains("/Models/RiggedGrunts/") ||
                 path.Contains("/Models/HandToHandGrunts/"));
            if (!packageModel) continue;

            Object model = AssetDatabase.LoadMainAssetAtPath(path);
            if (model == null) continue;

            HashSet<string> labels = new HashSet<string>(AssetDatabase.GetLabels(model));
            bool changed = labels.Add(PlannedClassLabel);
            changed |= labels.Add(CombatRoleLabel);
            if (!changed) continue;

            AssetDatabase.SetLabels(model, labels.OrderBy(label => label).ToArray());
            changedAssets++;
        }
        return changedAssets;
    }
}
#endif
