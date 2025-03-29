using UnityEngine;

namespace SilverGames.Metroidvania.Character
{
	public enum CharacterTypes
	{
		Player,
		Enemy
	}

	[CreateAssetMenu(fileName = "CharacterAnimationStrings", menuName = "SilverGames/CharacterAnimationStrings", order = 1)]
	public class CharacterAnimationStrings : ScriptableObject
	{
		[Header(" --- CHARACTER TYPE ---")]
		public CharacterTypes CharType;

		[Header(" --- ANIMATIONS ---")]
		public string ChangeAnimation;
		public string Idle;
		public string Walk;
		public string Jump;
		public string Fall;
		public string Roll;
		public string Attack;
		public string JumpAttack;
		public string Heal;
		public string Hit;
		public string Death;

		[Header(" --- EFFECTS ---")]
		public string AttackEffect;
	}
}
