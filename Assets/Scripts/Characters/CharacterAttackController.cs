using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SilverGames.Metroidvania.Character
{
    [RequireComponent(typeof(CharacterStateList))]
    public class CharacterAttackController : MonoBehaviour
    {
        [Header(" --- GENERAL SETTINGS ---")]
        public CharacterAnimatorController CharacterAnimator;
        protected CharacterStateList CharacterState;
        protected bool CanAttack;
        [Space(3)]

        [Header(" --- ATTACK SETTINGS ---")]
        [Tooltip("Number of attacks performed")]
        public int AttackCombo;
        [Tooltip("Max number of attacks to be performed")]
        public int MaxAttackCombo;
        [Tooltip("The damage the attack does")]
        public float AttackDamage;
        [Tooltip("The time it takes for the attack to complete")]
        public float AttackTime;
        [Tooltip("The time it takes for the character to be able to attack again")]
        public float AttackCooldown;
        public bool Attacked;

        private void Awake()
        {
            if (CharacterState == null)
                CharacterState = GetComponent<CharacterStateList>();
        }

        public void StartAttack()
        {
            if (AttackCombo < MaxAttackCombo)
                AttackCombo++;

            if (!Attacked && !CharacterState.Jumping)
                StartCoroutine(ComboAttack());
            else if (!Attacked && CharacterState.Jumping)
                StartCoroutine(JumpAttack());

            //      _timeSinceAttack += Time.deltaTime;

            //      if(_attack && _timeSinceAttack >= _timeBetweenAttack)
            //{
            //          _timeSinceAttack = 0;
            //          //play animation


            //}
        }

        IEnumerator ComboAttack()
        {
            print("comboAttack");
            Attacked = true;
            int combo = 1;

            CharacterState.Attacking = true;

            while (combo <= AttackCombo)
            {
                print("atacou " + combo);
                ExecuteAttackCommand(combo);
                combo++;
                //AttackTime = CharacterAnimator.GetAnimationLength();

                yield return new WaitForSeconds(0.1f);

                AttackTime = (CharacterAnimator.GetAnimationLength() - 0.1f);

                yield return new WaitForSeconds(AttackTime);
            }

            print("encerrou ataques");
            CharacterState.Attacking = false;
            CharacterAnimator.CheckEndAnimationNewState();

            yield return new WaitForSeconds(AttackCooldown);

            print("pode atacar novamente");
            AttackCombo = 0;
            Attacked = false;
        }

        IEnumerator JumpAttack()
		{
            print("jumpAttack");
            Attacked = true;

            CharacterState.Attacking = true;

            CharacterAnimator.JumpAttack();

            yield return new WaitForSeconds(0.1f);

            AttackTime = (CharacterAnimator.GetAnimationLength() - 0.2f);

            yield return new WaitForSeconds(AttackTime);

            print("encerrou jump attack");

            CharacterState.Attacking = false;
            CharacterAnimator.CheckEndAnimationNewState();

            yield return new WaitForSeconds(AttackCooldown);

            Attacked = false;

            yield return null;
		}

        private void ExecuteAttackCommand(int combo)
        {
            CharacterAnimator.AttackAnimation(combo);
        }
    }
}