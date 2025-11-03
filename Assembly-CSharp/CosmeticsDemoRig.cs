using System;
using System.Collections.Generic;
using GorillaTag.CosmeticSystem;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class CosmeticsDemoRig : MonoBehaviour
{
	[SerializeField]
	private VRRig _vrRig;

	private Transform[] _vrRigBoneXforms;

	private Transform[] _vrRigSlotXforms;

	[SerializeField]
	private Transform chestOffset;

	[SerializeField]
	private Transform leftArmOffset;

	[SerializeField]
	private Transform rightArmOffset;

	private Vector3 badgeDefaultPos;

	private Quaternion badgeDefaultRot;

	private bool isInitialized;

	private CosmeticsDemoRig.EdSpawnedCosmetic emptyCosmetic;

	private Material defaultFaceMaterial;

	[SerializeField]
	[HideInInspector]
	private Material myDefaultSkinMaterialInstance;

	[SerializeField]
	[HideInInspector]
	private Material materialToChangeTo0;

	[SerializeField]
	[HideInInspector]
	private Color monkeColor = new Color(0f, 0f, 0f);

	[SerializeField]
	[HideInInspector]
	private GorillaSkin currentSkin;

	[SerializeField]
	[HideInInspector]
	private GorillaSkin defaultSkin;

	[SerializeField]
	[HideInInspector]
	private Material[] faceMaterialSwaps = new Material[10];

	[HideInInspector]
	public int materialIndex;

	private int selectedMouth;

	[HideInInspector]
	public UnityEvent<Color> OnColorChange;

	[SerializeField]
	private CosmeticsDemoRig.EdSpawnedCosmetic[] spawnedCosmetics = new CosmeticsDemoRig.EdSpawnedCosmetic[16];

	[Serializable]
	private struct EdSpawnedCosmetic
	{
		public string itemName;

		public CosmeticSO so;

		public List<GameObject> objects;

		public List<GameObject> holdableObjects;

		public bool isEmpty;
	}
}
