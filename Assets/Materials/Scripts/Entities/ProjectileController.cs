using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour
{
    // Параметры снаряда
    public float speed = 10f;
    private int damage;

    // Ключевые объекты
    private Collider2D ownerCollider; // Хит бокс создателя снаряда (для игнорирования коллизии)
    private Rigidbody2D rb; // Сам снаряд

    // Анимации снаряда
    private Animator animator;
    private ProjectileAnimationStates State;
    private enum ProjectileAnimationStates
    {
        create,
        idle,
        destroy
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
    }

    public void LaunchAfterDelay(Collider2D owner, float delay, float direction, float gravityScale, int damage)
    {
        // Задаем параметры для снаряда
        this.damage = damage;
        this.ownerCollider = owner;

        // Задаем гравитацию для снаряда
        rb.gravityScale = gravityScale;

        // Включаем анимацию создания
        State = ProjectileAnimationStates.create;

        // игнорируем коллизию с тем, кто выстрелил
        Collider2D myCol = GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(myCol, ownerCollider);

        // Запуск снаряда
        StartCoroutine(Launch(delay, direction));
    }

    IEnumerator Launch(float delay, float direction)
    {
        yield return new WaitForSeconds(delay);

        // Анимация полета
        State = ProjectileAnimationStates.idle;

        // Задаем траекторию движения
        rb.linearVelocity = new Vector2(speed * direction, 0f);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        // Eсли у объекта есть здоровье — наносим урон
        HealthController hp = collider.GetComponent<HealthController>();

        if (hp != null)
        {
            hp.TakeDamage(damage);
        }

        // Анимация уничтожения снаряда
        State = ProjectileAnimationStates.destroy;

        // Уничтожаем снаряд
        Destroy(gameObject);
    }
}
