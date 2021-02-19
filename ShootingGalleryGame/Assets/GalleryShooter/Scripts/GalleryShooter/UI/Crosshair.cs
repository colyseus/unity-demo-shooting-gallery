using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public Color normalColor = Color.white;
    public Color hitColor = Color.red;
    public Image crossHair;
    private float showTime = 0.25f;

    private Coroutine hitRoutine = null;

    public void ShowHit()
    {
        if (hitRoutine != null)
        {
            StopCoroutine(hitRoutine);
        }
        hitRoutine = StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        crossHair.color = hitColor;
        yield return new WaitForSeconds(showTime);
        crossHair.color = normalColor;

        hitRoutine = null;
    }
}
