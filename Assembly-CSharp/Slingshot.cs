using System;
using GorillaLocomotion;
using GorillaNetworking;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Slingshot : ProjectileWeapon
{
	private void DestroyDummyProjectile()
	{
		if (this.hasDummyProjectile)
		{
			this.dummyProjectile.transform.localScale = Vector3.one * this.dummyProjectileInitialScale;
			this.dummyProjectile.GetComponent<SphereCollider>().enabled = true;
			ObjectPools.instance.Destroy(this.dummyProjectile);
			this.dummyProjectile = null;
			this.hasDummyProjectile = false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (this.elasticLeft)
		{
			this._elasticIntialWidthMultiplier = this.elasticLeft.widthMultiplier;
		}
	}

	public override void OnSpawn(VRRig rig)
	{
		base.OnSpawn(rig);
		this.myRig = rig;
	}

	internal override void OnEnable()
	{
		this.leftHandSnap = this.myRig.cosmeticReferences.Get(CosmeticRefID.SlingshotSnapLeft).transform;
		this.rightHandSnap = this.myRig.cosmeticReferences.Get(CosmeticRefID.SlingshotSnapRight).transform;
		this.currentState = TransferrableObject.PositionState.OnChest;
		this.itemState = TransferrableObject.ItemStates.State0;
		if (this.elasticLeft)
		{
			this.elasticLeft.positionCount = 2;
		}
		if (this.elasticRight)
		{
			this.elasticRight.positionCount = 2;
		}
		this.dummyProjectile = null;
		base.OnEnable();
	}

	internal override void OnDisable()
	{
		this.DestroyDummyProjectile();
		base.OnDisable();
	}

	protected override void LateUpdateShared()
	{
		base.LateUpdateShared();
		float num = Mathf.Abs(base.transform.lossyScale.x);
		Vector3 vector;
		if (this.InDrawingState())
		{
			if (!this.hasDummyProjectile)
			{
				this.dummyProjectile = ObjectPools.instance.Instantiate(this.projectilePrefab, true);
				this.hasDummyProjectile = true;
				SphereCollider component = this.dummyProjectile.GetComponent<SphereCollider>();
				component.enabled = false;
				this.dummyProjectileColliderRadius = component.radius;
				this.dummyProjectileInitialScale = this.dummyProjectile.transform.localScale.x;
				bool flag;
				bool flag2;
				bool flag3;
				base.GetIsOnTeams(out flag, out flag2, out flag3);
				this.dummyProjectile.GetComponent<SlingshotProjectile>().ApplyTeamModelAndColor(flag, flag2, flag3 && this.targetRig, this.targetRig ? this.targetRig.playerColor : default(Color));
			}
			if (this.disableInDraw != null)
			{
				this.disableInDraw.SetActive(false);
			}
			if (this.disableInDraw != null)
			{
				this.disableInDraw.SetActive(false);
			}
			float num2 = this.dummyProjectileInitialScale * num;
			this.dummyProjectile.transform.localScale = Vector3.one * num2;
			Vector3 position = this.drawingHand.transform.position;
			Vector3 position2 = this.centerOrigin.position;
			Vector3 normalized = (position2 - position).normalized;
			float num3 = (EquipmentInteractor.instance.grabRadius - this.dummyProjectileColliderRadius) * num;
			vector = position + normalized * num3;
			this.dummyProjectile.transform.position = vector;
			this.dummyProjectile.transform.rotation = Quaternion.LookRotation(position2 - vector, Vector3.up);
			if (!this.wasStretching)
			{
				UnityEvent<bool> stretchStartShared = this.StretchStartShared;
				if (stretchStartShared != null)
				{
					stretchStartShared.Invoke(!this.ForLeftHandSlingshot());
				}
				this.wasStretching = true;
			}
		}
		else
		{
			this.DestroyDummyProjectile();
			if (this.disableInDraw != null)
			{
				this.disableInDraw.SetActive(true);
			}
			vector = this.centerOrigin.position;
			if (this.wasStretching)
			{
				UnityEvent<bool> stretchEndShared = this.StretchEndShared;
				if (stretchEndShared != null)
				{
					stretchEndShared.Invoke(!this.ForLeftHandSlingshot());
				}
				this.wasStretching = false;
			}
		}
		this.center.position = vector;
		if (!this.disableLineRenderer)
		{
			this.elasticLeftPoints[0] = this.leftArm.position;
			this.elasticLeftPoints[1] = (this.elasticRightPoints[1] = vector);
			this.elasticRightPoints[0] = this.rightArm.position;
			this.elasticLeft.SetPositions(this.elasticLeftPoints);
			this.elasticRight.SetPositions(this.elasticRightPoints);
			this.elasticLeft.widthMultiplier = this._elasticIntialWidthMultiplier * num;
			this.elasticRight.widthMultiplier = this._elasticIntialWidthMultiplier * num;
		}
		if (!NetworkSystem.Instance.InRoom && this.disableWhenNotInRoom)
		{
			base.gameObject.SetActive(false);
		}
	}

	protected override void LateUpdateLocal()
	{
		base.LateUpdateLocal();
		if (this.InDrawingState())
		{
			if (this.ForLeftHandSlingshot())
			{
				this.drawingHand = EquipmentInteractor.instance.rightHand;
			}
			else
			{
				this.drawingHand = EquipmentInteractor.instance.leftHand;
			}
			GorillaTagger.Instance.StartVibration(!this.ForLeftHandSlingshot(), this.hapticsStrength, this.hapticsLength);
			if (!this.wasStretchingLocal)
			{
				UnityEvent<bool> stretchStartLocal = this.StretchStartLocal;
				if (stretchStartLocal != null)
				{
					stretchStartLocal.Invoke(!this.ForLeftHandSlingshot());
				}
				this.wasStretchingLocal = true;
				return;
			}
		}
		else if (this.wasStretchingLocal)
		{
			UnityEvent<bool> stretchEndLocal = this.StretchEndLocal;
			if (stretchEndLocal != null)
			{
				stretchEndLocal.Invoke(!this.ForLeftHandSlingshot());
			}
			this.wasStretchingLocal = false;
		}
	}

	protected override void LateUpdateReplicated()
	{
		base.LateUpdateReplicated();
		if (this.InDrawingState())
		{
			if (this.ForLeftHandSlingshot())
			{
				this.drawingHand = this.rightHandSnap.gameObject;
				return;
			}
			this.drawingHand = this.leftHandSnap.gameObject;
		}
	}

	public static bool IsSlingShotEnabled()
	{
		return !(GorillaTagger.Instance == null) && !(GorillaTagger.Instance.offlineVRRig == null) && GorillaTagger.Instance.offlineVRRig.cosmeticSet.HasItemOfCategory(CosmeticsController.CosmeticCategory.Chest);
	}

	public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
		if (!this.IsMyItem())
		{
			return;
		}
		bool flag = pointGrabbed == this.nock;
		if (flag && !base.InHand())
		{
			return;
		}
		base.OnGrab(pointGrabbed, grabbingHand);
		if (this.InDrawingState() || base.OnChest())
		{
			return;
		}
		if (flag)
		{
			if (grabbingHand == EquipmentInteractor.instance.leftHand)
			{
				EquipmentInteractor.instance.disableLeftGrab = true;
			}
			else
			{
				EquipmentInteractor.instance.disableRightGrab = true;
			}
			if (this.ForLeftHandSlingshot())
			{
				this.itemState = TransferrableObject.ItemStates.State2;
			}
			else
			{
				this.itemState = TransferrableObject.ItemStates.State3;
			}
			this.minTimeToLaunch = Time.time + this.delayLaunchTime;
			GorillaTagger.Instance.StartVibration(!this.ForLeftHandSlingshot(), GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration * 1.5f);
		}
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		base.OnRelease(zoneReleased, releasingHand);
		if (this.InDrawingState() && releasingHand == this.drawingHand)
		{
			if (releasingHand == EquipmentInteractor.instance.leftHand)
			{
				EquipmentInteractor.instance.disableLeftGrab = false;
			}
			else
			{
				EquipmentInteractor.instance.disableRightGrab = false;
			}
			if (this.ForLeftHandSlingshot())
			{
				this.currentState = TransferrableObject.PositionState.InLeftHand;
			}
			else
			{
				this.currentState = TransferrableObject.PositionState.InRightHand;
			}
			this.itemState = TransferrableObject.ItemStates.State0;
			GorillaTagger.Instance.StartVibration(this.ForLeftHandSlingshot(), GorillaTagger.Instance.tapHapticStrength * 2f, GorillaTagger.Instance.tapHapticDuration * 1.5f);
			if (Time.time > this.minTimeToLaunch && (releasingHand.transform.position - this.centerOrigin.transform.position).sqrMagnitude > this.minDrawDistanceToRelease * this.minDrawDistanceToRelease)
			{
				base.LaunchProjectile();
			}
		}
		else
		{
			EquipmentInteractor.instance.disableLeftGrab = false;
			EquipmentInteractor.instance.disableRightGrab = false;
		}
		return true;
	}

	public override void DropItemCleanup()
	{
		base.DropItemCleanup();
		this.currentState = TransferrableObject.PositionState.OnChest;
		this.itemState = TransferrableObject.ItemStates.State0;
	}

	public override bool AutoGrabTrue(bool leftGrabbingHand)
	{
		return true;
	}

	private bool ForLeftHandSlingshot()
	{
		return this.itemState == TransferrableObject.ItemStates.State2 || this.currentState == TransferrableObject.PositionState.InLeftHand;
	}

	private bool InDrawingState()
	{
		return this.itemState == TransferrableObject.ItemStates.State2 || this.itemState == TransferrableObject.ItemStates.State3;
	}

	protected override Vector3 GetLaunchPosition()
	{
		return this.dummyProjectile.transform.position;
	}

	protected override Vector3 GetLaunchVelocity()
	{
		float num = Mathf.Abs(base.transform.lossyScale.x);
		Vector3 vector = this.centerOrigin.position - this.center.position;
		vector /= num;
		Vector3 vector2 = Mathf.Min(this.springConstant * this.maxDraw, vector.magnitude * this.springConstant) * vector.normalized * num;
		Vector3 averagedVelocity = GTPlayer.Instance.AveragedVelocity;
		return vector2 + averagedVelocity;
	}

	[SerializeField]
	private bool disableLineRenderer;

	[FormerlySerializedAs("elastic")]
	public LineRenderer elasticLeft;

	public LineRenderer elasticRight;

	public Transform leftArm;

	public Transform rightArm;

	public Transform center;

	public Transform centerOrigin;

	private GameObject dummyProjectile;

	public GameObject drawingHand;

	public InteractionPoint nock;

	public InteractionPoint grip;

	public float springConstant;

	public float maxDraw;

	[SerializeField]
	private GameObject disableInDraw;

	[SerializeField]
	private float minDrawDistanceToRelease;

	[Header("Stretching Haptics")]
	[Space]
	[SerializeField]
	private bool playStretchingHaptics;

	[SerializeField]
	private float hapticsStrength = 0.1f;

	[SerializeField]
	private float hapticsLength = 0.1f;

	[Header("Stretching Events")]
	[Space]
	public UnityEvent<bool> StretchStartShared;

	public UnityEvent<bool> StretchEndShared;

	[Space]
	public UnityEvent<bool> StretchStartLocal;

	public UnityEvent<bool> StretchEndLocal;

	private bool wasStretching;

	private bool wasStretchingLocal;

	private Transform leftHandSnap;

	private Transform rightHandSnap;

	public bool disableWhenNotInRoom;

	private bool hasDummyProjectile;

	private float delayLaunchTime = 0.07f;

	private float minTimeToLaunch = -1f;

	private float dummyProjectileColliderRadius;

	private float dummyProjectileInitialScale;

	private int projectileCount;

	private Vector3[] elasticLeftPoints = new Vector3[2];

	private Vector3[] elasticRightPoints = new Vector3[2];

	private float _elasticIntialWidthMultiplier;

	private new VRRig myRig;

	public enum SlingshotState
	{
		NoState = 1,
		OnChest,
		LeftHandDrawing = 4,
		RightHandDrawing = 8
	}

	public enum SlingshotActions
	{
		Grab,
		Release
	}
}
