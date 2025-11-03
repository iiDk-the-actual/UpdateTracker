using System;
using GorillaExtensions;
using UnityEngine;

public class CosmeticCritterShadeFleeing : CosmeticCritter
{
	public override void OnSpawn()
	{
		this.spawnFX.Play();
		this.spawnAudioSource.clip = this.spawnAudioClips.GetRandomItem<AudioClip>();
		this.spawnAudioSource.GTPlay();
		this.pullVector = Vector3.zero;
	}

	public void SetFleePosition(Vector3 position, Vector3 fleeFrom)
	{
		this.origin = position;
		Vector3 vector = position - fleeFrom;
		this.fleeForward = vector.normalized;
		this.fleeRight = Vector3.Cross(this.fleeForward, Vector3.up);
		this.fleeUp = Vector3.Cross(this.fleeForward, this.fleeRight);
		this.trailingPosition = position + vector.normalized * 3f;
	}

	public override void SetRandomVariables()
	{
		float num = 0f;
		for (int i = 0; i < this.modelSwaps.Length; i++)
		{
			num += this.modelSwaps[i].relativeProbability;
			this.modelSwaps[i].gameObject.SetActive(false);
		}
		float num2 = Random.value * num;
		for (int j = 0; j < this.modelSwaps.Length; j++)
		{
			if (num2 < this.modelSwaps[j].relativeProbability)
			{
				this.modelSwaps[j].gameObject.SetActive(true);
				break;
			}
			num2 -= this.modelSwaps[j].relativeProbability;
		}
		this.fleeBobFrequencyXY = new Vector2(Random.Range(-1f, 1f) * this.fleeBobFrequencyXYMax.x, Random.Range(-1f, 1f) * this.fleeBobFrequencyXYMax.y);
		this.fleeBobMagnitudeXY = new Vector2(Random.Range(-1f, 1f) * this.fleeBobMagnitudeXYMax.x, Random.Range(-1f, 1f) * this.fleeBobMagnitudeXYMax.y);
	}

	public override void Tick()
	{
		float num = (float)base.GetAliveTime();
		Vector3 vector = this.origin + num * this.fleeForward + this.pullVector + Mathf.Sin(this.fleeBobFrequencyXY.x * num) * this.fleeBobMagnitudeXY.x * this.fleeRight + Mathf.Sin(this.fleeBobFrequencyXY.y * num) * this.fleeBobMagnitudeXY.y * this.fleeUp;
		Quaternion quaternion = Quaternion.LookRotation((vector - this.trailingPosition).normalized, Vector3.up);
		this.trailingPosition = Vector3.Lerp(this.trailingPosition, vector, 0.05f);
		base.transform.SetPositionAndRotation(vector, quaternion);
		this.animator.SetFloat(this.animatorProperty, Mathf.Sin(num * 3f) * 0.5f + 0.5f);
	}

	[Tooltip("Randomly selects one of these models when spawned, accounting for relative probabilities. For example, if one model has a probability of 1 and another a probability of 2, the second is twice as likely to be picked (and thus will be picked 67% of the time).")]
	[SerializeField]
	private CosmeticCritterShadeFleeing.ModelSwap[] modelSwaps;

	[Space]
	[Tooltip("Despawn the Shade after it has fled (fleed?) this many meters.")]
	[SerializeField]
	private float fleeDistanceToDespawn = 10f;

	[Tooltip("Flee away from the spotter at this many meters per second.")]
	[SerializeField]
	private float fleeSpeed;

	[Tooltip("The maximum strength the shade can move bob around in the horizontal and vertical axes, with final value chosen randomly.")]
	[SerializeField]
	private Vector2 fleeBobMagnitudeXYMax;

	[Tooltip("The maximum frequency the shade can move bob around in the horizontal and vertical axes, with final value chosen randomly.")]
	[SerializeField]
	private Vector2 fleeBobFrequencyXYMax;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private ParticleSystem spawnFX;

	[SerializeField]
	private AudioSource spawnAudioSource;

	[SerializeField]
	private AudioClip[] spawnAudioClips;

	[HideInInspector]
	public Vector3 pullVector;

	private Vector3 origin;

	private Vector3 fleeForward;

	private Vector3 fleeRight;

	private Vector3 fleeUp = Vector3.up;

	private Vector2 fleeBobFrequencyXY;

	private Vector2 fleeBobMagnitudeXY;

	private Vector3 trailingPosition;

	private float closestCatcherDistance;

	private int animatorProperty = Animator.StringToHash("Distance");

	[Serializable]
	private class ModelSwap
	{
		public float relativeProbability;

		public GameObject gameObject;
	}
}
