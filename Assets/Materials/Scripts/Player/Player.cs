using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // ---------------------------------------------- ПОЛЯ ----------------------------------------------

    [Header("Movement Settings")]
    [SerializeField] private float velocity = 3f;
    [SerializeField] private float jumpForce = 4f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;  // Ссылка на пустой объект в ногах
    [SerializeField] private Vector2 groundBoxSize = new Vector2(1f, 0.1f);
    [SerializeField] private float groundDistance = 0.05f;
    [SerializeField] private LayerMask groundLayer;  // Слой, на котором лежит земля
    [SerializeField] private LayerMask platformLayer; // Слой платформ

    [Header("Dash Settings")]
    [SerializeField] private float dashVelocity = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;


    [Header("Other")]
    [SerializeField] private int health = 100;
    [SerializeField] private float immortalTimeOnHit = 100;

    // Unity Components
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator animator;
    private Collider2D playerCollider;
    // Movement
    private float moveInput;
    // Jump
    private bool isGrounded;
    // Dash
    private bool isDashing;
    private bool canDash = true;
    // Drop
    private bool isDropping = false;
    // Immortality
    private bool isImmortal;
    // Animation state
    private PlayerAnimationStates State
    {
        get { return (PlayerAnimationStates)animator.GetInteger("State"); }
        set { animator.SetInteger("State", (int)value); }
    }

    public enum PlayerAnimationStates
    {
        idle,
        walk,
        jump,
        dash
    }

    // ---------------------------------------------- ПРИВАТНЫЕ МЕТОДЫ ----------------------------------------------

    // Инициализация
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
    }

    // Обработчик движений при нажатии 'A' и 'D'
    private void OnMove(InputValue value)
    {
        moveInput = value.Get<float>();
    }

    // Обработчик прыжка
    private void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            // Прыжок
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // Обработчик деша
    private void OnDash(InputValue value)
    {
        if (value.isPressed && canDash && !isDashing)
        {
            // Запуск деша
            StartCoroutine(Dash());
        }
    }

    // Обработчик проваливания вниз на платформе
    private void OnFall(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            StartCoroutine(Fall());
        }
    }

    // Физика
    private void FixedUpdate()
    {
        // Проверка, стоим ли мы на земле
        RaycastHit2D hit = Physics2D.BoxCast(
            groundCheck.position,
            groundBoxSize,
            0f,
            Vector2.down,
            groundDistance,
            groundLayer | platformLayer
        );

        isGrounded = hit.collider != null && hit.normal.y > 0.5f && !isDropping;

        // Во время деша нельзя двигаться
        if (isDashing) return;

        // Горизонтальное движение
        rb.linearVelocity = new Vector2(moveInput * velocity, rb.linearVelocity.y);
    }

    private void Update()
    {

        if (isDashing)
        {
            // Анимация деша
            //Debug.Log("dash");
            State = PlayerAnimationStates.dash;
            // Во время деша нельзя двигаться
            return;
        }

        // Анимация прыжка
        if (!isGrounded)
        {
            //Debug.Log("jump");
            State = PlayerAnimationStates.jump;
        }
        else
        {
            // Анимация покоя
            if (moveInput == 0)
            {
                //Debug.Log("idle");
                State = PlayerAnimationStates.idle;
            }
            // Анимация ходьбы
            else
            {
                //Debug.Log("walk");
                State = PlayerAnimationStates.walk;
            }
        }

        // Визуальное отражение спрайта
        if (moveInput > 0) sprite.flipX = false;
        else if (moveInput < 0) sprite.flipX = true;
    }

    // Корутина для деша
    private System.Collections.IEnumerator Dash()
    {
        // Пероеходим в режим деша
        canDash = false;
        isDashing = true;
        isImmortal = true;

        float direction = sprite.flipX ? -1f : 1f;

        // Отключаем гравитацию
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Задаем резкую скорость
        rb.linearVelocity = new Vector2(direction * dashVelocity, 0f);

        yield return new WaitForSeconds(dashDuration);

        // Возвращаем все к исходному состоянию
        isDashing = false;
        isImmortal = false;
        rb.gravityScale = originalGravity;

        // Ждем кд для деша
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // Корутина для проваливания сквозь платформу
    private System.Collections.IEnumerator Fall()
    {

        // Находим платформу под игроком
        Collider2D platform = Physics2D.OverlapCircle(groundCheck.position, groundDistance, platformLayer);

        if (platform != null)
        {
            // Отключаем коллизию
            Physics2D.IgnoreCollision(playerCollider, platform, true);
            isDropping = true;

            // Ждем, пока персонаж провалится сквозь платформу
            yield return new WaitForSeconds(0.3f);

            // Возвращаем коллизию
            Physics2D.IgnoreCollision(playerCollider, platform, false);
        }

        isDropping = false;
    }

    // Отладка
    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;

        Vector3 boxCenter = groundCheck.position + Vector3.down * groundDistance;

        Gizmos.DrawWireCube(boxCenter, new Vector3(groundBoxSize.x, groundBoxSize.y, 0));
    }

    // Корутина для активации неуязвимости
    private System.Collections.IEnumerator EnableImmortality(float immortalityTime)
    {
        // Устанавливаем неуязвимость
        isImmortal = true;

        // Делаем персонажа слегка прозрачным
        Color color = sprite.color;
        color.a = 0.85f;
        sprite.color = color;

        // Добавить анимацию неуязвимости
        yield return new WaitForSeconds(immortalityTime);

        // Убираем неуязвимость
        isImmortal = false;

        // Убираем полупрозрачность
        color.a = 1f;
        sprite.color = color;
    }

    // ---------------------------------------------- ПУБЛИЧНЫЕ МЕТОДЫ ----------------------------------------------

    // Получение урона
    public bool GetDamage(int damage)
    {
        if (isImmortal) return false;

        health = Math.Max(health - damage, 0);
        Debug.Log($"Health: {health}");

        StartCoroutine(EnableImmortality(immortalTimeOnHit));
        return true;
    }
}