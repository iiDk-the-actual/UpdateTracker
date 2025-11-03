using System;

namespace GorillaTagScripts
{
	public struct BuilderPieceData
	{
		public BuilderPieceData(BuilderPiece piece)
		{
			this.pieceId = piece.pieceId;
			this.pieceIndex = piece.pieceDataIndex;
			BuilderPiece parentPiece = piece.parentPiece;
			this.parentPieceIndex = ((parentPiece == null) ? (-1) : parentPiece.pieceDataIndex);
			BuilderPiece requestedParentPiece = piece.requestedParentPiece;
			this.requestedParentPieceIndex = ((requestedParentPiece == null) ? (-1) : requestedParentPiece.pieceDataIndex);
			this.preventSnapUntilMoved = piece.preventSnapUntilMoved;
			this.isBuiltIntoTable = piece.isBuiltIntoTable;
			this.state = piece.state;
			this.privatePlotIndex = piece.privatePlotIndex;
			this.isArmPiece = piece.isArmShelf;
			this.heldByActorNumber = piece.heldByPlayerActorNumber;
		}

		public int pieceId;

		public int pieceIndex;

		public int parentPieceIndex;

		public int requestedParentPieceIndex;

		public int heldByActorNumber;

		public int preventSnapUntilMoved;

		public bool isBuiltIntoTable;

		public BuilderPiece.State state;

		public int privatePlotIndex;

		public bool isArmPiece;
	}
}
