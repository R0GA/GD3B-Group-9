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
        bool inventoryOpen = InventoryUIManager.Instance.gameObject.activeInHierarchy;
        InventoryUIManager.Instance.ToggleInventory();

        // Update hotbar visibility
        if (HotbarUIManager.Instance != null)
        {
            HotbarUIManager.Instance.OnInventoryStateChanged(!inventoryOpen);
        }
    }

    private void OnGachaPressed(InputAction.CallbackContext context)
    {
        // Assuming GachaUIManager has similar toggle functionality
        GachaUIManager.Instance.ShowGachaUI();

        // Hide hotbar when gacha is open
        if (HotbarUIManager.Instance != null)
        {
            HotbarUIManager.Instance.OnInventoryStateChanged(true);
        }
    }
}