using Colyseus;
using UnityEngine;

public class ScoreboardController : MonoBehaviour
{
    public Scoreboard[] scoreboards;

    //This board will only display the scores, no messages
    public Scoreboard scoreOnlyBoard;

    public void EntityAdded(ExampleNetworkedEntity entity)
    {
        foreach (Scoreboard board in scoreboards)
        {
            board.CreateScoreEntry(entity);
        }

        scoreOnlyBoard?.CreateScoreEntry(entity);
    }

    public void EntityRemoved(ExampleNetworkedEntity entity, ColyseusNetworkedEntityView view)
    {
        foreach (Scoreboard board in scoreboards)
        {
            board.RemoveView(entity);
        }

        scoreOnlyBoard?.RemoveView(entity);
    }

    public void UpdateScore(ShootingGalleryScoreUpdateMessage updateMessage, int remainingTargets)
    {
        foreach (Scoreboard board in scoreboards)
        {
            board.UpdateScore(updateMessage, remainingTargets);
        }

        scoreOnlyBoard?.UpdateScore(updateMessage, remainingTargets);
    }

    private void ResetScores()
    {
        foreach (Scoreboard board in scoreboards)
        {
            board.ResetScores();
        }

        scoreOnlyBoard?.ResetScores();
    }

    public void BeginGame(int targetsInGame)
    {
        ResetScores();
        foreach (Scoreboard board in scoreboards)
        {
            board.SetTargetsText(targetsInGame);
            board.UpdateState(Scoreboard.eScoreboardState.INGAME);
        }
    }

    public void ResetScoreboards()
    {
        ResetScores();
        foreach (Scoreboard board in scoreboards)
        {
            board.UpdateState(Scoreboard.eScoreboardState.WAITING);
        }
    }

    public void SetMessage(string text)
    {
        foreach (Scoreboard board in scoreboards)
        {
            board.SetMessageText(text);
        }
    }

    public void CountDown(string countdownText)
    {
        foreach (Scoreboard board in scoreboards)
        {
            board.UpdateState(Scoreboard.eScoreboardState.COUNTDOWN);
            board.SetMessageText(countdownText);
        }
    }

    public void GameOver(string winnerID)
    {
        foreach (Scoreboard board in scoreboards)
        {
            board.UpdateState(Scoreboard.eScoreboardState.POSTGAME);
            board.SetMessageText(winnerID);
        }
    }
}