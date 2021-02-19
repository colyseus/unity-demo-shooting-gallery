using TMPro;
using UnityEngine;

public class TargetBase : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed = 0.0f;

    [SerializeField]
    private GameObject hitEffectPrefab = null;

    private ShootingGalleryTargetModel modelRef;

    [SerializeField]
    private TextMeshPro scoreValue = null;

    public string UID
    {
        get { return modelRef.uid; }
    }

    public int Row
    {
        get { return modelRef.row; }
    }

    public float MoveSpeed
    {
        get { return _moveSpeed; }
    }

    public void Init(ShootingGalleryTargetModel model)
    {
        modelRef = model;
        if (scoreValue != null)
        {
            scoreValue.text = model.value.ToString("N0");
        }
    }

    //This function is called in Bullet.cs OnCollisionEnter via SendMessageUpwards
    public virtual void OnHit(string entityID)
    {
        //Only send this message if you're the one who shot the target!
        if (entityID.Equals(ExampleManager.Instance.CurrentNetworkedEntity.id))
        {
            TargetController.onTargetDestroyed(entityID, modelRef);
        }
    }

    //Sent via message from websocket. Target will explode if anyone "scored" it, including local client
    public virtual void Explode()
    {
        gameObject.SetActive(false);
        GameObject effect = Instantiate(hitEffectPrefab, transform.position, transform.rotation);
        Destroy(effect, 2.0f);
        Destroy(gameObject);
    }
}