using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaTag.Reactions
{
	public class FireInstance : MonoBehaviour
	{
		protected void Awake()
		{
			FireManager.Register(this);
		}

		protected void OnDestroy()
		{
			FireManager.Unregister(this);
		}

		protected void OnEnable()
		{
			FireManager.OnEnable(this);
		}

		protected void OnDisable()
		{
			FireManager.OnDisable(this);
		}

		protected void OnTriggerEnter(Collider other)
		{
			FireManager.OnTriggerEnter(this, other);
		}

		[Header("Scene References")]
		[Tooltip("If not assigned it will try to auto assign to a component on the same GameObject.")]
		[SerializeField]
		internal Collider _collider;

		[Tooltip("If not assigned it will try to auto assign to a component on the same GameObject.")]
		[FormerlySerializedAs("_thermalSourceVolume")]
		[SerializeField]
		internal ThermalSourceVolume _thermalVolume;

		[SerializeField]
		internal ParticleSystem _particleSystem;

		[FormerlySerializedAs("_audioSource")]
		[SerializeField]
		internal AudioSource _loopingAudioSource;

		[Tooltip("The emissive color will be darkened on the materials of these renderers as the fire is extinguished.")]
		[SerializeField]
		internal Renderer[] _emissiveRenderers;

		[Header("Asset References")]
		[SerializeField]
		internal GTDirectAssetRef<AudioClip> _extinguishSound;

		[SerializeField]
		internal float _extinguishSoundVolume = 1f;

		[SerializeField]
		internal GTDirectAssetRef<AudioClip> _igniteSound;

		[SerializeField]
		internal float _igniteSoundVolume = 1f;

		[Header("Values")]
		[SerializeField]
		internal bool _despawnOnExtinguish = true;

		[SerializeField]
		internal float _maxLifetime = 10f;

		[Tooltip("How long it should take to reheat to it's default temperature.")]
		[SerializeField]
		internal float _reheatSpeed = 1f;

		[Tooltip("If you completely extinguish the object, how long should it stay extinguished?")]
		[SerializeField]
		internal float _stayExtinguishedDuration = 1f;

		internal float _defaultTemperature;

		internal float _timeSinceExtinguished;

		internal float _timeSinceDyingStart;

		internal float _timeAlive;

		internal float _psDefaultEmissionRate;

		internal ParticleSystem.EmissionModule _psEmissionModule;

		internal Vector3Int _spatialGridPosition;

		internal bool _isDespawning;

		internal float _deathStateDuration;

		internal MaterialPropertyBlock[] _emiRenderers_matPropBlocks;

		internal Color[] _emiRenderers_defaultColors;
	}
}
