using System;
using System.Collections.Generic;
using GorillaLocomotion;
using GorillaTag;
using UnityEngine;

public class SnowballMaker : MonoBehaviourPostTick
{
	public static SnowballMaker leftHandInstance { get; private set; }

	public static SnowballMaker rightHandInstance { get; private set; }

	public SnowballThrowable[] snowballs { get; private set; }

	private void Awake()
	{
		if (this.isLeftHand)
		{
			if (SnowballMaker.leftHandInstance == null)
			{
				SnowballMaker.leftHandInstance = this;
				return;
			}
			Object.Destroy(base.gameObject);
			return;
		}
		else
		{
			if (SnowballMaker.rightHandInstance == null)
			{
				SnowballMaker.rightHandInstance = this;
				return;
			}
			Object.Destroy(base.gameObject);
			return;
		}
	}

	private void Start()
	{
		this.handTransform = (this.isLeftHand ? GorillaTagger.Instance.offlineVRRig.myBodyDockPositions.leftHandTransform : GorillaTagger.Instance.offlineVRRig.myBodyDockPositions.rightHandTransform);
	}

	internal void SetupThrowables(SnowballThrowable[] newThrowables)
	{
		this.snowballs = newThrowables;
		for (int i = 0; i < this.snowballs.Length; i++)
		{
			for (int j = 0; j < this.snowballs[i].matDataIndexes.Count; j++)
			{
				this.matSnowballLookup.TryAdd(this.snowballs[i].matDataIndexes[j], this.snowballs[i]);
			}
		}
	}

	public override void PostTick()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		if (!CosmeticsV2Spawner_Dirty.allPartsInstantiated)
		{
			return;
		}
		if (this.snowballs == null)
		{
			return;
		}
		if (BuilderPieceInteractor.instance != null && BuilderPieceInteractor.instance.BlockSnowballCreation())
		{
			return;
		}
		if (!GTPlayer.hasInstance || !EquipmentInteractor.hasInstance || !GorillaTagger.hasInstance || !GorillaTagger.Instance.offlineVRRig || this.snowballs.Length == 0)
		{
			return;
		}
		int materialTouchIndex = GTPlayer.Instance.GetMaterialTouchIndex(this.isLeftHand);
		if (materialTouchIndex == 0)
		{
			if (Time.time > this.lastGroundContactTime + this.snowballCreationCooldownTime)
			{
				this.requiresFreshMaterialContact = false;
			}
			return;
		}
		this.lastGroundContactTime = Time.time;
		EquipmentInteractor instance = EquipmentInteractor.instance;
		bool flag = (this.isLeftHand ? instance.leftHandHeldEquipment : instance.rightHandHeldEquipment) != null;
		bool flag2 = (this.isLeftHand ? instance.isLeftGrabbing : instance.isRightGrabbing);
		bool flag3 = false;
		if (flag2 && !this.requiresFreshMaterialContact)
		{
			int num = -1;
			for (int i = 0; i < this.snowballs.Length; i++)
			{
				if (this.snowballs[i].gameObject.activeSelf)
				{
					num = i;
					break;
				}
			}
			SnowballThrowable snowballThrowable = ((num > -1) ? this.snowballs[num] : null);
			GrowingSnowballThrowable growingSnowballThrowable = snowballThrowable as GrowingSnowballThrowable;
			bool flag4 = (this.isLeftHand ? (!ConnectedControllerHandler.Instance.RightValid) : (!ConnectedControllerHandler.Instance.LeftValid));
			if (growingSnowballThrowable != null && (!GrowingSnowballThrowable.twoHandedSnowballGrowing || flag4 || flag3))
			{
				if (snowballThrowable.matDataIndexes.Contains(materialTouchIndex))
				{
					growingSnowballThrowable.IncreaseSize(1);
					GorillaTagger.Instance.StartVibration(this.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 8f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
					this.requiresFreshMaterialContact = true;
					return;
				}
			}
			else if (!flag)
			{
				SnowballThrowable snowballThrowable2;
				if (!this.matSnowballLookup.TryGetValue(materialTouchIndex, out snowballThrowable2))
				{
					return;
				}
				Transform transform = snowballThrowable2.transform;
				Transform transform2 = this.handTransform;
				XformOffset spawnOffset = snowballThrowable2.SpawnOffset;
				snowballThrowable2.SetSnowballActiveLocal(true);
				snowballThrowable2.velocityEstimator = this.velocityEstimator;
				transform.position = transform2.TransformPoint(spawnOffset.pos);
				transform.rotation = transform2.rotation * spawnOffset.rot;
				GorillaTagger.Instance.StartVibration(this.isLeftHand, GorillaTagger.Instance.tapHapticStrength * 0.5f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
				this.requiresFreshMaterialContact = true;
			}
		}
	}

	public bool TryCreateSnowball(int materialIndex, out SnowballThrowable result)
	{
		foreach (SnowballThrowable snowballThrowable in this.snowballs)
		{
			if (snowballThrowable.matDataIndexes.Contains(materialIndex))
			{
				Transform transform = snowballThrowable.transform;
				Transform transform2 = this.handTransform;
				XformOffset spawnOffset = snowballThrowable.SpawnOffset;
				snowballThrowable.SetSnowballActiveLocal(true);
				snowballThrowable.velocityEstimator = this.velocityEstimator;
				transform.position = transform2.TransformPoint(spawnOffset.pos);
				transform.rotation = transform2.rotation * spawnOffset.rot;
				GorillaTagger.Instance.StartVibration(this.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 8f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
				result = snowballThrowable;
				return true;
			}
		}
		result = null;
		return false;
	}

	public bool isLeftHand;

	public GorillaVelocityEstimator velocityEstimator;

	private float snowballCreationCooldownTime = 0.1f;

	private float lastGroundContactTime;

	private bool requiresFreshMaterialContact;

	private Transform handTransform;

	private Dictionary<int, SnowballThrowable> matSnowballLookup = new Dictionary<int, SnowballThrowable>();
}
