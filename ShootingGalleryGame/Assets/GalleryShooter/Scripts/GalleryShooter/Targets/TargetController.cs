using System;
using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    public delegate void OnTargetDestroyed(string entity, ShootingGalleryTargetModel model);

    public static OnTargetDestroyed onTargetDestroyed;

    [SerializeField]
    private GameObject defaultTarget = null;

    private List<ShootingGalleryTargetModel> newTargets;

    [SerializeField]
    private TargetPrefabPair[] potentialTargets = null;

    private int targetCount;

    [SerializeField]
    private TargetHolder targetHolder = null;

    private List<TargetBase> targetObjectsSpawned = new List<TargetBase>();

    [SerializeField]
    private List<string> trackedTargets = new List<string>();

    private void OnEnable()
    {
        onTargetDestroyed += HandleTargetHit;
        ExampleRoomController.onBeginRound += OnBeginRound;
        ExampleRoomController.onRoundEnd += OnRoundEnd;
    }

    private void OnDisable()
    {
        onTargetDestroyed -= HandleTargetHit;
        ExampleRoomController.onBeginRound -= OnBeginRound;
        ExampleRoomController.onRoundEnd -= OnRoundEnd;
    }

    public void GotNewTargetLineUp(ShootingGalleryNewTargetLineUpMessage targetLineUp)
    {
        if (targetLineUp == null || targetLineUp.targets == null)
        {
            LSLog.LogError("No targets came in");
            return;
        }

        targetCount = targetLineUp.targets.Length;
        newTargets = new List<ShootingGalleryTargetModel>();
        for (int i = 0; i < targetLineUp.targets.Length; ++i)
        {
            if (!trackedTargets.Contains(targetLineUp.targets[i].uid))
            {
                newTargets.Add(targetLineUp.targets[i]);
                trackedTargets.Add(targetLineUp.targets[i].uid);
            }
        }

        SpawnTargets(newTargets);
        string userID = ExampleManager.Instance.CurrentUser.id;
        ExampleManager.NetSend("setAttribute",
            new ExampleAttributeUpdateMessage
                {userId = userID, attributesToSet = new Dictionary<string, string> {{"readyState", "ready"}}});
    }

    private void OnBeginRound()
    {
        if (newTargets != null)
        {
            targetHolder.EnableMovement(true);
        }
    }

    private void SpawnTargets(List<ShootingGalleryTargetModel> newTargets)
    {
        List<TargetBase> spawnedTargets = new List<TargetBase>(); //Pass this to whatever will handle the targets
        foreach (ShootingGalleryTargetModel target in newTargets)
        {
            GameObject prefab = null;
            foreach (TargetPrefabPair targetPair in potentialTargets)
            {
                if (targetPair.id.Equals(target.id))
                {
                    prefab = targetPair.prefab;
                }
            }

            if (prefab == null)
            {
                LSLog.LogError($"Could not find a prefab for target with an ID of {target.id}... will use default");
                prefab = defaultTarget;
            }

            GameObject newTarget = Instantiate(prefab);
            TargetBase targetBase = newTarget.GetComponent<TargetBase>();
            targetBase.Init(target);
            targetObjectsSpawned.Add(targetBase);
            spawnedTargets.Add(targetBase);
        }

        targetHolder.Initialize(spawnedTargets);
        targetHolder.Reset();
    }

    private void HandleTargetHit(string entityID, ShootingGalleryTargetModel model)
    {
        GalleryGameManager.Instance.RegisterTargetKill(entityID, model.uid);
    }

    public void DestroyTargetByUID(string uid)
    {
        TargetBase deadTarget = null;
        foreach (TargetBase target in targetObjectsSpawned)
        {
            if (target.UID.Equals(uid))
            {
                deadTarget = target;
                break;
            }
        }

        if (deadTarget != null)
        {
            deadTarget.Explode();
            targetCount--;
        }
        else
        {
            LSLog.LogError($"Could not find a target object with uid {uid}");
        }
    }

    public int GetRemainingTargets()
    {
        return targetCount;
    }

    private void OnRoundEnd(Winner winner)
    {
        targetHolder.EnableMovement(false);
    }

    [Serializable]
    private class TargetPrefabPair
    {
#pragma warning disable 0649
        public int id;
#pragma warning restore 0649
        public GameObject prefab = null;
    }
}