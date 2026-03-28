using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("FirePoint")]
    public Transform firePoint;

    [Header("Blue")]
    public GameObject bluePrefab;
    public float blueLaunchDelay = 0.2f;
    public float blueGravityScale = 0f;
    public int blueDamage = 10;

    private void OnAttackBlue(InputValue value)
    {
        ShootBlue();
    }

    private void ShootBlue()
    {
        GameObject proj = Instantiate(bluePrefab, firePoint.position, Quaternion.identity);

        ProjectileController projectileScript = proj.GetComponent<ProjectileController>();
        projectileScript.LaunchAfterDelay(
            GetComponent<Collider2D>(), 
            blueLaunchDelay, 
            transform.localScale.x,
            blueGravityScale,
            blueDamage);
    }

}
