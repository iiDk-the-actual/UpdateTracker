using System;
using GorillaTagScripts.GhostReactor;
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

	public void ScanItem(GameEntityId id)
	{
		this.scanner.ScanItem(id);
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
			Debug.LogFormat("GRRecycler reactor is null?", Array.Empty<object>());
			return;
		}
		if (!this.reactor.grManager.IsAuthority())
		{
			Debug.LogFormat("GRRecycler is not authority.", Array.Empty<object>());
			return;
		}
		GRTool componentInParent = other.gameObject.GetComponentInParent<GRTool>();
		if (componentInParent == null)
		{
			Debug.LogFormat("GRRecycler Colliding Object is not a GRTool.", Array.Empty<object>());
			return;
		}
		GRTool.GRToolType toolType = other.gameObject.GetToolType();
		int recycleValue = this.GetRecycleValue(toolType);
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
		Debug.LogFormat("GRRecycler Recycle Value is {0}", new object[] { recycleValue });
		if (GRPlayer.Get(componentInParent.gameEntity.lastHeldByActorNumber) == null)
		{
			Debug.LogFormat("GRRecycler Tool Not last held by a player (?), can't recycle.", Array.Empty<object>());
			return;
		}
		Debug.LogFormat("GRRecycler Refunding player {0} {1} Currency and Destroying Tool.", new object[]
		{
			componentInParent.gameEntity.lastHeldByActorNumber,
			recycleValue
		});
		if (toolType != GRTool.GRToolType.None)
		{
			this.reactor.grManager.RequestRecycleItem(componentInParent.gameEntity.lastHeldByActorNumber, componentInParent.gameEntity.id, toolType);
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
