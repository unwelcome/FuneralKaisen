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
    private ThornEnimationStates State
    {
        get { return (ThornEnimationStates)animator.GetInteger("State"); }
        set { animator.SetInteger("State", (int)value); }
    }
    private enum ThornEnimationStates
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

    // Инициализация шипов
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        isActive = alwaysActive;
    }

    // Отсчет времени жизни
    private void Update()
    {
        if (!isActive)
        {
            State = ThornEnimationStates.idle_hidden;
            return;
        }

        activeTimer -= Time.deltaTime;

        if (activeTimer <= 0f)
        {
            Deactivate();
        }
    }

    // При первом контакте с шипами
    private void OnTriggerEnter2D(Collider2D collider)
    {
        Player player = collider.GetComponent<Player>();

        if (player != null && !isTriggered)
        {
            StartCoroutine(ActivateThorns()); // Активируем шипы
        }
    }

    // При продолжительном нахождении в шипах
    private void OnTriggerStay2D(Collider2D collider)
    {
        if (!isActive) return;

        Player player = collider.GetComponent<Player>();

        // Если это был игрок и он получил урон, то продливаем время жизни шипа
        if (player != null && player.GetDamage(damage))
        {
            activeTimer = activeTime;
        }
    }

    private System.Collections.IEnumerator ActivateThorns()
    {
        // Активируем шипы
        isTriggered = true;
        State = ThornEnimationStates.show;

        // Ожидаем задержку активации
        yield return new WaitForSeconds(activateDelay);

        // Шипы активированы
        isActive = true;
        activeTimer = activeTime;
        State = ThornEnimationStates.idle_active;
    }

    // Убираем шипы
    private void Deactivate()
    {
        isActive = false;
        isTriggered = false;
        State = ThornEnimationStates.hide;
    }
}
