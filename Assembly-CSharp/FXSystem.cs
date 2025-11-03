using System;
using System.Collections.Generic;
using GorillaExtensions;
using UnityEngine;

public static class FXSystem
{
	public static void PlayFXForRig(FXType fxType, IFXContext context, PhotonMessageInfoWrapped info = default(PhotonMessageInfoWrapped))
	{
		FXSystemSettings settings = context.settings;
		if (settings.forLocalRig)
		{
			context.OnPlayFX();
			return;
		}
		if (FXSystem.CheckCallSpam(settings, (int)fxType, info.SentServerTime))
		{
			context.OnPlayFX();
		}
	}

	public static void PlayFXForRigValidated(List<int> hashes, FXType fxType, IFXContext context, PhotonMessageInfoWrapped info = default(PhotonMessageInfoWrapped))
	{
		for (int i = 0; i < hashes.Count; i++)
		{
			if (!ObjectPools.instance.DoesPoolExist(hashes[i]))
			{
				return;
			}
		}
		FXSystem.PlayFXForRig(fxType, context, info);
	}

	public static void PlayFX<T>(FXType fxType, IFXContextParems<T> context, T args, PhotonMessageInfoWrapped info) where T : FXSArgs
	{
		FXSystemSettings settings = context.settings;
		if (settings.forLocalRig)
		{
			context.OnPlayFX(args);
			return;
		}
		if (FXSystem.CheckCallSpam(settings, (int)fxType, info.SentServerTime))
		{
			context.OnPlayFX(args);
		}
	}

	public static void PlayFXForRig<T>(FXType fxType, IFXEffectContext<T> context, PhotonMessageInfoWrapped info) where T : IFXEffectContextObject
	{
		FXSystemSettings settings = context.settings;
		if (!settings.forLocalRig && !FXSystem.CheckCallSpam(settings, (int)fxType, info.SentServerTime))
		{
			return;
		}
		FXSystem.PlayFX(context.effectContext);
	}

	public static void PlayFX(IFXEffectContextObject effectContext)
	{
		effectContext.OnTriggerActions();
		List<int> prefabPoolIds = effectContext.PrefabPoolIds;
		if (prefabPoolIds != null)
		{
			int count = prefabPoolIds.Count;
			for (int i = 0; i < count; i++)
			{
				int num = prefabPoolIds[i];
				if (num != -1)
				{
					GameObject gameObject = ObjectPools.instance.Instantiate(num, effectContext.Position, effectContext.Rotation, false);
					gameObject.SetActive(true);
					effectContext.OnPlayVisualFX(num, gameObject);
				}
			}
		}
		AudioSource soundSource = effectContext.SoundSource;
		if (soundSource.IsNull())
		{
			return;
		}
		AudioClip sound = effectContext.Sound;
		if (sound.IsNotNull())
		{
			soundSource.volume = effectContext.Volume;
			soundSource.pitch = effectContext.Pitch;
			soundSource.GTPlayOneShot(sound, 1f);
			effectContext.OnPlaySoundFX(soundSource);
		}
	}

	public static bool CheckCallSpam(FXSystemSettings settings, int index, double serverTime)
	{
		CallLimitType<CallLimiter> callLimitType = settings.callSettings[index];
		if (!callLimitType.UseNetWorkTime)
		{
			return callLimitType.CallLimitSettings.CheckCallTime(Time.time);
		}
		return callLimitType.CallLimitSettings.CheckCallServerTime(serverTime);
	}
}
