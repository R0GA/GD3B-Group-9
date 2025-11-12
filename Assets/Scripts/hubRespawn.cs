using UnityEngine;

public class hubRespawn : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
{
    Debug.Log("Killbox triggered by: " + other.name);

    playerRespawn respawn = other.GetComponentInParent<playerRespawn>();
    if (respawn != null)
    {
        Debug.Log("Respawning player...");
        respawn.RespawnToSafePosition();
    }
}


}
