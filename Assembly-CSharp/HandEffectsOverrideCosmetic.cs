using System;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

public class HandEffectsOverrideCosmetic : MonoBehaviour, ISpawnable
{
	public bool IsSpawned { get; set; }

	public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

	public void OnSpawn(VRRig rig)
	{
		this._rig = rig;
	}

	public void OnDespawn()
	{
	}

	public void OnEnable()
	{
		if (!this.isLeftHand)
		{
			this._rig.CosmeticHandEffectsOverride_Right.Add(this);
			return;
		}
		this._rig.CosmeticHandEffectsOverride_Left.Add(this);
	}

	public void OnDisable()
	{
		if (!this.isLeftHand)
		{
			this._rig.CosmeticHandEffectsOverride_Right.Remove(this);
			return;
		}
		this._rig.CosmeticHandEffectsOverride_Left.Remove(this);
	}

	public HandEffectsOverrideCosmetic.HandEffectType handEffectType;

	public bool isLeftHand;

	public HandEffectsOverrideCosmetic.EffectsOverride firstPerson;

	public HandEffectsOverrideCosmetic.EffectsOverride thirdPerson;

	private VRRig _rig;

	[Serializable]
	public class EffectsOverride
	{
		public GameObject effectVFX;

		public bool playHaptics;

		public float hapticStrength = 0.5f;

		public float hapticDuration = 0.5f;

		public bool parentEffect;
	}

	public enum HandEffectType
	{
		None,
		FistBump,
		HighFive
	}
}
