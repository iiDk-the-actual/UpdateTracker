using System;

public class GRToolScannable : GRScannable
{
	public override void Start()
	{
		base.Start();
		if (this.gameEntity != null)
		{
			this.tool = this.gameEntity.GetComponent<GRTool>();
			this.upgradePiece = this.gameEntity.GetComponent<GRToolUpgradePiece>();
		}
	}

	private void FetchMetadata(GhostReactor reactor)
	{
		if (this.metadata == null)
		{
			GRToolProgressionManager.ToolParts toolParts = GRToolProgressionManager.ToolParts.None;
			if (this.tool != null)
			{
				toolParts = GRUtils.GetToolPart(this.tool.toolType);
			}
			else if (this.upgradePiece != null)
			{
				toolParts = this.upgradePiece.matchingUpgrade;
			}
			if (toolParts != GRToolProgressionManager.ToolParts.None)
			{
				this.metadata = reactor.toolProgression.GetPartMetadata(toolParts);
			}
		}
	}

	public override string GetTitleText(GhostReactor reactor)
	{
		this.FetchMetadata(reactor);
		if (this.metadata == null)
		{
			return "Unknown";
		}
		return this.metadata.name;
	}

	public override string GetBodyText(GhostReactor reactor)
	{
		this.FetchMetadata(reactor);
		if (this.metadata == null)
		{
			return "Unknown";
		}
		return this.metadata.description;
	}

	public override string GetAnnotationText(GhostReactor reactor)
	{
		this.FetchMetadata(reactor);
		if (this.metadata == null)
		{
			return "Unknown";
		}
		return this.metadata.annotation;
	}

	private GRTool tool;

	private GRToolUpgradePiece upgradePiece;

	private GRToolProgressionManager.ToolProgressionMetaData metadata;
}
