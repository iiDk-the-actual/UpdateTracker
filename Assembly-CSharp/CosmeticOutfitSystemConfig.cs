using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CosmeticOutfitSystemConfig", menuName = "Gorilla Tag/Cosmetics/OutfitSystem", order = 0)]
public class CosmeticOutfitSystemConfig : ScriptableObject
{
	public int maxOutfits;

	public string mothershipKey;

	public char outfitSeparator;

	public char itemSeparator;

	public string selectedOutfitPref;
}
