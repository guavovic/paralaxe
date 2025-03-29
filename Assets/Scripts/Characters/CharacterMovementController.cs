using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SilverGames.Metroidvania.Character
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CharacterStateList))]
    public class CharacterMovementController : MonoBehaviour
    {
        [Header(" --- GENERAL SETTINGS ---")]
        public CharacterAnimatorController CharacterAnimator;
        protected Rigidbody2D Rb;
        protected CharacterStateList CharacterState;
        protected bool CanMove;
        protected bool CanJump;

        [Header(" --- HORIZONTAL MOVEMENT ---")]
        [Tooltip("Sets the movement speed on the ground")]
        public float WalkSpeed;
        public Vector3 CurrentMovement;
        [Space(3)]

        [Header(" --- VERTICAL MOVEMENT ---")]
        [Tooltip("Sets how high the object can jump")]
        public float JumpForce;
        [Tooltip("Sets how far the object can jump forward")]
        public float JumpForceX;
        //[Tooltip("Stores the jump button input")]
        //public int JumpBufferCounter = 0;
        //[Tooltip("Sets the max amount of frames the jump buffer input is stored")]
        //public int JumpBufferFrames;
        //[Tooltip("Stores the Grounded() bool")]
        //public float CoyoteTimeCounter = 0;
        //[Tooltip("Sets the max amount of frames the Grounded() bool is stored")]
        //public float CoyoteTime;
        [Tooltip("Keeps track of how many times the player has jumped in the air")]
        public int AirJumpCounter = 0;
        [Tooltip("The max no. of air jumps")]
        public int MaxAirJumps;
        //[Tooltip("The Jump button is pressed")]
        //public bool JumpPressed;
        [Tooltip("Stores the gravity scale at start")]
        public float Gravity;
        [Space(3)]

        [Header(" --- GROUND CHECK SETTINGS ---")]
        [Tooltip("Point at which ground check happens")]
        public Transform GroundCheckPoint;
        [Tooltip("How far down from ground check point is Grounded() checked")]
        public float GroundCheckY = 0.2f;
        [Tooltip("How far horizontally from ground check point to the edge of the player is")]
        public float GroundCheckX = 0.5f;
        [Tooltip("Sets the ground layer")]
        public LayerMask WhatIsGround;
        [Space(3)]

        [Header(" --- DASH SETTINGS ---")]
        [Tooltip("Speed of the dash")]
        public float RollSpeed;
        [Tooltip("Amount of time spend dashing")]
        public float RollTime;
        [Tooltip("Amount of time between dashes")]
        public float RollCooldown;
        [Tooltip("Object can use dash movement")]
        public bool CanRoll = true, Rolled;

        private void Awake()
        {
            if (Rb == null)
                Rb = GetComponent<Rigidbody2D>();

            if (CharacterState == null)
                CharacterState = GetComponent<CharacterStateList>();

            Gravity = Rb.gravityScale;
        }

        #region --- HORIZONTAL MOVEMENT ---

        public void MoveObject(Vector3 direction)
        {
            transform.position += (direction * WalkSpeed * Time.deltaTime);

            FlipObject(CurrentMovement);
        }

        public void FlipObject(Vector3 direction)
        {
            if (direction.x < 0)
            {
                transform.localScale = new Vector2(-1, transform.localScale.y);
                CharacterState.LookingRight = false;
            }
            else if (direction.x > 0)
            {
                transform.localScale = new Vector2(1, transform.localScale.y);
                CharacterState.LookingRight = true;
            }
        }

        #endregion

        #region --- VERTICAL MOVEMENT ---

        public void Jump(bool moving)
        {
            if (!CharacterState.Jumping)
            {
                print("primeiro pulo");
                //Rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
                //Rb.velocity = Vector2.up * JumpForce;

                TriggerJump(moving);
            }
            else if (CharacterState.Jumping && AirJumpCounter < MaxAirJumps)
            {
                print("secundo pulo");
                //Rb.AddForce(Vector2.up * JumpForce * 2, ForceMode2D.Impulse);
                //Rb.velocity = Vector2.up * JumpForce;
                TriggerJump(moving);
                AirJumpCounter++;
            }

            #region Unused
            /*print("Jump()");
            if(JumpBufferCounter > 0 && CoyoteTimeCounter > 0 && !CharacterState.Jumping)
            {
                print("primeiro if Jump()");
                //Rb.velocity = new Vector3(Rb.velocity.x, JumpForce);
                //Rb.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
                Rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
                CharacterState.Jumping = true;
            }


            else if (!Grounded() && AirJumpCounter < MaxAirJumps && JumpPressed && CharacterState.Jumping)
            {
                print("segundo if Jump()");
                CharacterState.Jumping = true;
                AirJumpCounter++;
                //Rb.velocity = new Vector3(Rb.velocity.x, JumpForce);
                Rb.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);

            }

            if (!JumpPressed && Rb.velocity.y > 3)
            {
                print("terceiro if Jump()");
                CharacterState.Jumping = false;
                //Rb.velocity = new Vector2(Rb.velocity.x, 0);
            }*/

            #endregion
        }

        private void TriggerJump(bool moving)
        {
            int dir = CharacterState.LookingRight ? 1 : -1;
            if (moving || CharacterState.Rolling)
                //Rb.velocity = new Vector2(JumpForceX * dir, JumpForce);
                Rb.AddForce(new Vector2((JumpForceX * dir), JumpForce), ForceMode2D.Impulse);
            else
                //Rb.velocity = Vector2.up * JumpForce;
                Rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);

            CharacterState.Jumping = true;
            CharacterAnimator.JumpAnimation();
        }

        public void StopJump()
        {
            if (CharacterState.Jumping)
            {
                print("soltou pulo no meio do click");
                Rb.velocity = new Vector2(Rb.velocity.x * 0.5f, Rb.velocity.y * 0.5f);
                //print(Rb.velocity);
            }
        }

        public bool Grounded()
        {
            if (Physics2D.Raycast(GroundCheckPoint.position, Vector2.down, GroundCheckY, WhatIsGround) ||
                Physics2D.Raycast(GroundCheckPoint.position + new Vector3(GroundCheckX, 0, 0), Vector2.down, GroundCheckY, WhatIsGround) ||
                Physics2D.Raycast(GroundCheckPoint.position + new Vector3(-GroundCheckX, 0, 0), Vector2.down, GroundCheckY, WhatIsGround))
                return true;
            else
                return false;
        }

        public void CheckIfIsOnGround()
        {
            CharacterState.Jumping = !Grounded();

            if (!CharacterState.Jumping)
                AirJumpCounter = 0;

            #region Unused

            /*if (Grounded())
            {
                CharacterState.Jumping = false;
                CoyoteTimeCounter = CoyoteTime;
                AirJumpCounter = 0;
            }
            else
            {
                CoyoteTimeCounter -= Time.deltaTime;
            }

            if (JumpPressed)
                JumpBufferCounter = JumpBufferFrames;
            else
                JumpBufferCounter--;*/

            #endregion
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Transform raycast = Physics2D.Raycast(GroundCheckPoint.position, Vector2.down, GroundCheckY, WhatIsGround).transform;
            if (raycast != null)
                Gizmos.DrawLine(GroundCheckPoint.position, raycast.position);
        }

        #endregion

        #region --- ROLL MOVEMENT ---

        public void StartRoll()
        {
            if (CanRoll && !Rolled && Grounded())
            {
                StartCoroutine(Roll());
                Rolled = true;
            }
        }

        IEnumerator Roll()
        {
            CanRoll = false;
            CharacterState.Rolling = true;
            CharacterAnimator.RollAnimation();
            //Rb.gravityScale = 0;
            int dir = CharacterState.LookingRight ? 1 : -1;
            //Rb.velocity = new Vector2(dir * DashSpeed, 0);
            //Rb.velocity = (Vector2.right * dir) * RollSpeed;
            Rb.AddForce((Vector2.right * dir) * RollSpeed, ForceMode2D.Impulse);

            if (Grounded())
            {
                //show dash effect
            }

            yield return new WaitForSeconds(0.1f);

            RollTime = CharacterAnimator.GetAnimationLength() - 0.2f;

            yield return new WaitForSeconds(RollTime);

            //Rb.gravityScale = Gravity;
            CharacterState.Rolling = false;

            CharacterAnimator.CheckEndAnimationNewState();

            yield return new WaitForSeconds(RollCooldown);

            CanRoll = true;
            Rolled = false;
        }

        #endregion
    }
}