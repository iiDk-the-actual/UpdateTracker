using System;
using GorillaLocomotion;
using Unity.Cinemachine;
using UnityEngine;

public class GorillaCameraFollow : MonoBehaviour
{
	private void Start()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			this.cameraParent.SetActive(false);
		}
		if (this.cinemachineCamera != null)
		{
			this.cinemachineFollow = this.cinemachineCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
			this.baseCameraRadius = this.cinemachineFollow.CameraRadius;
			this.baseFollowDistance = this.cinemachineFollow.CameraDistance;
			this.baseVerticalArmLength = this.cinemachineFollow.VerticalArmLength;
			this.baseShoulderOffset = this.cinemachineFollow.ShoulderOffset;
		}
	}

	private void LateUpdate()
	{
		if (this.cinemachineFollow != null)
		{
			float scale = GTPlayer.Instance.scale;
			this.cinemachineFollow.CameraRadius = this.baseCameraRadius * scale;
			this.cinemachineFollow.CameraDistance = this.baseFollowDistance * scale;
			this.cinemachineFollow.VerticalArmLength = this.baseVerticalArmLength * scale;
			this.cinemachineFollow.ShoulderOffset = this.baseShoulderOffset * scale;
		}
	}

	public Transform playerHead;

	public GameObject cameraParent;

	public Vector3 headOffset;

	public Vector3 eulerRotationOffset;

	public CinemachineVirtualCamera cinemachineCamera;

	private Cinemachine3rdPersonFollow cinemachineFollow;

	private float baseCameraRadius = 0.2f;

	private float baseFollowDistance = 2f;

	private float baseVerticalArmLength = 0.4f;

	private Vector3 baseShoulderOffset = new Vector3(0.5f, -0.4f, 0f);
}
