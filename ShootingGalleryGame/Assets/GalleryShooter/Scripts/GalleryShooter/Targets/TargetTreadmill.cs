using System.Collections.Generic;
using UnityEngine;

public class TargetTreadmill : MonoBehaviour
{
    public Transform farBounds;

    public bool isMoving = false;
    public Transform startBounds;
    private List<TargetTransform> targetRoots = new List<TargetTransform>();
    public Transform targetSpawnRoot;

    private List<TargetTransform> targetsReadyToSend = new List<TargetTransform>();

    public GameObject targetTransformPrefab;

    public float timeBetweenTargets = 0.5f;

    private float timer;

    private void Update()
    {
        if (!isMoving)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer >= timeBetweenTargets)
        {
            timer = 0.0f;
            TrySendTarget();
        }
    }

    public void Reset()
    {
        targetsReadyToSend = new List<TargetTransform>();
        timer = timeBetweenTargets; //Make it start immediately
        foreach (TargetTransform target in targetRoots)
        {
            target.isMoving = false;
            //When resetting, only send the targetTransforms that have targets attached. otherwise just place them offscreen
            PlaceTargetAtStart(target, target.HasTarget);
        }
    }

    public void HandTargets(List<TargetBase> targets)
    {
        for (int i = 0; i < targets.Count; ++i)
        {
            TargetTransform trans = GetOrSpawnTargetTransform();
            trans.HandTarget(targets[i]);
        }

        Reset();
    }

    private void PlaceTargetAtStart(TargetTransform target, bool sendAgain)
    {
        target.transform.localPosition = startBounds.localPosition;
        if (sendAgain)
        {
            targetsReadyToSend.Add(target);
        }
    }

    private TargetTransform GetAvailableTarget()
    {
        TargetTransform target = null;
        if (targetsReadyToSend.Count > 0)
        {
            target = targetsReadyToSend[0];
            targetsReadyToSend.RemoveAt(0);
        }

        return target;
    }

    private TargetTransform GetOrSpawnTargetTransform()
    {
        for (int i = 0; i < targetRoots.Count; ++i)
        {
            if (!targetRoots[i].HasTarget)
            {
                return targetRoots[i];
            }
        }

        //We haven't sent a root, so need to spawn a new one
        GameObject newTrans = Instantiate(targetTransformPrefab, targetSpawnRoot, false);
        TargetTransform target = newTrans.GetComponent<TargetTransform>();
        targetRoots.Add(target);
        targetsReadyToSend.Add(target);
        return target;
    }

    private void TrySendTarget()
    {
        TargetTransform target = GetAvailableTarget();
        if (target != null)
        {
            target.Move(farBounds.localPosition, farBounds.localPosition, PlaceTargetAtStart);
        }
    }
}