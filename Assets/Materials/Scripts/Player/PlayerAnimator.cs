using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    // Components
    private Animator animator;
    private PlayerMovement movementController;
    //private HealthController healthController;

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
        dash,
        death
    }

    // Init
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        movementController = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        //if (health <= 0)
        //{
        //    // Анимация смерти
        //    State = PlayerAnimationStates.death;
        //    return;
        //}
        if (movementController.isDashing)
        {
            // Анимация деша
            State = PlayerAnimationStates.dash;
            // Во время деша нельзя двигаться
            return;
        }

        // Анимация прыжка
        if (!movementController.isGrounded) State = PlayerAnimationStates.jump;
        else
        {
            // Анимация покоя
            if (movementController.moveInput == 0) State = PlayerAnimationStates.idle;
            // Анимация ходьбы
            else State = PlayerAnimationStates.walk;
        }
    }
}
