using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using SilverGames.Metroidvania.Camera;

namespace SilverGames.Metroidvania.Character.Player
{
	[RequireComponent(typeof(InputManager))]
	public class PlayerMovementController : CharacterMovementController
	{
		[Space(3)]
		[Header(" --- PLAYER SETTINGS ---")]
		[SerializeField] private InputManager _inputManager;
		private PlayerInputSystem _playerInput => _inputManager.PlayerInput;
		[Space(3)]

		[Header(" --- PLAYER HORIZONTAL MOVEMENT ---")]
		[SerializeField] private Vector2 _currentMovementInput;
		[SerializeField] private bool _isMovementPressed;

		[Header(" --- CAMERA CONTROL ---")]
		[SerializeField] private CameraManager _cameraManager;
		[SerializeField] private CameraFollowObject _cameraFollowObject;
		[SerializeField] private float _fallSpeedYDampingChangeThreshold;

		public bool IsFacingRight => CharacterState.LookingRight;

		private void Start()
		{
			if (_inputManager == null)
				_inputManager = GetComponent<InputManager>();

			HorizontalInputs();
			VerticalInputs();
			RollInputs();

			_cameraManager = CameraManager.Instance;
			_fallSpeedYDampingChangeThreshold = _cameraManager._fallSpeedYDampingChangeThreshold;
		}

		private void Update()
		{
			CheckIfIsOnGround();
		}

		private void FixedUpdate()
		{
			MovePlayer();

			PlayerIsFalling();
		}

		#region --- HORIZONTAL MOVEMENT ---

		private void MovePlayer()
		{
			if (_isMovementPressed && CharacterState.CanMove())
			{
				TurnCameraFollowObject(CurrentMovement.x);
				MoveObject(CurrentMovement);
			}

			CharacterState.Moving = _isMovementPressed;
		}

		private void TurnCameraFollowObject(float x)
		{
			bool facingRight = x > 0;

			if (x == 0)
				facingRight = IsFacingRight;

			_cameraFollowObject.CallTurn(facingRight);
		}

		private void PlayerIsFalling()
		{
			//check if player is falling
			if (Rb.velocity.y < 0 && !CharacterState.Falling && !CharacterState.Attacking)
			{
				CharacterState.Falling = true;
				//CharacterAnimator.FallAnimation();
			}
			if (Grounded() && CharacterState.Falling)
			{
				CharacterState.Falling = false;
				//CharacterAnimator.CheckEndAnimationNewState();
			}

			//if we are falling past a certain speed threshold
			if (Rb.velocity.y < _fallSpeedYDampingChangeThreshold && !_cameraManager.IsLerpingYDampin && !_cameraManager.LerpedFromPlayerFalling)
			{
				_cameraManager.LerpYDamping(true);
			}
			//if we are standing still or moving up
			if (Rb.velocity.y >= 0 && !_cameraManager.IsLerpingYDampin && _cameraManager.LerpedFromPlayerFalling)
			{
				//reset so it can be called again
				_cameraManager.LerpedFromPlayerFalling = false;
				_cameraManager.LerpYDamping(false);
			}
		}

		private void HorizontalInputs()
		{
			//movement inputs
			_playerInput.PlayerControls.Move.started += OnMovementInput;
			_playerInput.PlayerControls.Move.canceled += OnMovementCanceled;
			_playerInput.PlayerControls.Move.performed += OnMovementInput;

			CanMove = true;
		}

		private void OnMovementInput(InputAction.CallbackContext context)
		{
			if (!CanMove)
				return;

			_currentMovementInput = context.ReadValue<Vector2>();

			if (Grounded() && !CharacterState.IsJumping())
				CharacterAnimator.WalkAnimation();

			CurrentMovement.x = _currentMovementInput.x;
			CurrentMovement.y = 0;
			CurrentMovement.z = 0;

			_isMovementPressed = _currentMovementInput.x != 0;
			CharacterState.Moving = _isMovementPressed;
		}

		private void OnMovementCanceled(InputAction.CallbackContext context)
		{
			if (!CanMove)
				return;

			TurnCameraFollowObject(0);

			_isMovementPressed = false;
			CharacterState.Moving = false;

			if (Grounded())
				CharacterAnimator.CheckEndAnimationNewState();
		}

		#endregion

		#region --- VERTICAL MOVEMENT ---

		private void VerticalInputs()
		{
			//jump inputs
			//_inputManager.PlayerInput.PlayerControls.Jump.started += OnJumpStarted;
			_playerInput.PlayerControls.Jump.performed += OnJumpInput;
			_playerInput.PlayerControls.Jump.canceled += OnJumpCanceled;

			CanJump = true;
		}

		private void OnJumpInput(InputAction.CallbackContext context)
		{
			if (!CanJump)
				return;

			if (!CharacterState.CanJump())
				return;

			Jump(_isMovementPressed);
		}

		private void OnJumpCanceled(InputAction.CallbackContext context)
		{
			if (!CanJump)
				return;

			if (!CharacterState.CanJump())
				return;

			CharacterAnimator.FallAnimation();
			StopJump();
		}

		#endregion

		#region --- ROLL MOVEMENT ---

		private void RollInputs()
		{
			//dash inputs
			_playerInput.PlayerControls.Roll.started += OnRollInput;
			//_playerInput.PlayerControls.Dash.canceled += OnDashCanceled;

			CanRoll = true;
		}

		private void OnRollInput(InputAction.CallbackContext context)
		{
			if (!CanRoll)
				return;

			if (!CharacterState.CanRoll())
				return;

			StartRoll();
		}

		#endregion
	}
}
