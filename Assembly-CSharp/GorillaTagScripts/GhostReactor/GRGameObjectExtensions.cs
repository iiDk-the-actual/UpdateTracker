using System;
using UnityEngine;

namespace GorillaTagScripts.GhostReactor
{
	public static class GRGameObjectExtensions
	{
		public static GRTool.GRToolType GetToolType(this GameObject obj)
		{
			if (obj.GetComponentInParent<GRToolClub>() != null)
			{
				return GRTool.GRToolType.Club;
			}
			if (obj.GetComponentInParent<GRToolCollector>() != null)
			{
				return GRTool.GRToolType.Collector;
			}
			if (obj.GetComponentInParent<GRToolFlash>() != null)
			{
				return GRTool.GRToolType.Flash;
			}
			if (obj.GetComponentInParent<GRToolLantern>() != null)
			{
				return GRTool.GRToolType.Lantern;
			}
			if (obj.GetComponentInParent<GRToolRevive>() != null)
			{
				return GRTool.GRToolType.Revive;
			}
			if (obj.GetComponentInParent<GRToolShieldGun>() != null)
			{
				return GRTool.GRToolType.ShieldGun;
			}
			if (obj.GetComponentInParent<GRToolDirectionalShield>() != null)
			{
				return GRTool.GRToolType.DirectionalShield;
			}
			GRTool grtool = obj.GetComponentInParent<GRTool>();
			if (grtool != null && grtool.toolType == GRTool.GRToolType.HockeyStick)
			{
				return GRTool.GRToolType.HockeyStick;
			}
			grtool = obj.GetComponentInParent<GRTool>();
			if (grtool != null && grtool.toolType == GRTool.GRToolType.DockWrist)
			{
				return GRTool.GRToolType.DockWrist;
			}
			return GRTool.GRToolType.None;
		}
	}
}
