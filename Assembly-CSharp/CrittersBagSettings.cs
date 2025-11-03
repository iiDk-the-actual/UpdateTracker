using System;
using System.Collections.Generic;
using UnityEngine;

public class CrittersBagSettings : CrittersActorSettings
{
	public override void UpdateActorSettings()
	{
		base.UpdateActorSettings();
		CrittersBag crittersBag = (CrittersBag)this.parentActor;
		crittersBag.attachableCollider = this.attachableCollider;
		crittersBag.dropCube = this.dropCube;
		crittersBag.anchorLocation = this.anchorLocation;
		crittersBag.attachDisableColliders = this.attachDisableColliders;
		crittersBag.attachSound = this.attachSound;
		crittersBag.detachSound = this.detachSound;
		crittersBag.blockAttachTypes = this.blockAttachTypes;
	}

	public Collider attachableCollider;

	public BoxCollider dropCube;

	public CrittersAttachPoint.AnchoredLocationTypes anchorLocation;

	public List<Collider> attachDisableColliders;

	public AudioClip attachSound;

	public AudioClip detachSound;

	public List<CrittersActor.CrittersActorType> blockAttachTypes;
}
