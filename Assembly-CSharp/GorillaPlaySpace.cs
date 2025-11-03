using System;
using UnityEngine;

public class GorillaPlaySpace : MonoBehaviour
{
	public static GorillaPlaySpace Instance
	{
		get
		{
			return GorillaPlaySpace._instance;
		}
	}

	private void Awake()
	{
		if (GorillaPlaySpace._instance != null && GorillaPlaySpace._instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		GorillaPlaySpace._instance = this;
	}

	[OnEnterPlay_SetNull]
	private static GorillaPlaySpace _instance;

	public Collider headCollider;

	public Collider bodyCollider;

	public Transform rightHandTransform;

	public Transform leftHandTransform;

	public Vector3 headColliderOffset;

	public Vector3 bodyColliderOffset;

	private Vector3 lastLeftHandPosition;

	private Vector3 lastRightHandPosition;

	private Vector3 lastLeftHandPositionForTag;

	private Vector3 lastRightHandPositionForTag;

	private Vector3 lastBodyPositionForTag;

	private Vector3 lastHeadPositionForTag;

	private Rigidbody playspaceRigidbody;

	public Transform headsetTransform;

	public Vector3 rightHandOffset;

	public Vector3 leftHandOffset;

	public VRRig vrRig;

	public VRRig offlineVRRig;

	public float vibrationCooldown = 0.1f;

	public float vibrationDuration = 0.05f;

	private float leftLastTouchedSurface;

	private float rightLastTouchedSurface;

	public VRRig myVRRig;

	private float bodyHeight;

	public float tagCooldown;

	public float taggedTime;

	public float disconnectTime = 60f;

	public float maxStepVelocity = 2f;

	public float hapticWaitSeconds = 0.05f;

	public float tapHapticDuration = 0.05f;

	public float tapHapticStrength = 0.5f;

	public float tagHapticDuration = 0.15f;

	public float tagHapticStrength = 1f;

	public float taggedHapticDuration = 0.35f;

	public float taggedHapticStrength = 1f;
}
