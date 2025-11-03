using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CrittersCage : CrittersActor
{
	public Vector3 critterScale
	{
		get
		{
			if (this.subObjectIndex < this.critterScales.Length && this.subObjectIndex >= 0)
			{
				return this.critterScales[this.subObjectIndex];
			}
			return Vector3.one;
		}
	}

	public bool CanCatch
	{
		get
		{
			return this.heldByPlayer && !this.hasCritter && !this.inReleasingPosition && this._releaseCooldownEnd <= Time.time;
		}
	}

	public void SetHasCritter(bool value)
	{
		if (this.hasCritter != value && !value)
		{
			this._releaseCooldownEnd = Time.time + this.releaseCooldown;
		}
		this.hasCritter = value;
		this.UpdateCageVisuals();
	}

	public override void Initialize()
	{
		base.Initialize();
		this.hasCritter = false;
		this.heldByPlayer = false;
		this.inReleasingPosition = false;
		this.SetLidActive(true, false);
	}

	private void UpdateCageVisuals()
	{
		this.SetLidActive(!this.heldByPlayer || this.hasCritter, true);
	}

	private void SetLidActive(bool active, bool playAudio = true)
	{
		if (active != this._lidActive && playAudio)
		{
			this.sound.GTPlayOneShot(active ? this.openSound : this.closeSound, 1f);
		}
		this.lid.SetActive(active);
		this._lidActive = active;
	}

	protected override void RemoteGrabbedBy(CrittersActor grabbingActor)
	{
		base.RemoteGrabbedBy(grabbingActor);
		this.heldByPlayer = grabbingActor.isOnPlayer;
		this.UpdateCageVisuals();
	}

	public override void GrabbedBy(CrittersActor grabbingActor, bool positionOverride = false, Quaternion localRotation = default(Quaternion), Vector3 localOffset = default(Vector3), bool disableGrabbing = false)
	{
		base.GrabbedBy(grabbingActor, positionOverride, localRotation, localOffset, disableGrabbing);
		this.heldByPlayer = grabbingActor.isOnPlayer;
		this.UpdateCageVisuals();
	}

	public override void Released(bool keepWorldPosition, Quaternion rotation = default(Quaternion), Vector3 position = default(Vector3), Vector3 impulseVelocity = default(Vector3), Vector3 impulseAngularVelocity = default(Vector3))
	{
		base.Released(keepWorldPosition, rotation, position, impulseVelocity, impulseAngularVelocity);
		this.heldByPlayer = false;
		this.UpdateCageVisuals();
	}

	protected override void HandleRemoteReleased()
	{
		base.HandleRemoteReleased();
		this.heldByPlayer = false;
		this.UpdateCageVisuals();
	}

	public override bool ShouldDespawn()
	{
		return base.ShouldDespawn() && !this.hasCritter;
	}

	public override void SendDataByCrittersActorType(PhotonStream stream)
	{
		base.SendDataByCrittersActorType(stream);
		stream.SendNext(this.hasCritter);
	}

	public override bool UpdateSpecificActor(PhotonStream stream)
	{
		if (!base.UpdateSpecificActor(stream))
		{
			return false;
		}
		bool flag;
		if (!CrittersManager.ValidateDataType<bool>(stream.ReceiveNext(), out flag))
		{
			return false;
		}
		this.SetHasCritter(flag);
		return true;
	}

	public override int AddActorDataToList(ref List<object> objList)
	{
		base.AddActorDataToList(ref objList);
		objList.Add(this.hasCritter);
		return this.TotalActorDataLength();
	}

	public override int TotalActorDataLength()
	{
		return base.BaseActorDataLength() + 1;
	}

	public override int UpdateFromRPC(object[] data, int startingIndex)
	{
		startingIndex += base.UpdateFromRPC(data, startingIndex);
		bool flag;
		if (!CrittersManager.ValidateDataType<bool>(data[startingIndex], out flag))
		{
			return this.TotalActorDataLength();
		}
		this.SetHasCritter(flag);
		return this.TotalActorDataLength();
	}

	public Transform grabPosition;

	public Transform cagePosition;

	public float grabDistance;

	[SerializeField]
	private Vector3[] critterScales = new Vector3[] { Vector3.one };

	[SerializeField]
	private float releaseCooldown = 0.25f;

	[SerializeField]
	private AudioSource sound;

	[SerializeField]
	private AudioClip openSound;

	[SerializeField]
	private AudioClip closeSound;

	public GameObject lid;

	[NonSerialized]
	public bool heldByPlayer;

	[NonSerialized]
	private bool hasCritter;

	[NonSerialized]
	public bool inReleasingPosition;

	private float _releaseCooldownEnd;

	private bool _lidActive;
}
