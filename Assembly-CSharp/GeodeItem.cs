using System;
using GorillaTag;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class GeodeItem : TransferrableObject
{
	public override void OnSpawn(VRRig rig)
	{
		base.OnSpawn(rig);
		this.hasEffectsGameObject = this.effectsGameObject != null;
		this.effectsHaveBeenPlayed = false;
	}

	protected override void Start()
	{
		base.Start();
		this.itemState = TransferrableObject.ItemStates.State0;
		this.prevItemState = TransferrableObject.ItemStates.State0;
		this.InitToDefault();
	}

	public override void ResetToDefaultState()
	{
		base.ResetToDefaultState();
		this.InitToDefault();
		this.itemState = TransferrableObject.ItemStates.State0;
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		return base.OnRelease(zoneReleased, releasingHand) && this.itemState != TransferrableObject.ItemStates.State0 && !base.InHand();
	}

	public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
		base.OnGrab(pointGrabbed, grabbingHand);
		UnityEvent<GeodeItem> onGeodeGrabbed = this.OnGeodeGrabbed;
		if (onGeodeGrabbed == null)
		{
			return;
		}
		onGeodeGrabbed.Invoke(this);
	}

	private void InitToDefault()
	{
		this.cooldownRemaining = 0f;
		this.effectsHaveBeenPlayed = false;
		if (this.hasEffectsGameObject)
		{
			this.effectsGameObject.SetActive(false);
		}
		this.geodeFullMesh.SetActive(true);
		for (int i = 0; i < this.geodeCrackedMeshes.Length; i++)
		{
			this.geodeCrackedMeshes[i].SetActive(false);
		}
		this.hitLastFrame = false;
	}

	protected override void LateUpdateLocal()
	{
		base.LateUpdateLocal();
		if (this.itemState == TransferrableObject.ItemStates.State1)
		{
			this.cooldownRemaining -= Time.deltaTime;
			if (this.cooldownRemaining <= 0f)
			{
				this.itemState = TransferrableObject.ItemStates.State0;
				this.OnItemStateChanged();
			}
			return;
		}
		if (this.velocityEstimator.linearVelocity.magnitude < this.minHitVelocity)
		{
			return;
		}
		if (base.InHand())
		{
			int num = Physics.SphereCastNonAlloc(this.geodeFullMesh.transform.position, this.sphereRayRadius * Mathf.Abs(this.geodeFullMesh.transform.lossyScale.x), this.geodeFullMesh.transform.TransformDirection(Vector3.forward), this.collidersHit, this.rayCastMaxDistance, this.collisionLayerMask, QueryTriggerInteraction.Collide);
			this.hitLastFrame = num > 0;
		}
		if (!this.hitLastFrame)
		{
			return;
		}
		if (!GorillaParent.hasInstance)
		{
			return;
		}
		UnityEvent<GeodeItem> onGeodeCracked = this.OnGeodeCracked;
		if (onGeodeCracked != null)
		{
			onGeodeCracked.Invoke(this);
		}
		this.itemState = TransferrableObject.ItemStates.State1;
		this.cooldownRemaining = this.cooldown;
		this.index = (this.randomizeGeode ? this.RandomPickCrackedGeode() : 0);
	}

	protected override void LateUpdateShared()
	{
		base.LateUpdateShared();
		this.currentItemState = this.itemState;
		if (this.currentItemState != this.prevItemState)
		{
			this.OnItemStateChanged();
		}
		this.prevItemState = this.currentItemState;
	}

	private void OnItemStateChanged()
	{
		if (this.itemState == TransferrableObject.ItemStates.State0)
		{
			this.InitToDefault();
			return;
		}
		this.geodeFullMesh.SetActive(false);
		for (int i = 0; i < this.geodeCrackedMeshes.Length; i++)
		{
			this.geodeCrackedMeshes[i].SetActive(i == this.index);
		}
		RigContainer rigContainer;
		if (NetworkSystem.Instance.InRoom && GorillaGameManager.instance != null && !this.effectsHaveBeenPlayed && VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.LocalPlayer, out rigContainer))
		{
			rigContainer.Rig.netView.SendRPC("RPC_PlayGeodeEffect", RpcTarget.All, new object[] { this.geodeFullMesh.transform.position });
			this.effectsHaveBeenPlayed = true;
		}
		if (!NetworkSystem.Instance.InRoom && !this.effectsHaveBeenPlayed)
		{
			if (this.audioSource)
			{
				this.audioSource.GTPlay();
			}
			this.effectsHaveBeenPlayed = true;
		}
	}

	private int RandomPickCrackedGeode()
	{
		return Random.Range(0, this.geodeCrackedMeshes.Length);
	}

	[Tooltip("This GameObject will activate when the geode hits the ground with enough force.")]
	public GameObject effectsGameObject;

	public LayerMask collisionLayerMask;

	[Tooltip("Used to calculate velocity of the geode.")]
	public GorillaVelocityEstimator velocityEstimator;

	public float cooldown = 5f;

	[Tooltip("The velocity of the geode must be greater than this value to activate the effect.")]
	public float minHitVelocity = 0.2f;

	[Tooltip("Geode's full mesh before cracking")]
	public GameObject geodeFullMesh;

	[Tooltip("Geode's cracked open half different meshes, picked randomly")]
	public GameObject[] geodeCrackedMeshes;

	[Tooltip("The distance between te geode and the layer mask to detect whether it hits it")]
	public float rayCastMaxDistance = 0.2f;

	[FormerlySerializedAs("collisionRadius")]
	public float sphereRayRadius = 0.05f;

	[DebugReadout]
	private float cooldownRemaining;

	[DebugReadout]
	private bool hitLastFrame;

	[SerializeField]
	private AudioSource audioSource;

	public bool randomizeGeode = true;

	public UnityEvent<GeodeItem> OnGeodeCracked;

	public UnityEvent<GeodeItem> OnGeodeGrabbed;

	private bool hasEffectsGameObject;

	private bool effectsHaveBeenPlayed;

	private RaycastHit hit;

	private RaycastHit[] collidersHit = new RaycastHit[20];

	private TransferrableObject.ItemStates currentItemState;

	private TransferrableObject.ItemStates prevItemState;

	private int index;
}
