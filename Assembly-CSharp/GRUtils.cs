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

	public static GRToolProgressionManager.ToolParts GetToolPart(GRTool.GRToolType toolType)
	{
		switch (toolType)
		{
		case GRTool.GRToolType.Club:
			return GRToolProgressionManager.ToolParts.Baton;
		case GRTool.GRToolType.Collector:
			return GRToolProgressionManager.ToolParts.Collector;
		case GRTool.GRToolType.Flash:
			return GRToolProgressionManager.ToolParts.Flash;
		case GRTool.GRToolType.Lantern:
			return GRToolProgressionManager.ToolParts.Lantern;
		case GRTool.GRToolType.Revive:
			return GRToolProgressionManager.ToolParts.Revive;
		case GRTool.GRToolType.ShieldGun:
			return GRToolProgressionManager.ToolParts.ShieldGun;
		case GRTool.GRToolType.DirectionalShield:
			return GRToolProgressionManager.ToolParts.DirectionalShield;
		case GRTool.GRToolType.DockWrist:
			return GRToolProgressionManager.ToolParts.DockWrist;
		case GRTool.GRToolType.HockeyStick:
			return GRToolProgressionManager.ToolParts.HockeyStick;
		}
		return GRToolProgressionManager.ToolParts.None;
	}
}
