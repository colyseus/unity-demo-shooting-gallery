using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusIndicator : MonoBehaviour
{
    public Color waitingColor = Color.red;
    public Color readyColor = Color.green;
    public SpriteRenderer statusIndicator;

    public void UpdateReady(bool readyStatus)
    {
        if(statusIndicator != null)
            statusIndicator.color = readyStatus ? readyColor : waitingColor;
    }
}
