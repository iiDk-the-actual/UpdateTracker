using System;
using UnityEngine;

public class TrickTreatItem : RandomComponent<MeshRenderer>
{
	protected override void OnNextItem(MeshRenderer item)
	{
		for (int i = 0; i < this.items.Length; i++)
		{
			MeshRenderer meshRenderer = this.items[i];
			meshRenderer.enabled = meshRenderer == item;
		}
	}

	public void Randomize()
	{
		this.NextItem();
	}
}
