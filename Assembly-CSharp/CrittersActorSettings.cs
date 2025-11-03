using System;
using UnityEngine;

public class CrittersActorSettings : MonoBehaviour
{
	public virtual void OnEnable()
	{
		this.UpdateActorSettings();
	}

	public virtual void UpdateActorSettings()
	{
		this.parentActor.usesRB = this.usesRB;
		this.parentActor.rb.isKinematic = !this.usesRB;
		this.parentActor.equipmentStorable = this.canBeStored;
		this.parentActor.storeCollider = this.storeCollider;
		this.parentActor.equipmentStoreTriggerCollider = this.equipmentStoreTriggerCollider;
	}

	public CrittersActor parentActor;

	public bool usesRB;

	public bool canBeStored;

	public CapsuleCollider storeCollider;

	public CapsuleCollider equipmentStoreTriggerCollider;
}
