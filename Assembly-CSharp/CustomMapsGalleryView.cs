using System;
using System.Collections.Generic;
using Modio.Mods;
using UnityEngine;

public class CustomMapsGalleryView : MonoBehaviour
{
	public void ResetGallery()
	{
		for (int i = 0; i < this.modTiles.Count; i++)
		{
			this.modTiles[i].DeactivateTile();
		}
	}

	public bool DisplayGallery(List<Mod> mods, bool useMapName, out string error)
	{
		if (mods.Count > this.modTiles.Count)
		{
			GTDev.LogError<string>("Displayed Mod list is longer than the number of mod tiles in the gallery", null);
			error = "Displayed Mod list is longer than the number of mod tiles in the gallery";
			return false;
		}
		for (int i = 0; i < mods.Count; i++)
		{
			this.modTiles[i].SetMod(mods[i], useMapName);
		}
		error = string.Empty;
		return true;
	}

	public void ShowTileText(bool show, bool useMapName)
	{
		for (int i = 0; i < this.modTiles.Count; i++)
		{
			this.modTiles[i].ShowTileText(show, useMapName);
		}
	}

	public void ShowDetailsForEntry(int entryIndex)
	{
		if (this.modTiles.Count > entryIndex)
		{
			this.modTiles[entryIndex].ShowDetails();
		}
	}

	public void HighlightTileAtIndex(int tileIndex)
	{
		if (tileIndex > this.modTiles.Count)
		{
			return;
		}
		this.modTiles[tileIndex].HighlightTile();
	}

	[SerializeField]
	private List<CustomMapsModTile> modTiles = new List<CustomMapsModTile>();
}
