using UnityEngine;

public class MovementController : MonoBehaviour
{
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
    [SerializeField] private float jumpForce = 10f;
    public float Velocity => velocity;
    public float JumpForce => jumpForce;
    public float VelocityMultiplier = 1f;
}
