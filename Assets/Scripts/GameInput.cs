using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    #region Input Movement through analog stick

    float horizontalGameInput;
    public void MovementGameInput(InputAction.CallbackContext context) {
        horizontalGameInput = context.ReadValue<Vector2>().x;
    }

    #endregion
}
