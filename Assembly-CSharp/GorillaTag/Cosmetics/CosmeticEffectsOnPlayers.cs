using System;
using System.Collections.Generic;
using System.Linq;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaTag.CosmeticSystem;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaTag.Cosmetics
{
	public class CosmeticEffectsOnPlayers : MonoBehaviour, ISpawnable
	{
		private bool ShouldAffectRig(VRRig rig, CosmeticEffectsOnPlayers.TargetType target)
		{
			bool flag = rig == this.myRig;
			bool flag2;
			switch (target)
			{
			case CosmeticEffectsOnPlayers.TargetType.Owner:
				flag2 = flag;
				break;
			case CosmeticEffectsOnPlayers.TargetType.Others:
				flag2 = !flag;
				break;
			case CosmeticEffectsOnPlayers.TargetType.All:
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			return flag2;
		}

		private void Awake()
		{
			foreach (CosmeticEffectsOnPlayers.CosmeticEffect cosmeticEffect in this.allEffects)
			{
				this.allEffectsDict.TryAdd(cosmeticEffect.effectType, cosmeticEffect);
			}
		}

		public void SetKnockbackStrengthMultiplier(float value)
		{
			foreach (KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> keyValuePair in this.allEffectsDict)
			{
				keyValuePair.Value.knockbackStrengthMultiplier = value;
			}
		}

		public void ApplyAllEffects()
		{
			this.ApplyAllEffectsByDistance(base.transform.position);
		}

		public void ApplyAllEffectsByDistance(Transform _transform)
		{
			this.ApplyAllEffectsByDistance(_transform.position);
		}

		public void ApplyAllEffectsByDistance(Vector3 position)
		{
			foreach (KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> keyValuePair in this.allEffectsDict)
			{
				switch (keyValuePair.Key)
				{
				case CosmeticEffectsOnPlayers.EFFECTTYPE.Skin:
					this.ApplySkinByDistance(keyValuePair, position);
					break;
				case CosmeticEffectsOnPlayers.EFFECTTYPE.TagWithKnockback:
					this.ApplyTagWithKnockbackByDistance(keyValuePair, position);
					break;
				case CosmeticEffectsOnPlayers.EFFECTTYPE.InstantKnockback:
					this.ApplyInstantKnockbackByDistance(keyValuePair, position);
					break;
				case CosmeticEffectsOnPlayers.EFFECTTYPE.SFX:
					this.PlaySfxByDistance(keyValuePair, position);
					break;
				case CosmeticEffectsOnPlayers.EFFECTTYPE.VFX:
					this.PlayVFXByDistance(keyValuePair, position);
					break;
				}
			}
		}

		public void ApplyAllEffectsForRig(VRRig rig)
		{
			foreach (KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> keyValuePair in this.allEffectsDict)
			{
				switch (keyValuePair.Key)
				{
				case CosmeticEffectsOnPlayers.EFFECTTYPE.Skin:
					this.ApplySkinForRig(keyValuePair, rig);
					break;
				case CosmeticEffectsOnPlayers.EFFECTTYPE.TagWithKnockback:
					this.ApplyTagWithKnockbackForRig(keyValuePair, rig);
					break;
				case CosmeticEffectsOnPlayers.EFFECTTYPE.InstantKnockback:
					this.ApplyInstantKnockbackForRig(keyValuePair, rig);
					break;
				case CosmeticEffectsOnPlayers.EFFECTTYPE.VoiceOverride:
					this.ApplyVOForRig(keyValuePair, rig);
					break;
				case CosmeticEffectsOnPlayers.EFFECTTYPE.SFX:
					this.PlaySfxForRig(keyValuePair, rig);
					break;
				case CosmeticEffectsOnPlayers.EFFECTTYPE.VFX:
					this.PlayVFXForRig(keyValuePair, rig);
					break;
				}
			}
		}

		private void ApplySkinByDistance(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect, Vector3 position)
		{
			if (!effect.Value.IsGameModeAllowed())
			{
				return;
			}
			effect.Value.EffectStartedTime = Time.time;
			IEnumerable<VRRig> enumerable2;
			if (!PhotonNetwork.InRoom)
			{
				IEnumerable<VRRig> enumerable = new VRRig[] { GorillaTagger.Instance.offlineVRRig };
				enumerable2 = enumerable;
			}
			else
			{
				IEnumerable<VRRig> enumerable = GorillaParent.instance.vrrigs;
				enumerable2 = enumerable;
			}
			foreach (VRRig vrrig in enumerable2)
			{
				if (this.ShouldAffectRig(vrrig, effect.Value.target) && (vrrig.transform.position - position).IsShorterThan(effect.Value.effectDistanceRadius))
				{
					if (vrrig == this.myRig)
					{
						effect.Value.EffectDuration = effect.Value.effectDurationOwner;
					}
					vrrig.SpawnSkinEffects(effect);
				}
			}
		}

		private void ApplySkinForRig(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect, VRRig vrRig)
		{
			if (!effect.Value.IsGameModeAllowed() || !this.ShouldAffectRig(vrRig, effect.Value.target))
			{
				return;
			}
			effect.Value.EffectStartedTime = Time.time;
			if (vrRig == this.myRig)
			{
				effect.Value.EffectDuration = effect.Value.effectDurationOwner;
			}
			vrRig.SpawnSkinEffects(effect);
		}

		private void ApplyTagWithKnockbackForRig(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect, VRRig vrRig)
		{
			if (!effect.Value.IsGameModeAllowed() || !this.ShouldAffectRig(vrRig, effect.Value.target))
			{
				return;
			}
			effect.Value.EffectStartedTime = Time.time;
			if (vrRig == this.myRig)
			{
				effect.Value.EffectDuration = effect.Value.effectDurationOwner;
			}
			vrRig.EnableHitWithKnockBack(effect);
		}

		private void ApplyTagWithKnockbackByDistance(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect, Vector3 position)
		{
			if (!effect.Value.IsGameModeAllowed())
			{
				return;
			}
			effect.Value.EffectStartedTime = Time.time;
			IEnumerable<VRRig> enumerable2;
			if (!PhotonNetwork.InRoom)
			{
				IEnumerable<VRRig> enumerable = new VRRig[] { GorillaTagger.Instance.offlineVRRig };
				enumerable2 = enumerable;
			}
			else
			{
				IEnumerable<VRRig> enumerable = GorillaParent.instance.vrrigs;
				enumerable2 = enumerable;
			}
			foreach (VRRig vrrig in enumerable2)
			{
				if (this.ShouldAffectRig(vrrig, effect.Value.target) && (vrrig.transform.position - position).IsShorterThan(effect.Value.effectDistanceRadius))
				{
					if (vrrig == this.myRig)
					{
						effect.Value.EffectDuration = effect.Value.effectDurationOwner;
					}
					vrrig.EnableHitWithKnockBack(effect);
				}
			}
		}

		private void ApplyInstantKnockbackForRig(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect, VRRig vrRig)
		{
			if (!effect.Value.IsGameModeAllowed() || !this.ShouldAffectRig(vrRig, effect.Value.target))
			{
				return;
			}
			effect.Value.EffectStartedTime = Time.time;
			if (vrRig == this.myRig)
			{
				effect.Value.EffectDuration = effect.Value.effectDurationOwner;
			}
			Vector3 vector = vrRig.transform.position - base.transform.position;
			float num = (1f / vector.magnitude * effect.Value.knockbackStrength * effect.Value.knockbackStrengthMultiplier).ClampSafe(effect.Value.minKnockbackStrength, effect.Value.maxKnockbackStrength);
			if (effect.Value.applyScaleToKnockbackStrength)
			{
				num *= vrRig.scaleFactor;
			}
			RoomSystem.HitPlayer(vrRig.creator, vector.normalized, num);
			vrRig.ApplyInstanceKnockBack(effect);
		}

		private void ApplyInstantKnockbackByDistance(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect, Vector3 position)
		{
			if (!effect.Value.IsGameModeAllowed() || !this.ShouldAffectRig(GorillaTagger.Instance.offlineVRRig, effect.Value.target))
			{
				return;
			}
			effect.Value.EffectStartedTime = Time.time;
			if (GorillaTagger.Instance.offlineVRRig == this.myRig)
			{
				effect.Value.EffectDuration = effect.Value.effectDurationOwner;
			}
			Vector3 vector = GorillaTagger.Instance.offlineVRRig.transform.position - position;
			if (vector.IsShorterThan(effect.Value.effectDistanceRadius))
			{
				float magnitude = vector.magnitude;
				GTPlayer instance = GTPlayer.Instance;
				if (effect.Value.specialVerticalForce && (instance.IsHandTouching(true) || instance.IsHandTouching(false) || instance.BodyOnGround))
				{
					Vector3 vector2 = -Physics.gravity.normalized;
					Vector3 vector3 = Vector3.ProjectOnPlane(vector, vector2);
					vector = ((Vector3.Dot(vector / magnitude, vector2) > 0f) ? vector : vector3) + vector3.magnitude * vector2;
				}
				float num = (effect.Value.knockbackStrength * effect.Value.knockbackStrengthMultiplier / magnitude).ClampSafe(effect.Value.minKnockbackStrength, effect.Value.maxKnockbackStrength);
				if (effect.Value.applyScaleToKnockbackStrength)
				{
					num *= instance.scale;
				}
				instance.ApplyKnockback(vector.normalized, num, effect.Value.forceOffTheGround);
				GorillaTagger.Instance.offlineVRRig.ApplyInstanceKnockBack(effect);
			}
		}

		private void ApplyVOForRig(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect, VRRig rig)
		{
			if (!effect.Value.IsGameModeAllowed() || !this.ShouldAffectRig(rig, effect.Value.target))
			{
				return;
			}
			effect.Value.EffectStartedTime = Time.time;
			if (rig == this.myRig)
			{
				effect.Value.EffectDuration = effect.Value.effectDurationOwner;
			}
			rig.ActivateVOEffect(effect);
		}

		private void PlaySfxForRig(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect, VRRig vrRig)
		{
			if (!effect.Value.IsGameModeAllowed() || !this.ShouldAffectRig(vrRig, effect.Value.target))
			{
				return;
			}
			effect.Value.EffectStartedTime = Time.time;
			if (vrRig == this.myRig)
			{
				effect.Value.EffectDuration = effect.Value.effectDurationOwner;
			}
			vrRig.PlayCosmeticEffectSFX(effect);
		}

		private void PlaySfxByDistance(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect, Vector3 position)
		{
			if (!effect.Value.IsGameModeAllowed())
			{
				return;
			}
			effect.Value.EffectStartedTime = Time.time;
			IEnumerable<VRRig> enumerable2;
			if (!PhotonNetwork.InRoom)
			{
				IEnumerable<VRRig> enumerable = new VRRig[] { GorillaTagger.Instance.offlineVRRig };
				enumerable2 = enumerable;
			}
			else
			{
				IEnumerable<VRRig> enumerable = GorillaParent.instance.vrrigs;
				enumerable2 = enumerable;
			}
			foreach (VRRig vrrig in enumerable2)
			{
				if (this.ShouldAffectRig(vrrig, effect.Value.target) && (vrrig.transform.position - position).IsShorterThan(effect.Value.effectDistanceRadius))
				{
					if (vrrig == this.myRig)
					{
						effect.Value.EffectDuration = effect.Value.effectDurationOwner;
					}
					vrrig.PlayCosmeticEffectSFX(effect);
				}
			}
		}

		private void PlayVFXForRig(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect, VRRig vrRig)
		{
			if (!effect.Value.IsGameModeAllowed() || !this.ShouldAffectRig(vrRig, effect.Value.target))
			{
				return;
			}
			effect.Value.EffectStartedTime = Time.time;
			if (vrRig == this.myRig)
			{
				effect.Value.EffectDuration = effect.Value.effectDurationOwner;
			}
			vrRig.SpawnVFXEffect(effect);
		}

		private void PlayVFXByDistance(KeyValuePair<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> effect, Vector3 position)
		{
			if (!effect.Value.IsGameModeAllowed())
			{
				return;
			}
			effect.Value.EffectStartedTime = Time.time;
			IEnumerable<VRRig> enumerable2;
			if (!PhotonNetwork.InRoom)
			{
				IEnumerable<VRRig> enumerable = new VRRig[] { GorillaTagger.Instance.offlineVRRig };
				enumerable2 = enumerable;
			}
			else
			{
				IEnumerable<VRRig> enumerable = GorillaParent.instance.vrrigs;
				enumerable2 = enumerable;
			}
			foreach (VRRig vrrig in enumerable2)
			{
				if (this.ShouldAffectRig(vrrig, effect.Value.target) && (vrrig.transform.position - position).IsShorterThan(effect.Value.effectDistanceRadius))
				{
					if (vrrig == this.myRig)
					{
						effect.Value.EffectDuration = effect.Value.effectDurationOwner;
					}
					vrrig.SpawnVFXEffect(effect);
				}
			}
		}

		public bool IsSpawned { get; set; }

		public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

		public void OnSpawn(VRRig rig)
		{
			this.myRig = rig;
		}

		public void OnDespawn()
		{
		}

		public CosmeticEffectsOnPlayers.CosmeticEffect[] allEffects = new CosmeticEffectsOnPlayers.CosmeticEffect[0];

		private VRRig myRig;

		private Dictionary<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect> allEffectsDict = new Dictionary<CosmeticEffectsOnPlayers.EFFECTTYPE, CosmeticEffectsOnPlayers.CosmeticEffect>();

		[Serializable]
		public enum TargetType
		{
			Owner,
			Others,
			All
		}

		[Serializable]
		public class CosmeticEffect
		{
			public float knockbackStrengthMultiplier { get; set; }

			public bool IsGameModeAllowed()
			{
				GameModeType gameModeType = ((GameMode.ActiveGameMode != null) ? GameMode.ActiveGameMode.GameType() : GameModeType.Casual);
				return !this.excludeForGameModes.Contains(gameModeType);
			}

			public float EffectDuration
			{
				get
				{
					return this.effectDurationOthers;
				}
				set
				{
					this.effectDurationOthers = value;
				}
			}

			public float EffectStartedTime { get; set; }

			private bool IsSkin()
			{
				return this.effectType == CosmeticEffectsOnPlayers.EFFECTTYPE.Skin;
			}

			private bool IsTagKnockback()
			{
				return this.effectType == CosmeticEffectsOnPlayers.EFFECTTYPE.TagWithKnockback;
			}

			private bool IsInstantKnockback()
			{
				return this.effectType == CosmeticEffectsOnPlayers.EFFECTTYPE.InstantKnockback;
			}

			private bool HasKnockback()
			{
				CosmeticEffectsOnPlayers.EFFECTTYPE effecttype = this.effectType;
				return effecttype == CosmeticEffectsOnPlayers.EFFECTTYPE.TagWithKnockback || effecttype == CosmeticEffectsOnPlayers.EFFECTTYPE.InstantKnockback;
			}

			private bool IsVO()
			{
				return this.effectType == CosmeticEffectsOnPlayers.EFFECTTYPE.VoiceOverride;
			}

			private bool IsSFX()
			{
				return this.effectType == CosmeticEffectsOnPlayers.EFFECTTYPE.SFX;
			}

			private bool IsVFX()
			{
				return this.effectType == CosmeticEffectsOnPlayers.EFFECTTYPE.VFX;
			}

			private HashSet<GameModeType> Modes
			{
				get
				{
					if (this.modesHash == null)
					{
						this.modesHash = new HashSet<GameModeType>(this.excludeForGameModes);
					}
					return this.modesHash;
				}
			}

			public GameModeType[] excludeForGameModes;

			public CosmeticEffectsOnPlayers.EFFECTTYPE effectType;

			public float effectDistanceRadius;

			public CosmeticEffectsOnPlayers.TargetType target = CosmeticEffectsOnPlayers.TargetType.All;

			public float effectDurationOthers;

			public float effectDurationOwner;

			public GorillaSkin newSkin;

			[Tooltip("Use object pools")]
			public GameObject knockbackVFX;

			[FormerlySerializedAs("knockbackStrengthMultiplier")]
			public float knockbackStrength;

			public bool applyScaleToKnockbackStrength;

			[Tooltip("force pushing players with hands on the ground")]
			public bool forceOffTheGround;

			[Tooltip("Take the horizontal magnitude of the knockback, and add it opposite gravity. For example, being hit sideways will also impart a large upwards force. Breaks conservation of energy, but feels better to the player.")]
			public bool specialVerticalForce;

			[FormerlySerializedAs("minStrengthClamp")]
			public float minKnockbackStrength = 0.5f;

			[FormerlySerializedAs("maxStrengthClamp")]
			public float maxKnockbackStrength = 6f;

			public AudioClip[] voiceOverrideNormalClips;

			public AudioClip[] voiceOverrideLoudClips;

			public float voiceOverrideNormalVolume = 0.5f;

			public float voiceOverrideLoudVolume = 0.8f;

			public float voiceOverrideLoudThreshold = 0.175f;

			[Tooltip("plays sfx on player")]
			public List<AudioClip> sfxAudioClip;

			[Tooltip("plays vfx on player, must be in the global object pool and have a tag.")]
			public GameObject VFXGameObject;

			private HashSet<GameModeType> modesHash;
		}

		public enum EFFECTTYPE
		{
			Skin,
			[Obsolete("FPV has been removed, do not use, use Stick Object To Player instead")]
			TagWithKnockback = 2,
			InstantKnockback,
			VoiceOverride,
			SFX,
			VFX
		}
	}
}
