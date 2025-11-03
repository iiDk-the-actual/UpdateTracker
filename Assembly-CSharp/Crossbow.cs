using System;
using UnityEngine;

public class Crossbow : ProjectileWeapon
{
	protected override void Awake()
	{
		base.Awake();
		TransferrableObjectHoldablePart_Crank[] array = this.cranks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetOnCrankedCallback(new Action<float>(this.OnCrank));
		}
		this.SetReloadFraction(0f);
	}

	public void SetReloadFraction(float newFraction)
	{
		this.loadFraction = Mathf.Clamp01(newFraction);
		this.animator.SetFloat(this.ReloadFractionHashID, this.loadFraction);
		if (this.loadFraction == 1f && !this.dummyProjectile.enabled)
		{
			this.shootSfx.GTPlayOneShot(this.reloadComplete_audioClip, 1f);
			this.dummyProjectile.enabled = true;
			return;
		}
		if (this.loadFraction < 1f && this.dummyProjectile.enabled)
		{
			this.dummyProjectile.enabled = false;
		}
	}

	private void OnCrank(float degrees)
	{
		if (this.loadFraction == 1f)
		{
			return;
		}
		this.totalCrankDegrees += degrees;
		this.crankSoundDegrees += degrees;
		if (Mathf.Abs(this.crankSoundDegrees) > this.crankSoundDegreesThreshold)
		{
			this.playingCrankSoundUntilTimestamp = Time.time + this.crankSoundContinueDuration;
			this.crankSoundDegrees = 0f;
		}
		if (!this.reloadAudio.isPlaying && Time.time < this.playingCrankSoundUntilTimestamp)
		{
			this.reloadAudio.GTPlay();
		}
		this.SetReloadFraction(Mathf.Abs(this.totalCrankDegrees / this.crankTotalDegreesToReload));
		if (this.loadFraction >= 1f)
		{
			this.totalCrankDegrees = 0f;
		}
	}

	protected override Vector3 GetLaunchPosition()
	{
		return this.launchPosition.position;
	}

	protected override Vector3 GetLaunchVelocity()
	{
		return this.launchPosition.forward * this.launchSpeed * base.myRig.scaleFactor;
	}

	protected override void LateUpdateLocal()
	{
		base.LateUpdateLocal();
		if (!base.InHand())
		{
			this.wasPressingTrigger = false;
			return;
		}
		if ((base.InLeftHand() ? base.myRig.leftIndex.calcT : base.myRig.rightIndex.calcT) > 0.5f)
		{
			if (this.loadFraction == 1f && !this.wasPressingTrigger)
			{
				this.SetReloadFraction(0f);
				this.animator.SetTrigger(this.FireHashID);
				base.LaunchProjectile();
			}
			this.wasPressingTrigger = true;
		}
		else
		{
			this.wasPressingTrigger = false;
		}
		if (this.itemState.HasFlag(TransferrableObject.ItemStates.State0))
		{
			if (this.loadFraction < 1f)
			{
				this.itemState &= (TransferrableObject.ItemStates)(-2);
				return;
			}
		}
		else if (this.loadFraction == 1f)
		{
			this.itemState |= TransferrableObject.ItemStates.State0;
		}
	}

	protected override void LateUpdateReplicated()
	{
		base.LateUpdateReplicated();
		if (!base.InHand())
		{
			return;
		}
		if (this.itemState.HasFlag(TransferrableObject.ItemStates.State0))
		{
			this.SetReloadFraction(1f);
			return;
		}
		if (this.loadFraction == 1f)
		{
			this.SetReloadFraction(0f);
		}
	}

	protected override void LateUpdateShared()
	{
		base.LateUpdateShared();
		if (this.reloadAudio.isPlaying && Time.time > this.playingCrankSoundUntilTimestamp)
		{
			this.reloadAudio.GTStop();
		}
	}

	[SerializeField]
	private Transform launchPosition;

	[SerializeField]
	private float launchSpeed;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private float crankTotalDegreesToReload;

	[SerializeField]
	private TransferrableObjectHoldablePart_Crank[] cranks;

	[SerializeField]
	private MeshRenderer dummyProjectile;

	[SerializeField]
	private AudioSource reloadAudio;

	[SerializeField]
	private AudioClip reloadComplete_audioClip;

	[SerializeField]
	private float crankSoundContinueDuration = 0.1f;

	[SerializeField]
	private float crankSoundDegreesThreshold = 0.1f;

	private AnimHashId FireHashID = "Fire";

	private AnimHashId ReloadFractionHashID = "ReloadFraction";

	private float totalCrankDegrees;

	private float loadFraction;

	private float playingCrankSoundUntilTimestamp;

	private float crankSoundDegrees;

	private bool wasPressingTrigger;
}
