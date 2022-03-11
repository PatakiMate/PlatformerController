using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerInputController : MonoBehaviour
{
    private InputMaster _controls;
    private CharacterController _character;
    private Vector2 _axis;

    void Awake()
    {
        _character = GetComponent<CharacterController>();

        #region Setup Control Actions
        _controls = new InputMaster();
        _controls.Player.Movement.performed += ctx => Move(ctx.ReadValue<Vector2>());
        _controls.Player.Movement.canceled += ctx => Move(Vector2.zero);
        _controls.Player.Jump.started += Jump;
        _controls.Player.Jump.canceled += EndJump;
        _controls.Player.Jump.started += Glide;
        _controls.Player.Jump.canceled += EndGlide;
        _controls.Player.Dash.started += Dash;
        _controls.Player.Fly.started += Fly;
        _controls.Player.Fly.canceled += EndFly;
        #endregion
    }

    void OnEnable()
    {
        _controls.Player.Enable();
    }

    void OnDisable()
    {
        _controls.Player.Disable();
    }

    void FixedUpdate()
    {
        _character.Walk(_axis.x);
        _character.ClimbLadder(_axis.y);
        _character.StickToEdge(_axis.y);
    }

    private void Move(Vector2 _axis)
    {
        this._axis = _axis;
    }

    #region Jump
    private void Jump(InputAction.CallbackContext context)
    {
        if (_axis.y < 0)
        {
            _character.JumpDown();
        }
        else
        {
            _character.Jump();
        }
    }

    private void EndJump(InputAction.CallbackContext context)
    {
        _character.EndJump();
    }
    #endregion

    #region Glide
    private void Glide(InputAction.CallbackContext context)
    {
        _character.Glide();
    }
   
    private void EndGlide(InputAction.CallbackContext context)
    {
        _character.EndGlide();
    }
    #endregion

    #region Dash
    private void Dash(InputAction.CallbackContext context)
    {
        _character.Dash(_axis);
    }
    #endregion

    #region Fly
    private void Fly(InputAction.CallbackContext context)
    {
        _character.Flying = true;
    }

    private void EndFly(InputAction.CallbackContext context)
    {
        _character.Flying = false;
        _character.EndFly();
    }
    #endregion
}