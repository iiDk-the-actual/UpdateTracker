using System;
using UnityEngine;

[Serializable]
public struct PieceFallbackInfo
{
	[Tooltip("Check if the piece has Material Options set and the default material is in a starter set")]
	public bool materialSwapThisPrefab;

	[Tooltip("A piece in a starter set with the same builder attach grid configuration\n(check BuilderSetManager _starterPieceSets for pieces in starter sets)")]
	public BuilderPiece prefab;
}
