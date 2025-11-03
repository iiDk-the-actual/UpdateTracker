using System;
using UnityEngine;

[RequireComponent(typeof(PlayerColoredCosmetic))]
public class FXModifierPlayerColorSetter : FXModifier
{
	public override void UpdateScale(float scale, Color color)
	{
		this.playerColoredCosmetic.UpdateColor(color);
	}

	[SerializeField]
	private PlayerColoredCosmetic playerColoredCosmetic;
}
