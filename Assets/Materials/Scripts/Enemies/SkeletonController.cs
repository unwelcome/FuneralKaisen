using UnityEngine;
using System.Collections;

[RequireComponent(typeof(HealthController))]
public class SkeletonController : MonoBehaviour
{
    [Header("Атака")]
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private GameObject hitboxPrefab;
    [SerializeField] private Transform hitboxSpawnPoint;

    [Header("Аниматор — параметр и значения State")]
    [SerializeField] private string animStateParam = "State";
    [SerializeField] private int stateIdle   = 0;
    [SerializeField] private int stateAttack = 2;
    [SerializeField] private int stateHurt   = 4;
    [SerializeField] private int stateDeath  = 5;

    [Header("Компоненты (назначить вручную!)")]
    [SerializeField] private Animator animator;
    [Tooltip("Центр скелета для расчёта дистанции — перетащи Square")]
    [SerializeField] private Transform body;

    [Header("Поворот")]
    [Tooltip("true = спрайт по умолчанию смотрит вправо")]
    [SerializeField] private bool startFacingRight = true;

    private bool isDead         = false;
    private bool isAttacking    = false;
    private bool isHurt         = false;
    private bool canAttack      = true;
    private bool hitboxSpawned  = false;
    private bool isFacingRight;

    private int lastHealth;
    private int lastArmor;

    private HealthController healthController;
    private Transform player;

    // ── Инициализация ──────────────────────────────────────────────

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        healthController = GetComponent<HealthController>();
        isFacingRight    = startFacingRight;
    }

    private void OnEnable()
    {
        healthController.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        healthController.OnHealthChanged -= HandleHealthChanged;
    }

    private void Start()
    {
        lastHealth = healthController.Health;
        lastArmor  = healthController.Armor;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    // ── Основной цикл ──────────────────────────────────────────────

    private void Update()
    {
        if (isDead || player == null)  return;
        if (isAttacking || isHurt)     return;

        FacePlayer();

        if (Vector2.Distance(BodyPosition, player.position) <= attackRange && canAttack)
            StartCoroutine(AttackRoutine());
        else
            SetState(stateIdle);
    }

    // ── Атака ──────────────────────────────────────────────────────

    private IEnumerator AttackRoutine()
    {
        isAttacking   = true;
        canAttack     = false;
        hitboxSpawned = false;

        SetState(stateAttack);

        // Ждём Animation Event OnAttackEnd() или таймаут 2 сек
        float elapsed = 0f;
        while (isAttacking && elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        isAttacking = false;
        SetState(stateIdle);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // Animation Event — ставится на кадр удара в анимации атаки
    public void SpawnHitbox()
    {
        if (hitboxPrefab == null || isDead || hitboxSpawned) return;
        hitboxSpawned = true;

        Vector3 offset   = Vector3.right * (isFacingRight ? 0.8f : -0.8f);
        Vector3 spawnPos = hitboxSpawnPoint != null
            ? hitboxSpawnPoint.position
            : BodyPosition + offset;

        GameObject go = Instantiate(hitboxPrefab, spawnPos, Quaternion.identity);
        go.layer = LayerMask.NameToLayer("Default");
        if (go.TryGetComponent(out SkeletonHitbox hitbox))
            hitbox.damage = attackDamage;

        // Страховка: уничтожить хитбокс через 0.5 сек даже если скрипт не сработал
        Destroy(go, 0.5f);
    }

    // Animation Event — ставится на последний кадр анимации атаки
    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    // ── Урон (через HealthController) ──────────────────────────────

    private void HandleHealthChanged(int armor, int maxArmor, int health, int maxHealth)
    {
        bool damageTaken = health < lastHealth || armor < lastArmor;
        lastHealth = health;
        lastArmor  = armor;

        if (!damageTaken) return;

        if (health <= 0)
            Die();
        else if (!isHurt)
            StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        isHurt      = true;
        isAttacking = false;

        SetState(stateHurt);

        // Ждём Animation Event OnHurtEnd() или таймаут 1 сек
        float elapsed = 0f;
        while (isHurt && elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        isHurt = false;
        SetState(stateIdle);
    }

    // Animation Event — ставится на последний кадр анимации hurt
    public void OnHurtEnd()
    {
        isHurt = false;
    }

    // ── Смерть ─────────────────────────────────────────────────────

    private void Die()
    {
        isDead = true;
        StopAllCoroutines();

        if (TryGetComponent(out Collider2D col)) col.enabled = false;
        if (TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        SetState(stateDeath);
    }

    // Animation Event — последний кадр анимации смерти
    public void OnDeathAnimEnd()
    {
        Destroy(gameObject);
    }

    // ── Вспомогательные ────────────────────────────────────────────

    private void SetState(int state)
    {
        animator.SetInteger(animStateParam, state);
    }

    private void FacePlayer()
    {
        bool playerToRight = player.position.x > BodyPosition.x;
        if (playerToRight == isFacingRight) return;

        isFacingRight    = playerToRight;
        Vector3 s        = transform.localScale;
        s.x             *= -1f;
        transform.localScale = s;
    }

    private Vector3 BodyPosition => body != null ? body.position : transform.position;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(BodyPosition, attackRange);
    }
}
