using System;
using GorillaExtensions;
using UnityEngine;

namespace GorillaTag
{
	[ExecuteAlways]
	public class TextureTransitioner : MonoBehaviour, IResettableItem
	{
		protected void Awake()
		{
			if (Application.isPlaying || this.editorPreview)
			{
				TextureTransitionerManager.EnsureInstanceIsAvailable();
			}
			this.RefreshShaderParams();
			this.iDynamicFloat = (IDynamicFloat)this.dynamicFloatComponent;
			this.ResetToDefaultState();
		}

		protected void OnEnable()
		{
			TextureTransitionerManager.Register(this);
			if (Application.isPlaying && !this.remapInfo.IsValid())
			{
				Debug.LogError("Bad min/max values for remapRanges: " + this.GetComponentPath(int.MaxValue), this);
				base.enabled = false;
			}
			if (Application.isPlaying && this.textures.Length == 0)
			{
				Debug.LogError("Textures array is empty: " + this.GetComponentPath(int.MaxValue), this);
				base.enabled = false;
			}
			if (Application.isPlaying && this.iDynamicFloat == null)
			{
				if (this.dynamicFloatComponent == null)
				{
					Debug.LogError("dynamicFloatComponent cannot be null: " + this.GetComponentPath(int.MaxValue), this);
				}
				this.iDynamicFloat = (IDynamicFloat)this.dynamicFloatComponent;
				if (this.iDynamicFloat == null)
				{
					Debug.LogError("Component assigned to dynamicFloatComponent does not implement IDynamicFloat: " + this.GetComponentPath(int.MaxValue), this);
					base.enabled = false;
				}
			}
		}

		protected void OnDisable()
		{
			TextureTransitionerManager.Unregister(this);
		}

		private void RefreshShaderParams()
		{
			this.texTransitionShaderParam = Shader.PropertyToID(this.texTransitionShaderParamName);
			this.tex1ShaderParam = Shader.PropertyToID(this.tex1ShaderParamName);
			this.tex2ShaderParam = Shader.PropertyToID(this.tex2ShaderParamName);
		}

		public void ResetToDefaultState()
		{
			this.normalizedValue = 0f;
			this.transitionPercent = 0;
			this.tex1Index = 0;
			this.tex2Index = 0;
		}

		public bool editorPreview;

		[Tooltip("The component that will drive the texture transitions.")]
		public MonoBehaviour dynamicFloatComponent;

		[Tooltip("Set these values so that after remap 0 is the first texture in the textures list and 1 is the last.")]
		public GorillaMath.RemapFloatInfo remapInfo;

		public TextureTransitioner.DirectionRetentionMode directionRetentionMode;

		public string texTransitionShaderParamName = "_TexTransition";

		public string tex1ShaderParamName = "_MainTex";

		public string tex2ShaderParamName = "_Tex2";

		public Texture[] textures;

		public Renderer[] renderers;

		[NonSerialized]
		public IDynamicFloat iDynamicFloat;

		[NonSerialized]
		public int texTransitionShaderParam;

		[NonSerialized]
		public int tex1ShaderParam;

		[NonSerialized]
		public int tex2ShaderParam;

		[DebugReadout]
		[NonSerialized]
		public float normalizedValue;

		[DebugReadout]
		[NonSerialized]
		public int transitionPercent;

		[DebugReadout]
		[NonSerialized]
		public int tex1Index;

		[DebugReadout]
		[NonSerialized]
		public int tex2Index;

		public enum DirectionRetentionMode
		{
			None,
			IncreaseOnly,
			DecreaseOnly
		}
	}
}
