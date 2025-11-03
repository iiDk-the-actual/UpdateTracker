using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PropHuntHandFollower : MonoBehaviour, ICallBack
{
	public bool hasProp
	{
		get
		{
			return this._hasProp;
		}
		private set
		{
			this._hasProp = value;
		}
	}

	public bool IsInstantiatingAsync { get; private set; }

	public VRRig attachedToRig { get; private set; }

	public bool IsLeftHand
	{
		get
		{
			return this._isLeftHand;
		}
	}

	public void Awake()
	{
		this.attachedToRig = base.GetComponent<VRRig>();
		this.attachedToRig.propHuntHandFollower = this;
		this._isLocal = this.attachedToRig.isOfflineVRRig;
		this.raycastHits = new RaycastHit[20];
	}

	public void Start()
	{
		this.attachedToRig.AddLateUpdateCallback(this);
	}

	private void OnEnable()
	{
		GorillaPropHuntGameManager.RegisterPropHandFollower(this);
	}

	private void OnDisable()
	{
		if (GTAppState.isQuitting)
		{
			return;
		}
		this.DestroyProp();
		GorillaPropHuntGameManager.UnregisterPropHandFollower(this);
	}

	public void DestroyProp()
	{
		if (!this.hasProp || this._prop == null)
		{
			return;
		}
		PropHuntGrabbableProp propHuntGrabbableProp;
		PropHuntTaggableProp propHuntTaggableProp;
		if (this._prop.TryGetComponent<PropHuntGrabbableProp>(out propHuntGrabbableProp))
		{
			PropHuntPools.ReturnGrabbableProp(propHuntGrabbableProp);
		}
		else if (this._prop.TryGetComponent<PropHuntTaggableProp>(out propHuntTaggableProp))
		{
			PropHuntPools.ReturnTaggableProp(propHuntTaggableProp);
		}
		this._prop = null;
		this.hasProp = false;
	}

	public static void DestroyProp_NoPool(List<MeshCollider> _colliders, ref bool hasProp, ref GameObject _prop)
	{
		foreach (MeshCollider meshCollider in _colliders)
		{
			if (!(meshCollider == null))
			{
				meshCollider.gameObject.transform.parent = null;
				meshCollider.gameObject.SetActive(false);
			}
		}
		if (hasProp)
		{
			Object.Destroy(_prop);
		}
		_prop = null;
		hasProp = false;
	}

	public void OnRoundStart()
	{
	}

	public void CreateProp()
	{
		if (this.hasProp)
		{
			this.DestroyProp();
		}
		this._isLeftHand = false;
		int num = GorillaPropHuntGameManager.instance.GetSeed();
		if (NetworkSystem.Instance.InRoom)
		{
			num += this.attachedToRig.OwningNetPlayer.ActorNumber;
		}
		SRand srand = new SRand(num);
		string cosmeticId = GorillaPropHuntGameManager.instance.GetCosmeticId(srand.NextUInt());
		PropHuntTaggableProp propHuntTaggableProp;
		if (this._isLocal)
		{
			PropHuntGrabbableProp propHuntGrabbableProp;
			if (PropHuntPools.TryGetGrabbableProp(cosmeticId, out propHuntGrabbableProp))
			{
				this._grabbableProp = propHuntGrabbableProp;
				this._taggableProp = null;
				this._prop = propHuntGrabbableProp.gameObject;
				this._propOffset = this._grabbableProp.offset;
				propHuntGrabbableProp.handFollower = this;
				this.hasProp = true;
				for (int i = 0; i < propHuntGrabbableProp.interactionPoints.Count; i++)
				{
					propHuntGrabbableProp.interactionPoints[i].OnSpawn(this.attachedToRig);
				}
				return;
			}
		}
		else if (PropHuntPools.TryGetTaggableProp(cosmeticId, out propHuntTaggableProp))
		{
			this._taggableProp = propHuntTaggableProp;
			this._grabbableProp = null;
			this._prop = propHuntTaggableProp.gameObject;
			this._propOffset = propHuntTaggableProp.offset;
			propHuntTaggableProp.ownerRig = this.attachedToRig;
			this.hasProp = true;
		}
	}

	public void OnPropLoaded(AsyncOperationHandle<GameObject> handle)
	{
		this.IsInstantiatingAsync = false;
		CosmeticSO cosmeticSO = null;
		if (PropHuntHandFollower.TryPrepPropTemplate(handle.Result, this._isLocal, cosmeticSO, this._colliders, this._interactionPoints, out this._grabbableProp, out this._taggableProp))
		{
			this._prop = handle.Result;
			this.hasProp = this._prop != null;
			this._prop.SetActive(true);
			if (this._isLocal)
			{
				this._propOffset = this._grabbableProp.offset;
				this._grabbableProp.handFollower = this;
				for (int i = 0; i < this._interactionPoints.Count; i++)
				{
					this._interactionPoints[i].OnSpawn(this.attachedToRig);
				}
				return;
			}
			this._propOffset = this._taggableProp.offset;
			this._taggableProp.ownerRig = this.attachedToRig;
		}
	}

	public static bool TryPrepPropTemplate(GameObject _prop, bool _isLocal, CosmeticSO debugCosmeticSO, List<MeshCollider> _colliders, List<InteractionPoint> ref_interactionPoints, out PropHuntGrabbableProp grabbableProp, out PropHuntTaggableProp taggableProp)
	{
		if (_isLocal)
		{
			grabbableProp = _prop.AddComponent<PropHuntGrabbableProp>();
			taggableProp = null;
			grabbableProp.interactionPoints = ref_interactionPoints;
		}
		else
		{
			taggableProp = _prop.AddComponent<PropHuntTaggableProp>();
			grabbableProp = null;
		}
		bool flag = false;
		bool flag2 = true;
		Bounds bounds = default(Bounds);
		int num = 0;
		foreach (MeshRenderer meshRenderer in _prop.GetComponentsInChildren<MeshRenderer>())
		{
			MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
			if (!(component == null))
			{
				Mesh sharedMesh = component.sharedMesh;
				if (!(sharedMesh == null) && sharedMesh.isReadable)
				{
					flag = true;
					if (flag2)
					{
						bounds = meshRenderer.bounds;
					}
					else
					{
						bounds.Encapsulate(meshRenderer.bounds);
					}
					MeshCollider meshCollider;
					if (num >= _colliders.Count)
					{
						GameObject gameObject = new GameObject("PropHuntTaggable");
						gameObject.layer = 14;
						meshCollider = gameObject.AddComponent<MeshCollider>();
						meshCollider.convex = true;
						meshCollider.isTrigger = true;
						if (_isLocal)
						{
							ref_interactionPoints.Add(gameObject.AddComponent<InteractionPoint>());
						}
						_colliders.Add(meshCollider);
					}
					else
					{
						meshCollider = _colliders[num];
						meshCollider.gameObject.SetActive(true);
					}
					meshCollider.transform.parent = _prop.transform;
					meshCollider.transform.position = meshRenderer.transform.position;
					meshCollider.transform.rotation = meshRenderer.transform.rotation;
					meshCollider.sharedMesh = sharedMesh;
					num++;
					flag2 = false;
				}
			}
		}
		if (!flag)
		{
			bool flag3 = true;
			PropHuntHandFollower.DestroyProp_NoPool(_colliders, ref flag3, ref _prop);
			return false;
		}
		Vector3 vector = _prop.transform.InverseTransformPoint(bounds.center);
		if (_isLocal)
		{
			grabbableProp.interactionPoints = ref_interactionPoints;
			grabbableProp.offset = vector;
		}
		else
		{
			taggableProp.offset = vector;
		}
		return true;
	}

	void ICallBack.CallBack()
	{
		if (!this.hasProp || this._prop.IsNull())
		{
			return;
		}
		Transform transform = (this._isLeftHand ? this.attachedToRig.leftHand.rigTarget : this.attachedToRig.rightHand.rigTarget);
		Vector3 vector = transform.position;
		if (this.attachedToRig.isLocal)
		{
			vector = (this._isLeftHand ? this.attachedToRig.leftHand.overrideTarget.position : this.attachedToRig.rightHand.overrideTarget.position);
		}
		if ((this._isLeftHand ? Mathf.Max(this.attachedToRig.leftIndex.calcT, this.attachedToRig.leftMiddle.calcT) : Mathf.Max(this.attachedToRig.rightIndex.calcT, this.attachedToRig.rightMiddle.calcT)) > 0.5f)
		{
			this._prop.transform.rotation = transform.TransformRotation(this._lastRelativeAngle);
			this._prop.transform.position = this.GeoCollisionPoint(vector, transform.TransformPoint(this._lastRelativePos) + this._prop.transform.TransformVector(this._propOffset)) - this._prop.transform.TransformVector(this._propOffset);
			this._networkLastRelativePos = transform.InverseTransformPoint(this._prop.transform.position);
			this._networkLastRelativeAngle = transform.InverseTransformRotation(this._prop.transform.rotation);
			return;
		}
		Vector3 vector2 = transform.transform.position - this._prop.transform.TransformPoint(this._propOffset);
		if (vector2.IsLongerThan(GorillaPropHuntGameManager.instance.HandFollowDistance))
		{
			float num = vector2.magnitude - GorillaPropHuntGameManager.instance.HandFollowDistance;
			this._prop.transform.position = this.GeoCollisionPoint(vector, this._prop.transform.position + this._prop.transform.TransformVector(this._propOffset) + vector2.normalized * num) - this._prop.transform.TransformVector(this._propOffset);
		}
		this._lastRelativePos = transform.InverseTransformPoint(this._prop.transform.position);
		this._lastRelativeAngle = transform.InverseTransformRotation(this._prop.transform.rotation);
		this._networkLastRelativePos = this._lastRelativePos;
		this._networkLastRelativeAngle = this._lastRelativeAngle;
	}

	public Vector3 GeoCollisionPoint(Vector3 sourcePos, Vector3 targetPos)
	{
		Vector3 vector = targetPos - sourcePos;
		int num = Physics.RaycastNonAlloc(sourcePos, vector.normalized, this.raycastHits, vector.magnitude, this.collisionLayers, QueryTriggerInteraction.Ignore);
		if (num > 0)
		{
			float num2 = vector.sqrMagnitude;
			Vector3 vector2 = targetPos;
			for (int i = 0; i < num; i++)
			{
				Vector3 vector3 = this.raycastHits[i].point - sourcePos;
				if (vector3.sqrMagnitude < num2)
				{
					vector2 = this.raycastHits[i].point;
					num2 = vector3.sqrMagnitude;
				}
			}
			return vector2;
		}
		return targetPos;
	}

	public void SwitchHand(bool newIsLeftHand)
	{
		if (this._isLeftHand == newIsLeftHand)
		{
			return;
		}
		this._isLeftHand = newIsLeftHand;
		Transform transform = (this._isLeftHand ? this.attachedToRig.leftHand.rigTarget : this.attachedToRig.rightHand.rigTarget);
		this._lastRelativePos = transform.InverseTransformPoint(this._prop.transform.position);
		this._lastRelativeAngle = transform.InverseTransformRotation(this._prop.transform.rotation);
	}

	public void SetProp(bool isLeftHand, Vector3 propPos, Quaternion propRot)
	{
		this._isLeftHand = isLeftHand;
		this._lastRelativePos = propPos;
		this._lastRelativeAngle = propRot;
	}

	public long GetRelativePosRotLong()
	{
		if (this._prop.IsNull())
		{
			return BitPackUtils.PackHandPosRotForNetwork(Vector3.zero, Quaternion.identity);
		}
		return BitPackUtils.PackHandPosRotForNetwork(this._lastRelativePos, this._lastRelativeAngle);
	}

	private const bool _k__GT_PROP_HUNT__USE_POOLING__ = true;

	private const bool _k_isBetaOrEditor = false;

	private const float HandFollowDistance = 0.1f;

	private bool _hasProp;

	private bool _isLocal;

	private GameObject _prop;

	private bool _isLeftHand;

	private Vector3 _propOffset;

	private readonly List<MeshCollider> _colliders = new List<MeshCollider>(4);

	private readonly List<InteractionPoint> _interactionPoints = new List<InteractionPoint>(4);

	private Vector3 _lastRelativePos;

	private Quaternion _lastRelativeAngle;

	private Vector3 _networkLastRelativePos;

	private Quaternion _networkLastRelativeAngle;

	public LayerMask collisionLayers;

	private Vector3 targetPoint;

	private RaycastHit[] raycastHits;

	private PropHuntGrabbableProp _grabbableProp;

	private PropHuntTaggableProp _taggableProp;
}
