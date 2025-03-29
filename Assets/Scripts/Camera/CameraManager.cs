using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace SilverGames.Metroidvania.Camera
{

	public class CameraManager : MonoBehaviour
	{
		public static CameraManager Instance;

		[SerializeField] private CinemachineVirtualCamera[] _allVirtualCameras;

		[Header(" --- Controls for lerping the Y Damping during player Jump/Fall")]
		[SerializeField] private float _fallPanAmount = 0.25f;
		[SerializeField] private float _fallYPanTime = 0.35f;
		public float _fallSpeedYDampingChangeThreshold = -15f;

		public bool IsLerpingYDampin { get; private set; }
		public bool LerpedFromPlayerFalling { get; set; }

		private Coroutine _lerpYPanCoroutine;
		private CinemachineVirtualCamera _currentCamera;
		private CinemachineFramingTransposer _framingTransposer;

		private float _normYPanAmount;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;

			for (int i = 0; i < _allVirtualCameras.Length; i++)
			{
				if (_allVirtualCameras[i].enabled)
				{
					//set the current active camera
					_currentCamera = _allVirtualCameras[i];

					//set the framing transposer
					_framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
				}
			}

			//set the YDamping amount so its based on the inspector value
			_normYPanAmount = _framingTransposer.m_YDamping;
		}

		public void LerpYDamping(bool isPlayerFalling)
		{
			_lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
		}

		private IEnumerator LerpYAction(bool isPlayerFalling)
		{
			IsLerpingYDampin = true;

			//grab the starting damping amount
			float startDampAmount = _framingTransposer.m_YDamping;
			float endDampAmount = 0f;

			//determine the end damping amount
			if (isPlayerFalling)
			{
				endDampAmount = _fallPanAmount;
				LerpedFromPlayerFalling = true;
			}
			else
				endDampAmount = _normYPanAmount;

			//lerp the pan amount
			float elapsedTime = 0f;
			while (elapsedTime < _fallYPanTime)
			{
				elapsedTime += Time.deltaTime;

				float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, (elapsedTime / _fallYPanTime));
				_framingTransposer.m_YDamping = lerpedPanAmount;

				yield return null;
			}

			IsLerpingYDampin = false;
		}
	}
}
