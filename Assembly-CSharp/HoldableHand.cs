using System;
using GorillaGameModes;
using GorillaLocomotion;
using UnityEngine;

public class HoldableHand : HoldableObject, IGorillaSliceableSimple
{
	public VRRig Rig
	{
		get
		{
			return this.myPlayer;
		}
	}

	private void Start()
	{
		if (this.myPlayer.isOfflineVRRig)
		{
			base.gameObject.SetActive(false);
		}
		if (this.interactionPoint == null)
		{
			this.interactionPoint = base.GetComponent<InteractionPoint>();
		}
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void SliceUpdate()
	{
		this.interactionPoint.enabled = GameMode.ActiveGameMode is GorillaGuardianManager;
	}

	public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
		GorillaGuardianManager gorillaGuardianManager = GameMode.ActiveGameMode as GorillaGuardianManager;
		if (gorillaGuardianManager != null && !this.myPlayer.creator.IsLocal && gorillaGuardianManager.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
		{
			bool flag = grabbingHand == EquipmentInteractor.instance.leftHand;
			this.myPlayer.netView.SendRPC("GrabbedByPlayer", this.myPlayer.Creator, new object[] { this.isBody, this.isLeftHand, flag });
			this.myPlayer.ApplyLocalGrabOverride(this.isBody, this.isLeftHand, grabbingHand.transform);
			EquipmentInteractor.instance.UpdateHandEquipment(this, flag);
			this.ClearOtherGrabs(flag);
		}
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		if (!base.OnRelease(zoneReleased, releasingHand))
		{
			return false;
		}
		GorillaGuardianManager gorillaGuardianManager = GameMode.ActiveGameMode as GorillaGuardianManager;
		if (gorillaGuardianManager != null && !this.myPlayer.creator.IsLocal)
		{
			bool flag = releasingHand == EquipmentInteractor.instance.leftHand;
			Vector3 vector = Vector3.zero;
			if (gorillaGuardianManager.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
			{
				vector = GTPlayer.Instance.GetHandVelocityTracker(flag).GetAverageVelocity(true, 0.15f, false);
			}
			vector = Vector3.ClampMagnitude(vector, 20f);
			this.myPlayer.netView.SendRPC("DroppedByPlayer", this.myPlayer.Creator, new object[] { vector });
			this.myPlayer.ClearLocalGrabOverride();
			this.myPlayer.ApplyLocalTrajectoryOverride(vector);
			EquipmentInteractor.instance.UpdateHandEquipment(null, flag);
		}
		return true;
	}

	public override void OnHover(InteractionPoint pointHovered, GameObject hoveringHand)
	{
	}

	public override void DropItemCleanup()
	{
		this.myPlayer.ClearLocalGrabOverride();
	}

	private void ClearOtherGrabs(bool grabbedLeft)
	{
		IHoldableObject holdableObject = (grabbedLeft ? EquipmentInteractor.instance.rightHandHeldEquipment : EquipmentInteractor.instance.leftHandHeldEquipment);
		if (this.isBody)
		{
			if (holdableObject == this.myPlayer.leftHolds || holdableObject == this.myPlayer.rightHolds)
			{
				EquipmentInteractor.instance.UpdateHandEquipment(null, !grabbedLeft);
				return;
			}
		}
		else if (this.isLeftHand)
		{
			if (holdableObject == this.myPlayer.rightHolds || holdableObject == this.myPlayer.bodyHolds)
			{
				EquipmentInteractor.instance.UpdateHandEquipment(null, !grabbedLeft);
				return;
			}
		}
		else if (holdableObject == this.myPlayer.leftHolds || holdableObject == this.myPlayer.bodyHolds)
		{
			EquipmentInteractor.instance.UpdateHandEquipment(null, !grabbedLeft);
		}
	}

	[SerializeField]
	private VRRig myPlayer;

	[SerializeField]
	private bool isBody;

	[SerializeField]
	private bool isLeftHand;

	public InteractionPoint interactionPoint;
}
