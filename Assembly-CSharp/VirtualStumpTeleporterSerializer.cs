using System;
using System.Collections.Generic;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

internal class VirtualStumpTeleporterSerializer : GorillaSerializer
{
	public void NotifyPlayerTeleporting(short teleporterIdx, AudioSource localPlayerTeleporterAudioSource)
	{
		if ((int)teleporterIdx >= this.teleporters.Count)
		{
			return;
		}
		if (PhotonNetwork.InRoom)
		{
			base.SendRPC("ActivateTeleportVFX", true, new object[] { false, teleporterIdx });
		}
	}

	public void NotifyPlayerReturning(short teleporterIdx)
	{
		if ((int)teleporterIdx >= this.teleporters.Count)
		{
			return;
		}
		Debug.Log(string.Format("[VRTeleporterSerializer::NotifyPlayerReturning] Sending RPC to activate VFX at idx: {0}", teleporterIdx));
		if (PhotonNetwork.InRoom)
		{
			base.SendRPC("ActivateTeleportVFX", true, new object[] { true, teleporterIdx });
		}
	}

	[PunRPC]
	private void ActivateTeleportVFX(bool returning, short teleporterIdx, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "ActivateTeleportVFX");
		if ((int)teleporterIdx >= this.teleporters.Count)
		{
			return;
		}
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(player, out rigContainer) || !rigContainer.Rig.fxSettings.callSettings[13].CallLimitSettings.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		VirtualStumpTeleporter virtualStumpTeleporter = this.teleporters[(int)teleporterIdx];
		if (virtualStumpTeleporter.IsNotNull())
		{
			virtualStumpTeleporter.PlayTeleportEffects(false, !returning, null, false);
		}
	}

	public short GetTeleporterIndex(VirtualStumpTeleporter teleporter)
	{
		short num = 0;
		while ((int)num < this.teleporters.Count)
		{
			if (this.teleporters[(int)num] == teleporter)
			{
				return num;
			}
			num += 1;
		}
		return -1;
	}

	[SerializeField]
	public List<VirtualStumpTeleporter> teleporters = new List<VirtualStumpTeleporter>();

	[SerializeField]
	public List<ParticleSystem> teleporterVFX = new List<ParticleSystem>();

	[SerializeField]
	public List<ParticleSystem> returnVFX = new List<ParticleSystem>();

	[SerializeField]
	public List<AudioSource> teleportAudioSource = new List<AudioSource>();

	[SerializeField]
	public List<AudioClip> teleportingPlayerSoundClips = new List<AudioClip>();

	[SerializeField]
	public List<AudioClip> observerSoundClips = new List<AudioClip>();
}
