using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // ---------------------------------------------- ПОЛЯ ----------------------------------------------
    [Header("Graphics child")]
    [SerializeField] private Transform graphics;

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

    // Unity Components
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    // Movement
    public float FacingDirection { get; private set; } = 1f; // 1 - Right | -1 - Left
    public float moveInput { get; private set; } = 0;
    // Jump
    public bool isGrounded { get; private set; } = false;
    // Dash
    public bool isDashing { get; private set; } = false;
    private bool canDash = true;
    // Drop
    public bool isDropping { get; private set; } = false;

    // ---------------------------------------------- ПРИВАТНЫЕ МЕТОДЫ ----------------------------------------------

    // Инициализация
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        // Отключение касания вврагов
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Enemies"),
            true
        );

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

        isGrounded = hit.collider != null && !isDropping; // && hit.normal.y > 0.7f

        // Во время деша нельзя двигаться
        if (isDashing) return;

        // Горизонтальное движение
        rb.linearVelocity = new Vector2(moveInput * velocity, rb.linearVelocity.y);
    }

    // Отражение персонажа
    private void Update()
    {
        // Во время деша нельзя двигаться
        if (isDashing) return;

        // Отражение графического компонента
        if (moveInput > 0)
        {
            FacingDirection = 1f;
            graphics.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (moveInput < 0)
        {
            FacingDirection = -1f;
            graphics.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    // Корутина для деша
    private System.Collections.IEnumerator Dash()
    {
        // Пероеходим в режим деша
        canDash = false;
        isDashing = true;
        //isImmortal = true;

        float direction = FacingDirection;

        // Отключаем гравитацию
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Задаем резкую скорость
        rb.linearVelocity = new Vector2(direction * dashVelocity, 0f);

        yield return new WaitForSeconds(dashDuration);

        // Возвращаем все к исходному состоянию
        isDashing = false;
        //isImmortal = false;
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
}
