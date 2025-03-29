using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SilverGames.Metroidvania.Character
{
	public class CharacterAnimatorController : MonoBehaviour
	{
		[SerializeField] private Animator _animator;
		[SerializeField] private CharacterStateList _characterState;

		[Header(" --- ANIMATIONS ---")]
		[SerializeField] private CharacterAnimationStrings _animationStrings;
		private string _currentState;

		[Header(" --- EFFECTS ---")]
		[SerializeField] private Animator _attackEffectAnimator;

		private void Awake()
		{
			if (_animator == null)
				_animator = GetComponent<Animator>();
		}

		void ChangeAnimationState(string newState)
		{
			print("currentState " + _currentState + " - newState " + newState);
			//stop the same animation from interrupting itself
			if (_currentState == newState)
				return;

			//play the animation
			_animator.Play(newState);

			//reassign the current state
			_currentState = newState;
		}

		public void ChangeAnimation()
		{
			_animator.SetTrigger(_animationStrings.ChangeAnimation);
		}

		public void CheckEndAnimationNewState()
		{
			print("CheckEndAnimationNewState");
			if (_characterState.IdleState())
				IdleAnimation();
			else if (_characterState.IsMoving())
				WalkAnimation();
			else if (_characterState.IsJumping())
				JumpAnimation();
			else if (_characterState.IsFalling())
				FallAnimation();
		}

		public void IdleAnimation()
		{
			print("idle animation");
			ChangeAnimationState(_animationStrings.Idle);
		}

		public void WalkAnimation()
		{
			print("walk animation");
			//_animator.SetBool(_animationStrings.Walk, walking);
			ChangeAnimationState(_animationStrings.Walk);
		}

		public void JumpAnimation()
		{
			print("jump animation");
			//_animator.SetTrigger(_animationStrings.Jump);
			ChangeAnimationState(_animationStrings.Jump);
		}

		public void FallAnimation()
		{
			print("fall animation");
			//_animator.SetTrigger(_animationStrings.Fall);
			ChangeAnimationState(_animationStrings.Fall);
		}

		public void RollAnimation()
		{
			print("roll animation");
			//_animator.SetTrigger(_animationStrings.Roll);
			ChangeAnimationState(_animationStrings.Roll);
		}

		public void AttackAnimation(int combo)
		{
			//_animator.SetInteger(_animationStrings.Attack, combo);
			//_attackEffectAnimator.SetInteger(_animationStrings.Attack, combo);
			string attackString = _animationStrings.Attack + combo;
			ChangeAnimationState(attackString);
		}

		public void JumpAttack()
		{
			ChangeAnimationState(_animationStrings.JumpAttack);
		}

		public float GetAnimationLength()
		{
			float length = 0;
			string name = "";
			AnimatorClipInfo[] currentClipInfo;

			currentClipInfo = _animator.GetCurrentAnimatorClipInfo(0);
			length = currentClipInfo[0].clip.length;
			name = currentClipInfo[0].clip.name;

			print("Clipe name " + name + " - Clip length " + length);

			return length;
		}
	}
}

