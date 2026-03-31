using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // ---------------------------------------------- ПОЛЯ ----------------------------------------------
    [Header("Graphics child")]
    [SerializeField] private Transform graphics;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;  // Ссылка на пустой объект в ногах
    [SerializeField] private Vector2 groundBoxSize = new Vector2(1f, 0.1f);
    [SerializeField] private float groundDistance = 0.05f;
    [SerializeField] private LayerMask groundLayer;  // Слой, на котором лежит земля
    [SerializeField] private LayerMask platformLayer; // Слой платформ

    [Header("Movement Settings")]
    [SerializeField] private float velocity = 3f;
    [SerializeField] private float jumpForce = 4f;

    [Header("Jump Settings")]
    [SerializeField] private int maxJumpCount = 2;

    [Header("Dash Settings")]
    [SerializeField] private float dashVelocity = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("WallRun Settings")]
    [SerializeField] private float wallRunTime = 1.0f;
    [SerializeField] private float wallRunSpeed = 5f;
    [SerializeField] private float wallCheckDistance = 0.2f;

    // Unity Components
    private Rigidbody2D rb;
    private Collider2D playerCollider;

    // Movement
    public float FacingDirection { get; private set; } = 1f; // 1 - Right | -1 - Left
    public float moveInput { get; private set; } = 0;

    // Jump
    public bool isGrounded { get; private set; } = false;
    private int jumpCount;

    // Dash
    public bool isDashing { get; private set; } = false;
    private bool canDash = true;

    // Drop
    public bool isDropping { get; private set; } = false;

    // WallRun
    private float wallRunTimer;
    private bool isWallRunning;
    private bool isTouchingWall;
    private bool isJumpedAfterWallRun;
    private int wallSide; // -1 = слева, 1 = справа
    private float originalGravityScale;

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

        jumpCount = maxJumpCount;
        originalGravityScale = rb.gravityScale;
    }

    // Обработчик движений при нажатии 'A' и 'D'
    private void OnMove(InputValue value)
    {
        moveInput = value.Get<float>();
    }

    // Обработчик прыжка
    private void OnJump(InputValue value)
    {
        if (value.isPressed && (jumpCount > 0))
        {
            // Прыжок после бега по стене
            if (isWallRunning || IsWallHolding())
            {
                // Прыжок от стены
                rb.linearVelocity = new Vector2(-wallSide * jumpForce, jumpForce);

                // Запоминаем что отпрыгнули от стены, чтобы снова к ней не прилипнуть без второго прыжка
                isJumpedAfterWallRun = true;
                jumpCount--;

                // Останавливаем бег по стене
                StopWallRun();

                return;
            }

            // При втором и последующих прыжках после отпрыгивания от стены забываем, что отпрыгивали от нее (можно заново прилипнуть к стене)
            if (!isWallRunning && jumpCount <= maxJumpCount - 1)
            {
                wallRunTimer = wallRunTime;
                isJumpedAfterWallRun = false;
            }

            // Если это первый прыжок и он осуществляется уже в воздухе, то тратим дополнительный прыжок
            if (!isGrounded && jumpCount == maxJumpCount) jumpCount--;

            // Прыжок
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            jumpCount--;
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

        // Проверяем касание стены
        CheckWall();

        // Во время деша все другие виды движения недоступны
        if (isDashing) return;

        // Если касаемся стены, начинаем бежать по стене
        if (CanWallRun())
        {
            StartWallRun();
        }
        else
        {
            // Таймер бега по стене закончился - залипаем на стене
            if (IsWallHolding())
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0;
            } 
            else // Иначе падаем вниз
            {
                StopWallRun();
            }
        }

        // Горизонтальное движение
        rb.linearVelocity = new Vector2(moveInput * velocity, rb.linearVelocity.y);
    }

    // Отражение персонажа
    private void Update()
    {
        // Если стоим на земле - сбрасываем счетчики
        if (isGrounded)
        {
            wallRunTimer = wallRunTime;
            isJumpedAfterWallRun = false;
            jumpCount = maxJumpCount;
        }

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
        rb.gravityScale = 0f;

        // Задаем резкую скорость
        rb.linearVelocity = new Vector2(direction * dashVelocity, 0f);

        yield return new WaitForSeconds(dashDuration);

        // Возвращаем все к исходному состоянию
        isDashing = false;
        //isImmortal = false;
        rb.gravityScale = originalGravityScale;

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

    // Проверка касания стены
    private void CheckWall()
    {
        // Вычисляем половину ширины персонажа
        float halfWidth = playerCollider.bounds.extents.x;

        // Сдвигаем луч для проверки касания стены на края персонажа
        Vector2 originRight = new Vector2(playerCollider.bounds.center.x + halfWidth, playerCollider.bounds.center.y);
        Vector2 originLeft = new Vector2(playerCollider.bounds.center.x - halfWidth, playerCollider.bounds.center.y);

        // Проверяем контакт со стеной
        RaycastHit2D leftHit = Physics2D.Raycast(originLeft, Vector2.left, wallCheckDistance, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(originRight, Vector2.right, wallCheckDistance, groundLayer);

        if (leftHit.collider != null)
        {
            isTouchingWall = true;
            wallSide = -1;
        }
        else if (rightHit.collider != null)
        {
            isTouchingWall = true;
            wallSide = 1;
        }
        else
        {
            isTouchingWall = false;
            wallSide = 0;
        }
    }

    // Проверка возможности бега по стене
    private bool CanWallRun()
    {
        return !isGrounded && // не на земле
               isTouchingWall && // касаемся стены
               moveInput == wallSide && // жмём в сторону стены
               !isJumpedAfterWallRun && // еще не отпрыгивали от стены
               wallRunTimer > 0; // есть время для бега по стене
    }

    // Зависание на стене
    private bool IsWallHolding()
    {
        return !isGrounded && // не на земле
               isTouchingWall && // касаемся стены
               moveInput == wallSide && // жмём в сторону стены
               !isJumpedAfterWallRun && // еще не отпрыгивали от стены
               wallRunTimer <= 0; // время для бега по стене закончилось
    }

    // Запуск бега по стене
    private void StartWallRun()
    {
        isWallRunning = true;

        // Сбрасываем счетчик прыжков
        jumpCount = maxJumpCount;

        // Бежим вверх
        rb.linearVelocity = new Vector2(0, wallRunSpeed);

        // Уменьшаем время бега по стене
        wallRunTimer -= Time.fixedDeltaTime;

        // отключаем гравитацию
        rb.gravityScale = 0;
    }

    // Остановка бега по стене
    private void StopWallRun()
    {
        isWallRunning = false;
        rb.gravityScale = originalGravityScale; // вернуть гравитацию
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
