using System;
using Photon.Pun;
using UnityEngine;

public class PlantableFlagManager : MonoBehaviourPun, IPunObservable
{
	public void ResetMyFlags()
	{
		foreach (PlantableObject plantableObject in this.flags)
		{
			if (plantableObject.IsMyItem())
			{
				if (plantableObject.currentState != TransferrableObject.PositionState.Dropped)
				{
					plantableObject.DropItem();
				}
				plantableObject.ResetToHome();
			}
		}
	}

	public void ResetAllFlags()
	{
		foreach (PlantableObject plantableObject in this.flags)
		{
			if (!plantableObject.IsMyItem())
			{
				plantableObject.worldShareableInstance.GetComponent<RequestableOwnershipGuard>().RequestOwnershipImmediately(delegate
				{
				});
			}
			if (plantableObject.currentState != TransferrableObject.PositionState.Dropped)
			{
				plantableObject.DropItem();
			}
			plantableObject.ResetToHome();
		}
	}

	public void RainbowifyAllFlags(float saturation = 1f, float value = 1f)
	{
		Color red = Color.red;
		for (int i = 0; i < this.flags.Length; i++)
		{
			Color color = Color.HSVToRGB((float)i / (float)this.flags.Length, saturation, value);
			PlantableObject plantableObject = this.flags[i];
			if (plantableObject)
			{
				plantableObject.colorR = color;
				plantableObject.colorG = Color.black;
			}
		}
	}

	public void Awake()
	{
		this.mode = new FlagCauldronColorer.ColorMode[this.flags.Length];
		this.flagColors = new PlantableObject.AppliedColors[this.flags.Length][];
		for (int i = 0; i < this.flags.Length; i++)
		{
			this.flagColors[i] = new PlantableObject.AppliedColors[20];
		}
	}

	public void Update()
	{
		if (this.mode == null)
		{
			this.mode = new FlagCauldronColorer.ColorMode[this.flags.Length];
		}
		if (this.flagColors == null)
		{
			this.flagColors = new PlantableObject.AppliedColors[this.flags.Length][];
			for (int i = 0; i < this.flags.Length; i++)
			{
				this.flagColors[i] = new PlantableObject.AppliedColors[20];
			}
		}
		for (int j = 0; j < this.flags.Length; j++)
		{
			PlantableObject plantableObject = this.flags[j];
			if (plantableObject.IsMyItem())
			{
				Vector3.SqrMagnitude(plantableObject.flagTip.position - base.transform.position);
			}
		}
	}

	[PunRPC]
	public void UpdateFlagColorRPC(int flagIndex, int colorIndex, PhotonMessageInfo info)
	{
		PlantableObject plantableObject = this.flags[flagIndex];
		if (colorIndex == 0)
		{
			plantableObject.ClearColors();
			return;
		}
		plantableObject.AddColor((PlantableObject.AppliedColors)colorIndex);
	}

	public void UpdateFlagColors()
	{
		for (int i = 0; i < this.flagColors.Length; i++)
		{
			PlantableObject.AppliedColors[] array = this.flagColors[i];
			PlantableObject plantableObject = this.flags[i];
			if (!plantableObject.IsMyItem() && array.Length <= 20)
			{
				plantableObject.dippedColors = array;
				plantableObject.UpdateDisplayedDippedColor();
			}
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			for (int i = 0; i < this.flagColors.Length; i++)
			{
				for (int j = 0; j < 20; j++)
				{
					stream.SendNext((int)this.flagColors[i][j]);
				}
			}
			return;
		}
		for (int k = 0; k < this.flagColors.Length; k++)
		{
			for (int l = 0; l < 20; l++)
			{
				this.flagColors[k][l] = (PlantableObject.AppliedColors)stream.ReceiveNext();
			}
		}
		this.UpdateFlagColors();
	}

	public PlantableObject[] flags;

	public FlagCauldronColorer[] cauldrons;

	public FlagCauldronColorer.ColorMode[] mode;

	public PlantableObject.AppliedColors[][] flagColors;
}
