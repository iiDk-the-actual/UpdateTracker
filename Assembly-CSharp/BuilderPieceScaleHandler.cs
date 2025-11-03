using System;
using System.Collections.Generic;
using GorillaTagScripts.Builder;
using UnityEngine;

public class BuilderPieceScaleHandler : MonoBehaviour, IBuilderPieceComponent
{
	public void OnPieceCreate(int pieceType, int pieceId)
	{
		foreach (BuilderScaleAudioRadius builderScaleAudioRadius in this.audioScalers)
		{
			builderScaleAudioRadius.SetScale(this.myPiece.GetScale());
		}
		foreach (BuilderScaleParticles builderScaleParticles in this.particleScalers)
		{
			builderScaleParticles.SetScale(this.myPiece.GetScale());
		}
	}

	public void OnPieceDestroy()
	{
		foreach (BuilderScaleAudioRadius builderScaleAudioRadius in this.audioScalers)
		{
			builderScaleAudioRadius.RevertScale();
		}
		foreach (BuilderScaleParticles builderScaleParticles in this.particleScalers)
		{
			builderScaleParticles.RevertScale();
		}
	}

	public void OnPiecePlacementDeserialized()
	{
	}

	public void OnPieceActivate()
	{
	}

	public void OnPieceDeactivate()
	{
	}

	[SerializeField]
	private BuilderPiece myPiece;

	[SerializeField]
	private List<BuilderScaleAudioRadius> audioScalers = new List<BuilderScaleAudioRadius>();

	[SerializeField]
	private List<BuilderScaleParticles> particleScalers = new List<BuilderScaleParticles>();
}
