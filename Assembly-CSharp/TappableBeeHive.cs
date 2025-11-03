using System;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

public class TappableBeeHive : Tappable
{
	private void Awake()
	{
		if (this.swarmEmergeFromPoint == null || this.swarmEmergeToPoint == null)
		{
			Debug.LogError("TappableBeeHive: Disabling because swarmEmergePoint is null at: " + base.transform.GetPath(), this);
			base.enabled = false;
			return;
		}
		base.GetComponent<SlingshotProjectileHitNotifier>().OnProjectileHit += this.OnSlingshotHit;
	}

	public override void OnTapLocal(float tapStrength, float tapTime, PhotonMessageInfoWrapped info)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (this.swarmEmergeFromPoint == null || this.swarmEmergeToPoint == null)
		{
			return;
		}
		if (NetworkSystem.Instance.IsMasterClient && AngryBeeSwarm.instance.isDormant)
		{
			AngryBeeSwarm.instance.Emerge(this.swarmEmergeFromPoint.transform.position, this.swarmEmergeToPoint.transform.position);
		}
	}

	public void OnSlingshotHit(SlingshotProjectile projectile, Collision collision)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (this.swarmEmergeFromPoint == null || this.swarmEmergeToPoint == null)
		{
			return;
		}
		if (PhotonNetwork.IsMasterClient && AngryBeeSwarm.instance.isDormant)
		{
			AngryBeeSwarm.instance.Emerge(this.swarmEmergeFromPoint.transform.position, this.swarmEmergeToPoint.transform.position);
		}
	}

	[SerializeField]
	private GameObject swarmEmergeFromPoint;

	[SerializeField]
	private GameObject swarmEmergeToPoint;

	[SerializeField]
	private GameObject honeycombSurface;

	[SerializeField]
	private float honeycombDisableDuration;

	[NonSerialized]
	private TimeSince _timeSinceLastTap;

	private float reenableHoneycombAtTimestamp;

	private Coroutine reenableHoneycombCoroutine;
}
