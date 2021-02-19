using System.Collections.Generic;
using UnityEngine;

public class TargetHolder : MonoBehaviour
{
    [SerializeField]
    private TargetTreadmill[] treadmills = null;

    public void Initialize(List<TargetBase> spawnedTargets)
    {
        Dictionary<int, List<TargetBase>> targets = new Dictionary<int, List<TargetBase>>();
        for (int i = 0; i < spawnedTargets.Count; ++i)
        {
            if (!targets.ContainsKey(spawnedTargets[i].Row))
            {
                targets.Add(spawnedTargets[i].Row, new List<TargetBase>());
            }

            targets[spawnedTargets[i].Row].Add(spawnedTargets[i]);
        }

        foreach (KeyValuePair<int, List<TargetBase>> pair in targets)
        {
            if (pair.Key - 1 < treadmills.Length)
            {
                treadmills[pair.Key - 1].HandTargets(pair.Value);
            }
        }
    }

    public void EnableMovement(bool enabled)
    {
        for (int i = 0; i < treadmills.Length; ++i)
        {
            treadmills[i].isMoving = enabled;
        }
    }

    public void Reset()
    {
        for (int i = 0; i < treadmills.Length; ++i)
        {
            treadmills[i].Reset();
        }
    }
}