using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryInput : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction inventoryAction;
    private InputAction gachaAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        inventoryAction = playerInput.actions["OpenInventory"];
        gachaAction = playerInput.actions["OpenGacha"];
    }

    private void OnEnable()
    {
        inventoryAction.performed += OnInventoryPressed;
        gachaAction.performed += OnGachaPressed;
    }

    private void OnDisable()
    {
        inventoryAction.performed -= OnInventoryPressed;
        gachaAction.performed -= OnGachaPressed;
    }

    private void OnInventoryPressed(InputAction.CallbackContext context)
    {
        InventoryUIManager.Instance.ToggleInventory();
    }
    private void OnGachaPressed(InputAction.CallbackContext context)
    {
        GachaUIManager.Instance.ShowGachaUI();
    }
}