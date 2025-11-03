using System;
using GorillaLocomotion;
using Liv.Lck.GorillaTag;
using UnityEngine;

public class LckTabletSizeManager : MonoBehaviour
{
	private void Start()
	{
		GTLckController controller = this._controller;
		controller.OnFOVUpdated = (Action<CameraMode>)Delegate.Combine(controller.OnFOVUpdated, new Action<CameraMode>(this.UpdateCustomNearClip));
		this._controller.OnHorizontalModeChanged += this.OnHorizontalModeChanged;
	}

	private void OnDestroy()
	{
		this._controller.OnHorizontalModeChanged -= this.OnHorizontalModeChanged;
		GTLckController controller = this._controller;
		controller.OnFOVUpdated = (Action<CameraMode>)Delegate.Remove(controller.OnFOVUpdated, new Action<CameraMode>(this.UpdateCustomNearClip));
	}

	private void OnHorizontalModeChanged(bool mode)
	{
		this.UpdateCustomNearClip(CameraMode.Selfie);
		this.UpdateCustomNearClip(CameraMode.FirstPerson);
	}

	private void UpdateCustomNearClip(CameraMode mode)
	{
		if (GTPlayer.Instance.IsDefaultScale)
		{
			return;
		}
		switch (mode)
		{
		case CameraMode.Selfie:
			this.SetCustomNearClip(this._selfieCamera);
			return;
		case CameraMode.FirstPerson:
			this.SetCustomNearClip(this._firstPersonCamera);
			break;
		case CameraMode.ThirdPerson:
		case CameraMode.Drone:
			break;
		default:
			return;
		}
	}

	private void SetCustomNearClip(Camera cam)
	{
		if (GTPlayer.Instance.IsDefaultScale)
		{
			return;
		}
		Matrix4x4 matrix4x;
		if (this._controller.HorizontalMode)
		{
			matrix4x = Matrix4x4.Perspective(cam.fieldOfView, 1.777778f, this._customNearClip, cam.farClipPlane);
		}
		else
		{
			matrix4x = Matrix4x4.Perspective(cam.fieldOfView, 0.5625f, this._customNearClip, cam.farClipPlane);
		}
		cam.projectionMatrix = matrix4x;
	}

	private void ClearCustomNearClip()
	{
		this._selfieCamera.ResetProjectionMatrix();
		this._firstPersonCamera.ResetProjectionMatrix();
	}

	private void PlayerBecameSmall()
	{
		this._firstPersonCamera.transform.localPosition = this._firstPersonCamShrinkPosition;
		this._tabletFollower.SetPlayerSizeModifier(false, this._shrinkSize);
		if (!this._lckDirectGrabbable.isGrabbed)
		{
			this.SetCameraOnNeck();
		}
		this.SetCustomNearClip(this._selfieCamera);
		this.SetCustomNearClip(this._firstPersonCamera);
	}

	private void PlayerBecameDefaultSize()
	{
		this._firstPersonCamera.transform.localPosition = this._firstPersonCamDefaultPosition;
		this._tabletFollower.SetPlayerSizeModifier(true, 1f);
		if (!this._lckDirectGrabbable.isGrabbed)
		{
			this.SetCameraOnNeck();
		}
		this.ClearCustomNearClip();
	}

	private void SetCameraOnNeck()
	{
		GameObject gameObject = Camera.main.transform.Find("LCKBodyCameraSpawner(Clone)").gameObject;
		if (gameObject != null)
		{
			gameObject.GetComponent<LckBodyCameraSpawner>().ManuallySetCameraOnNeck();
		}
	}

	private void Update()
	{
		if (!GTPlayer.Instance.IsDefaultScale && this._isDefaultScale != GTPlayer.Instance.IsDefaultScale)
		{
			this._isDefaultScale = false;
			this.PlayerBecameSmall();
			return;
		}
		if (GTPlayer.Instance.IsDefaultScale && this._isDefaultScale != GTPlayer.Instance.IsDefaultScale)
		{
			this._isDefaultScale = true;
			this.PlayerBecameDefaultSize();
		}
	}

	[SerializeField]
	private GTLckController _controller;

	[SerializeField]
	private LckDirectGrabbable _lckDirectGrabbable;

	[SerializeField]
	private GtTabletFollower _tabletFollower;

	[SerializeField]
	private Camera _firstPersonCamera;

	[SerializeField]
	private Camera _selfieCamera;

	private Vector3 _firstPersonCamShrinkPosition = new Vector3(0f, 0f, -0.78f);

	private Vector3 _firstPersonCamDefaultPosition = Vector3.zero;

	private float _shrinkSize = 0.06f;

	private Vector3 _shrinkVector = new Vector3(0.06f, 0.06f, 0.06f);

	private float _customNearClip = 0.0006f;

	private bool _isDefaultScale = true;
}
