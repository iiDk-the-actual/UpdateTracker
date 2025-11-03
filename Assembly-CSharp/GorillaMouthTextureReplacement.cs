using System;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

public class GorillaMouthTextureReplacement : MonoBehaviour, ISpawnable
{
	public bool IsSpawned { get; set; }

	public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

	public void OnDespawn()
	{
	}

	public void OnSpawn(VRRig rig)
	{
		this.myRig = rig;
	}

	private void OnEnable()
	{
		this.myRig.GetComponent<GorillaMouthFlap>().SetMouthTextureReplacement(this.newMouthAtlas);
	}

	private void OnDisable()
	{
		this.myRig.GetComponent<GorillaMouthFlap>().ClearMouthTextureReplacement();
	}

	[SerializeField]
	private Texture2D newMouthAtlas;

	private VRRig myRig;
}
