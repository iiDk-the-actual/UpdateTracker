using System;
using GorillaNetworking;
using UnityEngine;

public class WardrobeInstance : MonoBehaviour
{
	public void Start()
	{
		CosmeticsController.instance.AddWardrobeInstance(this);
	}

	public void OnDestroy()
	{
		CosmeticsController.instance.RemoveWardrobeInstance(this);
	}

	public WardrobeItemButton[] wardrobeItemButtons;

	public HeadModel selfDoll;
}
