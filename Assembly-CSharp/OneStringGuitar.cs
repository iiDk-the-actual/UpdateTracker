using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaTag.CosmeticSystem;
using Photon.Pun;
using UnityEngine;

public class OneStringGuitar : TransferrableObject
{
	public override Matrix4x4 GetDefaultTransformationMatrix()
	{
		return Matrix4x4.identity;
	}

	public override void OnSpawn(VRRig rig)
	{
		base.OnSpawn(rig);
		this.chestColliderLeft = this._GetChestColliderByPath(rig, "GorillaPlayerNetworkedRigAnchor/rig/body/Old Cosmetics Body/OneStringGuitarStick/Center/BaseTransformLeft");
		this.chestColliderRight = this._GetChestColliderByPath(rig, "GorillaPlayerNetworkedRigAnchor/rig/body/Old Cosmetics Body/OneStringGuitarStick/Center/BaseTransformRight");
		this.currentChestCollider = this.chestColliderLeft;
		Transform[] array;
		string text;
		if (!GTHardCodedBones.TryGetBoneXforms(rig, out array, out text))
		{
			Debug.LogError("OneStringGuitar: Error getting bone Transforms: " + text, this);
			return;
		}
		this.parentHandLeft = array[9];
		this.parentHandRight = array[27];
		this.parentHand = this.parentHandRight;
		this.leftHandIndicator = GorillaTagger.Instance.leftHandTriggerCollider.GetComponent<GorillaTriggerColliderHandIndicator>();
		this.rightHandIndicator = GorillaTagger.Instance.rightHandTriggerCollider.GetComponent<GorillaTriggerColliderHandIndicator>();
		this.sphereRadius = this.leftHandIndicator.GetComponent<SphereCollider>().radius;
		this.itemState = TransferrableObject.ItemStates.State0;
		this.nullHit = default(RaycastHit);
		this.strumList.Add(this.strumCollider);
		this.lastState = OneStringGuitar.GuitarStates.Club;
		this.startingLeftChestOffset = this.chestOffsetLeft;
		this.startingRightChestOffset = this.chestOffsetRight;
		this.startingUnsnapDistance = this.unsnapDistance;
		this.selfInstrumentIndex = rig.AssignInstrumentToInstrumentSelfOnly(this);
		for (int i = 0; i < this.frets.Length; i++)
		{
			this.fretsList.Add(this.frets[i]);
		}
	}

	private Collider _GetChestColliderByPath(VRRig vrRig, string chestColliderLeftPath)
	{
		Transform transform;
		if (!vrRig.transform.TryFindByExactPath(chestColliderLeftPath, out transform))
		{
			Debug.LogError("DEACTIVATING! do you move this without updating the script? could not find this transform: \"" + chestColliderLeftPath + "\"");
			base.gameObject.SetActive(false);
		}
		Collider component = transform.GetComponent<Collider>();
		if (!component)
		{
			Debug.LogError("DEACTIVATING! found transform but couldn't find collider at path: \"" + chestColliderLeftPath + "\"");
			base.gameObject.SetActive(false);
		}
		return component;
	}

	internal override void OnEnable()
	{
		base.OnEnable();
		if (this.currentState == TransferrableObject.PositionState.InLeftHand)
		{
			this.fretHandIndicator = this.leftHandIndicator;
			this.strumHandIndicator = this.rightHandIndicator;
		}
		else
		{
			this.fretHandIndicator = this.rightHandIndicator;
			this.strumHandIndicator = this.leftHandIndicator;
		}
		if (base.IsLocalObject())
		{
			this.parentHand = GTPlayer.Instance.GetHandFollower(this.currentState == TransferrableObject.PositionState.InLeftHand);
		}
		this.initOffset = Vector3.zero;
		this.initRotation = Quaternion.identity;
	}

	internal override void OnDisable()
	{
		base.OnDisable();
		this.angleSnapped = false;
		this.positionSnapped = false;
		this.lastState = OneStringGuitar.GuitarStates.Club;
		this.itemState = TransferrableObject.ItemStates.State0;
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		if (!base.OnRelease(zoneReleased, releasingHand))
		{
			return false;
		}
		if (!this.CanDeactivate())
		{
			return false;
		}
		if (base.InHand())
		{
			return false;
		}
		this.itemState = TransferrableObject.ItemStates.State0;
		return true;
	}

	protected override void LateUpdateShared()
	{
		base.LateUpdateShared();
		if (this.lastState != (OneStringGuitar.GuitarStates)this.itemState)
		{
			this.angleSnapped = false;
			this.positionSnapped = false;
		}
		if (this.itemState == TransferrableObject.ItemStates.State0)
		{
			Vector3 vector = ((this.currentState == TransferrableObject.PositionState.InLeftHand) ? this.startPositionLeft : this.startPositionRight);
			Quaternion quaternion = ((this.currentState == TransferrableObject.PositionState.InLeftHand) ? this.startQuatLeft : this.startQuatRight);
			this.UpdateNonPlayingPosition(vector, quaternion);
		}
		else if (this.itemState == TransferrableObject.ItemStates.State1)
		{
			Vector3 vector2 = ((this.currentState == TransferrableObject.PositionState.InLeftHand) ? this.reverseGripPositionLeft : this.reverseGripPositionRight);
			Quaternion quaternion2 = ((this.currentState == TransferrableObject.PositionState.InLeftHand) ? this.reverseGripQuatLeft : this.reverseGripQuatRight);
			this.UpdateNonPlayingPosition(vector2, quaternion2);
			if (this.IsMyItem() && (this.chestTouch.transform.position - this.currentChestCollider.transform.position).magnitude < this.snapDistance)
			{
				this.itemState = TransferrableObject.ItemStates.State2;
				this.angleSnapped = false;
				this.positionSnapped = false;
				this.currentChestCollider.gameObject.SetActive(true);
			}
		}
		else if (this.itemState == TransferrableObject.ItemStates.State2)
		{
			Quaternion quaternion3 = ((this.currentState == TransferrableObject.PositionState.InLeftHand) ? this.holdingOffsetRotationLeft : this.holdingOffsetRotationRight);
			Vector3 vector3 = ((this.currentState == TransferrableObject.PositionState.InLeftHand) ? this.chestOffsetLeft : this.chestOffsetRight);
			Quaternion quaternion4 = Quaternion.LookRotation(this.parentHand.position - this.currentChestCollider.transform.position) * quaternion3;
			if (!this.angleSnapped && Quaternion.Angle(base.transform.rotation, quaternion4) > this.angleLerpSnap)
			{
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion4, this.lerpValue);
			}
			else
			{
				this.angleSnapped = true;
				base.transform.rotation = quaternion4;
			}
			Vector3 vector4 = this.currentChestCollider.transform.position + base.transform.rotation * vector3;
			if (!this.positionSnapped && (base.transform.position - vector4).magnitude > this.vectorLerpSnap)
			{
				base.transform.position = Vector3.Lerp(base.transform.position, this.currentChestCollider.transform.position + base.transform.rotation * vector3, this.lerpValue);
			}
			else
			{
				this.positionSnapped = true;
				base.transform.position = vector4;
			}
			if (this.currentState == TransferrableObject.PositionState.InRightHand)
			{
				this.parentHand = this.parentHandRight;
			}
			else
			{
				this.parentHand = this.parentHandLeft;
			}
			if (this.IsMyItem())
			{
				this.unsnapDistance = this.startingUnsnapDistance * base.myRig.transform.localScale.x;
				if (this.currentState == TransferrableObject.PositionState.InRightHand)
				{
					this.chestOffsetRight = Vector3.Scale(this.startingRightChestOffset, base.myRig.transform.localScale);
					this.currentChestCollider = this.chestColliderRight;
					this.fretHandIndicator = this.rightHandIndicator;
					this.strumHandIndicator = this.leftHandIndicator;
				}
				else
				{
					this.chestOffsetLeft = Vector3.Scale(this.startingLeftChestOffset, base.myRig.transform.localScale);
					this.currentChestCollider = this.chestColliderLeft;
					this.fretHandIndicator = this.leftHandIndicator;
					this.strumHandIndicator = this.rightHandIndicator;
				}
				if (this.Unsnap())
				{
					this.itemState = TransferrableObject.ItemStates.State1;
					this.angleSnapped = false;
					this.positionSnapped = false;
					if (this.currentState == TransferrableObject.PositionState.InLeftHand)
					{
						EquipmentInteractor.instance.wasLeftGrabPressed = true;
					}
					else
					{
						EquipmentInteractor.instance.wasRightGrabPressed = true;
					}
					this.currentChestCollider.gameObject.SetActive(false);
				}
				else
				{
					if (!this.handIn)
					{
						this.CheckFretFinger(this.fretHandIndicator.transform);
						HitChecker.CheckHandHit(ref this.collidersHitCount, this.interactableMask, this.sphereRadius, ref this.nullHit, ref this.raycastHits, ref this.raycastHitList, ref this.spherecastSweep, ref this.strumHandIndicator);
						if (this.collidersHitCount > 0)
						{
							int i = 0;
							while (i < this.collidersHitCount)
							{
								if (this.raycastHits[i].collider != null && this.strumCollider == this.raycastHits[i].collider)
								{
									GorillaTagger.Instance.StartVibration(this.strumHandIndicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 6f, GorillaTagger.Instance.tapHapticDuration);
									this.PlayNote(this.currentFretIndex, Mathf.Max(Mathf.Min(1f, this.strumHandIndicator.currentVelocity.magnitude / this.maxVelocity) * this.maxVolume, this.minVolume));
									if (!NetworkSystem.Instance.InRoom || this.selfInstrumentIndex <= -1)
									{
										break;
									}
									NetworkView myVRRig = GorillaTagger.Instance.myVRRig;
									if (myVRRig == null)
									{
										break;
									}
									myVRRig.SendRPC("RPC_PlaySelfOnlyInstrument", RpcTarget.Others, new object[]
									{
										this.selfInstrumentIndex,
										this.currentFretIndex,
										this.audioSource.volume
									});
									break;
								}
								else
								{
									i++;
								}
							}
						}
					}
					this.handIn = HitChecker.CheckHandIn(ref this.anyHit, ref this.collidersHit, this.sphereRadius * base.transform.lossyScale.x, this.interactableMask, ref this.strumHandIndicator, ref this.strumList);
				}
			}
		}
		this.lastState = (OneStringGuitar.GuitarStates)this.itemState;
	}

	public override void PlayNote(int note, float volume)
	{
		this.audioSource.time = 0.005f;
		this.audioSource.clip = this.audioClips[note];
		this.audioSource.volume = volume;
		this.audioSource.GTPlay();
		base.PlayNote(note, volume);
	}

	private bool Unsnap()
	{
		return (this.parentHand.position - this.chestTouch.position).magnitude > this.unsnapDistance;
	}

	private void CheckFretFinger(Transform finger)
	{
		for (int i = 0; i < this.collidersHit.Length; i++)
		{
			this.collidersHit[i] = null;
		}
		this.collidersHitCount = Physics.OverlapSphereNonAlloc(finger.position, this.sphereRadius, this.collidersHit, this.interactableMask, QueryTriggerInteraction.Collide);
		this.currentFretIndex = 5;
		if (this.collidersHitCount > 0)
		{
			for (int j = 0; j < this.collidersHit.Length; j++)
			{
				if (this.fretsList.Contains(this.collidersHit[j]))
				{
					this.currentFretIndex = this.fretsList.IndexOf(this.collidersHit[j]);
					if (this.currentFretIndex != this.lastFretIndex)
					{
						GorillaTagger.Instance.StartVibration(this.fretHandIndicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 6f, GorillaTagger.Instance.tapHapticDuration);
					}
					this.lastFretIndex = this.currentFretIndex;
					return;
				}
			}
			return;
		}
		if (this.lastFretIndex != -1)
		{
			GorillaTagger.Instance.StartVibration(this.fretHandIndicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 6f, GorillaTagger.Instance.tapHapticDuration);
		}
		this.lastFretIndex = -1;
	}

	public void UpdateNonPlayingPosition(Vector3 positionTarget, Quaternion rotationTarget)
	{
		if (!this.angleSnapped)
		{
			if (Quaternion.Angle(rotationTarget, base.transform.localRotation) < this.angleLerpSnap)
			{
				this.angleSnapped = true;
				base.transform.localRotation = rotationTarget;
			}
			else
			{
				base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, rotationTarget, this.lerpValue);
			}
		}
		if (!this.positionSnapped)
		{
			if ((base.transform.localPosition - positionTarget).magnitude < this.vectorLerpSnap)
			{
				this.positionSnapped = true;
				base.transform.localPosition = positionTarget;
				return;
			}
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, positionTarget, this.lerpValue);
		}
	}

	public override bool CanDeactivate()
	{
		return !base.gameObject.activeSelf || this.itemState == TransferrableObject.ItemStates.State0 || this.itemState == TransferrableObject.ItemStates.State1;
	}

	public override bool CanActivate()
	{
		return this.itemState == TransferrableObject.ItemStates.State0 || this.itemState == TransferrableObject.ItemStates.State1;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		if (this.itemState == TransferrableObject.ItemStates.State0)
		{
			this.itemState = TransferrableObject.ItemStates.State1;
			return;
		}
		this.itemState = TransferrableObject.ItemStates.State0;
	}

	public void GenerateVectorOffsetLeft()
	{
		this.chestOffsetLeft = base.transform.position - this.chestColliderLeft.transform.position;
		this.holdingOffsetRotationLeft = Quaternion.LookRotation(base.transform.position - this.chestColliderLeft.transform.position);
	}

	public void GenerateVectorOffsetRight()
	{
		this.chestOffsetRight = base.transform.position - this.chestColliderRight.transform.position;
		this.holdingOffsetRotationRight = Quaternion.LookRotation(base.transform.position - this.chestColliderRight.transform.position);
	}

	public void GenerateReverseGripOffsetLeft()
	{
		this.reverseGripPositionLeft = base.transform.localPosition;
		this.reverseGripQuatLeft = base.transform.localRotation;
	}

	public void GenerateClubOffsetLeft()
	{
		this.startPositionLeft = base.transform.localPosition;
		this.startQuatLeft = base.transform.localRotation;
	}

	public void GenerateReverseGripOffsetRight()
	{
		this.reverseGripPositionRight = base.transform.localPosition;
		this.reverseGripQuatRight = base.transform.localRotation;
	}

	public void GenerateClubOffsetRight()
	{
		this.startPositionRight = base.transform.localPosition;
		this.startQuatRight = base.transform.localRotation;
	}

	public void TestClubPositionRight()
	{
		base.transform.localPosition = this.startPositionRight;
		base.transform.localRotation = this.startQuatRight;
	}

	public void TestReverseGripPositionRight()
	{
		base.transform.localPosition = this.reverseGripPositionRight;
		base.transform.localRotation = this.reverseGripQuatRight;
	}

	public void TestPlayingPositionRight()
	{
		base.transform.rotation = Quaternion.LookRotation(this.parentHand.position - this.currentChestCollider.transform.position) * this.holdingOffsetRotationRight;
		base.transform.position = this.chestColliderRight.transform.position + base.transform.rotation * this.chestOffsetRight;
	}

	public Vector3 chestOffsetLeft;

	public Vector3 chestOffsetRight;

	public Quaternion holdingOffsetRotationLeft;

	public Quaternion holdingOffsetRotationRight;

	public Quaternion chestRotationOffset;

	[NonSerialized]
	public Collider currentChestCollider;

	[NonSerialized]
	public Collider chestColliderLeft;

	[NonSerialized]
	public Collider chestColliderRight;

	public float lerpValue = 0.25f;

	public AudioSource audioSource;

	private Transform parentHand;

	private Transform parentHandLeft;

	private Transform parentHandRight;

	public float unsnapDistance;

	public float snapDistance;

	public Vector3 startPositionLeft;

	public Quaternion startQuatLeft;

	public Vector3 reverseGripPositionLeft;

	public Quaternion reverseGripQuatLeft;

	public Vector3 startPositionRight;

	public Quaternion startQuatRight;

	public Vector3 reverseGripPositionRight;

	public Quaternion reverseGripQuatRight;

	public float angleLerpSnap = 1f;

	public float vectorLerpSnap = 0.01f;

	private bool angleSnapped;

	private bool positionSnapped;

	public Transform chestTouch;

	private int collidersHitCount;

	private Collider[] collidersHit = new Collider[20];

	private RaycastHit[] raycastHits = new RaycastHit[20];

	private List<RaycastHit> raycastHitList = new List<RaycastHit>();

	private RaycastHit nullHit;

	public Collider[] collidersToBeIn;

	public LayerMask interactableMask;

	public int currentFretIndex;

	public int lastFretIndex;

	public Collider[] frets;

	private List<Collider> fretsList = new List<Collider>();

	public AudioClip[] audioClips;

	private GorillaTriggerColliderHandIndicator leftHandIndicator;

	private GorillaTriggerColliderHandIndicator rightHandIndicator;

	private GorillaTriggerColliderHandIndicator fretHandIndicator;

	private GorillaTriggerColliderHandIndicator strumHandIndicator;

	private float sphereRadius;

	private bool anyHit;

	private bool handIn;

	private Vector3 spherecastSweep;

	public Collider strumCollider;

	public float maxVolume = 1f;

	public float minVolume = 0.05f;

	public float maxVelocity = 2f;

	private List<Collider> strumList = new List<Collider>();

	private int selfInstrumentIndex = -1;

	private OneStringGuitar.GuitarStates lastState;

	private Vector3 startingLeftChestOffset;

	private Vector3 startingRightChestOffset;

	private float startingUnsnapDistance;

	private enum GuitarStates
	{
		Club = 1,
		HeldReverseGrip,
		Playing = 4
	}
}
