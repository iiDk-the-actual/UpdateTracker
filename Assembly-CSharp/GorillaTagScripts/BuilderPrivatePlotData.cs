using System;

namespace GorillaTagScripts
{
	public struct BuilderPrivatePlotData
	{
		public BuilderPrivatePlotData(BuilderPiecePrivatePlot plot)
		{
			this.plotState = plot.plotState;
			this.ownerActorNumber = plot.GetOwnerActorNumber();
			this.isUnderCapacityLeft = false;
			this.isUnderCapacityRight = false;
		}

		public BuilderPiecePrivatePlot.PlotState plotState;

		public int ownerActorNumber;

		public bool isUnderCapacityLeft;

		public bool isUnderCapacityRight;
	}
}
