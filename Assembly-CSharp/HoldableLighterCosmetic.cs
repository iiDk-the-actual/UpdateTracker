using System;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class HoldableLighterCosmetic : MonoBehaviour
{
	private void OnEnable()
	{
	}

	private void Awake()
	{
		this.rig = base.GetComponentInParent<VRRig>();
		this.parentTransferable = base.GetComponentInParent<TransferrableObject>();
	}

	private bool IsMyItem()
	{
		return this.rig != null && this.rig.isOfflineVRRig;
	}

	private void DebugPull()
	{
		this.TriggerPulled();
	}

	private void DebugRelease()
	{
		this.TriggerReleased();
	}

	public void TriggerPulled()
	{
		this.triggerHeld = true;
		if (this.OwnerID == 0)
		{
			this.TrySetID();
		}
		double time = PhotonNetwork.Time;
		switch (this.GetResultAtTime(time, this.OwnerID))
		{
		case HoldableLighterCosmetic.LighterResult.Flicker:
		{
			UnityEvent onFlicker = this.OnFlicker;
			if (onFlicker != null)
			{
				onFlicker.Invoke();
			}
			if (this.parentTransferable.IsMyItem())
			{
				GorillaTagger.Instance.StartVibration(this.parentTransferable.InLeftHand(), 0.1f, 0.1f);
				return;
			}
			break;
		}
		case HoldableLighterCosmetic.LighterResult.Light:
		{
			UnityEvent onLight = this.OnLight;
			if (onLight != null)
			{
				onLight.Invoke();
			}
			if (this.parentTransferable.IsMyItem())
			{
				GorillaTagger.Instance.StartVibration(this.parentTransferable.InLeftHand(), 0.1f, 0.1f);
				return;
			}
			break;
		}
		case HoldableLighterCosmetic.LighterResult.Explode:
		{
			UnityEvent onExplode = this.OnExplode;
			if (onExplode != null)
			{
				onExplode.Invoke();
			}
			if (this.parentTransferable.IsMyItem())
			{
				GorillaTagger.Instance.StartVibration(this.parentTransferable.InLeftHand(), 0.75f, 0.5f);
			}
			break;
		}
		default:
			return;
		}
	}

	private HoldableLighterCosmetic.LighterResult GetResultAtTime(double photonTime, int seed)
	{
		int num = (int)Math.Floor(photonTime);
		float num2 = (float)new Random(seed ^ num).NextDouble();
		if (num2 < this.explodeWeight)
		{
			return HoldableLighterCosmetic.LighterResult.Explode;
		}
		if (num2 < this.explodeWeight + this.lightWeight)
		{
			return HoldableLighterCosmetic.LighterResult.Light;
		}
		return HoldableLighterCosmetic.LighterResult.Flicker;
	}

	public void TriggerReleased()
	{
		this.triggerHeld = false;
		UnityEvent onTriggerRelease = this.OnTriggerRelease;
		if (onTriggerRelease == null)
		{
			return;
		}
		onTriggerRelease.Invoke();
	}

	private void TrySetID()
	{
		if (this.parentTransferable.IsLocalObject())
		{
			PlayFabAuthenticator instance = PlayFabAuthenticator.instance;
			if (instance != null)
			{
				string playFabPlayerId = instance.GetPlayFabPlayerId();
				Type type = base.GetType();
				this.OwnerID = (playFabPlayerId + ((type != null) ? type.ToString() : null)).GetStaticHash();
				return;
			}
		}
		else if (this.parentTransferable.targetRig != null && this.parentTransferable.targetRig.creator != null)
		{
			string userId = this.parentTransferable.targetRig.creator.UserId;
			Type type2 = base.GetType();
			this.OwnerID = (userId + ((type2 != null) ? type2.ToString() : null)).GetStaticHash();
		}
	}

	private int OwnerID;

	[Header("Weights (0 to 1 total)")]
	[Range(0f, 1f)]
	public float flickerWeight = 0.5f;

	[Range(0f, 1f)]
	public float lightWeight = 0.3f;

	[Range(0f, 1f)]
	public float explodeWeight = 0.2f;

	[Header("Unity Events")]
	public UnityEvent OnFlicker;

	public UnityEvent OnLight;

	public UnityEvent OnExplode;

	public UnityEvent OnTriggerRelease;

	private HoldableLighterCosmetic.LighterResult[] resultTimeline;

	private bool triggerHeld;

	private float lastCheckTime;

	private VRRig rig;

	private TransferrableObject parentTransferable;

	public enum LighterResult
	{
		Flicker,
		Light,
		Explode
	}
}
