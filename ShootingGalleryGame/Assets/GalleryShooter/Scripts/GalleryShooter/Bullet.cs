using UnityEngine;

public class Bullet : MonoBehaviour
{
    private string _ownerId;

    private Rigidbody _rigidbody;

    [SerializeField]
    private GameObject impactEffect = null;

    public void Fire(string owner, float speed, Vector3 direction)
    {
        _ownerId = owner;
        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        _rigidbody.velocity = direction * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject impactFX = Instantiate(impactEffect, transform.position, Quaternion.identity);
        Destroy(impactFX, 1.0f);
        Destroy(gameObject);
        collision.gameObject.SendMessageUpwards("OnHit", _ownerId, SendMessageOptions.DontRequireReceiver);
    }
}