using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothing = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    void FixedUpdate()
    {
        if (target == null) return;

        // Рассчитываем позицию для камеры относительно позиции персонажа
        Vector3 targetPosition = target.position + offset;

        // Плавно перемещаем камеру в новую позицию
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
    }
}
