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
        // Only allow opening inventory in the hub
        if (!PlayerController.IsInHub())
        {
            // Do not hide hotbar if inventory can't be opened
            return;
        }

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
        // Only allow opening gacha in the hub
        if (!PlayerController.IsInHub())
        {
            // Do not hide hotbar if gacha can't be opened
            return;
        }

        bool gachaOpen = GachaUIManager.Instance.IsActive();
        // Assuming GachaUIManager has similar toggle functionality
        GachaUIManager.Instance.ShowGachaUI();

        if (HotbarUIManager.Instance != null)
        {
            HotbarUIManager.Instance.OnInventoryStateChanged(!gachaOpen);
        }
    }
}