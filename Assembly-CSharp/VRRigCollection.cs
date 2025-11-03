using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CompositeTriggerEvents))]
public class VRRigCollection : MonoBehaviour
{
	public List<RigContainer> Rigs
	{
		get
		{
			return this.containedRigs;
		}
	}

	private void OnEnable()
	{
		this.collisionTriggerEvents.CompositeTriggerEnter += this.OnRigTriggerEnter;
		this.collisionTriggerEvents.CompositeTriggerExit += this.OnRigTriggerExit;
	}

	private void OnDisable()
	{
		for (int i = this.containedRigs.Count - 1; i >= 0; i--)
		{
			this.RigDisabled(this.containedRigs[i]);
		}
		this.collisionTriggerEvents.CompositeTriggerEnter -= this.OnRigTriggerEnter;
		this.collisionTriggerEvents.CompositeTriggerExit -= this.OnRigTriggerExit;
	}

	private void OnRigTriggerEnter(Collider other)
	{
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		RigContainer rigContainer;
		if (attachedRigidbody == null || !attachedRigidbody.TryGetComponent<RigContainer>(out rigContainer) || other != rigContainer.HeadCollider || this.containedRigs.Contains(rigContainer))
		{
			return;
		}
		rigContainer.RigEvents.disableEvent += this.RigDisabled;
		this.containedRigs.Add(rigContainer);
		Action<RigContainer> action = this.playerEnteredCollection;
		if (action == null)
		{
			return;
		}
		action(rigContainer);
	}

	private void OnRigTriggerExit(Collider other)
	{
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		RigContainer rigContainer;
		if (attachedRigidbody == null || !attachedRigidbody.TryGetComponent<RigContainer>(out rigContainer) || other != rigContainer.HeadCollider || !this.containedRigs.Contains(rigContainer))
		{
			return;
		}
		rigContainer.RigEvents.disableEvent -= this.RigDisabled;
		this.containedRigs.Remove(rigContainer);
		Action<RigContainer> action = this.playerLeftCollection;
		if (action == null)
		{
			return;
		}
		action(rigContainer);
	}

	private void RigDisabled(RigContainer rig)
	{
		this.collisionTriggerEvents.ResetColliderMask(rig.HeadCollider);
		this.collisionTriggerEvents.ResetColliderMask(rig.BodyCollider);
	}

	private bool HasRig(VRRig rig)
	{
		for (int i = 0; i < this.containedRigs.Count; i++)
		{
			if (this.containedRigs[i].Rig == rig)
			{
				return true;
			}
		}
		return false;
	}

	private bool HasRig(NetPlayer player)
	{
		for (int i = 0; i < this.containedRigs.Count; i++)
		{
			if (this.containedRigs[i].Creator == player)
			{
				return true;
			}
		}
		return false;
	}

	public readonly List<RigContainer> containedRigs = new List<RigContainer>(10);

	[SerializeField]
	private CompositeTriggerEvents collisionTriggerEvents;

	public Action<RigContainer> playerEnteredCollection;

	public Action<RigContainer> playerLeftCollection;
}
