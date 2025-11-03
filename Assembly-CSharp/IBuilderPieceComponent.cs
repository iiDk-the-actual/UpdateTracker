using System;

public interface IBuilderPieceComponent
{
	void OnPieceCreate(int pieceType, int pieceId);

	void OnPieceDestroy();

	void OnPiecePlacementDeserialized();

	void OnPieceActivate();

	void OnPieceDeactivate();
}
