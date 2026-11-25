using UnityEngine;

public class HazardZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController2D>(out PlayerController2D player))
        {
            player.RespawnAtStart();
        }
    }
}