using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Gun : MonoBehaviour
{
    private const int clipSize = 10;

    [SerializeField]
    private Transform barrelTip = null;

    [SerializeField]
    private GameObject bulletPrefab = null;

    private bool canFire = true;

    [SerializeField]
    private TextMeshPro clipCountText = null;

    private int currentClip = clipSize;

    [SerializeField]
    private float fireSpeed = 1.0f;

    [SerializeField]
    private ParticleSystem muzzleFlash = null;

    [SerializeField]
    private float rateOfFire = 0.5f;

    [SerializeField]
    private GameObject reloadIcon = null;

    [SerializeField]
    private float reloadTime = 5f;

    private void Awake()
    {
        reloadIcon.SetActive(false);
        clipCountText.text = "";
    }

    public void Fire(string firingEntity)
    {
        if (!canFire)
        {
            return;
        }

        muzzleFlash.Play();
        if (firingEntity.Equals(ExampleManager.Instance.CurrentNetworkedEntity.id))
        {
            GameObject newBullet = Instantiate(bulletPrefab, barrelTip.position, Quaternion.identity);
            Bullet bullet = newBullet.GetComponent<Bullet>();
            bullet.Fire(firingEntity, fireSpeed, barrelTip.forward);
            StartCoroutine(FireDelay());
        }
    }

    private IEnumerator FireDelay()
    {
        canFire = false;
        --currentClip;
        currentClip = Math.Max(0, currentClip);
        UpdateCountText();
        if (currentClip <= 0)
        {
            reloadIcon.SetActive(true);
            yield return new WaitForSeconds(reloadTime);
            reloadIcon.SetActive(false);
            currentClip = clipSize;
            UpdateCountText();
        }
        else
        {
            yield return new WaitForSeconds(rateOfFire);
        }

        canFire = true;
    }

    private void UpdateCountText()
    {
        if (currentClip == clipSize)
        {
            clipCountText.text = "";
        }
        else
        {
            clipCountText.text = $"{currentClip}/{clipSize}";
        }
    }

    public void Reload()
    {
        if (!canFire)
        {
            return;
        }

        currentClip = 0;
        StartCoroutine(FireDelay());
    }
}