using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private PlayerController _playerControl;
    // Start is called before the first frame update
    void Start()
    {
        _playerControl = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMoveInput();

        if (_playerControl.actionBuffer != 0)
        {
            _playerControl.actionBuffer = Mathf.MoveTowards(_playerControl.actionBuffer, 0, Time.deltaTime);
        }
    }

    public void LeftButton(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            _playerControl.leftButton = true;
        }

        if (value.canceled)
        {
            _playerControl.leftButton = false;
        }
    }

    public void RightButton(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            _playerControl.rightButton = true;
        }

        if (value.canceled)
        {
            _playerControl.rightButton = false;
        }
    }

    public void UpButton(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            _playerControl.upButton = true;
        }

        if (value.canceled)
        {
            _playerControl.upButton = false;
        }
    }

    public void DownButton(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            _playerControl.downButton = true;
        }

        if (value.canceled)
        {
            _playerControl.downButton = false;
        }
    }

    public void ActionButton (InputAction.CallbackContext value)
    {
        if (value.started)
        {
            StartCoroutine(ActionButtonPressed());
            _playerControl.actionButton = true;
            _playerControl.actionBuffer = 0.1f;
        }

        if (value.canceled)
        {
            _playerControl.actionButton = false;
        }
    }



    public void UpdateMoveInput()
    {
        if(_playerControl.leftButton && !_playerControl.rightButton)
        {
            _playerControl.moveStick.x = -1;
        }else if(!_playerControl.leftButton && _playerControl.rightButton)
        {
            _playerControl.moveStick.x = 1;
        }
        else if ((_playerControl.leftButton && _playerControl.rightButton) || (!_playerControl.leftButton && !_playerControl.rightButton))
        {
            _playerControl.moveStick.x = 0;
        }

        if (_playerControl.upButton && !_playerControl.downButton)
        {
            _playerControl.moveStick.y = 1;
        }
        else if (!_playerControl.upButton && _playerControl.downButton)
        {
            _playerControl.moveStick.y = -1;
        }
        else if ((_playerControl.upButton && _playerControl.downButton) || (!_playerControl.upButton && !_playerControl.downButton))
        {
            _playerControl.moveStick.y = 0;
        }

    }

 

    IEnumerator ActionButtonPressed()
    {
        _playerControl.hasPressedAction = true;
        yield return new WaitForEndOfFrame();
        _playerControl.hasPressedAction = false;
    }
}
