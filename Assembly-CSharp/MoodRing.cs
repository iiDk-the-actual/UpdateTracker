using System;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using Photon.Pun;
using UnityEngine;

public class MoodRing : MonoBehaviour, ISpawnable
{
	bool ISpawnable.IsSpawned { get; set; }

	ECosmeticSelectSide ISpawnable.CosmeticSelectedSide { get; set; }

	void ISpawnable.OnDespawn()
	{
	}

	void ISpawnable.OnSpawn(VRRig rig)
	{
		this.myRig = rig;
	}

	private void Update()
	{
		if ((this.attachedToLeftHand ? this.myRig.leftIndex.calcT : this.myRig.rightIndex.calcT) > 0.5f)
		{
			if (!this.isCycling)
			{
				this.animRedValue = this.myRig.playerColor.r;
				this.animGreenValue = this.myRig.playerColor.g;
				this.animBlueValue = this.myRig.playerColor.b;
			}
			this.isCycling = true;
			this.RainbowCycle(ref this.animRedValue, ref this.animGreenValue, ref this.animBlueValue);
			this.myRig.InitializeNoobMaterialLocal(this.animRedValue, this.animGreenValue, this.animBlueValue);
			return;
		}
		if (this.isCycling)
		{
			this.isCycling = false;
			if (this.myRig.isOfflineVRRig)
			{
				this.animRedValue = Mathf.Round(this.animRedValue * 9f) / 9f;
				this.animGreenValue = Mathf.Round(this.animGreenValue * 9f) / 9f;
				this.animBlueValue = Mathf.Round(this.animBlueValue * 9f) / 9f;
				GorillaTagger.Instance.UpdateColor(this.animRedValue, this.animGreenValue, this.animBlueValue);
				if (NetworkSystem.Instance.InRoom)
				{
					GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, new object[] { this.animRedValue, this.animGreenValue, this.animBlueValue });
				}
				PlayerPrefs.SetFloat("redValue", this.animRedValue);
				PlayerPrefs.SetFloat("greenValue", this.animGreenValue);
				PlayerPrefs.SetFloat("blueValue", this.animBlueValue);
				PlayerPrefs.Save();
			}
		}
	}

	private void RainbowCycle(ref float r, ref float g, ref float b)
	{
		float num = this.furCycleSpeed * Time.deltaTime;
		if (r == 1f)
		{
			if (b > 0f)
			{
				b = Mathf.Clamp01(b - num);
				return;
			}
			if (g < 1f)
			{
				g = Mathf.Clamp01(g + num);
				return;
			}
			r = Mathf.Clamp01(r - num);
			return;
		}
		else if (g == 1f)
		{
			if (r > 0f)
			{
				r = Mathf.Clamp01(r - num);
				return;
			}
			if (b < 1f)
			{
				b = Mathf.Clamp01(b + num);
				return;
			}
			g = Mathf.Clamp01(g - num);
			return;
		}
		else
		{
			if (b != 1f)
			{
				r = Mathf.Clamp01(r + num);
				return;
			}
			if (g > 0f)
			{
				g = Mathf.Clamp01(g - num);
				return;
			}
			if (r < 1f)
			{
				r = Mathf.Clamp01(r + num);
				return;
			}
			b = Mathf.Clamp01(b - num);
			return;
		}
	}

	[SerializeField]
	private bool attachedToLeftHand;

	private VRRig myRig;

	[SerializeField]
	private float rotationSpeed;

	[SerializeField]
	private float furCycleSpeed;

	private float nextFurCycleTimestamp;

	private float animRedValue;

	private float animGreenValue;

	private float animBlueValue;

	private bool isCycling;
}
