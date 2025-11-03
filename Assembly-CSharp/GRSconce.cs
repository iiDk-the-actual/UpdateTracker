using System;
using UnityEngine;

public class GRSconce : MonoBehaviour
{
	private void Awake()
	{
		if (this.tool != null)
		{
			this.tool.OnEnergyChange += this.OnEnergyChange;
		}
		if (this.gameEntity != null)
		{
			this.gameEntity.OnStateChanged += this.OnStateChange;
		}
		this.state = GRSconce.State.Off;
		this.StopLight();
	}

	private bool IsAuthority()
	{
		return this.gameEntity.IsAuthority();
	}

	private void SetState(GRSconce.State newState)
	{
		this.state = newState;
		GRSconce.State state = this.state;
		if (state != GRSconce.State.Off)
		{
			if (state == GRSconce.State.On)
			{
				this.StartLight();
			}
		}
		else
		{
			this.StopLight();
		}
		if (this.IsAuthority())
		{
			this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
		}
	}

	private void StartLight()
	{
		this.gameLight.gameObject.SetActive(true);
		this.audioSource.volume = this.lightOnSoundVolume;
		this.audioSource.clip = this.lightOnSound;
		this.audioSource.Play();
		this.meshRenderer.material = this.onMaterial;
	}

	private void StopLight()
	{
		this.gameLight.gameObject.SetActive(false);
		this.meshRenderer.material = this.offMaterial;
	}

	private void OnEnergyChange(GRTool tool, int energy, GameEntityId chargingEntityId)
	{
		if (this.IsAuthority() && this.state == GRSconce.State.Off && tool.IsEnergyFull())
		{
			this.SetState(GRSconce.State.On);
		}
	}

	private void OnStateChange(long prevState, long nextState)
	{
		if (!this.IsAuthority())
		{
			GRSconce.State state = (GRSconce.State)nextState;
			this.SetState(state);
		}
	}

	public GameEntity gameEntity;

	public GameLight gameLight;

	public GRTool tool;

	public MeshRenderer meshRenderer;

	public Material offMaterial;

	public Material onMaterial;

	public AudioSource audioSource;

	public AudioClip lightOnSound;

	public float lightOnSoundVolume;

	private GRSconce.State state;

	private enum State
	{
		Off,
		On
	}
}
