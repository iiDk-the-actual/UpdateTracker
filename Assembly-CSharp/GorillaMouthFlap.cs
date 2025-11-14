using System;
using UnityEngine;

public class GorillaMouthFlap : MonoBehaviour, IGorillaSliceableSimple
{
	private void Start()
	{
		this.speaker = base.GetComponent<GorillaSpeakerLoudness>();
		this.targetFaceRenderer = this.targetFace.GetComponent<Renderer>();
		this.facePropBlock = new MaterialPropertyBlock();
		this.hasDefaultMouthAtlas = false;
		if (this.targetFaceRenderer != null)
		{
			this.SetDefaultMouthAtlas(this.targetFaceRenderer.material);
		}
	}

	public void EnableLeafBlower()
	{
		this.leafBlowerActiveUntilTimestamp = Time.time + 0.1f;
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
		this.lastTimeUpdated = Time.time;
		this.deltaTime = Time.deltaTime;
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void SliceUpdate()
	{
		this.deltaTime = Time.time - this.lastTimeUpdated;
		this.lastTimeUpdated = Time.time;
		if (this.speaker == null)
		{
			this.speaker = base.GetComponent<GorillaSpeakerLoudness>();
			return;
		}
		float num = 0f;
		if (this.speaker.IsSpeaking)
		{
			num = this.speaker.Loudness;
		}
		this.CheckMouthflapChange(this.speaker.IsMicEnabled, num);
		MouthFlapLevel mouthFlapLevel = this.noMicFace;
		if (this.leafBlowerActiveUntilTimestamp > Time.time)
		{
			mouthFlapLevel = this.leafBlowerFace;
		}
		else if (this.useMicEnabled)
		{
			mouthFlapLevel = this.mouthFlapLevels[this.activeFlipbookIndex];
		}
		this.UpdateMouthFlapFlipbook(mouthFlapLevel);
	}

	private void CheckMouthflapChange(bool isMicEnabled, float currentLoudness)
	{
		if (isMicEnabled)
		{
			this.useMicEnabled = true;
			int i = this.mouthFlapLevels.Length - 1;
			while (i >= 0)
			{
				if (currentLoudness >= this.mouthFlapLevels[i].maxRequiredVolume)
				{
					return;
				}
				if (currentLoudness > this.mouthFlapLevels[i].minRequiredVolume)
				{
					if (this.activeFlipbookIndex != i)
					{
						this.activeFlipbookIndex = i;
						this.activeFlipbookPlayTime = 0f;
						return;
					}
					return;
				}
				else
				{
					i--;
				}
			}
			return;
		}
		if (this.useMicEnabled)
		{
			this.useMicEnabled = false;
			this.activeFlipbookPlayTime = 0f;
		}
	}

	private void UpdateMouthFlapFlipbook(MouthFlapLevel mouthFlap)
	{
		Material material = this.targetFaceRenderer.material;
		this.activeFlipbookPlayTime += this.deltaTime;
		this.activeFlipbookPlayTime %= mouthFlap.cycleDuration;
		int num = Mathf.FloorToInt(this.activeFlipbookPlayTime * (float)mouthFlap.faces.Length / mouthFlap.cycleDuration);
		material.SetTextureOffset(this._MouthMap, mouthFlap.faces[num]);
	}

	public void SetMouthTextureReplacement(Texture2D replacementMouthAtlas)
	{
		Material material = this.targetFaceRenderer.material;
		this.SetDefaultMouthAtlas(material);
		material.SetTexture(this._MouthMap, replacementMouthAtlas);
	}

	public void ClearMouthTextureReplacement()
	{
		this.targetFaceRenderer.material.SetTexture(this._MouthMap, this.defaultMouthAtlas);
	}

	public Material SetFaceMaterialReplacement(Material replacementFaceMaterial)
	{
		if (!this.hasDefaultFaceMaterial)
		{
			this.defaultFaceMaterial = this.targetFaceRenderer.material;
			this.hasDefaultFaceMaterial = true;
		}
		this.targetFaceRenderer.material = replacementFaceMaterial;
		return this.targetFaceRenderer.material;
	}

	public void ClearFaceMaterialReplacement()
	{
		if (this.hasDefaultFaceMaterial)
		{
			this.targetFaceRenderer.material = this.defaultFaceMaterial;
		}
	}

	private void SetDefaultMouthAtlas(Material face)
	{
		if (!this.hasDefaultMouthAtlas)
		{
			this.defaultMouthAtlas = face.GetTexture(this._MouthMap);
			this.hasDefaultMouthAtlas = true;
		}
	}

	public GameObject targetFace;

	public MouthFlapLevel[] mouthFlapLevels;

	public MouthFlapLevel noMicFace;

	public MouthFlapLevel leafBlowerFace;

	private bool useMicEnabled;

	private float leafBlowerActiveUntilTimestamp;

	private int activeFlipbookIndex;

	private float activeFlipbookPlayTime;

	private GorillaSpeakerLoudness speaker;

	private float lastTimeUpdated;

	private float deltaTime;

	private Renderer targetFaceRenderer;

	private MaterialPropertyBlock facePropBlock;

	private Texture defaultMouthAtlas;

	private Material defaultFaceMaterial;

	private bool hasDefaultMouthAtlas;

	private bool hasDefaultFaceMaterial;

	private ShaderHashId _MouthMap = "_MouthMap";

	private ShaderHashId _BaseMap = "_BaseMap";
}
