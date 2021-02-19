using TMPro;
using UnityEngine;

public class ScoreboardEntry : MonoBehaviour
{
    public int currentScore;

    [HideInInspector]
    public ExampleNetworkedEntity entityRef;

    [SerializeField]
    private GameObject highlight = null;

    [SerializeField]
    private TextMeshProUGUI playerName = null;

    [SerializeField]
    private TextMeshProUGUI playerScore = null;

    public void Init(ExampleNetworkedEntity entity)
    {
        entityRef = entity;
        PlayerController playerRef = GalleryGameManager.Instance.GetPlayerView(entity.id);
        if (playerRef != null)
        {
            playerName.text = playerRef.userName;
        }
        else
        {
            playerName.text = entityRef.id;
        }

        playerScore.text = "0";
        highlight.SetActive(ExampleManager.Instance.CurrentUser != null &&
                            string.Equals(ExampleManager.Instance.CurrentUser.id, entityRef.ownerId));
    }

    public void UpdateScore(int score)
    {
        playerScore.text = score.ToString("N0");
        currentScore = score;
    }
}