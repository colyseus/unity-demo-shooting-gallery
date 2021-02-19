using System;
using UnityEngine;

public class TargetTransform : MonoBehaviour
{
    private Vector3 destination;

    public bool isMoving;
    private Vector3 maxPos;
    private Action<TargetTransform, bool> onReachDest;
    private TargetBase targetBase;

    private GameObject targetGameObject;

    [SerializeField]
    private Transform targetRoot = null;

    public bool HasTarget
    {
        get { return targetGameObject != null; }
    }

    public void Move(Vector3 dest, Vector3 max, Action<TargetTransform, bool> _onReachDest)
    {
        destination = dest;
        maxPos = max;
        onReachDest = _onReachDest;
        isMoving = true;
    }

    public void HandTarget(TargetBase target)
    {
        targetGameObject = target.gameObject;
        targetBase = target;
        targetGameObject.transform.SetParent(targetRoot, false);
        targetGameObject.transform.localPosition = Vector3.zero;
    }

    private void Reset()
    {
        isMoving = false;
    }

    private void FixedUpdate()
    {
        if (!isMoving)
        {
            return;
        }

        transform.localPosition =
            Vector3.MoveTowards(transform.localPosition, destination, targetBase.MoveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.localPosition, maxPos) <= 0.25f) //We're close enough, trigger our onReach callback and reset ourselves
        {
            onReachDest?.Invoke(this, true);
            Reset();
        }
    }
}