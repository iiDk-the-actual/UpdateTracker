using System;
using GorillaLocomotion;
using UnityEngine;

public class SIGadgetTapTeleporterDeployable : MonoBehaviour, IGameEntityComponent
{
	private void Awake()
	{
	}

	private void OnEnable()
	{
		this.activateTime = Time.time + this.activateDelay;
	}

	private void LateUpdate()
	{
		if (Time.time > this.timeToDie && this.gameEntity.IsAuthority())
		{
			if (this.linkedPoint != null)
			{
				this.linkedPoint.ClearLink();
			}
			this.gameEntity.manager.RequestDestroyItem(this.gameEntity.id);
		}
	}

	public void OnEntityInit()
	{
		int num;
		BitPackUtils.UnpackIntsFromLong(this.gameEntity.createData, out this.selectionId, out num);
		if ((float)num < 0f)
		{
			this.timeToDie = float.PositiveInfinity;
		}
		else
		{
			this.timeToDie = Time.time + (float)num;
		}
		this.UpdateSelectionDisplay();
	}

	private void UpdateSelectionDisplay()
	{
		if (this.selectionId == 0)
		{
			this.selectionColorDisplay.material = this.selectionColor1;
			return;
		}
		if (this.selectionId == 1)
		{
			this.selectionColorDisplay.material = this.selectionColor2;
		}
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long newState)
	{
		if (this.gameEntity.IsAuthority())
		{
			return;
		}
		int num;
		int num2;
		BitPackUtils.UnpackIntsFromLong(newState, out num, out num2);
		GameEntity gameEntityFromNetId = this.gameEntity.manager.GetGameEntityFromNetId(num);
		if (gameEntityFromNetId != null)
		{
			SIGadgetTapTeleporter component = gameEntityFromNetId.GetComponent<SIGadgetTapTeleporter>();
			this._pad = component;
			this.identifierColor = this._pad.identifierColor;
		}
		GameEntity gameEntityFromNetId2 = this.gameEntity.manager.GetGameEntityFromNetId(num2);
		if (gameEntityFromNetId2 != null)
		{
			this.linkedPoint = gameEntityFromNetId2.GetComponent<SIGadgetTapTeleporterDeployable>();
			if (this.linkedPoint.linkedPoint == null)
			{
				this.linkedPoint.linkedPoint = this;
				this.linkedPoint._pad = this._pad;
				this.linkedPoint.identifierColor = this.identifierColor;
				this.linkedPoint.UpdateLinkDisplay();
			}
		}
		else
		{
			this.linkedPoint = null;
		}
		this.UpdateLinkDisplay();
	}

	public void SetLink(SIGadgetTapTeleporter newPad, SIGadgetTapTeleporterDeployable newLink)
	{
		this._pad = newPad;
		this.linkedPoint = newLink;
		this.identifierColor = this._pad.identifierColor;
		int num = -1;
		if (this.linkedPoint != null)
		{
			num = this.linkedPoint.gameEntity.GetNetId();
		}
		this.gameEntity.RequestState(this.gameEntity.id, BitPackUtils.PackIntsIntoLong(this._pad.gameEntity.GetNetId(), num));
		this.UpdateLinkDisplay();
		this.stealth.enabled = this._pad.useStealthTeleporters;
		this.maintainVelocity = this._pad.isVelocityPreserved;
	}

	private void ClearLink()
	{
		this.linkedPoint = null;
		this.gameEntity.RequestState(this.gameEntity.id, BitPackUtils.PackIntsIntoLong(this._pad.gameEntity.GetNetId(), -1));
		this.UpdateLinkDisplay();
	}

	private void UpdateLinkDisplay()
	{
		Renderer[] array = this.identifierColorDisplay;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].material.color = this.identifierColor;
		}
		if (this.linkedPoint != null)
		{
			Vector3 vector = this.linkedPoint.transform.position - base.transform.position;
			this.linkDirectionIndicator.gameObject.SetActive(true);
			this.linkDirectionIndicator.transform.rotation = Quaternion.LookRotation(base.transform.forward, vector.normalized);
			return;
		}
		this.linkDirectionIndicator.gameObject.SetActive(false);
	}

	public void TryTeleport()
	{
		if (this.activateTime < Time.time && SIGadgetTapTeleporterDeployable.reteleportTime < Time.time && (!this.requiresSurfaceTapSinceTeleport || GorillaTagger.Instance.hasTappedSurface))
		{
			this.TeleportToLinked();
		}
	}

	private void ResetRetriggerBlock()
	{
		SIGadgetTapTeleporterDeployable.reteleportTime = Time.time + SIGadgetTapTeleporterDeployable.reteleportDelay;
	}

	private void TeleportToLinked()
	{
		if (this.linkedPoint == null || !this.linkedPoint.gameObject.activeSelf)
		{
			return;
		}
		Vector3 position = this.destination.position;
		if (Vector3.Distance(GTPlayer.Instance.transform.position, position) > this.teleportCheckDistance)
		{
			return;
		}
		this.ResetRetriggerBlock();
		if (this.requiresSurfaceTapSinceTeleport)
		{
			GorillaTagger.Instance.ResetTappedSurfaceCheck();
		}
		Vector3 position2 = this.linkedPoint.destination.position;
		Quaternion rotation = GTPlayer.Instance.transform.rotation;
		GTPlayer.Instance.TeleportTo(position2, rotation, this.maintainVelocity, true);
		this.linkedPoint.teleportSoundbank.Play();
	}

	public GameEntity gameEntity;

	[SerializeField]
	private Transform destination;

	[SerializeField]
	private Renderer[] identifierColorDisplay;

	[SerializeField]
	private Transform linkDirectionIndicator;

	[SerializeField]
	private Renderer selectionColorDisplay;

	[SerializeField]
	private Material selectionColor1;

	[SerializeField]
	private Material selectionColor2;

	[SerializeField]
	private SoundBankPlayer teleportSoundbank;

	[SerializeField]
	private SIGameEntityStealthVisibility stealth;

	[SerializeField]
	private bool requiresSurfaceTapSinceTeleport;

	private bool maintainVelocity;

	private int selectionId;

	private SIGadgetTapTeleporter _pad;

	private SIGadgetTapTeleporterDeployable linkedPoint;

	private float activateDelay = 0.3f;

	private float activateTime;

	private static float reteleportDelay = 0.3f;

	private static float reteleportTime;

	private Color identifierColor;

	private float timeToDie = -1f;

	private float teleportCheckDistance = 2f;
}
