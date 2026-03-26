using UnityEngine;

public class ThornsController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool alwaysActive = false;
    [SerializeField] private float activateDelay = 0.5f;
    [SerializeField] private float activeTime = 1.5f;
    [SerializeField] private int damage = 10;

    // Анимации
    private Animator animator;
    private ThornAnimationStates State
    {
        get { return (ThornAnimationStates)animator.GetInteger("State"); }
        set { animator.SetInteger("State", (int)value); }
    }
    private enum ThornAnimationStates
    {
        idle_hidden,
        show,
        idle_active,
        hide
    }

    // Поля
    private bool isActive;
    private bool isTriggered;
    private float activeTimer = 0f;
    private Player playerInside;

    // Инициализация шипов
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        isActive = alwaysActive;
    }

    private void Update()
    {
        if (!isActive)
        {
            State = ThornAnimationStates.idle_hidden;
            return;
        }

        // Обновляем таймер шипов пока игрок внутри и наносим урон
        if (playerInside != null)
        {
            activeTimer = activeTime;
            playerInside.GetDamage(damage);
        }
        // Иначе уменьшаем таймер жизни шипов
        else activeTimer -= Time.deltaTime;

        // Время жизни истекло -> отключаем
        if (activeTimer <= 0f) Deactivate();
    }

    // При первом контакте с шипами запоминаем игрока
    private void OnTriggerEnter2D(Collider2D collider)
    {
        Player player = collider.GetComponent<Player>();

        if (player != null)
        {
            playerInside = player;
            if (!isTriggered) StartCoroutine(ActivateThorns()); // Активируем шипы
        }
    }

    // При выходе игрока из шипов
    private void OnTriggerExit2D(Collider2D collider)
    {
        Player player = collider.GetComponent<Player>();

        if (player != null && playerInside == player)
        {
            playerInside = null;
        }
    }

    private System.Collections.IEnumerator ActivateThorns()
    {
        // Активируем шипы
        isTriggered = true;
        State = ThornAnimationStates.show;

        // Ожидаем задержку активации
        yield return new WaitForSeconds(activateDelay);

        // Шипы активированы
        isActive = true;
        activeTimer = activeTime;
        State = ThornAnimationStates.idle_active;
    }

    // Убираем шипы
    private void Deactivate()
    {
        isActive = false;
        isTriggered = false;
        State = ThornAnimationStates.hide;
    }
}
