using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SilverGames.Metroidvania.Character.Player
{
    [RequireComponent(typeof(InputManager))]
    public class PlayerAttackController : CharacterAttackController
    {
        [Space(3)]
        [Header(" --- PLAYER SETTINGS ---")]
        [SerializeField] private InputManager _inputManager;
        private PlayerInputSystem _playerInput => _inputManager.PlayerInput;

        // Start is called before the first frame update
        void Start()
        {
            if (_inputManager == null)
                _inputManager = GetComponent<InputManager>();

            AttackInputs();
        }

        private void AttackInputs()
        {
            //attack inputs
            _playerInput.PlayerControls.Attack.started += OnAttackInput;
        }

        private void OnAttackInput(InputAction.CallbackContext context)
        {
            if (CharacterState.CanAttack() || CharacterState.CanJumpAttack())
                StartAttack();
        }
    }
}
