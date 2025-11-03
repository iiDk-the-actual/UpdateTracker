using System;
using GorillaLocomotion;
using UnityEngine;

internal struct OnHandTapFX : IFXEffectContext<HandEffectContext>
{
	public HandEffectContext effectContext
	{
		get
		{
			HandEffectContext handEffect = this.rig.GetHandEffect(this.isLeftHand, this.stiltID);
			this.rig.SetHandEffectData(handEffect, this.surfaceIndex, this.isDownTap, this.isLeftHand, this.stiltID, this.volume, this.speed, this.tapDir);
			return handEffect;
		}
	}

	public FXSystemSettings settings
	{
		get
		{
			return this.rig.fxSettings;
		}
	}

	public VRRig rig;

	public Vector3 tapDir;

	public bool isDownTap;

	public bool isLeftHand;

	public StiltID stiltID;

	public int surfaceIndex;

	public float volume;

	public float speed;
}
