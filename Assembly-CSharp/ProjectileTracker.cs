using System;
using System.Collections.Generic;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

internal static class ProjectileTracker
{
	static ProjectileTracker()
	{
		RoomSystem.LeftRoomEvent += new Action(ProjectileTracker.ClearProjectiles);
		RoomSystem.PlayerLeftEvent += new Action<NetPlayer>(ProjectileTracker.RemovePlayerProjectiles);
	}

	public static void RemovePlayerProjectiles(NetPlayer player)
	{
		LoopingArray<ProjectileTracker.ProjectileInfo> loopingArray;
		if (ProjectileTracker.m_playerProjectiles.TryGetValue(player, out loopingArray))
		{
			ProjectileTracker.ResetPlayerProjectiles(loopingArray);
			ProjectileTracker.m_playerProjectiles.Remove(player);
			ProjectileTracker.m_projectileInfoPool.Return(loopingArray);
		}
	}

	private static void ClearProjectiles()
	{
		foreach (LoopingArray<ProjectileTracker.ProjectileInfo> loopingArray in ProjectileTracker.m_playerProjectiles.Values)
		{
			ProjectileTracker.ResetPlayerProjectiles(loopingArray);
			ProjectileTracker.m_projectileInfoPool.Return(loopingArray);
		}
		ProjectileTracker.m_playerProjectiles.Clear();
	}

	private static void ResetPlayerProjectiles(LoopingArray<ProjectileTracker.ProjectileInfo> projectiles)
	{
		for (int i = 0; i < projectiles.Length; i++)
		{
			SlingshotProjectile projectileInstance = projectiles[i].projectileInstance;
			if (!projectileInstance.IsNull() && projectileInstance.projectileOwner != NetworkSystem.Instance.LocalPlayer && projectileInstance.gameObject.activeSelf)
			{
				projectileInstance.Deactivate();
			}
		}
	}

	public static int AddAndIncrementLocalProjectile(SlingshotProjectile projectile, Vector3 intialVelocity, Vector3 initialPosition, float scale)
	{
		SlingshotProjectile projectileInstance = ProjectileTracker.m_localProjectiles[ProjectileTracker.m_localProjectiles.CurrentIndex].projectileInstance;
		if (projectileInstance.IsNotNull() && projectileInstance != projectile && projectileInstance.projectileOwner == NetworkSystem.Instance.LocalPlayer && projectileInstance.gameObject.activeSelf)
		{
			projectileInstance.Deactivate();
		}
		ProjectileTracker.ProjectileInfo projectileInfo = new ProjectileTracker.ProjectileInfo(PhotonNetwork.Time, intialVelocity, initialPosition, scale, projectile);
		return ProjectileTracker.m_localProjectiles.AddAndIncrement(in projectileInfo);
	}

	public static void AddRemotePlayerProjectile(NetPlayer player, SlingshotProjectile projectile, int projectileIndex, double timeShot, Vector3 intialVelocity, Vector3 initialPosition, float scale)
	{
		LoopingArray<ProjectileTracker.ProjectileInfo> loopingArray;
		if (!ProjectileTracker.m_playerProjectiles.ContainsKey(player))
		{
			loopingArray = ProjectileTracker.m_projectileInfoPool.Take();
			ProjectileTracker.m_playerProjectiles[player] = loopingArray;
		}
		else
		{
			loopingArray = ProjectileTracker.m_playerProjectiles[player];
		}
		if (projectileIndex < 0 || projectileIndex >= loopingArray.Length)
		{
			GorillaNot.instance.SendReport("invlProj", player.UserId, player.NickName);
			return;
		}
		SlingshotProjectile projectileInstance = loopingArray[projectileIndex].projectileInstance;
		if (projectileInstance.IsNotNull() && projectileInstance.projectileOwner == player && projectileInstance.gameObject.activeSelf)
		{
			projectileInstance.Deactivate();
		}
		ProjectileTracker.ProjectileInfo projectileInfo = new ProjectileTracker.ProjectileInfo(timeShot, intialVelocity, initialPosition, scale, projectile);
		loopingArray[projectileIndex] = projectileInfo;
	}

	public static ProjectileTracker.ProjectileInfo GetLocalProjectile(int index)
	{
		return ProjectileTracker.m_localProjectiles[index];
	}

	public static ValueTuple<bool, ProjectileTracker.ProjectileInfo> GetAndRemoveRemotePlayerProjectile(NetPlayer player, int index)
	{
		ValueTuple<bool, ProjectileTracker.ProjectileInfo> valueTuple = new ValueTuple<bool, ProjectileTracker.ProjectileInfo>(false, default(ProjectileTracker.ProjectileInfo));
		LoopingArray<ProjectileTracker.ProjectileInfo> loopingArray;
		if (index < 0 || index >= ProjectileTracker.m_localProjectiles.Length || !ProjectileTracker.m_playerProjectiles.TryGetValue(player, out loopingArray))
		{
			return valueTuple;
		}
		ProjectileTracker.ProjectileInfo projectileInfo = loopingArray[index];
		if (projectileInfo.projectileInstance.IsNotNull())
		{
			valueTuple.Item1 = true;
			valueTuple.Item2 = projectileInfo;
			loopingArray[index] = default(ProjectileTracker.ProjectileInfo);
		}
		return valueTuple;
	}

	private static LoopingArray<ProjectileTracker.ProjectileInfo>.Pool m_projectileInfoPool = new LoopingArray<ProjectileTracker.ProjectileInfo>.Pool(50, 9);

	private static LoopingArray<ProjectileTracker.ProjectileInfo> m_localProjectiles = new LoopingArray<ProjectileTracker.ProjectileInfo>(50);

	public static readonly Dictionary<NetPlayer, LoopingArray<ProjectileTracker.ProjectileInfo>> m_playerProjectiles = new Dictionary<NetPlayer, LoopingArray<ProjectileTracker.ProjectileInfo>>(9);

	public struct ProjectileInfo
	{
		public ProjectileInfo(double newTime, Vector3 newVel, Vector3 origin, float newScale, SlingshotProjectile projectile)
		{
			this.timeLaunched = newTime;
			this.shotVelocity = newVel;
			this.launchOrigin = origin;
			this.scale = newScale;
			this.projectileInstance = projectile;
			this.hasImpactOverride = projectile.playerImpactEffectPrefab.IsNotNull();
		}

		public double timeLaunched;

		public Vector3 shotVelocity;

		public Vector3 launchOrigin;

		public float scale;

		public SlingshotProjectile projectileInstance;

		public bool hasImpactOverride;
	}
}
