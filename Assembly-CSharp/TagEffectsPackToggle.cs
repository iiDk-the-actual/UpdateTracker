using System;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using TagEffects;
using UnityEngine;

public class TagEffectsPackToggle : MonoBehaviour, ISpawnable
{
	bool ISpawnable.IsSpawned { get; set; }

	ECosmeticSelectSide ISpawnable.CosmeticSelectedSide { get; set; }

	void ISpawnable.OnSpawn(VRRig rig)
	{
		this._rig = rig;
	}

	void ISpawnable.OnDespawn()
	{
	}

	private void OnEnable()
	{
		this.Apply();
	}

	private void OnDisable()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		this.Remove();
	}

	public void Apply()
	{
		this._rig.CosmeticEffectPack = this.tagEffectPack;
	}

	public void Remove()
	{
		this._rig.CosmeticEffectPack = null;
	}

	private VRRig _rig;

	[SerializeField]
	private TagEffectPack tagEffectPack;
}
