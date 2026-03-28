using UnityEngine;

public class HealthController : MonoBehaviour
{
    public int health;

    public bool TakeDamage(int damage)
    {

        Debug.Log($"Take {damage} damage points");
        
        return true;
    }
}
