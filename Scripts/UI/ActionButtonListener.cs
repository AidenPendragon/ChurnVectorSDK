using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ActionButtonListener : MonoBehaviour {
    private static List<ActionButtonListener> actionStack = new();
    [SerializeField]
    private InputActionReference action;

    public static bool HasAction() => actionStack.Count > 0;

    private Button button;

    private void Awake() {
        button = GetComponent<Button>();
    }

    private void OnEnable() {
        action.action.performed += OnPerformed;
        actionStack.Add(this);
    }

    private void OnDisable() {
        action.action.performed -= OnPerformed;
        actionStack.Remove(this);
    }

    void OnPerformed(InputAction.CallbackContext ctx) {
        if (actionStack[^1] != this) {
            return;
        }
        if (EventSystem.current.currentSelectedGameObject == button.gameObject) {
            button.onClick?.Invoke();
        } else if (button.IsInteractable()) {
            button.Select();
        }
    }
}
