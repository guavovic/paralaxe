using UnityEngine;
using UnityEngine.InputSystem;

namespace SilverGames.Metroidvania.Character.Player
{
	public class InputManager : MonoBehaviour
	{
		private PlayerInputSystem _playerInput;
		public PlayerInputSystem PlayerInput => _playerInput;

		private void Awake()
		{
			_playerInput = new PlayerInputSystem();
		}

		private void OnEnable()
		{
			_playerInput.Enable();
		}

		private void OnDisable()
		{
			_playerInput.Disable();
		}
	}
}