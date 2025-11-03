using System;
using UnityEngine;

public class HoldableHandle : InteractionPoint
{
	public new HoldableObject Holdable
	{
		get
		{
			return this.holdable;
		}
	}

	public CapsuleCollider Capsule
	{
		get
		{
			return this.handleCapsuleTrigger;
		}
	}

	[SerializeField]
	private HoldableObject holdable;

	[SerializeField]
	private CapsuleCollider handleCapsuleTrigger;
}
