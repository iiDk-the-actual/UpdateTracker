using System;
using System.Collections.Generic;

namespace GorillaTagScripts
{
	[Serializable]
	public class BuilderTableData
	{
		public BuilderTableData()
		{
			this.version = 4;
			this.numEdits = 0;
			this.numPieces = 0;
			this.pieceType = new List<int>(1024);
			this.pieceId = new List<int>(1024);
			this.parentId = new List<int>(1024);
			this.attachIndex = new List<int>(1024);
			this.parentAttachIndex = new List<int>(1024);
			this.placement = new List<int>(1024);
			this.materialType = new List<int>(1024);
			this.overlapingPieces = new List<int>(1024);
			this.overlappedPieces = new List<int>(1024);
			this.overlapInfo = new List<long>(1024);
			this.timeOffset = new List<int>(1024);
		}

		public void Clear()
		{
			this.numPieces = 0;
			this.pieceType.Clear();
			this.pieceId.Clear();
			this.parentId.Clear();
			this.attachIndex.Clear();
			this.parentAttachIndex.Clear();
			this.placement.Clear();
			this.materialType.Clear();
			this.overlapingPieces.Clear();
			this.overlappedPieces.Clear();
			this.overlapInfo.Clear();
			this.timeOffset.Clear();
		}

		public const int BUILDER_TABLE_DATA_VERSION = 4;

		public int version;

		public int numEdits;

		public int numPieces;

		public List<int> pieceType;

		public List<int> pieceId;

		public List<int> parentId;

		public List<int> attachIndex;

		public List<int> parentAttachIndex;

		public List<int> placement;

		public List<int> materialType;

		public List<int> overlapingPieces;

		public List<int> overlappedPieces;

		public List<long> overlapInfo;

		public List<int> timeOffset;
	}
}
