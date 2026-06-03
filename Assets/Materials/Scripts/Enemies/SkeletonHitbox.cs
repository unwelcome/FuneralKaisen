using UnityEngine;

public class SkeletonHitbox : MonoBehaviour
{
    [HideInInspector] public int damage = 20;
    [SerializeField] private float lifetime = 0.15f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        HealthController health = other.GetComponentInParent<HealthController>();
        if (health == null) return;

        health.GetDamage(damage);
        Destroy(gameObject);
    }
}
