using System;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.Serialization;

public class GorillaSkin : ScriptableObject
{
	public Mesh bodyMesh
	{
		get
		{
			return this._bodyMesh;
		}
	}

	public bool allowHeadless
	{
		get
		{
			return !this._disableHeadless;
		}
	}

	public static GorillaSkin CopyWithInstancedMaterials(GorillaSkin basis)
	{
		GorillaSkin gorillaSkin = ScriptableObject.CreateInstance<GorillaSkin>();
		gorillaSkin._chestMaterial = ((basis._chestMaterial != null) ? new Material(basis._chestMaterial) : null);
		gorillaSkin._bodyMaterial = ((basis._bodyMaterial != null) ? new Material(basis._bodyMaterial) : null);
		gorillaSkin._scoreboardMaterial = ((basis._scoreboardMaterial != null) ? new Material(basis._scoreboardMaterial) : null);
		gorillaSkin._bodyMesh = basis.bodyMesh;
		return gorillaSkin;
	}

	public Material bodyMaterial
	{
		get
		{
			return this._bodyMaterial;
		}
	}

	public Material chestMaterial
	{
		get
		{
			return this._chestMaterial;
		}
	}

	public Material scoreboardMaterial
	{
		get
		{
			return this._scoreboardMaterial;
		}
	}

	public static void ShowActiveSkin(VRRig rig)
	{
		bool flag;
		GorillaSkin activeSkin = GorillaSkin.GetActiveSkin(rig, out flag);
		GorillaSkin.ShowSkin(rig, activeSkin, flag);
	}

	public void ApplySkinToMannequin(GameObject mannequin, bool swapMesh = false)
	{
		SkinnedMeshRenderer skinnedMeshRenderer;
		if (!mannequin.TryGetComponent<SkinnedMeshRenderer>(out skinnedMeshRenderer))
		{
			MeshRenderer meshRenderer;
			if (mannequin.TryGetComponent<MeshRenderer>(out meshRenderer))
			{
				Material[] sharedMaterials = meshRenderer.sharedMaterials;
				sharedMaterials[0] = this.bodyMaterial;
				sharedMaterials[1] = this.chestMaterial;
				meshRenderer.sharedMaterials = sharedMaterials;
			}
			return;
		}
		int subMeshCount = skinnedMeshRenderer.sharedMesh.subMeshCount;
		if (swapMesh && this.bodyMesh != null)
		{
			skinnedMeshRenderer.sharedMesh = this.bodyMesh;
		}
		int subMeshCount2 = skinnedMeshRenderer.sharedMesh.subMeshCount;
		if (subMeshCount == subMeshCount2)
		{
			Material[] sharedMaterials2 = skinnedMeshRenderer.sharedMaterials;
			sharedMaterials2[0] = this.bodyMaterial;
			if (subMeshCount > 2)
			{
				sharedMaterials2[1] = this.chestMaterial;
			}
			skinnedMeshRenderer.sharedMaterials = sharedMaterials2;
			return;
		}
		if (skinnedMeshRenderer.sharedMaterials.Length == subMeshCount)
		{
			if (subMeshCount2 == 2 && subMeshCount > subMeshCount2)
			{
				Material[] array = new Material[]
				{
					this.bodyMaterial,
					skinnedMeshRenderer.sharedMaterials[2]
				};
				skinnedMeshRenderer.sharedMaterials = array;
				return;
			}
			if (subMeshCount2 == 3 && subMeshCount < subMeshCount2 && skinnedMeshRenderer.sharedMaterials.Length > 1)
			{
				Material[] array2 = new Material[]
				{
					this.bodyMaterial,
					this.chestMaterial,
					skinnedMeshRenderer.sharedMaterials[1]
				};
				skinnedMeshRenderer.sharedMaterials = array2;
				return;
			}
			Debug.LogError(string.Format("Unexpected Submesh count {0} {1}", subMeshCount, subMeshCount2));
			return;
		}
		else
		{
			if (subMeshCount2 == 2)
			{
				Material[] array3 = new Material[] { this.bodyMaterial };
				skinnedMeshRenderer.sharedMaterials = array3;
				return;
			}
			if (subMeshCount2 == 3)
			{
				Material[] array4 = new Material[] { this.bodyMaterial, this.chestMaterial };
				skinnedMeshRenderer.sharedMaterials = array4;
				return;
			}
			Debug.LogError(string.Format("Unexpected Submesh count {0}", subMeshCount2));
			return;
		}
	}

	public static GorillaSkin GetActiveSkin(VRRig rig, out bool useDefaultBodySkin)
	{
		if (rig.CurrentModeSkin.IsNotNull())
		{
			useDefaultBodySkin = false;
			return rig.CurrentModeSkin;
		}
		if (rig.TemporaryEffectSkin.IsNotNull())
		{
			useDefaultBodySkin = false;
			return rig.TemporaryEffectSkin;
		}
		if (rig.CurrentCosmeticSkin.IsNotNull())
		{
			useDefaultBodySkin = false;
			return rig.CurrentCosmeticSkin;
		}
		useDefaultBodySkin = true;
		return rig.defaultSkin;
	}

	public static void ShowSkin(VRRig rig, GorillaSkin skin, bool useDefaultBodySkin = false)
	{
		if (skin.bodyMesh != null)
		{
			rig.bodyRenderer.SetCosmeticBodyMesh(skin.bodyMesh);
		}
		else
		{
			rig.bodyRenderer.ClearCosmeticBodyMesh();
		}
		if (useDefaultBodySkin)
		{
			rig.materialsToChangeTo[0] = rig.myDefaultSkinMaterialInstance;
		}
		else
		{
			rig.materialsToChangeTo[0] = skin.bodyMaterial;
		}
		rig.bodyRenderer.SetSkinMaterials(rig.materialsToChangeTo[rig.setMatIndex], skin.chestMaterial, skin.allowHeadless);
		rig.scoreboardMaterial = skin.scoreboardMaterial;
	}

	public static void ApplyToRig(VRRig rig, GorillaSkin skin, GorillaSkin.SkinType type)
	{
		bool flag;
		GorillaSkin activeSkin = GorillaSkin.GetActiveSkin(rig, out flag);
		switch (type)
		{
		case GorillaSkin.SkinType.cosmetic:
			rig.CurrentCosmeticSkin = skin;
			break;
		case GorillaSkin.SkinType.gameMode:
			rig.CurrentModeSkin = skin;
			break;
		case GorillaSkin.SkinType.temporaryEffect:
			rig.TemporaryEffectSkin = skin;
			break;
		default:
			Debug.LogError("Unknown skin slot");
			break;
		}
		bool flag2;
		GorillaSkin activeSkin2 = GorillaSkin.GetActiveSkin(rig, out flag2);
		if (activeSkin != activeSkin2)
		{
			GorillaSkin.ShowSkin(rig, activeSkin2, flag2);
		}
	}

	[FormerlySerializedAs("chestMaterial")]
	[FormerlySerializedAs("chestEarsMaterial")]
	[SerializeField]
	private Material _chestMaterial;

	[FormerlySerializedAs("bodyMaterial")]
	[SerializeField]
	private Material _bodyMaterial;

	[SerializeField]
	private Material _scoreboardMaterial;

	[Tooltip("Check this if skin materials are incompatible with HeadlessMonkeRig mesh")]
	[SerializeField]
	private bool _disableHeadless;

	[Space]
	[SerializeField]
	private Mesh _bodyMesh;

	[Space]
	[NonSerialized]
	private Material _bodyRuntime;

	[NonSerialized]
	private Material _chestRuntime;

	[NonSerialized]
	private Material _scoreRuntime;

	public enum SkinType
	{
		cosmetic,
		gameMode,
		temporaryEffect
	}
}
