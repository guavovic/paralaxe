using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SilverGames.Metroidvania.Character
{
    public class CharacterStateList : MonoBehaviour
    {
        public bool Moving = false;
        public bool Jumping = false;
        public bool Rolling = false;
        public bool Falling = false;
        public bool Attacking = false;
        public bool RecoilingX, RecoilingY;
        public bool LookingRight;
        public bool Invincible = false;
        public bool Healing = false;
        public bool Dying = false;
        public bool LookingMap;

        public bool IdleState()
        {
            ///
            /// Idle pode ir para os seguintes estados
            /// 
            /// Moving
            /// Rolling
            /// Jumping
            /// Healing
            /// Attacking_1
            /// Hit
            ///

            if (!Moving && !Jumping && !Rolling && !Falling && !Attacking && !Healing && !Dying)
                return true;
            else
                return false;
        }

        public bool CanMove()
        {
            ///
            /// Walk pode ir para os seguintes estados
            /// 
            /// Idle - ao parar comando de mover
            /// Rolling - vai parar de andar enquanto faz o roll
            /// Jumping - ao clicar no botão de pular
            /// Attacking_1 - vai parar de andar enquanto ataca
            /// Healing - vai parar de andar enquanto cura
            /// Hit - vai parar de andar quando leva hit
            ///

            if (!Rolling && !Attacking && !Healing && !Dying)
                return true;
            else
                return false;
        }

        public bool IsMoving()
		{
            if (Moving && !Rolling && !Attacking && !Healing && !Dying)
                return true;
            else 
                return false;
		}


        public bool CanRoll()
        {
            ///
            /// Rolling pode ir para os seguintes estados
            /// 
            /// Idle - se não estiver com botão de mover segurado
            /// Moving - se estiver com botão de mover segurado
            ///

            if (!Jumping && !Rolling && !Falling && !Attacking && !Healing && !Dying)
                return true;
            else
                return false;
        }

        public bool IsRolling()
		{
            if (Rolling && !Attacking && !Jumping && !Falling && !Healing && !Dying)
                return true;
            else
                return false;
		}

        public bool CanJump()
        {
            ///
            /// Jumping pode ir para os seguintes estados
            /// 
            /// Idle - se já estiver no chão e botão de mover não estiver segurado
            /// Moving - se já estiver no chão e botão de mover estiver segurado
            /// Falling - ao começar a cair
            /// Attacking - se ainda estiver no ar
            /// Hit - vai 'parar' pulo quando levar dano
            ///

            if (!Jumping && !Rolling && !Falling && !Attacking && !Healing && !Dying)
                return true;
            else
                return false;
        }

        public bool IsJumping()
		{
            if (Jumping && !Rolling && !Healing && !Falling && !Dying)
                return true;
            else
                return false;
		}

        public bool IsFalling()
		{
            if (Jumping && Falling && !Rolling && !Healing && !Dying)
                return true;
            else
                return false;
		}

        public bool CanAttack()
        {
            ///
            /// Attacking pode ir para os seguintes estados
            /// 
            /// Idle - se não estiver segurando botão de mover
            /// Moving - se estiver segurando botão de mover
            /// Hit - vai parar de atacar quando levar dano
            ///

            if (!Rolling && !Healing && !Jumping && !Dying)
                return true;
            else
                return false;
        }

        public bool IsAttacking()
		{
            if (Attacking && !Jumping && !Falling && !Rolling && !Healing && !Dying)
                return true;
            else 
                return false;
		}

        public bool CanJumpAttack()
        {
            ///
            /// Jump attack pode ir para os seguintes estados
            /// 
            /// Idle - se já estivar no chão
            /// Moving - se já estivar no chão e estiver segurando botão de mover
            /// Falling - se estiver caindo
            /// 
            ///

            if (Jumping && !Rolling && !Healing && !Attacking && !Dying)
                return true;
            else
                return false;
        }

        public bool IsJumpAttacking()
		{
            if (Attacking && Jumping && !Rolling && !Healing && !Dying)
                return true;
            else
                return false;
		}

        public bool CanHeal()
        {
            ///
            /// Healing pode ir para os seguintes estados
            /// 
            /// Idle
            /// Moving - se estiver segurando botão de mover
            ///

            if (!Moving && !Jumping && !Rolling && !Falling && !Attacking && !Healing && !Dying)
                return true;
            else
                return false;

        }

        public bool IsHealing()
		{
            if (Healing && !Jumping && !Rolling && !Falling && !Attacking && !Dying)
                return true;
            else
                return false;
		}
    }
}