using System;
using System.Collections.Generic;
using UnityEngine;

public class RecruitRoster : MonoBehaviour
{
    public static RecruitRoster Instance { get; private set; }
    public static event Action RosterChanged;

    public List<RecruitedOperativeData> recruits = new List<RecruitedOperativeData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CampaignPersistence.LoadInto(this);
    }

    public void AddRecruit(EnemyOperative operative)
    {
        if (operative == null) return;

        RecruitedOperativeData data = new RecruitedOperativeData
        {
            displayName = operative.profile != null ? operative.profile.displayName : operative.name,
            rank = operative.Rank,
            specialization = operative.Specialization,
            recruitmentValue = operative.RecruitmentValue,
            visualResourcePath = operative.visualResourcePath,
            companionPrefab = operative.profile != null ? operative.profile.companionPrefab : null
        };

        recruits.Add(data);

        if (SecretLairManager.Instance != null)
        {
            SecretLairManager.Instance.AddCurrency(Mathf.RoundToInt(operative.RecruitmentValue * 0.25f));
        }

        RosterChanged?.Invoke();
        CampaignPersistence.SaveCurrent();
    }

    public RecruitedOperativeData GetRecruit(int index)
    {
        if (index < 0 || index >= recruits.Count) return null;
        return recruits[index];
    }
}

[Serializable]
public class RecruitedOperativeData
{
    public string displayName;
    public EnemyRank rank;
    public OperativeSpecialization specialization;
    public int recruitmentValue;
    public string activeOutfit = "Field Suit";
    public string visualResourcePath;
    public GameObject companionPrefab;
}
