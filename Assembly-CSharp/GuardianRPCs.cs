using System;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

internal class GuardianRPCs : RPCNetworkBase
{
	public override void SetClassTarget(IWrappedSerializable target, GorillaWrappedSerializer netHandler)
	{
		this.guardianManager = (GorillaGuardianManager)target;
		this.serializer = (GameModeSerializer)netHandler;
	}

	[PunRPC]
	public void GuardianRequestEject(PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "GuardianRequestEject");
		PhotonMessageInfoWrapped photonMessageInfoWrapped = new PhotonMessageInfoWrapped(info);
		if (photonMessageInfoWrapped.Sender != null)
		{
			this.guardianManager.EjectGuardian(photonMessageInfoWrapped.Sender);
		}
	}

	[PunRPC]
	public void GuardianLaunchPlayer(Vector3 velocity, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "GuardianLaunchPlayer");
		PhotonMessageInfoWrapped photonMessageInfoWrapped = new PhotonMessageInfoWrapped(info);
		if (!this.guardianManager.IsPlayerGuardian(photonMessageInfoWrapped.Sender))
		{
			GorillaNot.instance.SendReport("Sent LaunchPlayer when not a guardian", photonMessageInfoWrapped.Sender.UserId, photonMessageInfoWrapped.Sender.NickName);
			return;
		}
		float num = 10000f;
		if (!(in velocity).IsValid(in num))
		{
			return;
		}
		if (!this.launchCallLimit.CheckCallTime(Time.time))
		{
			return;
		}
		this.guardianManager.LaunchPlayer(photonMessageInfoWrapped.Sender, velocity);
	}

	[PunRPC]
	public void ShowSlapEffects(Vector3 location, Vector3 direction, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "ShowSlapEffects");
		PhotonMessageInfoWrapped photonMessageInfoWrapped = new PhotonMessageInfoWrapped(info);
		if (!this.guardianManager.IsPlayerGuardian(photonMessageInfoWrapped.Sender))
		{
			GorillaNot.instance.SendReport("Sent ShowSlapEffects when not a guardian", photonMessageInfoWrapped.Sender.UserId, photonMessageInfoWrapped.Sender.NickName);
			return;
		}
		float num = 10000f;
		if ((in location).IsValid(in num))
		{
			float num2 = 10000f;
			if ((in direction).IsValid(in num2))
			{
				if (!this.slapFXCallLimit.CheckCallTime(Time.time))
				{
					return;
				}
				this.guardianManager.PlaySlapEffect(location, direction);
				return;
			}
		}
	}

	[PunRPC]
	public void ShowSlamEffect(Vector3 location, Vector3 direction, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "ShowSlamEffect");
		PhotonMessageInfoWrapped photonMessageInfoWrapped = new PhotonMessageInfoWrapped(info);
		if (!this.guardianManager.IsPlayerGuardian(photonMessageInfoWrapped.Sender))
		{
			GorillaNot.instance.SendReport("Sent ShowSlamEffect when not a guardian", photonMessageInfoWrapped.Sender.UserId, photonMessageInfoWrapped.Sender.NickName);
			return;
		}
		float num = 10000f;
		if ((in location).IsValid(in num))
		{
			float num2 = 10000f;
			if ((in direction).IsValid(in num2))
			{
				if (!this.slamFXCallLimit.CheckCallTime(Time.time))
				{
					return;
				}
				this.guardianManager.PlaySlamEffect(location, direction);
				return;
			}
		}
	}

	private GameModeSerializer serializer;

	private GorillaGuardianManager guardianManager;

	private CallLimiter launchCallLimit = new CallLimiter(5, 0.5f, 0.5f);

	private CallLimiter slapFXCallLimit = new CallLimiter(5, 0.5f, 0.5f);

	private CallLimiter slamFXCallLimit = new CallLimiter(5, 0.5f, 0.5f);
}
