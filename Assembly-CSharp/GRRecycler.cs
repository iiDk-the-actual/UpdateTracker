using System;
using UnityEngine;

public class GRRecycler : MonoBehaviourTick
{
	public override void Tick()
	{
		if (this.closed && !this.anim.isPlaying)
		{
			if (!this.playedAudio)
			{
				this.audioSource.volume = this.recyclerRunningAudioVolume;
				this.audioSource.PlayOneShot(this.recyclerRunningAudio);
				this.playedAudio = true;
			}
			this.timeRemaining -= Time.deltaTime;
			if (this.timeRemaining <= 0f)
			{
				this.anim.PlayQueued("Recycler_Open", QueueMode.CompleteOthers);
				this.closed = false;
				if (this.closeEffects != null && this.openEffects != null)
				{
					this.closeEffects.Stop();
					this.openEffects.Play();
				}
			}
		}
	}

	public void Init(GhostReactor reactor)
	{
		this.reactor = reactor;
	}

	public int GetRecycleValue(GRTool.GRToolType type)
	{
		return this.reactor.toolProgression.GetRecycleShiftCredit(type);
	}

	public void ScanItem(GRTool.GRToolType toolType)
	{
		this.scanner.ScanItem(toolType);
	}

	public void RecycleItem()
	{
		if (this.anim != null)
		{
			this.anim.Play("Recycler_Close");
		}
		if (this.closeEffects != null && this.openEffects != null)
		{
			this.openEffects.Stop();
			this.closeEffects.Play();
		}
		this.closed = true;
		this.playedAudio = false;
		this.timeRemaining = this.closeDuration;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (this.reactor == null)
		{
			return;
		}
		if (!this.reactor.grManager.IsAuthority())
		{
			return;
		}
		GRTool.GRToolType grtoolType = GRTool.GRToolType.None;
		GRTool componentInParent = other.gameObject.GetComponentInParent<GRTool>();
		if (componentInParent == null)
		{
			return;
		}
		if (other.gameObject.GetComponentInParent<GRToolClub>() != null)
		{
			grtoolType = GRTool.GRToolType.Club;
		}
		else if (other.gameObject.GetComponentInParent<GRToolCollector>() != null)
		{
			grtoolType = GRTool.GRToolType.Collector;
		}
		else if (other.gameObject.GetComponentInParent<GRToolFlash>() != null)
		{
			grtoolType = GRTool.GRToolType.Flash;
		}
		else if (other.gameObject.GetComponentInParent<GRToolLantern>() != null)
		{
			grtoolType = GRTool.GRToolType.Lantern;
		}
		else if (other.gameObject.GetComponentInParent<GRToolRevive>() != null)
		{
			grtoolType = GRTool.GRToolType.Revive;
		}
		else if (other.gameObject.GetComponentInParent<GRToolShieldGun>() != null)
		{
			grtoolType = GRTool.GRToolType.ShieldGun;
		}
		else if (other.gameObject.GetComponentInParent<GRToolDirectionalShield>() != null)
		{
			grtoolType = GRTool.GRToolType.DirectionalShield;
		}
		else if (componentInParent.toolType == GRTool.GRToolType.HockeyStick)
		{
			grtoolType = componentInParent.toolType;
		}
		else if (componentInParent.toolType == GRTool.GRToolType.DockWrist)
		{
			grtoolType = componentInParent.toolType;
		}
		int recycleValue = this.GetRecycleValue(grtoolType);
		if (this.reactor != null)
		{
			int count = this.reactor.vrRigs.Count;
			for (int i = 0; i < count; i++)
			{
				GRPlayer grplayer = GRPlayer.Get(this.reactor.vrRigs[i]);
				if (grplayer != null)
				{
					grplayer.IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat.EarnedCredits, (float)recycleValue);
				}
			}
		}
		if (GRPlayer.Get(componentInParent.gameEntity.lastHeldByActorNumber) == null)
		{
			return;
		}
		if (grtoolType != GRTool.GRToolType.None)
		{
			this.reactor.grManager.RequestRecycleItem(componentInParent.gameEntity.lastHeldByActorNumber, componentInParent.gameEntity.id, grtoolType);
		}
	}

	private GameEntity gameEntity;

	public ParticleSystem closeEffects;

	public ParticleSystem openEffects;

	[NonSerialized]
	public GhostReactor reactor;

	public GRRecyclerScanner scanner;

	public Animation anim;

	public float closeDuration = 1f;

	private float timeRemaining;

	private bool closed;

	private bool playedAudio;

	public AudioSource audioSource;

	public AudioClip recyclerRunningAudio;

	public float recyclerRunningAudioVolume = 0.5f;
}
