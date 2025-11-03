using System;

public class BuilderPieceInteractorFindNearby : MonoBehaviourPostTick
{
	private void Awake()
	{
	}

	public override void PostTick()
	{
		if (this.pieceInteractor != null)
		{
			this.pieceInteractor.StartFindNearbyPieces();
		}
	}

	public BuilderPieceInteractor pieceInteractor;
}
