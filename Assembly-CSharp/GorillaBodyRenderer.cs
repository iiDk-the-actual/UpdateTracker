using System;
using System.Collections.Generic;
using UnityEngine;

public class GorillaBodyRenderer : MonoBehaviour
{
	public GorillaBodyType bodyType
	{
		get
		{
			return this._bodyType;
		}
		set
		{
			this.SetBodyType(value);
		}
	}

	public bool renderFace
	{
		get
		{
			return this._renderFace;
		}
	}

	public static bool ForceSkeleton
	{
		get
		{
			return GorillaBodyRenderer.oopsAllSkeletons;
		}
	}

	public GorillaBodyType gameModeBodyType { get; private set; }

	public Material myDefaultSkinMaterialInstance { get; private set; }

	public SkinnedMeshRenderer GetBody(GorillaBodyType type)
	{
		if (type < GorillaBodyType.Default || type >= (GorillaBodyType)this._renderersCache.Length)
		{
			return null;
		}
		return this._renderersCache[(int)type];
	}

	public SkinnedMeshRenderer ActiveBody
	{
		get
		{
			return this.GetBody(this._bodyType);
		}
	}

	public static void SetAllSkeletons(bool allSkeletons)
	{
		GorillaBodyRenderer.oopsAllSkeletons = allSkeletons;
		GorillaTagger.Instance.offlineVRRig.bodyRenderer.Refresh();
		foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
		{
			vrrig.bodyRenderer.Refresh();
		}
	}

	public void SetSkeletonBodyActive(bool active)
	{
		this.bodySkeleton.gameObject.SetActive(active);
	}

	public void SetGameModeBodyType(GorillaBodyType bodyType)
	{
		if (this.gameModeBodyType == bodyType)
		{
			return;
		}
		this.gameModeBodyType = bodyType;
		this.Refresh();
	}

	public void SetCosmeticBodyType(GorillaBodyType bodyType)
	{
		if (this.cosmeticBodyType == bodyType)
		{
			return;
		}
		this.cosmeticBodyType = bodyType;
		this.Refresh();
	}

	public void SetDefaults()
	{
		this.gameModeBodyType = GorillaBodyType.Default;
		this.cosmeticBodyType = GorillaBodyType.Default;
		this.Refresh();
	}

	private void Refresh()
	{
		this.SetBodyType(this.GetActiveBodyType());
	}

	public void SetMaterialIndex(int materialIndex)
	{
		this._lastMatIndex = materialIndex;
		switch (this.bodyType)
		{
		case GorillaBodyType.Default:
			this.bodyDefault.sharedMaterial = this.rig.materialsToChangeTo[materialIndex];
			return;
		case GorillaBodyType.NoHead:
			if (materialIndex == 0 && !this._applySkinToHeadlessMesh)
			{
				this.bodyNoHead.sharedMaterial = this.myDefaultSkinMaterialInstance;
				return;
			}
			this.bodyNoHead.sharedMaterial = this.rig.materialsToChangeTo[materialIndex];
			return;
		case GorillaBodyType.Skeleton:
			this.rig.skeleton.SetMaterialIndex(materialIndex);
			return;
		default:
			return;
		}
	}

	public void SetSkinMaterials(Material bodyMat, Material chestMat, bool allowHeadless)
	{
		this.EnsureInstantiatedMaterial();
		if (chestMat == null)
		{
			if (this._cachedSkinMaterials.Length != 1)
			{
				this._cachedSkinMaterials = new Material[1];
			}
			this._cachedSkinMaterials[0] = bodyMat;
		}
		else
		{
			if (this._cachedSkinMaterials.Length < 2)
			{
				this._cachedSkinMaterials = new Material[2];
			}
			this._cachedSkinMaterials[0] = bodyMat;
			this._cachedSkinMaterials[1] = chestMat;
		}
		this._applySkinToHeadlessMesh = allowHeadless;
		GorillaBodyType bodyType = this.bodyType;
		if (bodyType == GorillaBodyType.Default)
		{
			this.bodyDefault.sharedMaterials = this._cachedSkinMaterials;
			this.bodyDefault.sharedMaterial = this.rig.materialsToChangeTo[this._lastMatIndex];
			return;
		}
		if (bodyType != GorillaBodyType.NoHead)
		{
			return;
		}
		if (this._applySkinToHeadlessMesh)
		{
			this.bodyNoHead.sharedMaterials = this._cachedSkinMaterials;
			this.bodyNoHead.sharedMaterial = this.rig.materialsToChangeTo[this._lastMatIndex];
			return;
		}
		this.bodyNoHead.sharedMaterials = this._defaultSkinMaterials;
		if (this._lastMatIndex != 0)
		{
			this.bodyNoHead.sharedMaterial = this.rig.materialsToChangeTo[this._lastMatIndex];
		}
	}

	public void SetupAsLocalPlayerBody()
	{
		this.faceRenderer.gameObject.layer = 22;
	}

	public GorillaBodyType GetActiveBodyType()
	{
		if (GorillaBodyRenderer.oopsAllSkeletons)
		{
			return GorillaBodyType.Skeleton;
		}
		if (this.gameModeBodyType == GorillaBodyType.Default)
		{
			return this.cosmeticBodyType;
		}
		return this.gameModeBodyType;
	}

	private void SetBodyType(GorillaBodyType type)
	{
		if (this._bodyType == type)
		{
			return;
		}
		this.SetBodyEnabled(this._bodyType, false);
		this._bodyType = type;
		this.SetBodyEnabled(type, true);
		this._renderFace = this._bodyType != GorillaBodyType.NoHead && this._bodyType != GorillaBodyType.Skeleton && this._bodyType != GorillaBodyType.Invisible;
		if (this.faceRenderer != null)
		{
			this.faceRenderer.enabled = this._renderFace;
		}
		switch (type)
		{
		case GorillaBodyType.Default:
			this.bodyDefault.sharedMaterials = this._cachedSkinMaterials;
			this.bodyDefault.sharedMaterial = this.rig.materialsToChangeTo[this._lastMatIndex];
			this.UpdateBodyMaterialColor(this.rig.playerColor);
			return;
		case GorillaBodyType.NoHead:
			if (this._applySkinToHeadlessMesh)
			{
				this.bodyNoHead.sharedMaterials = this._cachedSkinMaterials;
				this.bodyNoHead.sharedMaterial = this.rig.materialsToChangeTo[this._lastMatIndex];
			}
			else
			{
				this.bodyNoHead.sharedMaterials = this._defaultSkinMaterials;
				if (this._lastMatIndex != 0)
				{
					this.bodyNoHead.sharedMaterial = this.rig.materialsToChangeTo[this._lastMatIndex];
				}
			}
			this.UpdateBodyMaterialColor(this.rig.playerColor);
			return;
		case GorillaBodyType.Skeleton:
			this.rig.skeleton.SetMaterialIndex(this._lastMatIndex);
			this.rig.skeleton.UpdateColor(this.rig.playerColor);
			return;
		default:
			return;
		}
	}

	public void SetCosmeticBodyMesh(Mesh mesh)
	{
		if (this.defaultBodyMesh == null)
		{
			this.defaultBodyMesh = this.bodyDefault.sharedMesh;
		}
		this.bodyDefault.sharedMesh = mesh;
	}

	public void ClearCosmeticBodyMesh()
	{
		if (this.defaultBodyMesh != null)
		{
			this.bodyDefault.sharedMesh = this.defaultBodyMesh;
		}
	}

	private void SetBodyEnabled(GorillaBodyType bodyType, bool enabled)
	{
		SkinnedMeshRenderer body = this.GetBody(bodyType);
		if (body == null)
		{
			return;
		}
		body.enabled = enabled;
		Transform[] bones = body.bones;
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].gameObject.SetActive(enabled);
		}
	}

	private void Awake()
	{
		this.Setup();
	}

	public void SharedStart()
	{
		if (this.rig == null)
		{
			this.rig = base.GetComponentInParent<VRRig>();
		}
		this.EnsureInstantiatedMaterial();
	}

	private void Setup()
	{
		if (this.rig == null)
		{
			this.rig = base.GetComponentInParent<VRRig>();
		}
		this._renderersCache = new SkinnedMeshRenderer[EnumData<GorillaBodyType>.Shared.Values.Length];
		this._renderersCache[0] = this.bodyDefault;
		this._renderersCache[1] = this.bodyNoHead;
		this._renderersCache[2] = this.bodySkeleton;
		this.SetBodyEnabled(GorillaBodyType.Default, true);
		this.SetBodyEnabled(GorillaBodyType.NoHead, false);
		this.SetBodyEnabled(GorillaBodyType.Skeleton, false);
		this._cachedSkinMaterials = this.bodyDefault.sharedMaterials;
		this._bodyType = GorillaBodyType.Default;
		this._bodyType = GorillaBodyType.Default;
		this.defaultBodyMesh = this.bodyDefault.sharedMesh;
		this.EnsureInstantiatedMaterial();
		this.UpdateColor(this.rig.playerColor);
		this.Refresh();
	}

	public void EnsureInstantiatedMaterial()
	{
		if (this.myDefaultSkinMaterialInstance == null)
		{
			this.myDefaultSkinMaterialInstance = Object.Instantiate<Material>(this.rig.materialsToChangeTo[0]);
			this.rig.materialsToChangeTo[0] = this.myDefaultSkinMaterialInstance;
		}
		if (this._defaultSkinMaterials.Length == 0)
		{
			this._defaultSkinMaterials = new Material[2];
			this._defaultSkinMaterials[0] = this.myDefaultSkinMaterialInstance;
			this._defaultSkinMaterials[1] = this.rig.defaultSkin.chestMaterial;
		}
	}

	public void ResetBodyMaterial()
	{
		this.bodyDefault.sharedMaterial = this.rig.materialsToChangeTo[0];
		this.bodyNoHead.sharedMaterial = (this._applySkinToHeadlessMesh ? this.rig.materialsToChangeTo[0] : this.myDefaultSkinMaterialInstance);
	}

	public void UpdateColor(Color color)
	{
		this.UpdateBodyMaterialColor(color);
		if (this.bodyType == GorillaBodyType.Skeleton)
		{
			this.rig.skeleton.UpdateColor(color);
		}
	}

	private void UpdateBodyMaterialColor(Color color)
	{
		this.EnsureInstantiatedMaterial();
		if (this.myDefaultSkinMaterialInstance != null)
		{
			this.myDefaultSkinMaterialInstance.color = color;
		}
	}

	[SerializeField]
	private GorillaBodyType _bodyType;

	[SerializeField]
	private bool _renderFace = true;

	public MeshRenderer faceRenderer;

	[SerializeField]
	private SkinnedMeshRenderer bodyDefault;

	[SerializeField]
	private SkinnedMeshRenderer bodyNoHead;

	[SerializeField]
	private SkinnedMeshRenderer bodySkeleton;

	private int _lastMatIndex;

	private Mesh defaultBodyMesh;

	private static bool oopsAllSkeletons;

	private GorillaBodyType cosmeticBodyType;

	[SerializeField]
	private Material[] _cachedSkinMaterials = new Material[0];

	[SerializeField]
	private Material[] _defaultSkinMaterials = new Material[0];

	private bool _applySkinToHeadlessMesh;

	[Space]
	[NonSerialized]
	private SkinnedMeshRenderer[] _renderersCache = new SkinnedMeshRenderer[0];

	private static readonly List<Material> gEmptyDefaultMats = new List<Material>();

	[Space]
	public VRRig rig;
}
