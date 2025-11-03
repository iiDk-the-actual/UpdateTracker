using System;

public class GRUtils
{
	public static string GetToolName(GRTool.GRToolType toolType)
	{
		switch (toolType)
		{
		case GRTool.GRToolType.Club:
			return "Baton";
		case GRTool.GRToolType.Collector:
			return "Collector";
		case GRTool.GRToolType.Flash:
			return "Flash";
		case GRTool.GRToolType.Lantern:
			return "Lantern";
		case GRTool.GRToolType.Revive:
			return "Revive";
		case GRTool.GRToolType.ShieldGun:
			return "Shield";
		case GRTool.GRToolType.DirectionalShield:
			return "Deflector";
		case GRTool.GRToolType.DockWrist:
			return "Dock";
		case GRTool.GRToolType.HockeyStick:
			return "Stick";
		}
		return "Unknown";
	}
}
