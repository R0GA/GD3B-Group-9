using UnityEngine;

public class PlayerAnimEvent : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private void Start()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void WeaponAttack()
    {
        playerMovement.Attack();
    }
}
