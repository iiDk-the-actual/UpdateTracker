using System;
using UnityEngine;

public class VoicePitchShiftCosmetic : MonoBehaviour
{
	public float Pitch
	{
		get
		{
			return this.pitch;
		}
		set
		{
			value = Mathf.Clamp(value, 0.6666667f, 1.5f);
			if (this.myRig == null)
			{
				this.pitch = value;
				return;
			}
			if (!Mathf.Approximately(value, this.pitch))
			{
				this.pitch = value;
				this.myRig.SetPitchShiftCosmeticsDirty();
			}
		}
	}

	private void OnEnable()
	{
		if (this.myRig == null)
		{
			this.myRig = base.GetComponentInParent<VRRig>();
		}
		if (this.myRig != null)
		{
			this.myRig.PitchShiftCosmetics.Add(this);
			this.myRig.SetPitchShiftCosmeticsDirty();
		}
	}

	private void OnDisable()
	{
		if (this.myRig != null)
		{
			this.myRig.PitchShiftCosmetics.Remove(this);
			this.myRig.SetPitchShiftCosmeticsDirty();
		}
	}

	private const float MIN = 0.6666667f;

	private const float MAX = 1.5f;

	[Tooltip("If multiple cosmetics are equipped that modify the pitch, their values will be averaged. Has a minimum pitch of 2/3 and a maximum of 1.5.")]
	[Range(0.6666667f, 1.5f)]
	[SerializeField]
	private float pitch = 1f;

	private VRRig myRig;
}
