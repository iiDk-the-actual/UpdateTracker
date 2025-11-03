using System;
using GorillaNetworking;
using UnityEngine;

namespace GorillaTag.CosmeticSystem
{
	[Serializable]
	public struct CosmeticInfoV2 : ISerializationCallbackReceiver
	{
		public bool hasHoldableParts
		{
			get
			{
				CosmeticPart[] array = this.holdableParts;
				return array != null && array.Length > 0;
			}
		}

		public bool hasWardrobeParts
		{
			get
			{
				CosmeticPart[] array = this.wardrobeParts;
				return array != null && array.Length > 0;
			}
		}

		public bool hasStoreParts
		{
			get
			{
				CosmeticPart[] array = this.storeParts;
				return array != null && array.Length > 0;
			}
		}

		public bool hasFunctionalParts
		{
			get
			{
				CosmeticPart[] array = this.functionalParts;
				return array != null && array.Length > 0;
			}
		}

		public bool hasFirstPersonViewParts
		{
			get
			{
				CosmeticPart[] array = this.firstPersonViewParts;
				return array != null && array.Length > 0;
			}
		}

		public bool hasLocalRigParts
		{
			get
			{
				CosmeticPart[] array = this.localRigParts;
				return array != null && array.Length > 0;
			}
		}

		public CosmeticInfoV2(string displayName)
		{
			this.enabled = true;
			this.season = null;
			this.displayName = displayName;
			this.playFabID = "";
			this.category = CosmeticsController.CosmeticCategory.None;
			this.icon = null;
			this.isHoldable = false;
			this.isThrowable = false;
			this.usesBothHandSlots = false;
			this.hideWardrobeMannequin = false;
			this.holdableParts = new CosmeticPart[0];
			this.functionalParts = new CosmeticPart[0];
			this.wardrobeParts = new CosmeticPart[0];
			this.storeParts = new CosmeticPart[0];
			this.firstPersonViewParts = new CosmeticPart[0];
			this.localRigParts = new CosmeticPart[0];
			this.setCosmetics = new CosmeticSO[0];
			this.anchorAntiIntersectOffsets = default(CosmeticAnchorAntiIntersectOffsets);
			this.debugCosmeticSOName = "__UNINITIALIZED__";
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			this._OnAfterDeserialize_InitializePartsArray(ref this.holdableParts, ECosmeticPartType.Holdable);
			this._OnAfterDeserialize_InitializePartsArray(ref this.functionalParts, ECosmeticPartType.Functional);
			this._OnAfterDeserialize_InitializePartsArray(ref this.wardrobeParts, ECosmeticPartType.Wardrobe);
			this._OnAfterDeserialize_InitializePartsArray(ref this.storeParts, ECosmeticPartType.Store);
			this._OnAfterDeserialize_InitializePartsArray(ref this.firstPersonViewParts, ECosmeticPartType.FirstPerson);
			this._OnAfterDeserialize_InitializePartsArray(ref this.localRigParts, ECosmeticPartType.LocalRig);
			if (this.setCosmetics == null)
			{
				this.setCosmetics = Array.Empty<CosmeticSO>();
			}
		}

		private void _OnAfterDeserialize_InitializePartsArray(ref CosmeticPart[] parts, ECosmeticPartType partType)
		{
			for (int i = 0; i < parts.Length; i++)
			{
				parts[i].partType = partType;
				ref CosmeticAttachInfo[] ptr = ref parts[i].attachAnchors;
				if (ptr == null)
				{
					ptr = Array.Empty<CosmeticAttachInfo>();
				}
			}
		}

		public bool enabled;

		[Tooltip("// TODO: (2024-09-27 MattO) season will determine what addressables bundle it will be in and wheter it should be active based on release time of season.\n\nThe assigned season will determine what folder the Cosmetic will go in and how it will be listed in the Cosmetic Browser.")]
		[Delayed]
		public SeasonSO season;

		[Tooltip("Name that is displayed in the store during purchasing.")]
		[Delayed]
		public string displayName;

		[Tooltip("ID used on the PlayFab servers that must be unique. If this does not exist on the playfab servers then an error will be thrown. In notion search for \"Cosmetics - Adding a PlayFab ID\".")]
		[Delayed]
		public string playFabID;

		public Sprite icon;

		[Tooltip("Category determines which category button in the user's wardrobe (which are the two rows of buttons with equivalent names) have to be pressed to access the cosmetic along with others in the same category.")]
		public StringEnum<CosmeticsController.CosmeticCategory> category;

		[Obsolete("(2024-08-13 MattO) Will be removed after holdables array is fully implemented. Check length of `holdableParts` instead.")]
		[HideInInspector]
		public bool isHoldable;

		public bool isThrowable;

		public bool usesBothHandSlots;

		public bool hideWardrobeMannequin;

		public const string holdableParts_infoBoxShortMsg = "\"Holdable Parts\" must have a Holdable component (or inherits like TransferrableObject).";

		public const string holdableParts_infoBoxDetailedMsg = "\"Holdable Parts\" must have a Holdable component (or inherits like TransferrableObject).\n\nHoldables are prefabs that have Holdable components. The prefab asset's transform will be moved between the listed \n attach points on \"Gorilla Player Networked.prefab\" when grabbed by the player \n";

		[Space]
		[Tooltip("\"Holdable Parts\" must have a Holdable component (or inherits like TransferrableObject).\n\nHoldables are prefabs that have Holdable components. The prefab asset's transform will be moved between the listed \n attach points on \"Gorilla Player Networked.prefab\" when grabbed by the player \n")]
		public CosmeticPart[] holdableParts;

		public const string functionalParts_infoBoxShortMsg = "\"Wearable Parts\" will be attached to \"Gorilla Player Networked.prefab\" instances.";

		public const string functionalParts_infoBoxDetailedMsg = "\"Wearable Parts\" will be attached to \"Gorilla Player Networked.prefab\" instances.\n\nThese individual parts which also handle the core functionality of the cosmetic. In most cases there will only be one part, there can be multiple parts for cases like rings which might be on both left and right hands.\n\nThese parts will be parented to the bones of  \"Gorilla Player Networked.prefab\" instances which includes the VRRig component.\n\nIf a \"First Person View\" part or \"Local Rig Part\" is set it will be enabled instead of the wearable parts for the local player";

		[Space]
		[Tooltip("\"Wearable Parts\" will be attached to \"Gorilla Player Networked.prefab\" instances.\n\nThese individual parts which also handle the core functionality of the cosmetic. In most cases there will only be one part, there can be multiple parts for cases like rings which might be on both left and right hands.\n\nThese parts will be parented to the bones of  \"Gorilla Player Networked.prefab\" instances which includes the VRRig component.\n\nIf a \"First Person View\" part or \"Local Rig Part\" is set it will be enabled instead of the wearable parts for the local player")]
		public CosmeticPart[] functionalParts;

		public const string wardrobeParts_infoBoxShortMsg = "\"Wardrobe Parts\" will be attached to \"Head Model.prefab\" instances.";

		public const string wardrobeParts_infoBoxDetailedMsg = "\"Wardrobe Parts\" will be attached to \"Head Model.prefab\" instances.\n\nThese parts should be static meshes not skinned and not have any scripts attached. They should only be simple visual representations.\n\nThese prefabs are shown on the satellite wardrobe, and in the store (if \"Store Parts\" is left empty)";

		[Space]
		[Tooltip("\"Wardrobe Parts\" will be attached to \"Head Model.prefab\" instances.\n\nThese parts should be static meshes not skinned and not have any scripts attached. They should only be simple visual representations.\n\nThese prefabs are shown on the satellite wardrobe, and in the store (if \"Store Parts\" is left empty)")]
		public CosmeticPart[] wardrobeParts;

		public const string storeParts_infoBoxShortMsg = "\"Store Parts\" are spawned into the Dynamic Cosmetic Stands in city.";

		public const string storeParts_infoBoxDetailedMsg = "\"Store Parts\" are spawned into the Dynamic Cosmetic Stands in city.\nStore parts only need to be specified if the store display should be different than the wardrobe display";

		[Space]
		[Tooltip("\"Store Parts\" are spawned into the Dynamic Cosmetic Stands in city.\nStore parts only need to be specified if the store display should be different than the wardrobe display")]
		public CosmeticPart[] storeParts;

		public const string firstPersonViewParts_infoBoxShortMsg = "\"First Person View Parts\" will be attached to the local monke's camera.\nFirst person parts are enabled instead of \"Wearable Parts\" for the local player";

		public const string firstPersonViewParts_infoBoxDetailedMsg = "\"First Person View Parts\" will be attached to the local monke's camera.\nFirst person parts are enabled instead of \"Wearable Parts\" for the local player\nThese are used for any peripheral view meshes on the No Mirror layer, usually on HAT or FACE items";

		[Space]
		[Tooltip("\"First Person View Parts\" will be attached to the local monke's camera.\nFirst person parts are enabled instead of \"Wearable Parts\" for the local player\nThese are used for any peripheral view meshes on the No Mirror layer, usually on HAT or FACE items")]
		public CosmeticPart[] firstPersonViewParts;

		public const string localRigParts_infoBoxShortMsg = "\"Local Mirror Parts\" will be attached to the local player's rig instead of \"Wearable Parts\".";

		public const string localRigParts_infoBoxDetailedMsg = "\"Local Mirror Parts\" will be attached to the local player's rig instead of \"Wearable Parts\".\nThese objects can be used in addition to first person view parts.\nThese can be used for mirror view meshes (usually HAT or FACE items)\nAny item with GTPosRotConstraints should be parented to the rig and not the camera";

		[Space]
		[Tooltip("\"Local Mirror Parts\" will be attached to the local player's rig instead of \"Wearable Parts\".\nThese objects can be used in addition to first person view parts.\nThese can be used for mirror view meshes (usually HAT or FACE items)\nAny item with GTPosRotConstraints should be parented to the rig and not the camera")]
		public CosmeticPart[] localRigParts;

		[Space]
		[Tooltip("When this cosmetic is equipped, these offsets will be applied to the other objects on the player that are likely to clip\nSHIRT items ususally offset the badge, nametag, and chest items\n PAW items usually offset the hunt computer and builder watch")]
		public CosmeticAnchorAntiIntersectOffsets anchorAntiIntersectOffsets;

		[Space]
		[Tooltip("TODO COMMENT")]
		public CosmeticSO[] setCosmetics;

		[NonSerialized]
		public string debugCosmeticSOName;
	}
}
