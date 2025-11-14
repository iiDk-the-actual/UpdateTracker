using System;
using GorillaNetworking;
using UnityEngine;

public class CommonActions : MonoBehaviour
{
	public void LoadSavedOutfit(int index)
	{
		if (CosmeticsController.instance)
		{
			CosmeticsController.instance.LoadSavedOutfit(index);
		}
	}

	public void LoadPrevOutfit()
	{
		if (CosmeticsController.instance)
		{
			CosmeticsController.instance.PressWardrobeScrollOutfit(false);
		}
	}

	public void LoadNextOutfit()
	{
		if (CosmeticsController.instance)
		{
			CosmeticsController.instance.PressWardrobeScrollOutfit(true);
		}
	}
}
