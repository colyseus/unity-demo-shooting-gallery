using System.Collections.Generic;
using LucidSightTools;
using TMPro;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    public enum eScoreboardState
    {
        NONE,
        WAITING,
        COUNTDOWN,
        INGAME,
        POSTGAME
    }

    private eScoreboardState currentState;

    [SerializeField]
    private GameObject inGameRoot = null;

    [SerializeField]
    private GameObject messageRoot = null;

    [SerializeField]
    private TextMeshProUGUI messageText = null;

    [SerializeField]
    private GameObject scoreEntryPrefab = null;

    [SerializeField]
    private Transform scoreRoot = null;

    private List<ScoreboardEntry> spawnedEntries = new List<ScoreboardEntry>();

    [SerializeField]
    private TextMeshProUGUI targetsText = null;

    public void CreateScoreEntry(ExampleNetworkedEntity entity)
    {
        GameObject newEntry = Instantiate(scoreEntryPrefab, scoreRoot, false);
        ScoreboardEntry entry = newEntry.GetComponent<ScoreboardEntry>();
        entry.Init(entity);
        spawnedEntries.Add(entry);
    }

    public void UpdateScore(ShootingGalleryScoreUpdateMessage updateMessage, int remainingTargets)
    {
        ScoreboardEntry entryForView = GetEntryByID(updateMessage.entityID);
        if (entryForView != null)
        {
            entryForView.UpdateScore(updateMessage.score);
            UpdateEntryOrder();
        }
        else
        {
            LSLog.LogError("Tried to Update a score but couldn't find an entry!");
        }

        SetTargetsText(remainingTargets);
    }

    public void SetTargetsText(int remainingTargets)
    {
        if (targetsText)
        {
            targetsText.text = $"Targets Left:\n{remainingTargets}";
        }
    }

    public void RemoveView(ExampleNetworkedEntity entity)
    {
        ScoreboardEntry entryForView = GetEntryByID(entity.id);

        if (entryForView != null)
        {
            spawnedEntries.Remove(entryForView);
            Destroy(entryForView.gameObject);
        }
        else
        {
            LSLog.LogError("Player left game but we do not have a scoreboard entry for them!");
        }
    }

    private ScoreboardEntry GetEntryByID(string entityID)
    {
        ScoreboardEntry entryForView = null;
        foreach (ScoreboardEntry score in spawnedEntries)
        {
            if (score.entityRef.id.Equals(entityID))
            {
                entryForView = score;
            }
        }

        return entryForView;
    }

    public void ResetScores()
    {
        foreach (ScoreboardEntry score in spawnedEntries)
        {
            score.UpdateScore(0);
        }
    }

    public void UpdateState(eScoreboardState state)
    {
        if (state == currentState)
        {
            return;
        }

        currentState = state;
        UpdateTransforms(state);
    }

    public void SetMessageText(string text)
    {
        messageText.text = text;
    }

    private void UpdateTransforms(eScoreboardState state)
    {
        messageRoot.SetActive(state != eScoreboardState.INGAME);
        inGameRoot.SetActive(state == eScoreboardState.INGAME);
    }

    private void UpdateEntryOrder()
    {
        spawnedEntries.Sort((x, y) =>
        {
            int scoreX = x != null ? x.currentScore : -1;
            int scoreY = y != null ? y.currentScore : -1;

            return scoreY.CompareTo(scoreX);
        });

        for (int i = 0; i < spawnedEntries.Count; ++i)
        {
            spawnedEntries[i].transform.SetSiblingIndex(i);
        }
    }
}