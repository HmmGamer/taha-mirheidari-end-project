using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeController : MonoBehaviour
{
    private Vector2 _startTouchPos;
    private bool _isSwiping;
    private float _minSwipeDistance = 50f;
    private Vector3Int _SwipeDirection;

    void Update()
    {
        //_SwipeDirection = Vector3Int.zero;

        if (Input.touchCount > 0)
        {
            Touch iTouch = Input.GetTouch(0);

            if (iTouch.phase == TouchPhase.Began)
            {
                _isSwiping = true;
                _startTouchPos = iTouch.position;
            }
            else if (iTouch.phase == TouchPhase.Ended && _isSwiping)
            {
                Vector2 iDelta = iTouch.position - _startTouchPos;

                if (iDelta.magnitude >= _minSwipeDistance)
                {
                    if (Mathf.Abs(iDelta.x) > Mathf.Abs(iDelta.y))
                    {
                        if (iDelta.x > 0) _SwipeDirection = new Vector3Int(1, 0, 0);
                        else _SwipeDirection = new Vector3Int(-1, 0, 0);
                    }
                    else
                    {
                        if (iDelta.y > 0) _SwipeDirection = new Vector3Int(0, 1, 0);
                        else _SwipeDirection = new Vector3Int(0, -1, 0);
                    }
                    InputManager._instance._OnNewPlayerInput(_SwipeDirection);
                }
                _isSwiping = false;
            }
        }
    }
}
