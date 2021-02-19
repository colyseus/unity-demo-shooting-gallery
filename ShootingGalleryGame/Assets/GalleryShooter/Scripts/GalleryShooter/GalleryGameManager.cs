using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colyseus;
using Colyseus.Schema;
using LucidSightTools;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GalleryGameManager : MonoBehaviour
{
    private string _countDownString = "";

    private bool _showCountdown;

    // State variables
    //============================
    [SerializeField]
    private GameObject ingameLightingRoot = null;


    public PlayerController prefab;

    [SerializeField]
    private GameObject pregameLightingRoot = null;

    public ScoreboardController scoreboardController;

    public TargetController targetsController;

    public GameUIController uiController;
    private string userReadyState = "";

    public static GalleryGameManager Instance { get; private set; }

    public enum eGameState
    {
        NONE,
        WAITING,
        WAITINGFOROTHERS,
        SENDTARGETS,
        BEGINROUND,
        SIMULATEROUND,
        ENDROUND
    }

    private eGameState currentGameState;
    private eGameState lastGameState;
    //============================

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private IEnumerator Start()
    {
        while (ExampleManager.Instance.IsInRoom == false)
        {
            yield return 0;
        }
    }

    //Subscribe to messages that will be sent from the server
    private void OnEnable()
    {
        ExampleRoomController.onAddNetworkEntity += OnNetworkAdd;
        ExampleRoomController.onRemoveNetworkEntity += OnNetworkRemove;
        ExampleRoomController.onScoreUpdate += OnScoreUpdate;
        ExampleRoomController.onGotTargetLineUp += GotNewTargetLineUp;

        ExampleRoomController.onRoomStateChanged += OnRoomStateChanged;
        ExampleRoomController.onBeginRoundCountDown += OnBeginRoundCountDown;
        ExampleRoomController.onBeginRound += OnBeginRound;
        ExampleRoomController.onRoundEnd += OnRoundEnd;

        ExampleRoomController.OnCurrentUserStateChanged += OnUserStateChanged;

        ArrangeLighting(false);
        scoreboardController.ResetScoreboards();
        scoreboardController.SetMessage(GetPlayerReadyState());
        uiController.AllowExit(true);
    }

    //Unsubscribe
    private void OnDisable()
    {
        ExampleRoomController.onAddNetworkEntity -= OnNetworkAdd;
        ExampleRoomController.onRemoveNetworkEntity -= OnNetworkRemove;
        ExampleRoomController.onScoreUpdate -= OnScoreUpdate;
        ExampleRoomController.onGotTargetLineUp -= GotNewTargetLineUp;

        ExampleRoomController.onRoomStateChanged -= OnRoomStateChanged;
        ExampleRoomController.onBeginRoundCountDown -= OnBeginRoundCountDown;
        ExampleRoomController.onBeginRound -= OnBeginRound;
        ExampleRoomController.onRoundEnd -= OnRoundEnd;

        ExampleRoomController.OnCurrentUserStateChanged -= OnUserStateChanged;
    }

    private void OnBeginRoundCountDown()
    {
        _showCountdown = true;
        scoreboardController.ResetScoreboards();
        uiController.AllowExit(false);
        uiController.UpdatePlayerReadiness(false);
        ArrangeLighting(true);
    }

    private void OnBeginRound()
    {
        StartCoroutine(DelayedRoundBegin());
    }

    private IEnumerator DelayedRoundBegin()
    {
        yield return new WaitForSeconds(1);
        _countDownString = "";
        _showCountdown = false;
        uiController.UpdatePlayerReadiness(false);
        scoreboardController.BeginGame(targetsController.GetRemainingTargets());
    }

    private void OnRoundEnd(Winner winner)
    {
        PlayerController player = GetPlayerView(ExampleManager.Instance.CurrentNetworkedEntity.id);
        if (player != null)
        {
            player.UpdateReadyState(false);
        }
        string winnerMessage = GetWinningMessage(winner);
        scoreboardController.GameOver(winnerMessage);
        StartCoroutine(DelayedRoundEnd());
    }

    private IEnumerator DelayedRoundEnd()
    {
        yield return new WaitForSeconds(5);
        ArrangeLighting(false);
        if ((currentGameState == eGameState.WAITING || currentGameState == eGameState.WAITINGFOROTHERS) && lastGameState == eGameState.ENDROUND)
        {
            scoreboardController.SetMessage(GetPlayerReadyState());
            uiController.UpdatePlayerReadiness(AwaitingPlayerReady());
            uiController.AllowExit(true);
        }
    }

    private void Update()
    {
        if (AwaitingPlayerReady() && Input.GetKeyDown(KeyCode.Return))
        {
            PlayerReadyToPlay();
        }
    }

    private string GetWinningMessage(Winner winner)
    {
        string winnerMessage = "";

        if (winner.tie)
        {
            winnerMessage = $"TIE!\nThese players tied with a top score of {winner.score}:\n";
            for (int i = 0; i < winner.tied.Length; i++)
            {
                PlayerController p = GetPlayerView(winner.tied[i]);
                if (p != null)
                {
                    winnerMessage += $"{(p ? p.userName : winner.tied[i])}\n";
                }
            }
        }
        else
        {
            PlayerController p = GetPlayerView(winner.id);
            if (p != null)
            {
                winnerMessage = $"Round Over!\n{(p ? p.userName : winner.id)} wins!";
            }
        }

        return winnerMessage;
    }

    private eGameState TranslateGameState(string gameState)
    {
        switch (gameState)
        {
            case "Waiting":
            {
                PlayerController player = GetPlayerView(ExampleManager.Instance.CurrentNetworkedEntity.id);
                if (player != null)
                {
                    return player.isReady ? eGameState.WAITINGFOROTHERS : eGameState.WAITING;
                }

                return eGameState.WAITING;
            }
            case "SendTargets":
            {
                return eGameState.SENDTARGETS;
            }
            case "BeginRound":
            {
                return eGameState.BEGINROUND;
            }
            case "SimulateRound":
            {
                return eGameState.SIMULATEROUND;
            }
            case "EndRound":
            {
                return eGameState.ENDROUND;
            }
            default:
                return eGameState.NONE;
        }
    }

    private void OnRoomStateChanged(ColyseusMapSchema<string> attributes)
    {
        if (_showCountdown && attributes.ContainsKey("countDown"))
        {
            _countDownString = attributes["countDown"];
            scoreboardController.CountDown(_countDownString);
        }

        if (attributes.ContainsKey("currentGameState"))
        {
            eGameState nextState = TranslateGameState(attributes["currentGameState"]);
            if (IsSafeStateTransition(currentGameState, nextState))
            {
                currentGameState = nextState;
            }
            else
            {
                LSLog.LogError($"CurrentGameState: Failed to transition from {currentGameState} to {nextState}");
            }
        }

        if (attributes.ContainsKey("lastGameState"))
        {
            eGameState nextState = TranslateGameState(attributes["lastGameState"]);
            if (IsSafeStateTransition(lastGameState, nextState))
            {
                lastGameState = nextState;
            }
            else
            {
                LSLog.LogError($"LastGameState: Failed to transition from {lastGameState} to {nextState}");
            }
        }
    }

    private bool IsSafeStateTransition(eGameState fromState, eGameState nextState)
    {
        if (fromState == nextState)
            return true;

        switch (fromState)
        {
            case eGameState.WAITING:
            {
                return nextState == eGameState.WAITINGFOROTHERS || nextState == eGameState.SENDTARGETS;
            }
            case eGameState.WAITINGFOROTHERS:
            {
                return nextState == eGameState.SENDTARGETS || nextState == eGameState.BEGINROUND;
            }
            case eGameState.BEGINROUND:
            {
                return nextState == eGameState.SIMULATEROUND || nextState  == eGameState.ENDROUND;
            }
            case eGameState.SIMULATEROUND:
            {
                return nextState == eGameState.ENDROUND || nextState == eGameState.WAITING || nextState == eGameState.WAITINGFOROTHERS;
            }
            case eGameState.ENDROUND:
            {
                return nextState == eGameState.WAITING || nextState == eGameState.WAITINGFOROTHERS ||
                       nextState == eGameState.SENDTARGETS;
            }
            default:
            {
                return true;
            }
        }
    }

    private void OnUserStateChanged(ColyseusMapSchema<string> attributeChanges)
    {
        if (attributeChanges.TryGetValue("readyState", out string readyState))
        {
            userReadyState = readyState;

            if (AwaitingAnyPlayerReady())
            {
                scoreboardController.SetMessage(GetPlayerReadyState());
                uiController.UpdatePlayerReadiness(AwaitingPlayerReady());
            }
        }
    }

    private string GetPlayerReadyState()
    {
        string readyState = "Waiting for you to ready up!";

        PlayerController player = GetPlayerView(ExampleManager.Instance.CurrentNetworkedEntity.id);
        if (player != null)
        {
            readyState = player.isReady ? "Waiting on other players..." : "Waiting for you to ready up!";
        }

        return readyState;
    }

    public bool AwaitingPlayerReady()
    {
        //Returns true if we're waiting for THIS player to be ready
        if (currentGameState == eGameState.WAITING)
        {
            return true;
        }

        return false;
    }

    private bool AwaitingAnyPlayerReady()
    {
        //Returns true if the server is waiting for anyone to be ready
        return currentGameState == eGameState.WAITING || currentGameState == eGameState.WAITINGFOROTHERS;
    }

    private void OnNetworkAdd(ExampleNetworkedEntity entity)
    {
        if (ExampleManager.Instance.HasEntityView(entity.id))
        {
            LSLog.LogImportant("View found! For " + entity.id);
            scoreboardController.EntityAdded(entity); //Already exists in scene which means it has been initialized
        }
        else
        {
            LSLog.LogImportant("No View found for " + entity.id);
            CreateView(entity);
        }
    }

    private void OnNetworkRemove(ExampleNetworkedEntity entity, ColyseusNetworkedEntityView view)
    {
        RemoveView(view);
        scoreboardController.EntityRemoved(entity, view);
    }

    private void CreateView(ExampleNetworkedEntity entity)
    {
        StartCoroutine(WaitingEntityAdd(entity));
    }

    IEnumerator WaitingEntityAdd(ExampleNetworkedEntity entity)
    {
        PlayerController newView = Instantiate(prefab);
        ExampleManager.Instance.RegisterNetworkedEntityView(entity, newView);
        newView.gameObject.SetActive(true);
        float seconds = 0;
        float delayAmt = 1.0f;
        //Wait until we have the view's username to add it's scoreboard entry
        while (string.IsNullOrEmpty(newView.userName))
        {
            yield return new WaitForSeconds(delayAmt);
            seconds += delayAmt;
            if (seconds >= 30) //If 30 seconds go by and we don't have a userName, should still continue
            {
                newView.userName = "GUEST";
            }
        }

        scoreboardController.EntityAdded(entity);
    }

    private void RemoveView(ColyseusNetworkedEntityView view)
    {
        view.SendMessage("OnEntityRemoved", SendMessageOptions.DontRequireReceiver);
    }

    private void GotNewTargetLineUp(ShootingGalleryNewTargetLineUpMessage targetLineUp)
    {
        targetsController.GotNewTargetLineUp(targetLineUp);
    }

    private void OnScoreUpdate(ShootingGalleryScoreUpdateMessage update)
    {
        PlayerController pc = GetPlayerView(update.entityID);
        if (pc != null)
        {
            pc.ShowTargetHit();
        }

        targetsController.DestroyTargetByUID(update.targetUID);
        scoreboardController.UpdateScore(update, targetsController.GetRemainingTargets());
    }

    public void RegisterTargetKill(string entityID, string targetID)
    {
        ExampleManager.CustomServerMethod("scoreTarget", new object[] {entityID, targetID});
    }

    public void PlayerReadyToPlay()
    {
        uiController.UpdatePlayerReadiness(false);
        ExampleManager.NetSend("setAttribute",
            new ExampleAttributeUpdateMessage
            {
                userId = ExampleManager.Instance.CurrentUser.id,
                attributesToSet = new Dictionary<string, string> {{"readyState", "ready"}}
            });
        
        PlayerController player = GetPlayerView(ExampleManager.Instance.CurrentNetworkedEntity.id);
        if (player != null)
        {
            player.SetPause(false);
            player.UpdateReadyState(true);
        }
    }

    public PlayerController GetPlayerView(string entityID)
    {
        if (ExampleManager.Instance.HasEntityView(entityID))
        {
            return ExampleManager.Instance.GetEntityView(entityID) as PlayerController;
        }
        
        return null;
    }

    private void ArrangeLighting(bool ingame)
    {
        ingameLightingRoot.SetActive(ingame);
        pregameLightingRoot.SetActive(!ingame);
    }

    public void OnQuitGame()
    {
        if (ExampleManager.Instance.IsInRoom)
        {
            //Find playerController for this player
            PlayerController pc = GetPlayerView(ExampleManager.Instance.CurrentNetworkedEntity.id);
            if (pc != null)
            {
                pc.enabled = false; //Stop all the messages and updates
            }

            ExampleManager.Instance.LeaveAllRooms(() => { SceneManager.LoadScene("Lobby"); });
        }
    }

#if UNITY_EDITOR
    private void OnDestroy()
    {
        ExampleManager.Instance.OnEditorQuit();
    }
#endif
}