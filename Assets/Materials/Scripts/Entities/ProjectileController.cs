using System;
using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour
{
    // Параметры снаряда
    public LayerMask groundLayers;
    public int baseDamage = 10;
    public float Speed = 10f;

    private int damage;
    private float gravityScale;
    private Vector2 direction;
    private Animator animator;
    private Rigidbody2D rb;


    // Инициализация полей
    public void InitProjectile(Collider2D owner, Vector2 direction, float damageScale)
    {
        rb = GetComponent<Rigidbody2D>();

        // Отключаем гравитацию на время создания снаряда
        gravityScale = rb.gravityScale;
        rb.gravityScale = 0f;

        // Снаряд стоит на месте
        rb.linearVelocity = Vector2.zero;

        // игнорируем коллизию с тем, кто выстрелил
        Collider2D thisCol = GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(thisCol, owner);

        this.direction = direction;
        damage = (int)Math.Round(baseDamage * damageScale);

        animator = GetComponentInChildren<Animator>();
    }


    // Проверка попадания
    public void OnTriggerEnter2D(Collider2D collider)
    {
        // Проверка попадания в стену / землю
        if ((groundLayers.value & (1 << collider.gameObject.layer)) != 0)
        {
            Debug.Log("hit wall!");

            animator.SetTrigger("IsHitted");

            return;
        }

        // Eсли у объекта есть здоровье — наносим урон
        HealthController targetHealthController = collider.GetComponent<HealthController>();

        if (targetHealthController != null)
        {
            // Сущность неуязвима, снаряд игнорирует
            if (!targetHealthController.GetDamage(damage)) return;

            animator.SetTrigger("IsHitted");
        }
    }


    // Запуск снаряда
    public void LaunchProjectile()
    {
        // Запуск снаряда
        rb.linearVelocity = new Vector2(Speed * direction.x, Speed * direction.y);

        // Возвращаем гравитацию
        rb.gravityScale = gravityScale;
    }


    // Остановка снаряда
    public void StopProjectile()
    {
        // Останавливаем снаряд
        Rigidbody2D rb = animator.GetComponentInParent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
    }


    // Уничтожение снаряда
    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }

}
