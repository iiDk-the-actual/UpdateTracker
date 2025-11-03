using System;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

public class GorillaFaceTextureReplacement : MonoBehaviour, ISpawnable
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
		this.myRig.GetComponent<GorillaMouthFlap>().SetFaceMaterialReplacement(this.newFaceMaterial);
		Material material = this.myRig.GetComponent<GorillaMouthFlap>().SetFaceMaterialReplacement(this.newFaceMaterial);
		MeshRenderer[] array = this.alsoApplyFaceTo;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sharedMaterial = material;
		}
	}

	private void OnDisable()
	{
		this.myRig.GetComponent<GorillaMouthFlap>().ClearFaceMaterialReplacement();
	}

	[SerializeField]
	private Material newFaceMaterial;

	private VRRig myRig;

	[SerializeField]
	private MeshRenderer[] alsoApplyFaceTo;
}
