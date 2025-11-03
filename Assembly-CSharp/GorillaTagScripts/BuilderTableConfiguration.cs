using System;

namespace GorillaTagScripts
{
	[Serializable]
	public class BuilderTableConfiguration
	{
		public BuilderTableConfiguration()
		{
			this.version = 0;
			this.TableResourceLimits = new int[3];
			this.PlotResourceLimits = new int[3];
			this.updateCountdownDate = string.Empty;
		}

		public const int CONFIGURATION_VERSION = 0;

		public int version;

		public int[] TableResourceLimits;

		public int[] PlotResourceLimits;

		public int DroppedPieceLimit;

		public string updateCountdownDate;
	}
}
