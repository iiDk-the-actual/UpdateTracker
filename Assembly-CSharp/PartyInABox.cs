using System;
using UnityEngine;

public class PartyInABox : MonoBehaviour
{
	private void Awake()
	{
		this.Reset();
	}

	private void OnEnable()
	{
		this.Reset();
	}

	public void Cranked_ReleaseParty()
	{
		if (!this.parentHoldable.IsLocalObject())
		{
			return;
		}
		this.ReleaseParty();
	}

	public void ReleaseParty()
	{
		if (this.isReleased)
		{
			return;
		}
		if (this.parentHoldable.IsLocalObject())
		{
			this.parentHoldable.itemState |= TransferrableObject.ItemStates.State0;
			GorillaTagger.Instance.StartVibration(true, this.partyHapticStrength, this.partyHapticDuration);
			GorillaTagger.Instance.StartVibration(false, this.partyHapticStrength, this.partyHapticDuration);
		}
		this.isReleased = true;
		this.spring.enabled = true;
		this.anim.Play();
		this.particles.Play();
		this.partyAudio.Play();
	}

	private void Update()
	{
		if (this.parentHoldable.IsLocalObject())
		{
			return;
		}
		if (this.parentHoldable.itemState.HasFlag(TransferrableObject.ItemStates.State0))
		{
			if (!this.isReleased)
			{
				this.ReleaseParty();
				return;
			}
		}
		else if (this.isReleased)
		{
			this.Reset();
		}
	}

	public void Reset()
	{
		this.isReleased = false;
		this.parentHoldable.itemState &= (TransferrableObject.ItemStates)(-2);
		this.spring.enabled = false;
		this.anim.Stop();
		foreach (PartyInABox.ForceTransform forceTransform in this.forceTransforms)
		{
			forceTransform.Apply();
		}
	}

	[SerializeField]
	private TransferrableObject parentHoldable;

	[SerializeField]
	private ParticleSystem particles;

	[SerializeField]
	private Animation anim;

	[SerializeField]
	private SpringyWobbler spring;

	[SerializeField]
	private AudioSource partyAudio;

	[SerializeField]
	private float partyHapticStrength;

	[SerializeField]
	private float partyHapticDuration;

	private bool isReleased;

	[SerializeField]
	private PartyInABox.ForceTransform[] forceTransforms;

	[Serializable]
	private struct ForceTransform
	{
		public void Apply()
		{
			this.transform.localPosition = this.localPosition;
			this.transform.localRotation = this.localRotation;
		}

		public Transform transform;

		public Vector3 localPosition;

		public Quaternion localRotation;
	}
}
