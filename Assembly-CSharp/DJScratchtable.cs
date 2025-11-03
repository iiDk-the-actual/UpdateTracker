using System;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

public class DJScratchtable : MonoBehaviour
{
	public void SetPlaying(bool playing)
	{
		this.isPlaying = playing;
	}

	private void OnTriggerStay(Collider collider)
	{
		if (!base.enabled)
		{
			return;
		}
		GorillaTriggerColliderHandIndicator componentInParent = collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
		if (componentInParent == null)
		{
			return;
		}
		Vector3 vector = (base.transform.parent.InverseTransformPoint(collider.transform.position) - base.transform.localPosition).WithY(0f);
		float num = Mathf.Atan2(vector.z, vector.x) * 57.29578f;
		if (this.isTouching)
		{
			base.transform.localRotation = Quaternion.LookRotation(vector) * this.firstTouchRotation;
			if (this.isPlaying)
			{
				float num2 = Mathf.DeltaAngle(this.lastScratchSoundAngle, num);
				if (num2 > this.scratchMinAngle)
				{
					if (Time.time > this.cantForwardScratchUntilTimestamp)
					{
						this.scratchPlayer.Play(ScratchSoundType.Forward, this.isLeft);
						this.cantForwardScratchUntilTimestamp = Time.time + this.scratchCooldown;
						this.lastScratchSoundAngle = num;
						GorillaTagger.Instance.StartVibration(componentInParent.isLeftHand, this.hapticStrength, this.hapticDuration);
					}
				}
				else if (num2 < -this.scratchMinAngle && Time.time > this.cantBackScratchUntilTimestamp)
				{
					this.scratchPlayer.Play(ScratchSoundType.Back, this.isLeft);
					this.cantBackScratchUntilTimestamp = Time.time + this.scratchCooldown;
					this.lastScratchSoundAngle = num;
					GorillaTagger.Instance.StartVibration(componentInParent.isLeftHand, this.hapticStrength, this.hapticDuration);
				}
			}
		}
		else
		{
			this.firstTouchRotation = Quaternion.Inverse(Quaternion.LookRotation(base.transform.InverseTransformPoint(collider.transform.position).WithY(0f)));
			if (this.isPlaying)
			{
				this.PauseTrack();
				this.scratchPlayer.Play(ScratchSoundType.Pause, this.isLeft);
				this.lastScratchSoundAngle = num;
				this.cantForwardScratchUntilTimestamp = Time.time + this.scratchCooldown;
				this.cantBackScratchUntilTimestamp = Time.time + this.scratchCooldown;
			}
		}
		this.isTouching = true;
	}

	private void OnTriggerExit(Collider collider)
	{
		if (!base.enabled)
		{
			return;
		}
		if (collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() == null)
		{
			return;
		}
		if (this.isPlaying)
		{
			this.ResumeTrack();
			this.scratchPlayer.Play(ScratchSoundType.Resume, this.isLeft);
		}
		this.isTouching = false;
	}

	public void SelectTrack(int track)
	{
		this.lastSelectedTrack = track;
		if (track == 0)
		{
			this.turntableVisual.Stop();
			this.isPlaying = false;
		}
		else
		{
			this.turntableVisual.Run();
			this.isPlaying = true;
		}
		int num = track - 1;
		for (int i = 0; i < this.tracks.Length; i++)
		{
			if (num == i)
			{
				float num2 = (float)(PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time)) % this.trackDuration;
				this.tracks[i].Play();
				this.tracks[i].time = num2;
			}
			else
			{
				this.tracks[i].Stop();
			}
		}
	}

	public void PauseTrack()
	{
		for (int i = 0; i < this.tracks.Length; i++)
		{
			this.tracks[i].Stop();
		}
		this.pausedUntilTimestamp = Time.time + 1f;
	}

	public void ResumeTrack()
	{
		this.SelectTrack(this.lastSelectedTrack);
		this.pausedUntilTimestamp = 0f;
	}

	[SerializeField]
	private bool isLeft;

	[SerializeField]
	private DJScratchSoundPlayer scratchPlayer;

	[SerializeField]
	private float scratchCooldown;

	[SerializeField]
	private float scratchMinAngle;

	[SerializeField]
	private AudioSource[] tracks;

	[SerializeField]
	private CosmeticFan turntableVisual;

	[SerializeField]
	private float trackDuration;

	[SerializeField]
	private float hapticStrength;

	[SerializeField]
	private float hapticDuration;

	private int lastSelectedTrack;

	private bool isPlaying;

	private bool isTouching;

	private Quaternion firstTouchRotation;

	private float lastScratchSoundAngle;

	private float cantForwardScratchUntilTimestamp;

	private float cantBackScratchUntilTimestamp;

	private float pausedUntilTimestamp;
}
