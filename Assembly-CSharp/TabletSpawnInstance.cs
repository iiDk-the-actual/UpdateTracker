using System;
using Liv.Lck.GorillaTag;
using UnityEngine;

public class TabletSpawnInstance : IDisposable
{
	public event Action onGrabbed;

	public event Action onReleased;

	public LckDirectGrabbable directGrabbable
	{
		get
		{
			return this._lckSocialCameraManager.lckDirectGrabbable;
		}
	}

	public bool ResetLocalPose()
	{
		if (this._cameraSpawnInstanceTransform == null)
		{
			return false;
		}
		this._cameraSpawnInstanceTransform.localPosition = Vector3.zero;
		this._cameraSpawnInstanceTransform.localRotation = Quaternion.identity;
		return true;
	}

	public bool ResetParent()
	{
		if (this._cameraSpawnInstanceTransform == null)
		{
			return false;
		}
		this._cameraSpawnInstanceTransform.SetParent(this._cameraSpawnParentTransform);
		return true;
	}

	public bool SetParent(Transform transform)
	{
		if (this._cameraSpawnInstanceTransform == null)
		{
			return false;
		}
		this._cameraSpawnInstanceTransform.SetParent(transform);
		return true;
	}

	public bool cameraActive
	{
		get
		{
			return this._cameraActive;
		}
		set
		{
			this._cameraActive = value;
			if (this._lckSocialCameraManager != null)
			{
				this._lckSocialCameraManager.cameraActive = this._cameraActive;
			}
		}
	}

	public bool uiVisible
	{
		get
		{
			return this._uiVisible;
		}
		set
		{
			this._uiVisible = value;
			if (this._lckSocialCameraManager != null)
			{
				this._lckSocialCameraManager.uiVisible = this._uiVisible;
			}
		}
	}

	public bool isSpawned
	{
		get
		{
			return this._cameraGameObjectInstance != null;
		}
	}

	public TabletSpawnInstance(GameObject cameraSpawnPrefab, Transform cameraSpawnParentTransform)
	{
		this._cameraSpawnPrefab = cameraSpawnPrefab;
		this._cameraSpawnParentTransform = cameraSpawnParentTransform;
	}

	public void SpawnCamera()
	{
		if (!this.isSpawned)
		{
			this._cameraGameObjectInstance = Object.Instantiate<GameObject>(this._cameraSpawnPrefab, this._cameraSpawnParentTransform);
			this._lckSocialCameraManager = this._cameraGameObjectInstance.GetComponent<LckSocialCameraManager>();
			this._lckSocialCameraManager.lckDirectGrabbable.onGrabbed += delegate
			{
				Action action = this.onGrabbed;
				if (action == null)
				{
					return;
				}
				action();
			};
			this._lckSocialCameraManager.lckDirectGrabbable.onReleased += delegate
			{
				Action action2 = this.onReleased;
				if (action2 == null)
				{
					return;
				}
				action2();
			};
			this._cameraSpawnInstanceTransform = this._cameraGameObjectInstance.transform;
			this.Controller = this._cameraGameObjectInstance.GetComponent<GTLckController>();
		}
		this.uiVisible = this.uiVisible;
		this.cameraActive = this.cameraActive;
	}

	public Vector3 position
	{
		get
		{
			if (this._cameraSpawnInstanceTransform == null)
			{
				return Vector3.zero;
			}
			return this._cameraSpawnInstanceTransform.position;
		}
	}

	public Quaternion rotation
	{
		get
		{
			if (this._cameraSpawnInstanceTransform == null)
			{
				return Quaternion.identity;
			}
			return this._cameraSpawnInstanceTransform.rotation;
		}
	}

	public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
	{
		if (this._cameraSpawnInstanceTransform == null)
		{
			return;
		}
		this._cameraSpawnInstanceTransform.SetPositionAndRotation(position, rotation);
	}

	public void SetLocalScale(Vector3 scale)
	{
		if (this._cameraSpawnInstanceTransform == null)
		{
			return;
		}
		this._cameraSpawnInstanceTransform.localScale = scale;
	}

	public void Dispose()
	{
		if (this._cameraGameObjectInstance != null)
		{
			Object.Destroy(this._cameraGameObjectInstance);
			this._cameraGameObjectInstance = null;
		}
	}

	private GameObject _cameraGameObjectInstance;

	private GameObject _cameraSpawnPrefab;

	private GameEvents _GtCamera;

	private Transform _cameraSpawnParentTransform;

	private Transform _cameraSpawnInstanceTransform;

	public GTLckController Controller;

	private LckSocialCameraManager _lckSocialCameraManager;

	private bool _cameraActive;

	private bool _uiVisible;
}
