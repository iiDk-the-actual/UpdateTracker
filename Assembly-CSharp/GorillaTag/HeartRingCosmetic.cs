using System;
using GorillaExtensions;
using UnityEngine;

namespace GorillaTag
{
	[DefaultExecutionOrder(1250)]
	public class HeartRingCosmetic : MonoBehaviour
	{
		protected void Awake()
		{
			Application.quitting += delegate
			{
				base.enabled = false;
			};
		}

		protected void OnEnable()
		{
			this.particleSystem = this.effects.GetComponentInChildren<ParticleSystem>(true);
			this.audioSource = this.effects.GetComponentInChildren<AudioSource>(true);
			this.ownerRig = base.GetComponentInParent<VRRig>();
			bool flag = this.ownerRig != null && this.ownerRig.head != null && this.ownerRig.head.rigTarget != null;
			base.enabled = flag;
			this.effects.SetActive(flag);
			if (!flag)
			{
				Debug.LogError("Disabling HeartRingCosmetic. Could not find owner head. Scene path: " + base.transform.GetPath(), this);
				return;
			}
			this.ownerHead = ((this.ownerRig != null) ? this.ownerRig.head.rigTarget.transform : base.transform);
			this.maxEmissionRate = this.particleSystem.emission.rateOverTime.constant;
			this.maxVolume = this.audioSource.volume;
		}

		protected void LateUpdate()
		{
			Transform transform = base.transform;
			Vector3 position = transform.position;
			float x = transform.lossyScale.x;
			float num = this.effectActivationRadius * this.effectActivationRadius * x * x;
			bool flag = (this.ownerHead.TransformPoint(this.headToMouthOffset) - position).sqrMagnitude < num;
			ParticleSystem.EmissionModule emission = this.particleSystem.emission;
			emission.rateOverTime = Mathf.Lerp(emission.rateOverTime.constant, flag ? this.maxEmissionRate : 0f, Time.deltaTime / 0.1f);
			this.audioSource.volume = Mathf.Lerp(this.audioSource.volume, flag ? this.maxVolume : 0f, Time.deltaTime / 2f);
			this.ownerRig.UsingHauntedRing = this.isHauntedVoiceChanger && flag;
			if (this.ownerRig.UsingHauntedRing)
			{
				this.ownerRig.HauntedRingVoicePitch = this.hauntedVoicePitch;
			}
		}

		public GameObject effects;

		[SerializeField]
		private bool isHauntedVoiceChanger;

		[SerializeField]
		private float hauntedVoicePitch = 0.75f;

		[AssignInCorePrefab]
		public float effectActivationRadius = 0.15f;

		private readonly Vector3 headToMouthOffset = new Vector3(0f, 0.0208f, 0.171f);

		private VRRig ownerRig;

		private Transform ownerHead;

		private ParticleSystem particleSystem;

		private AudioSource audioSource;

		private float maxEmissionRate;

		private float maxVolume;

		private const float emissionFadeTime = 0.1f;

		private const float volumeFadeTime = 2f;
	}
}
