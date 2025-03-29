using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SilverGames.Metroidvania.Character.Player;

namespace SilverGames.Metroidvania.Camera
{
    public class CameraFollowObject : MonoBehaviour
    {
        [Header(" --- REFERENCES ---")]
        [SerializeField] private Transform _playerTransform;

        [Header(" --- FLIP ROTATION STATS ---")]
        [SerializeField] private float _flipYRotationTime = 0.5f;

        private PlayerMovementController _player;
        private Coroutine _turnCoroutine;
        private bool _isFacingRight;

        private void Start()
        {
            if (_player == null)
                _player = _playerTransform.gameObject.GetComponent<PlayerMovementController>();

            _isFacingRight = _player.IsFacingRight;
        }

        private void Update()
        {
            transform.position = _playerTransform.position;
        }

        public void CallTurn(bool facingRight)
        {
            if (_isFacingRight == facingRight)
                return;

            print("call turn " + _player.IsFacingRight);
            _turnCoroutine = StartCoroutine(FlipYLerp(facingRight));
        }

        private IEnumerator FlipYLerp(bool facingRight)
        {
            float startRotation = transform.localEulerAngles.y;
            float endRotationAmount = DetermineEndRotation(facingRight);
            float yRotation = 0f;

            float elapsedTime = 0f;
            while (elapsedTime < _flipYRotationTime)
            {
                elapsedTime += Time.deltaTime;

                //lerp y rotation
                yRotation = Mathf.Lerp(startRotation, endRotationAmount, (elapsedTime / _flipYRotationTime));
                transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

                yield return null;
            }
        }

        private float DetermineEndRotation(bool facingRight)
        {
            _isFacingRight = facingRight;

            if (_isFacingRight)
                return 0f;
            else
                return 180f;
        }
    }
}