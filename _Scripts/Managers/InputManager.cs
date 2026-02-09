using UnityEngine;
using UnityEngine.Events;

public class InputManager : Singleton_Abs<InputManager>
{
    public static UnityEvent<Vector3Int> _onNewInputDirection = new UnityEvent<Vector3Int>();

    #region Unity Editor
#if UNITY_EDITOR
    public void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 input = new Vector2 { x = horizontal, y = vertical };

        if (input.x != 0)
        {
            input.y = 0;
        }

        if (input == Vector2.zero) return;

        Vector3Int direction = new Vector3Int((int)input.x, (int)input.y, 0);
        _onNewInputDirection?.Invoke(direction);
    }
#endif
    #endregion
    //
    // new input system for Pc
    //public void _OnNewPlayerInput(InputAction.CallbackContext context)
    //{
    //    Vector2 input = context.ReadValue<Vector2>();

    //    if (input.x != 0)
    //    {
    //        input.y = 0;
    //    }
    //    if (input == Vector2.zero) return;

    //    Vector3Int direction = new Vector3Int((int)input.x, (int)input.y, 0);
    //    _onNewInputDirection?.Invoke(direction);
    //}
    public void _OnNewPlayerInput(Vector3Int iDirection)
    {
        _onNewInputDirection?.Invoke(iDirection);
    }
}
