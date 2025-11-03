using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ApplyMaterialProperty : MonoBehaviour
{
	private void Start()
	{
		this.UpdateShaderPropertyIds();
		if (this.applyOnStart)
		{
			this.Apply();
		}
	}

	public void Apply()
	{
		if (!this._renderer)
		{
			this._renderer = base.GetComponent<Renderer>();
		}
		ApplyMaterialProperty.ApplyMode applyMode = this.mode;
		if (applyMode == ApplyMaterialProperty.ApplyMode.MaterialInstance)
		{
			this.ApplyMaterialInstance();
			return;
		}
		if (applyMode != ApplyMaterialProperty.ApplyMode.MaterialPropertyBlock)
		{
			return;
		}
		this.ApplyMaterialPropertyBlock();
	}

	public void SetColor(string propertyName, Color color)
	{
		this.SetColor(Shader.PropertyToID(propertyName), color);
	}

	public void SetColor(int propertyId, Color color)
	{
		ApplyMaterialProperty.CustomMaterialData orCreateData = this.GetOrCreateData(propertyId, null);
		orCreateData.dataType = ApplyMaterialProperty.SuportedTypes.Color;
		orCreateData.color = color;
	}

	public void SetFloat(string propertyName, float value)
	{
		this.SetFloat(Shader.PropertyToID(propertyName), value);
	}

	public void SetFloat(int propertyId, float value)
	{
		ApplyMaterialProperty.CustomMaterialData orCreateData = this.GetOrCreateData(propertyId, null);
		orCreateData.dataType = ApplyMaterialProperty.SuportedTypes.Float;
		orCreateData.@float = value;
	}

	private ApplyMaterialProperty.CustomMaterialData GetOrCreateData(int id, string propertyName)
	{
		for (int i = 0; i < this.customData.Count; i++)
		{
			if (this.customData[i].id == id)
			{
				return this.customData[i];
			}
		}
		ApplyMaterialProperty.CustomMaterialData customMaterialData = new ApplyMaterialProperty.CustomMaterialData(id, propertyName);
		this.customData.Add(customMaterialData);
		return customMaterialData;
	}

	private void ApplyMaterialInstance()
	{
		if (!this._instance)
		{
			this._instance = base.GetComponent<MaterialInstance>();
			if (this._instance == null)
			{
				this._instance = base.gameObject.AddComponent<MaterialInstance>();
			}
		}
		Material material = (this.targetMaterial = this._instance.Material);
		for (int i = 0; i < this.customData.Count; i++)
		{
			switch (this.customData[i].dataType)
			{
			case ApplyMaterialProperty.SuportedTypes.Color:
				material.SetColor(this.customData[i].id, this.customData[i].color);
				break;
			case ApplyMaterialProperty.SuportedTypes.Float:
				material.SetFloat(this.customData[i].id, this.customData[i].@float);
				break;
			case ApplyMaterialProperty.SuportedTypes.Vector2:
				material.SetVector(this.customData[i].id, this.customData[i].vector2);
				break;
			case ApplyMaterialProperty.SuportedTypes.Vector3:
				material.SetVector(this.customData[i].id, this.customData[i].vector3);
				break;
			case ApplyMaterialProperty.SuportedTypes.Vector4:
				material.SetVector(this.customData[i].id, this.customData[i].vector4);
				break;
			case ApplyMaterialProperty.SuportedTypes.Texture2D:
				material.SetTexture(this.customData[i].id, this.customData[i].texture2D);
				break;
			}
		}
		this._renderer.SetPropertyBlock(this._block);
	}

	private void ApplyMaterialPropertyBlock()
	{
		if (this._block == null)
		{
			this._block = new MaterialPropertyBlock();
		}
		this._renderer.GetPropertyBlock(this._block);
		for (int i = 0; i < this.customData.Count; i++)
		{
			switch (this.customData[i].dataType)
			{
			case ApplyMaterialProperty.SuportedTypes.Color:
				this._block.SetColor(this.customData[i].id, this.customData[i].color);
				break;
			case ApplyMaterialProperty.SuportedTypes.Float:
				this._block.SetFloat(this.customData[i].id, this.customData[i].@float);
				break;
			case ApplyMaterialProperty.SuportedTypes.Vector2:
				this._block.SetVector(this.customData[i].id, this.customData[i].vector2);
				break;
			case ApplyMaterialProperty.SuportedTypes.Vector3:
				this._block.SetVector(this.customData[i].id, this.customData[i].vector3);
				break;
			case ApplyMaterialProperty.SuportedTypes.Vector4:
				this._block.SetVector(this.customData[i].id, this.customData[i].vector4);
				break;
			case ApplyMaterialProperty.SuportedTypes.Texture2D:
				this._block.SetTexture(this.customData[i].id, this.customData[i].texture2D);
				break;
			}
		}
		this._renderer.SetPropertyBlock(this._block);
	}

	private void UpdateShaderPropertyIds()
	{
		for (int i = 0; i < this.customData.Count; i++)
		{
			if (this.customData[i] != null && !string.IsNullOrEmpty(this.customData[i].name))
			{
				this.customData[i].id = Shader.PropertyToID(this.customData[i].name);
			}
		}
	}

	public ApplyMaterialProperty.ApplyMode mode = ApplyMaterialProperty.ApplyMode.MaterialPropertyBlock;

	[FormerlySerializedAs("materialToApplyBlock")]
	public Material targetMaterial;

	[SerializeField]
	private MaterialInstance _instance;

	[SerializeField]
	private Renderer _renderer;

	public List<ApplyMaterialProperty.CustomMaterialData> customData;

	[SerializeField]
	private bool applyOnStart;

	[NonSerialized]
	private MaterialPropertyBlock _block;

	public enum ApplyMode
	{
		MaterialInstance,
		MaterialPropertyBlock
	}

	public enum SuportedTypes
	{
		Color,
		Float,
		Vector2,
		Vector3,
		Vector4,
		Texture2D
	}

	[Serializable]
	public class CustomMaterialData
	{
		public CustomMaterialData(string propertyName)
		{
			this.name = propertyName;
			this.id = Shader.PropertyToID(propertyName);
			this.dataType = ApplyMaterialProperty.SuportedTypes.Color;
			this.color = default(Color);
			this.@float = 0f;
			this.vector2 = default(Vector2);
			this.vector3 = default(Vector3);
			this.vector4 = default(Vector4);
			this.texture2D = null;
		}

		public CustomMaterialData(int propertyId, string propertyName)
		{
			this.name = propertyName;
			this.id = propertyId;
			this.dataType = ApplyMaterialProperty.SuportedTypes.Color;
			this.color = default(Color);
			this.@float = 0f;
			this.vector2 = default(Vector2);
			this.vector3 = default(Vector3);
			this.vector4 = default(Vector4);
			this.texture2D = null;
		}

		public override int GetHashCode()
		{
			return new ValueTuple<int, ApplyMaterialProperty.SuportedTypes, Color, float, Vector2, Vector3, Vector4, ValueTuple<Texture2D>>(this.id, this.dataType, this.color, this.@float, this.vector2, this.vector3, this.vector4, new ValueTuple<Texture2D>(this.texture2D)).GetHashCode();
		}

		public string name;

		public int id;

		public ApplyMaterialProperty.SuportedTypes dataType;

		public Color color;

		public float @float;

		public Vector2 vector2;

		public Vector3 vector3;

		public Vector4 vector4;

		public Texture2D texture2D;
	}
}
