using System;
using Photon.Pun;
using UnityEngine;

namespace GorillaTagScripts.Builder
{
	public class BuilderTrafficLight : MonoBehaviour, IBuilderPieceComponent
	{
		private void Start()
		{
			this.materialProps = new MaterialPropertyBlock();
		}

		private void SetState(BuilderTrafficLight.LightState state)
		{
			this.lightState = state;
			if (this.materialProps == null)
			{
				this.materialProps = new MaterialPropertyBlock();
			}
			Color color = this.yellowOff;
			Color color2 = this.redOff;
			Color color3 = this.greenOff;
			switch (state)
			{
			case BuilderTrafficLight.LightState.Red:
				color2 = this.redOn;
				break;
			case BuilderTrafficLight.LightState.Yellow:
				color = this.yellowOn;
				break;
			case BuilderTrafficLight.LightState.Green:
				color3 = this.greenOn;
				break;
			}
			this.redLight.GetPropertyBlock(this.materialProps);
			this.materialProps.SetColor(ShaderProps._BaseColor, color2);
			this.redLight.SetPropertyBlock(this.materialProps);
			this.materialProps.SetColor(ShaderProps._BaseColor, color);
			this.yellowLight.SetPropertyBlock(this.materialProps);
			this.materialProps.SetColor(ShaderProps._BaseColor, color3);
			this.greenLight.SetPropertyBlock(this.materialProps);
		}

		private void Update()
		{
			if (this.piece == null || this.piece.state == BuilderPiece.State.AttachedAndPlaced)
			{
				float num = Time.time;
				if (PhotonNetwork.InRoom)
				{
					uint num2 = (uint)PhotonNetwork.ServerTimestamp;
					if (this.piece != null)
					{
						num2 = (uint)(PhotonNetwork.ServerTimestamp - this.piece.activatedTimeStamp);
					}
					num = num2 / 1000f;
				}
				float num3 = num % this.cycleDuration / this.cycleDuration;
				num3 = (num3 + this.startPercentageOffset) % 1f;
				int num4 = (int)this.stateCurve.Evaluate(num3);
				if (num4 != (int)this.lightState)
				{
					this.SetState((BuilderTrafficLight.LightState)num4);
				}
			}
		}

		public void OnPieceCreate(int pieceType, int pieceId)
		{
			this.SetState(BuilderTrafficLight.LightState.Off);
		}

		public void OnPieceDestroy()
		{
		}

		public void OnPiecePlacementDeserialized()
		{
		}

		public void OnPieceActivate()
		{
		}

		public void OnPieceDeactivate()
		{
			this.SetState(BuilderTrafficLight.LightState.Off);
		}

		[SerializeField]
		private BuilderPiece piece;

		[SerializeField]
		private MeshRenderer redLight;

		[SerializeField]
		private MeshRenderer yellowLight;

		[SerializeField]
		private MeshRenderer greenLight;

		[SerializeField]
		private float cycleDuration = 10f;

		[SerializeField]
		private float startPercentageOffset = 0.5f;

		[SerializeField]
		private Color redOn = Color.red;

		[SerializeField]
		private Color redOff = Color.gray;

		[SerializeField]
		private Color yellowOn = Color.yellow;

		[SerializeField]
		private Color yellowOff = Color.gray;

		[SerializeField]
		private Color greenOn = Color.green;

		[SerializeField]
		private Color greenOff = Color.gray;

		private MaterialPropertyBlock materialProps;

		[SerializeField]
		private AnimationCurve stateCurve;

		private BuilderTrafficLight.LightState lightState = BuilderTrafficLight.LightState.Off;

		private enum LightState
		{
			Red,
			Yellow,
			Green,
			Off
		}
	}
}
