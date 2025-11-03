using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BuilderPieceSet01", menuName = "Gorilla Tag/Builder/PieceSet", order = 0)]
public class BuilderPieceSet : ScriptableObject
{
	public string SetName
	{
		get
		{
			return this.setName;
		}
	}

	public int GetIntIdentifier()
	{
		return this.playfabID.GetStaticHash();
	}

	public DateTime GetScheduleDateTime()
	{
		if (this.isScheduled)
		{
			try
			{
				return DateTime.Parse(this.scheduledDate, CultureInfo.InvariantCulture);
			}
			catch
			{
				return DateTime.MinValue;
			}
		}
		return DateTime.MinValue;
	}

	[Tooltip("Display Name - Fallback for Localization")]
	public string setName;

	public GameObject displayModel;

	[Tooltip("If this should error if no localization is found")]
	public bool isLocalized;

	[Tooltip("Localized Display Name")]
	public LocalizedString setLocName;

	[FormerlySerializedAs("uniqueId")]
	[Tooltip("If purchaseable, this should be a valid playfabID starting with LD\nIf a starter set, this just needs to be a unique string from the other set IDs")]
	public string playfabID;

	[Tooltip("(Optional) Default Material ID applied to all prefabs with BuilderMaterialOptions")]
	public string materialId;

	[Tooltip("(Optional) If this set is not available on launch day use scheduling")]
	public bool isScheduled;

	public string scheduledDate = "1/1/0001 00:00:00";

	[Tooltip("A group of pieces on the same shelf")]
	public List<BuilderPieceSet.BuilderPieceSubset> subsets;

	public enum BuilderPieceCategory
	{
		FLAT,
		TALL,
		HALF_HEIGHT,
		BEAM,
		SLOPE,
		OVERSIZED,
		SPECIAL_DISPLAY,
		FUNCTIONAL = 18,
		DECORATIVE,
		MISC
	}

	[Serializable]
	public class BuilderPieceSubset
	{
		public string GetShelfButtonName()
		{
			return this.shelfButtonName;
		}

		[Tooltip("(Optional) Text to put on the shelf button if not the set name")]
		public string shelfButtonName;

		public LocalizedString localizedShelfButtonName;

		public BuilderPieceSet.BuilderPieceCategory pieceCategory;

		public List<BuilderPieceSet.PieceInfo> pieceInfos;
	}

	[Serializable]
	public struct PieceInfo
	{
		public BuilderPiece piecePrefab;

		[Tooltip("(Optional) should this piece use a materialID other than the set's materialID")]
		public bool overrideSetMaterial;

		[Tooltip("material type string should match an entry in this prefab's BuilderMaterialOptions\nIf multiple are in the list the piece will cycle through materials when spawned\nTo have each variant on the shelf create a new pieceInfo for each color")]
		public string[] pieceMaterialTypes;
	}

	public class BuilderDisplayGroup
	{
		public BuilderDisplayGroup()
		{
			this.displayName = string.Empty;
			this.pieceSubsets = new List<BuilderPieceSet.BuilderPieceSubset>();
			this.defaultMaterial = string.Empty;
			this.setID = -1;
			this.uniqueGroupID = string.Empty;
		}

		public BuilderDisplayGroup(string groupName, string material, int inSetID, string groupID)
		{
			this.displayName = groupName;
			this.pieceSubsets = new List<BuilderPieceSet.BuilderPieceSubset>();
			this.defaultMaterial = material;
			this.setID = inSetID;
			this.uniqueGroupID = groupID;
		}

		public int GetDisplayGroupIdentifier()
		{
			return this.uniqueGroupID.GetStaticHash();
		}

		public string displayName;

		public List<BuilderPieceSet.BuilderPieceSubset> pieceSubsets;

		public string defaultMaterial;

		public int setID;

		public string uniqueGroupID;
	}
}
