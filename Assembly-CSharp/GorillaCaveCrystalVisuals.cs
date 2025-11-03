using System;
using GorillaTagScripts;
using UnityEngine;

public class GorillaCaveCrystalVisuals : MonoBehaviour
{
	public float lerp
	{
		get
		{
			return this._lerp;
		}
		set
		{
			this._lerp = value;
		}
	}

	public void Setup()
	{
		base.TryGetComponent<MeshRenderer>(out this._renderer);
		if (this._renderer == null)
		{
			return;
		}
		this._setup = GorillaCaveCrystalSetup.Instance;
		this._sharedMaterial = this._renderer.sharedMaterial;
		this._initialized = this.crysalPreset != null && this._renderer != null && this._sharedMaterial != null;
		this.Update();
	}

	private void Start()
	{
		this.UpdateAlbedo();
		this.ForceUpdate();
	}

	public void UpdateAlbedo()
	{
		if (!this._initialized)
		{
			return;
		}
		if (this.instanceAlbedo == null)
		{
			return;
		}
		if (this._block == null)
		{
			this._block = new MaterialPropertyBlock();
		}
		this._renderer.GetPropertyBlock(this._block);
		this._block.SetTexture(GorillaCaveCrystalVisuals._MainTex, this.instanceAlbedo);
		this._renderer.SetPropertyBlock(this._block);
	}

	private void Awake()
	{
		this.UpdateAlbedo();
		this.Update();
	}

	private void Update()
	{
		if (!this._initialized)
		{
			return;
		}
		if (Application.isPlaying)
		{
			int hashCode = new ValueTuple<CrystalVisualsPreset, float>(this.crysalPreset, this._lerp).GetHashCode();
			if (this._lastState == hashCode)
			{
				return;
			}
			this._lastState = hashCode;
		}
		if (this._block == null)
		{
			this._block = new MaterialPropertyBlock();
		}
		CrystalVisualsPreset.VisualState stateA = this.crysalPreset.stateA;
		CrystalVisualsPreset.VisualState stateB = this.crysalPreset.stateB;
		Color color = Color.Lerp(stateA.albedo, stateB.albedo, this._lerp);
		Color color2 = Color.Lerp(stateA.emission, stateB.emission, this._lerp);
		this._renderer.GetPropertyBlock(this._block);
		this._block.SetColor(GorillaCaveCrystalVisuals._Color, color);
		this._block.SetColor(GorillaCaveCrystalVisuals._EmissionColor, color2);
		this._renderer.SetPropertyBlock(this._block);
	}

	public void ForceUpdate()
	{
		this._lastState = 0;
		this.Update();
	}

	private static void InitializeCrystals()
	{
		foreach (GorillaCaveCrystalVisuals gorillaCaveCrystalVisuals in Object.FindObjectsByType<GorillaCaveCrystalVisuals>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID))
		{
			gorillaCaveCrystalVisuals.UpdateAlbedo();
			gorillaCaveCrystalVisuals.ForceUpdate();
			gorillaCaveCrystalVisuals._lastState = -1;
		}
	}

	public CrystalVisualsPreset crysalPreset;

	[SerializeField]
	[Range(0f, 1f)]
	private float _lerp;

	[Space]
	public MeshRenderer _renderer;

	public Material _sharedMaterial;

	[SerializeField]
	public Texture2D instanceAlbedo;

	[SerializeField]
	private bool _initialized;

	[SerializeField]
	private int _lastState;

	[SerializeField]
	public GorillaCaveCrystalSetup _setup;

	private MaterialPropertyBlock _block;

	[NonSerialized]
	private bool _ranSetupOnce;

	private static readonly ShaderHashId _Color = "_Color";

	private static readonly ShaderHashId _EmissionColor = "_EmissionColor";

	private static readonly ShaderHashId _MainTex = "_MainTex";
}
