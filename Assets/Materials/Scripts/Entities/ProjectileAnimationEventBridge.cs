using UnityEngine;

public class ProjectileAnimationEventBridge : MonoBehaviour
{
    private ProjectileController projectileController;

    private void Start()
    {
        projectileController = GetComponentInParent<ProjectileController>();
    }

    // Запуск снаряда
    public void LaunchProjectile()
    {
        projectileController.LaunchProjectile();
    }


    // Остановка снаряда
    public void StopProjectile()
    {
        projectileController.StopProjectile();
    }


    // Уничтожение снаряда
    public void DestroyProjectile()
    {
        projectileController.DestroyProjectile();
    }
}
