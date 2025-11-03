using System;
using UnityEngine;
using UnityEngine.Serialization;

[Obsolete("replaced with ThrowableSetDressing.cs")]
public class MagicIngredient : TransferrableObject
{
	public override void OnSpawn(VRRig rig)
	{
		base.OnSpawn(rig);
		this.item = this.worldShareableInstance;
		this.grabPtInitParent = this.anchor.transform.parent;
	}

	private void ReParent()
	{
		Transform transform = this.anchor.transform;
		base.gameObject.transform.parent = transform;
		transform.parent = this.grabPtInitParent;
	}

	public void Disable()
	{
		this.DropItem();
		base.OnDisable();
		if (this.item)
		{
			this.item.OnDisable();
		}
		base.gameObject.SetActive(false);
	}

	[FormerlySerializedAs("IngredientType")]
	public MagicIngredientType IngredientTypeSO;

	public Transform rootParent;

	private WorldShareableItem item;

	private Transform grabPtInitParent;
}
