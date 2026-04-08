using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("FirePoint")]
    public Transform firePoint;

    [Header("Blue")]
    public GameObject bluePrefab;

    private Collider2D collider;
    private SpriteRenderer sprite;
    private PlayerMovement movement;

    private void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        movement = GetComponent<PlayerMovement>();
        collider = GetComponent<Collider2D>();
    }

    private void OnAttackBlue(InputValue value)
    {
        ShootBlue();
    }

    private void ShootBlue()
    {
        // Создаем снаряд
        GameObject proj = Instantiate(bluePrefab, firePoint.position, Quaternion.identity);

        // Инициализируем поля снаряда
        ProjectileController projectileController = proj.GetComponent<ProjectileController>();
        projectileController.InitProjectile(collider, new Vector2(movement.FacingDirection, 0f), 1f);
    }

}
