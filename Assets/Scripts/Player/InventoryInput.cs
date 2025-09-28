using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryInput : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction inventoryAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        inventoryAction = playerInput.actions["OpenInventory"];
    }

    private void OnEnable()
    {
        inventoryAction.performed += OnInventoryPressed;
    }

    private void OnDisable()
    {
        inventoryAction.performed -= OnInventoryPressed;
    }

    private void OnInventoryPressed(InputAction.CallbackContext context)
    {
        InventoryUIManager.Instance.ToggleInventory();
    }
}