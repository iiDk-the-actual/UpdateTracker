using System;
using UnityEngine;

public class FPSController : MonoBehaviour
{
	[HideInInspector]
	public event FPSController.OnStateChangeEventHandler OnStartEvent;

	public event FPSController.OnStateChangeEventHandler OnStopEvent;

	public float baseMoveSpeed = 4f;

	public float shiftMoveSpeed = 8f;

	public float ctrlMoveSpeed = 1f;

	public float lookHorizontal = 0.4f;

	public float lookVertical = 0.25f;

	[SerializeField]
	private Vector3 leftControllerPosOffset = new Vector3(-0.2f, -0.25f, 0.3f);

	[SerializeField]
	private Vector3 leftControllerRotationOffset = new Vector3(265f, -82f, 28f);

	[SerializeField]
	private Vector3 rightControllerPosOffset = new Vector3(0.2f, -0.25f, 0.3f);

	[SerializeField]
	private Vector3 rightControllerRotationOffset = new Vector3(263f, 318f, 485f);

	[SerializeField]
	private bool toggleGrab;

	[SerializeField]
	private bool clampGrab;

	private bool controlRightHand;

	public LayerMask HandMask;

	public delegate void OnStateChangeEventHandler();
}
