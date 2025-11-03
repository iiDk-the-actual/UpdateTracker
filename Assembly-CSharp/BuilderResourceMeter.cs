using System;
using GorillaTagScripts;
using UnityEngine;
using UnityEngine.Serialization;

public class BuilderResourceMeter : MonoBehaviour
{
	private void Awake()
	{
		this.fillColor = this.resourceColors.GetColorForType(this._resourceType);
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		this.fillCube.GetPropertyBlock(materialPropertyBlock);
		materialPropertyBlock.SetColor(ShaderProps._BaseColor, this.fillColor);
		this.fillCube.SetPropertyBlock(materialPropertyBlock);
		materialPropertyBlock.SetColor(ShaderProps._BaseColor, this.emptyColor);
		this.emptyCube.SetPropertyBlock(materialPropertyBlock);
		this.fillAmount = this.fillTarget;
	}

	private void Start()
	{
		ZoneManagement instance = ZoneManagement.instance;
		instance.onZoneChanged = (Action)Delegate.Combine(instance.onZoneChanged, new Action(this.OnZoneChanged));
		this.OnZoneChanged();
	}

	private void OnDestroy()
	{
		if (ZoneManagement.instance != null)
		{
			ZoneManagement instance = ZoneManagement.instance;
			instance.onZoneChanged = (Action)Delegate.Remove(instance.onZoneChanged, new Action(this.OnZoneChanged));
		}
	}

	private void OnZoneChanged()
	{
		bool flag = ZoneManagement.instance.IsZoneActive(GTZone.monkeBlocks);
		if (flag != this.inBuilderZone)
		{
			this.inBuilderZone = flag;
			if (!flag)
			{
				this.fillCube.enabled = false;
				this.emptyCube.enabled = false;
				return;
			}
			this.fillCube.enabled = true;
			this.emptyCube.enabled = true;
			this.OnAvailableResourcesChange();
		}
	}

	public void OnAvailableResourcesChange()
	{
		if (this.table == null || this.table.maxResources == null)
		{
			return;
		}
		this.resourceMax = this.table.maxResources[(int)this._resourceType];
		int num = this.table.usedResources[(int)this._resourceType];
		if (num != this.usedResource)
		{
			this.usedResource = num;
			this.SetNormalizedFillTarget((float)(this.resourceMax - this.usedResource) / (float)this.resourceMax);
		}
	}

	public void UpdateMeterFill()
	{
		if (this.animatingMeter)
		{
			float num = Mathf.MoveTowards(this.fillAmount, this.fillTarget, this.lerpSpeed * Time.deltaTime);
			this.UpdateFill(num);
		}
	}

	private void UpdateFill(float newFill)
	{
		this.fillAmount = newFill;
		if (Mathf.Approximately(this.fillAmount, this.fillTarget))
		{
			this.fillAmount = this.fillTarget;
			this.animatingMeter = false;
		}
		if (!this.inBuilderZone)
		{
			return;
		}
		if (this.fillAmount <= 1E-45f)
		{
			this.fillCube.enabled = false;
			float num = this.meterHeight / this.meshHeight;
			Vector3 vector = new Vector3(this.emptyCube.transform.localScale.x, num, this.emptyCube.transform.localScale.z);
			Vector3 vector2 = new Vector3(0f, this.meterHeight / 2f, 0f);
			this.emptyCube.transform.localScale = vector;
			this.emptyCube.transform.localPosition = vector2;
			this.emptyCube.enabled = true;
			return;
		}
		if (this.fillAmount >= 1f)
		{
			float num2 = this.meterHeight / this.meshHeight;
			Vector3 vector3 = new Vector3(this.fillCube.transform.localScale.x, num2, this.fillCube.transform.localScale.z);
			Vector3 vector4 = new Vector3(0f, this.meterHeight / 2f, 0f);
			this.fillCube.transform.localScale = vector3;
			this.fillCube.transform.localPosition = vector4;
			this.fillCube.enabled = true;
			this.emptyCube.enabled = false;
			return;
		}
		float num3 = this.meterHeight / this.meshHeight * this.fillAmount;
		Vector3 vector5 = new Vector3(this.fillCube.transform.localScale.x, num3, this.fillCube.transform.localScale.z);
		Vector3 vector6 = new Vector3(0f, num3 * this.meshHeight / 2f, 0f);
		this.fillCube.transform.localScale = vector5;
		this.fillCube.transform.localPosition = vector6;
		this.fillCube.enabled = true;
		float num4 = this.meterHeight / this.meshHeight * (1f - this.fillAmount);
		Vector3 vector7 = new Vector3(this.emptyCube.transform.localScale.x, num4, this.emptyCube.transform.localScale.z);
		Vector3 vector8 = new Vector3(0f, this.meterHeight - num4 * this.meshHeight / 2f, 0f);
		this.emptyCube.transform.localScale = vector7;
		this.emptyCube.transform.localPosition = vector8;
		this.emptyCube.enabled = true;
	}

	public void SetNormalizedFillTarget(float fill)
	{
		this.fillTarget = Mathf.Clamp(fill, 0f, 1f);
		this.animatingMeter = true;
	}

	public BuilderResourceColors resourceColors;

	public MeshRenderer fillCube;

	public MeshRenderer emptyCube;

	private Color fillColor = Color.white;

	public Color emptyColor = Color.black;

	[FormerlySerializedAs("MeterHeight")]
	public float meterHeight = 2f;

	public float meshHeight = 1f;

	public BuilderResourceType _resourceType;

	private float fillAmount;

	[Range(0f, 1f)]
	[SerializeField]
	private float fillTarget;

	public float lerpSpeed = 0.5f;

	private bool animatingMeter;

	private int resourceMax = -1;

	private int usedResource = -1;

	private bool inBuilderZone;

	internal BuilderTable table;
}
