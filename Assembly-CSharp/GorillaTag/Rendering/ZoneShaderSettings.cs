using System;
using GorillaExtensions;
using GT_CustomMapSupportRuntime;
using UnityEngine;

namespace GorillaTag.Rendering
{
	public class ZoneShaderSettings : MonoBehaviour, ITickSystemPost
	{
		[DebugReadout]
		public static ZoneShaderSettings defaultsInstance { get; private set; }

		public static bool hasDefaultsInstance { get; private set; }

		[DebugReadout]
		public static ZoneShaderSettings activeInstance { get; private set; }

		public static bool hasActiveInstance { get; private set; }

		public bool isActiveInstance
		{
			get
			{
				return ZoneShaderSettings.activeInstance == this;
			}
		}

		[DebugReadout]
		private float GroundFogDepthFadeSq
		{
			get
			{
				return 1f / Mathf.Max(1E-05f, this._groundFogDepthFadeSize * this._groundFogDepthFadeSize);
			}
		}

		[DebugReadout]
		private float GroundFogHeightFade
		{
			get
			{
				return 1f / Mathf.Max(1E-05f, this._groundFogHeightFadeSize);
			}
		}

		public void SetZoneLiquidTypeKeywordEnum(ZoneShaderSettings.EZoneLiquidType liquidType)
		{
			if (liquidType == ZoneShaderSettings.EZoneLiquidType.None)
			{
				Shader.EnableKeyword("_GLOBAL_ZONE_LIQUID_TYPE__NONE");
			}
			else
			{
				Shader.DisableKeyword("_GLOBAL_ZONE_LIQUID_TYPE__NONE");
			}
			if (liquidType == ZoneShaderSettings.EZoneLiquidType.Water)
			{
				Shader.EnableKeyword("_GLOBAL_ZONE_LIQUID_TYPE__WATER");
			}
			else
			{
				Shader.DisableKeyword("_GLOBAL_ZONE_LIQUID_TYPE__WATER");
			}
			if (liquidType == ZoneShaderSettings.EZoneLiquidType.Lava)
			{
				Shader.EnableKeyword("_GLOBAL_ZONE_LIQUID_TYPE__LAVA");
				return;
			}
			Shader.DisableKeyword("_GLOBAL_ZONE_LIQUID_TYPE__LAVA");
		}

		public void SetZoneLiquidShapeKeywordEnum(ZoneShaderSettings.ELiquidShape shape)
		{
			if (shape == ZoneShaderSettings.ELiquidShape.Plane)
			{
				Shader.EnableKeyword("_ZONE_LIQUID_SHAPE__PLANE");
			}
			else
			{
				Shader.DisableKeyword("_ZONE_LIQUID_SHAPE__PLANE");
			}
			if (shape == ZoneShaderSettings.ELiquidShape.Cylinder)
			{
				Shader.EnableKeyword("_ZONE_LIQUID_SHAPE__CYLINDER");
				return;
			}
			Shader.DisableKeyword("_ZONE_LIQUID_SHAPE__CYLINDER");
		}

		public static int shaderParam_ZoneLiquidPosRadiusSq { get; private set; } = Shader.PropertyToID("_ZoneLiquidPosRadiusSq");

		public static float GetWaterY()
		{
			return ZoneShaderSettings.activeInstance.mainWaterSurfacePlane.position.y;
		}

		protected void Awake()
		{
			this.hasMainWaterSurfacePlane = this.mainWaterSurfacePlane != null && (this.mainWaterSurfacePlane_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyNewValue || this.isDefaultValues);
			this.hasDynamicWaterSurfacePlane = this.hasMainWaterSurfacePlane && !this.mainWaterSurfacePlane.gameObject.isStatic;
			this.hasLiquidBottomTransform = this.liquidBottomTransform != null && (this.liquidBottomTransform_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyNewValue || this.isDefaultValues);
			this.CheckDefaultsInstance();
			if (this._activateOnAwake)
			{
				this.BecomeActiveInstance(false);
			}
		}

		protected void OnEnable()
		{
			if (this.hasDynamicWaterSurfacePlane)
			{
				TickSystem<object>.AddPostTickCallback(this);
			}
		}

		protected void OnDisable()
		{
			TickSystem<object>.RemovePostTickCallback(this);
		}

		protected void OnDestroy()
		{
			if (ZoneShaderSettings.defaultsInstance == this)
			{
				ZoneShaderSettings.hasDefaultsInstance = false;
			}
			if (ZoneShaderSettings.activeInstance == this)
			{
				ZoneShaderSettings.hasActiveInstance = false;
			}
			TickSystem<object>.RemovePostTickCallback(this);
		}

		bool ITickSystemPost.PostTickRunning { get; set; }

		void ITickSystemPost.PostTick()
		{
			if (ZoneShaderSettings.activeInstance == this && Application.isPlaying && !ApplicationQuittingState.IsQuitting)
			{
				this.UpdateMainPlaneShaderProperty();
			}
		}

		private void UpdateMainPlaneShaderProperty()
		{
			Transform transform = null;
			bool flag = false;
			if (this.hasMainWaterSurfacePlane && (this.mainWaterSurfacePlane_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyNewValue || this.isDefaultValues))
			{
				flag = true;
				transform = this.mainWaterSurfacePlane;
			}
			else if (this.mainWaterSurfacePlane_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue && ZoneShaderSettings.hasDefaultsInstance && ZoneShaderSettings.defaultsInstance.hasMainWaterSurfacePlane)
			{
				flag = true;
				transform = ZoneShaderSettings.defaultsInstance.mainWaterSurfacePlane;
			}
			if (!flag)
			{
				return;
			}
			Vector3 position = transform.position;
			Vector3 up = transform.up;
			float num = -Vector3.Dot(up, position);
			Shader.SetGlobalVector(this.shaderParam_GlobalMainWaterSurfacePlane, new Vector4(up.x, up.y, up.z, num));
			ZoneShaderSettings.ELiquidShape eliquidShape;
			if (this.liquidShape_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyNewValue || this.isDefaultValues)
			{
				eliquidShape = this.liquidShape;
			}
			else if (this.liquidShape_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue && ZoneShaderSettings.hasDefaultsInstance)
			{
				eliquidShape = ZoneShaderSettings.defaultsInstance.liquidShape;
			}
			else
			{
				eliquidShape = ZoneShaderSettings.liquidShape_previousValue;
			}
			ZoneShaderSettings.liquidShape_previousValue = eliquidShape;
			float num2;
			if ((this.liquidBottomTransform_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyNewValue || this.isDefaultValues) && this.hasLiquidBottomTransform)
			{
				num2 = this.liquidBottomTransform.position.y;
			}
			else if (this.liquidBottomTransform_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue && ZoneShaderSettings.hasDefaultsInstance && ZoneShaderSettings.defaultsInstance.hasLiquidBottomTransform)
			{
				num2 = ZoneShaderSettings.defaultsInstance.liquidBottomTransform.position.y;
			}
			else
			{
				num2 = this.liquidBottomPosY_previousValue;
			}
			float num3;
			if (this.liquidShapeRadius_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyNewValue || this.isDefaultValues)
			{
				num3 = this.liquidShapeRadius;
			}
			else if (this.liquidShape_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue && ZoneShaderSettings.hasDefaultsInstance)
			{
				num3 = ZoneShaderSettings.defaultsInstance.liquidShapeRadius;
			}
			else
			{
				num3 = ZoneShaderSettings.liquidShapeRadius_previousValue;
			}
			if (eliquidShape == ZoneShaderSettings.ELiquidShape.Cylinder)
			{
				Shader.SetGlobalVector(ZoneShaderSettings.shaderParam_ZoneLiquidPosRadiusSq, new Vector4(position.x, num2, position.z, num3 * num3));
				ZoneShaderSettings.liquidShapeRadius_previousValue = num3;
			}
		}

		private void CheckDefaultsInstance()
		{
			if (!this.isDefaultValues)
			{
				return;
			}
			if (ZoneShaderSettings.hasDefaultsInstance && ZoneShaderSettings.defaultsInstance != null && ZoneShaderSettings.defaultsInstance != this)
			{
				string path = ZoneShaderSettings.defaultsInstance.transform.GetPath();
				Debug.LogError(string.Concat(new string[]
				{
					"ZoneShaderSettings: Destroying conflicting defaults instance.\n- keeping: \"",
					path,
					"\"\n- destroying (this): \"",
					base.transform.GetPath(),
					"\""
				}), this);
				Object.Destroy(base.gameObject);
				return;
			}
			ZoneShaderSettings.defaultsInstance = this;
			ZoneShaderSettings.hasDefaultsInstance = true;
			this.BecomeActiveInstance(false);
		}

		public void BecomeActiveInstance(bool force = false)
		{
			if (ZoneShaderSettings.activeInstance == this && !force)
			{
				return;
			}
			if (ZoneShaderSettings.activeInstance.IsNotNull())
			{
				TickSystem<object>.RemovePostTickCallback(ZoneShaderSettings.activeInstance);
			}
			if (this.hasDynamicWaterSurfacePlane)
			{
				TickSystem<object>.AddPostTickCallback(this);
			}
			this.ApplyValues();
			ZoneShaderSettings.activeInstance = this;
			ZoneShaderSettings.hasActiveInstance = true;
		}

		public static void ActivateDefaultSettings()
		{
			if (ZoneShaderSettings.hasDefaultsInstance)
			{
				ZoneShaderSettings.defaultsInstance.BecomeActiveInstance(false);
			}
		}

		public void SetGroundFogValue(Color fogColor, float fogDepthFade, float fogHeight, float fogHeightFade)
		{
			this.groundFogColor_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
			this.groundFogColor = fogColor;
			this.groundFogDepthFade_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
			this._groundFogDepthFadeSize = fogDepthFade;
			this.groundFogHeight_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
			this.groundFogHeight = fogHeight;
			this.groundFogHeightFade_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
			this._groundFogHeightFadeSize = fogHeightFade;
			this.BecomeActiveInstance(true);
		}

		private void ApplyValues()
		{
			if (!ZoneShaderSettings.hasDefaultsInstance || ApplicationQuittingState.IsQuitting)
			{
				return;
			}
			this.ApplyColor(ZoneShaderSettings.groundFogColor_shaderProp, this.groundFogColor_overrideMode, this.groundFogColor, ZoneShaderSettings.defaultsInstance.groundFogColor);
			this.ApplyFloat(ZoneShaderSettings.groundFogDepthFadeSq_shaderProp, this.groundFogDepthFade_overrideMode, this.GroundFogDepthFadeSq, ZoneShaderSettings.defaultsInstance.GroundFogDepthFadeSq);
			this.ApplyFloat(ZoneShaderSettings.groundFogHeight_shaderProp, this.groundFogHeight_overrideMode, this.groundFogHeight, ZoneShaderSettings.defaultsInstance.groundFogHeight);
			this.ApplyFloat(ZoneShaderSettings.groundFogHeightFade_shaderProp, this.groundFogHeightFade_overrideMode, this.GroundFogHeightFade, ZoneShaderSettings.defaultsInstance.GroundFogHeightFade);
			if (this.zoneLiquidType_overrideMode != ZoneShaderSettings.EOverrideMode.LeaveUnchanged)
			{
				ZoneShaderSettings.EZoneLiquidType ezoneLiquidType = ((this.zoneLiquidType_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyNewValue) ? this.zoneLiquidType : ZoneShaderSettings.defaultsInstance.zoneLiquidType);
				if (ezoneLiquidType != ZoneShaderSettings.liquidType_previousValue || !ZoneShaderSettings.isInitialized)
				{
					this.SetZoneLiquidTypeKeywordEnum(ezoneLiquidType);
					ZoneShaderSettings.liquidType_previousValue = ezoneLiquidType;
				}
			}
			if (this.liquidShape_overrideMode != ZoneShaderSettings.EOverrideMode.LeaveUnchanged)
			{
				ZoneShaderSettings.ELiquidShape eliquidShape = ((this.liquidShape_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyNewValue) ? this.liquidShape : ZoneShaderSettings.defaultsInstance.liquidShape);
				if (eliquidShape != ZoneShaderSettings.liquidShape_previousValue || !ZoneShaderSettings.isInitialized)
				{
					this.SetZoneLiquidShapeKeywordEnum(eliquidShape);
					ZoneShaderSettings.liquidShape_previousValue = eliquidShape;
				}
			}
			this.ApplyFloat(ZoneShaderSettings.shaderParam_GlobalZoneLiquidUVScale, this.zoneLiquidUVScale_overrideMode, this.zoneLiquidUVScale, ZoneShaderSettings.defaultsInstance.zoneLiquidUVScale);
			this.ApplyColor(ZoneShaderSettings.shaderParam_GlobalWaterTintColor, this.underwaterTintColor_overrideMode, this.underwaterTintColor, ZoneShaderSettings.defaultsInstance.underwaterTintColor);
			this.ApplyColor(ZoneShaderSettings.shaderParam_GlobalUnderwaterFogColor, this.underwaterFogColor_overrideMode, this.underwaterFogColor, ZoneShaderSettings.defaultsInstance.underwaterFogColor);
			this.ApplyVector(ZoneShaderSettings.shaderParam_GlobalUnderwaterFogParams, this.underwaterFogParams_overrideMode, this.underwaterFogParams, ZoneShaderSettings.defaultsInstance.underwaterFogParams);
			this.ApplyVector(ZoneShaderSettings.shaderParam_GlobalUnderwaterCausticsParams, this.underwaterCausticsParams_overrideMode, this.underwaterCausticsParams, ZoneShaderSettings.defaultsInstance.underwaterCausticsParams);
			this.ApplyTexture(ZoneShaderSettings.shaderParam_GlobalUnderwaterCausticsTex, this.underwaterCausticsTexture_overrideMode, this.underwaterCausticsTexture, ZoneShaderSettings.defaultsInstance.underwaterCausticsTexture);
			this.ApplyVector(ZoneShaderSettings.shaderParam_GlobalUnderwaterEffectsDistanceToSurfaceFade, this.underwaterEffectsDistanceToSurfaceFade_overrideMode, this.underwaterEffectsDistanceToSurfaceFade, ZoneShaderSettings.defaultsInstance.underwaterEffectsDistanceToSurfaceFade);
			this.ApplyTexture(ZoneShaderSettings.shaderParam_GlobalLiquidResidueTex, this.liquidResidueTex_overrideMode, this.liquidResidueTex, ZoneShaderSettings.defaultsInstance.liquidResidueTex);
			this.ApplyFloat(ZoneShaderSettings.shaderParam_ZoneWeatherMapDissolveProgress, this.zoneWeatherMapDissolveProgress_overrideMode, this.zoneWeatherMapDissolveProgress, ZoneShaderSettings.defaultsInstance.zoneWeatherMapDissolveProgress);
			this.UpdateMainPlaneShaderProperty();
			ZoneShaderSettings.isInitialized = true;
		}

		private void ApplyColor(int shaderProp, ZoneShaderSettings.EOverrideMode overrideMode, Color value, Color defaultValue)
		{
			if (overrideMode == ZoneShaderSettings.EOverrideMode.ApplyNewValue || this.isDefaultValues)
			{
				Shader.SetGlobalColor(shaderProp, value.linear);
				return;
			}
			if (overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				Shader.SetGlobalColor(shaderProp, defaultValue.linear);
			}
		}

		private void ApplyFloat(int shaderProp, ZoneShaderSettings.EOverrideMode overrideMode, float value, float defaultValue)
		{
			if (overrideMode == ZoneShaderSettings.EOverrideMode.ApplyNewValue || this.isDefaultValues)
			{
				Shader.SetGlobalFloat(shaderProp, value);
				return;
			}
			if (overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				Shader.SetGlobalFloat(shaderProp, defaultValue);
			}
		}

		private void ApplyVector(int shaderProp, ZoneShaderSettings.EOverrideMode overrideMode, Vector2 value, Vector2 defaultValue)
		{
			if (overrideMode == ZoneShaderSettings.EOverrideMode.ApplyNewValue || this.isDefaultValues)
			{
				Shader.SetGlobalVector(shaderProp, value);
				return;
			}
			if (overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				Shader.SetGlobalVector(shaderProp, defaultValue);
			}
		}

		private void ApplyVector(int shaderProp, ZoneShaderSettings.EOverrideMode overrideMode, Vector3 value, Vector3 defaultValue)
		{
			if (overrideMode == ZoneShaderSettings.EOverrideMode.ApplyNewValue || this.isDefaultValues)
			{
				Shader.SetGlobalVector(shaderProp, value);
				return;
			}
			if (overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				Shader.SetGlobalVector(shaderProp, defaultValue);
			}
		}

		private void ApplyVector(int shaderProp, ZoneShaderSettings.EOverrideMode overrideMode, Vector4 value, Vector4 defaultValue)
		{
			if (overrideMode == ZoneShaderSettings.EOverrideMode.ApplyNewValue || this.isDefaultValues)
			{
				Shader.SetGlobalVector(shaderProp, value);
				return;
			}
			if (overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				Shader.SetGlobalVector(shaderProp, defaultValue);
			}
		}

		private void ApplyTexture(int shaderProp, ZoneShaderSettings.EOverrideMode overrideMode, Texture2D value, Texture2D defaultValue)
		{
			if (overrideMode == ZoneShaderSettings.EOverrideMode.ApplyNewValue || this.isDefaultValues)
			{
				Shader.SetGlobalTexture(shaderProp, value);
				return;
			}
			if (overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				Shader.SetGlobalTexture(shaderProp, defaultValue);
			}
		}

		public void CopySettings(CMSZoneShaderSettings cmsZoneShaderSettings, bool rerunAwake = false)
		{
			this._activateOnAwake = cmsZoneShaderSettings.activateOnLoad;
			if (cmsZoneShaderSettings.applyGroundFog)
			{
				this.groundFogColor_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetGroundFogColorOverrideMode();
				this.groundFogColor = cmsZoneShaderSettings.groundFogColor;
				this.groundFogHeight_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetGroundFogHeightOverrideMode();
				if (cmsZoneShaderSettings.groundFogHeightPlane.IsNotNull())
				{
					this.groundFogHeight = cmsZoneShaderSettings.groundFogHeightPlane.position.y;
				}
				else
				{
					this.groundFogHeight = cmsZoneShaderSettings.groundFogHeight;
				}
				this.groundFogHeightFade_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetGroundFogHeightFadeOverrideMode();
				this._groundFogHeightFadeSize = cmsZoneShaderSettings.groundFogHeightFadeSize;
				this.groundFogDepthFade_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetGroundFogDepthFadeOverrideMode();
				this._groundFogDepthFadeSize = cmsZoneShaderSettings.groundFogDepthFadeSize;
			}
			else
			{
				this.groundFogColor_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.groundFogColor = new Color(0f, 0f, 0f, 0f);
				this.groundFogHeight = -9999f;
			}
			if (cmsZoneShaderSettings.applyLiquidEffects)
			{
				this.zoneLiquidType_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetZoneLiquidTypeOverrideMode();
				this.zoneLiquidType = (ZoneShaderSettings.EZoneLiquidType)cmsZoneShaderSettings.GetZoneLiquidType();
				this.liquidShape_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetLiquidShapeOverrideMode();
				this.liquidShape = (ZoneShaderSettings.ELiquidShape)cmsZoneShaderSettings.GetZoneLiquidShape();
				this.liquidShapeRadius_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetLiquidShapeRadiusOverrideMode();
				this.liquidShapeRadius = cmsZoneShaderSettings.liquidShapeRadius;
				this.liquidBottomTransform_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetLiquidBottomTransformOverrideMode();
				this.liquidBottomTransform = cmsZoneShaderSettings.liquidBottomTransform;
				this.zoneLiquidUVScale_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetZoneLiquidUVScaleOverrideMode();
				this.zoneLiquidUVScale = cmsZoneShaderSettings.zoneLiquidUVScale;
				this.underwaterTintColor_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetUnderwaterTintColorOverrideMode();
				this.underwaterTintColor = cmsZoneShaderSettings.underwaterTintColor;
				this.underwaterFogColor_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetUnderwaterFogColorOverrideMode();
				this.underwaterFogColor = cmsZoneShaderSettings.underwaterFogColor;
				this.underwaterFogParams_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetUnderwaterFogParamsOverrideMode();
				this.underwaterFogParams = cmsZoneShaderSettings.underwaterFogParams;
				this.underwaterCausticsParams_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetUnderwaterCausticsParamsOverrideMode();
				this.underwaterCausticsParams = cmsZoneShaderSettings.underwaterCausticsParams;
				this.underwaterCausticsTexture_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetUnderwaterCausticsTextureOverrideMode();
				this.underwaterCausticsTexture = cmsZoneShaderSettings.underwaterCausticsTexture;
				this.underwaterEffectsDistanceToSurfaceFade_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetUnderwaterEffectsDistanceToSurfaceFadeOverrideMode();
				this.underwaterEffectsDistanceToSurfaceFade = cmsZoneShaderSettings.underwaterEffectsDistanceToSurfaceFade;
				this.liquidResidueTex_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetLiquidResidueTextureOverrideMode();
				this.liquidResidueTex = cmsZoneShaderSettings.liquidResidueTex;
				this.mainWaterSurfacePlane_overrideMode = (ZoneShaderSettings.EOverrideMode)cmsZoneShaderSettings.GetMainWaterSurfacePlaneOverrideMode();
				this.mainWaterSurfacePlane = cmsZoneShaderSettings.mainWaterSurfacePlane;
			}
			else
			{
				this.underwaterTintColor_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.underwaterTintColor = new Color(0f, 0f, 0f, 0f);
				this.underwaterFogColor_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.underwaterFogColor = new Color(0f, 0f, 0f, 0f);
				this.mainWaterSurfacePlane_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				Transform transform = base.gameObject.transform.Find("DummyWaterPlane");
				GameObject gameObject;
				if (transform != null)
				{
					gameObject = transform.gameObject;
				}
				else
				{
					gameObject = new GameObject("DummyWaterPlane");
					gameObject.transform.SetParent(base.gameObject.transform);
					gameObject.transform.rotation = Quaternion.identity;
					gameObject.transform.position = new Vector3(0f, -9999f, 0f);
				}
				this.mainWaterSurfacePlane = gameObject.transform;
			}
			this.zoneWeatherMapDissolveProgress_overrideMode = ZoneShaderSettings.EOverrideMode.LeaveUnchanged;
			if (rerunAwake)
			{
				this.Awake();
			}
		}

		public void CopySettings(ZoneShaderSettings zoneShaderSettings, bool rerunAwake = false)
		{
			this._activateOnAwake = zoneShaderSettings._activateOnAwake;
			this.groundFogColor_overrideMode = zoneShaderSettings.groundFogColor_overrideMode;
			this.groundFogColor = zoneShaderSettings.groundFogColor;
			this.groundFogHeight_overrideMode = zoneShaderSettings.groundFogHeight_overrideMode;
			this.groundFogHeight = zoneShaderSettings.groundFogHeight;
			this.groundFogHeightFade_overrideMode = zoneShaderSettings.groundFogHeightFade_overrideMode;
			this._groundFogHeightFadeSize = zoneShaderSettings._groundFogHeightFadeSize;
			this.groundFogDepthFade_overrideMode = zoneShaderSettings.groundFogDepthFade_overrideMode;
			this._groundFogDepthFadeSize = zoneShaderSettings._groundFogDepthFadeSize;
			this.zoneLiquidType_overrideMode = zoneShaderSettings.zoneLiquidType_overrideMode;
			this.zoneLiquidType = zoneShaderSettings.zoneLiquidType;
			this.liquidShape_overrideMode = zoneShaderSettings.liquidShape_overrideMode;
			this.liquidShape = zoneShaderSettings.liquidShape;
			this.liquidShapeRadius_overrideMode = zoneShaderSettings.liquidShapeRadius_overrideMode;
			this.liquidShapeRadius = zoneShaderSettings.liquidShapeRadius;
			this.liquidBottomTransform_overrideMode = zoneShaderSettings.liquidBottomTransform_overrideMode;
			this.liquidBottomTransform = zoneShaderSettings.liquidBottomTransform;
			this.zoneLiquidUVScale_overrideMode = zoneShaderSettings.zoneLiquidUVScale_overrideMode;
			this.zoneLiquidUVScale = zoneShaderSettings.zoneLiquidUVScale;
			this.underwaterTintColor_overrideMode = zoneShaderSettings.underwaterTintColor_overrideMode;
			this.underwaterTintColor = zoneShaderSettings.underwaterTintColor;
			this.underwaterFogColor_overrideMode = zoneShaderSettings.underwaterFogColor_overrideMode;
			this.underwaterFogColor = zoneShaderSettings.underwaterFogColor;
			this.underwaterFogParams_overrideMode = zoneShaderSettings.underwaterFogParams_overrideMode;
			this.underwaterFogParams = zoneShaderSettings.underwaterFogParams;
			this.underwaterCausticsParams_overrideMode = zoneShaderSettings.underwaterCausticsParams_overrideMode;
			this.underwaterCausticsParams = zoneShaderSettings.underwaterCausticsParams;
			this.underwaterCausticsTexture_overrideMode = zoneShaderSettings.underwaterCausticsTexture_overrideMode;
			this.underwaterCausticsTexture = zoneShaderSettings.underwaterCausticsTexture;
			this.underwaterEffectsDistanceToSurfaceFade_overrideMode = zoneShaderSettings.underwaterEffectsDistanceToSurfaceFade_overrideMode;
			this.underwaterEffectsDistanceToSurfaceFade = zoneShaderSettings.underwaterEffectsDistanceToSurfaceFade;
			this.liquidResidueTex_overrideMode = zoneShaderSettings.liquidResidueTex_overrideMode;
			this.liquidResidueTex = zoneShaderSettings.liquidResidueTex;
			this.mainWaterSurfacePlane_overrideMode = zoneShaderSettings.mainWaterSurfacePlane_overrideMode;
			this.mainWaterSurfacePlane = zoneShaderSettings.mainWaterSurfacePlane;
			this.zoneWeatherMapDissolveProgress_overrideMode = zoneShaderSettings.zoneWeatherMapDissolveProgress_overrideMode;
			this.zoneWeatherMapDissolveProgress = zoneShaderSettings.zoneWeatherMapDissolveProgress;
			if (rerunAwake)
			{
				this.Awake();
			}
		}

		public void ReplaceDefaultValues(ZoneShaderSettings defaultZoneShaderSettings, bool rerunAwake = false)
		{
			if (this.groundFogColor_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.groundFogColor_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.groundFogColor = defaultZoneShaderSettings.groundFogColor;
			}
			if (this.groundFogHeight_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.groundFogHeight_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.groundFogHeight = defaultZoneShaderSettings.groundFogHeight;
			}
			if (this.groundFogHeightFade_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.groundFogHeightFade_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this._groundFogHeightFadeSize = defaultZoneShaderSettings._groundFogHeightFadeSize;
			}
			if (this.groundFogDepthFade_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.groundFogDepthFade_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this._groundFogDepthFadeSize = defaultZoneShaderSettings._groundFogDepthFadeSize;
			}
			if (this.zoneLiquidType_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.zoneLiquidType_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.zoneLiquidType = defaultZoneShaderSettings.zoneLiquidType;
			}
			if (this.liquidShape_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.liquidShape_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.liquidShape = defaultZoneShaderSettings.liquidShape;
			}
			if (this.liquidShapeRadius_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.liquidShapeRadius_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.liquidShapeRadius = defaultZoneShaderSettings.liquidShapeRadius;
			}
			if (this.liquidBottomTransform_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.liquidBottomTransform_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.liquidBottomTransform = defaultZoneShaderSettings.liquidBottomTransform;
			}
			if (this.zoneLiquidUVScale_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.zoneLiquidUVScale_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.zoneLiquidUVScale = defaultZoneShaderSettings.zoneLiquidUVScale;
			}
			if (this.underwaterTintColor_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.underwaterTintColor_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.underwaterTintColor = defaultZoneShaderSettings.underwaterTintColor;
			}
			if (this.underwaterFogColor_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.underwaterFogColor_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.underwaterFogColor = defaultZoneShaderSettings.underwaterFogColor;
			}
			if (this.underwaterFogParams_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.underwaterFogParams_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.underwaterFogParams = defaultZoneShaderSettings.underwaterFogParams;
			}
			if (this.underwaterCausticsParams_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.underwaterCausticsParams_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.underwaterCausticsParams = defaultZoneShaderSettings.underwaterCausticsParams;
			}
			if (this.underwaterCausticsTexture_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.underwaterCausticsTexture_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.underwaterCausticsTexture = defaultZoneShaderSettings.underwaterCausticsTexture;
			}
			if (this.underwaterEffectsDistanceToSurfaceFade_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.underwaterEffectsDistanceToSurfaceFade_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.underwaterEffectsDistanceToSurfaceFade = defaultZoneShaderSettings.underwaterEffectsDistanceToSurfaceFade;
			}
			if (this.liquidResidueTex_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.liquidResidueTex_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.liquidResidueTex = defaultZoneShaderSettings.liquidResidueTex;
			}
			if (this.mainWaterSurfacePlane_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.mainWaterSurfacePlane_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.mainWaterSurfacePlane = defaultZoneShaderSettings.mainWaterSurfacePlane;
			}
			if (rerunAwake)
			{
				this.Awake();
			}
		}

		public void ReplaceDefaultValues(CMSZoneShaderSettings.CMSZoneShaderProperties defaultZoneShaderProperties, bool rerunAwake = false)
		{
			if (this.groundFogColor_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.groundFogColor_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.groundFogColor = defaultZoneShaderProperties.groundFogColor;
			}
			if (this.groundFogHeight_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.groundFogHeight_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.groundFogHeight = defaultZoneShaderProperties.groundFogHeight;
			}
			if (this.groundFogHeightFade_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.groundFogHeightFade_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this._groundFogHeightFadeSize = defaultZoneShaderProperties.groundFogHeightFadeSize;
			}
			if (this.groundFogDepthFade_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.groundFogDepthFade_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this._groundFogDepthFadeSize = defaultZoneShaderProperties.groundFogDepthFadeSize;
			}
			if (this.zoneLiquidType_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.zoneLiquidType_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.zoneLiquidType = (ZoneShaderSettings.EZoneLiquidType)defaultZoneShaderProperties.zoneLiquidType;
			}
			if (this.liquidShape_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.liquidShape_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.liquidShape = (ZoneShaderSettings.ELiquidShape)defaultZoneShaderProperties.liquidShape;
			}
			if (this.liquidShapeRadius_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.liquidShapeRadius_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.liquidShapeRadius = defaultZoneShaderProperties.liquidShapeRadius;
			}
			if (this.liquidBottomTransform_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.liquidBottomTransform_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.liquidBottomTransform = defaultZoneShaderProperties.liquidBottomTransform;
			}
			if (this.zoneLiquidUVScale_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.zoneLiquidUVScale_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.zoneLiquidUVScale = defaultZoneShaderProperties.zoneLiquidUVScale;
			}
			if (this.underwaterTintColor_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.underwaterTintColor_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.underwaterTintColor = defaultZoneShaderProperties.underwaterTintColor;
			}
			if (this.underwaterFogColor_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.underwaterFogColor_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.underwaterFogColor = defaultZoneShaderProperties.underwaterFogColor;
			}
			if (this.underwaterFogParams_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.underwaterFogParams_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.underwaterFogParams = defaultZoneShaderProperties.underwaterFogParams;
			}
			if (this.underwaterCausticsParams_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.underwaterCausticsParams_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.underwaterCausticsParams = defaultZoneShaderProperties.underwaterCausticsParams;
			}
			if (this.underwaterCausticsTexture_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.underwaterCausticsTexture_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.underwaterCausticsTexture = defaultZoneShaderProperties.underwaterCausticsTexture;
			}
			if (this.underwaterEffectsDistanceToSurfaceFade_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.underwaterEffectsDistanceToSurfaceFade_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.underwaterEffectsDistanceToSurfaceFade = defaultZoneShaderProperties.underwaterEffectsDistanceToSurfaceFade;
			}
			if (this.liquidResidueTex_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.liquidResidueTex_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.liquidResidueTex = defaultZoneShaderProperties.liquidResidueTex;
			}
			if (this.mainWaterSurfacePlane_overrideMode == ZoneShaderSettings.EOverrideMode.ApplyDefaultValue)
			{
				this.mainWaterSurfacePlane_overrideMode = ZoneShaderSettings.EOverrideMode.ApplyNewValue;
				this.mainWaterSurfacePlane = defaultZoneShaderProperties.mainWaterSurfacePlane;
			}
			if (rerunAwake)
			{
				this.Awake();
			}
		}

		[OnEnterPlay_Set(false)]
		private static bool isInitialized;

		[Tooltip("Set this to true for cases like it is the first ZoneShaderSettings that should be activated when entering a scene.")]
		[SerializeField]
		private bool _activateOnAwake;

		[Tooltip("These values will be used as the default global values that will be fallen back to when not in a zone and that the other scripts will reference.")]
		public bool isDefaultValues;

		private static readonly int groundFogColor_shaderProp = Shader.PropertyToID("_ZoneGroundFogColor");

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode groundFogColor_overrideMode;

		[SerializeField]
		private Color groundFogColor = new Color(0.7f, 0.9f, 1f, 1f);

		private static readonly int groundFogDepthFadeSq_shaderProp = Shader.PropertyToID("_ZoneGroundFogDepthFadeSq");

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode groundFogDepthFade_overrideMode;

		[SerializeField]
		private float _groundFogDepthFadeSize = 20f;

		private static readonly int groundFogHeight_shaderProp = Shader.PropertyToID("_ZoneGroundFogHeight");

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode groundFogHeight_overrideMode;

		[SerializeField]
		private float groundFogHeight = 7.45f;

		private static readonly int groundFogHeightFade_shaderProp = Shader.PropertyToID("_ZoneGroundFogHeightFade");

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode groundFogHeightFade_overrideMode;

		[SerializeField]
		private float _groundFogHeightFadeSize = 20f;

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode zoneLiquidType_overrideMode;

		[SerializeField]
		private ZoneShaderSettings.EZoneLiquidType zoneLiquidType = ZoneShaderSettings.EZoneLiquidType.Water;

		[OnEnterPlay_Set(ZoneShaderSettings.EZoneLiquidType.None)]
		private static ZoneShaderSettings.EZoneLiquidType liquidType_previousValue = ZoneShaderSettings.EZoneLiquidType.None;

		[OnEnterPlay_Set(false)]
		private static bool didEverSetLiquidShape;

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode liquidShape_overrideMode;

		[SerializeField]
		private ZoneShaderSettings.ELiquidShape liquidShape;

		[OnEnterPlay_Set(ZoneShaderSettings.ELiquidShape.Plane)]
		private static ZoneShaderSettings.ELiquidShape liquidShape_previousValue = ZoneShaderSettings.ELiquidShape.Plane;

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode liquidShapeRadius_overrideMode;

		[Tooltip("Fog params are: start, distance (end - start), unused, unused")]
		[SerializeField]
		private float liquidShapeRadius = 1f;

		[OnEnterPlay_Set(1f)]
		private static float liquidShapeRadius_previousValue;

		private bool hasLiquidBottomTransform;

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode liquidBottomTransform_overrideMode;

		[Tooltip("TODO: remove this when there is a way to precalculate the nearest triangle plane per vertex so it will work better for rivers.")]
		[SerializeField]
		private Transform liquidBottomTransform;

		private float liquidBottomPosY_previousValue;

		private static readonly int shaderParam_GlobalZoneLiquidUVScale = Shader.PropertyToID("_GlobalZoneLiquidUVScale");

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode zoneLiquidUVScale_overrideMode;

		[Tooltip("Fog params are: start, distance (end - start), unused, unused")]
		[SerializeField]
		private float zoneLiquidUVScale = 1f;

		private static readonly int shaderParam_GlobalWaterTintColor = Shader.PropertyToID("_GlobalWaterTintColor");

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode underwaterTintColor_overrideMode;

		[SerializeField]
		private Color underwaterTintColor = new Color(0.3f, 0.65f, 1f, 0.2f);

		private static readonly int shaderParam_GlobalUnderwaterFogColor = Shader.PropertyToID("_GlobalUnderwaterFogColor");

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode underwaterFogColor_overrideMode;

		[SerializeField]
		private Color underwaterFogColor = new Color(0.12f, 0.41f, 0.77f);

		private static readonly int shaderParam_GlobalUnderwaterFogParams = Shader.PropertyToID("_GlobalUnderwaterFogParams");

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode underwaterFogParams_overrideMode;

		[Tooltip("Fog params are: start, distance (end - start), unused, unused")]
		[SerializeField]
		private Vector4 underwaterFogParams = new Vector4(-5f, 40f, 0f, 0f);

		private static readonly int shaderParam_GlobalUnderwaterCausticsParams = Shader.PropertyToID("_GlobalUnderwaterCausticsParams");

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode underwaterCausticsParams_overrideMode;

		[Tooltip("Caustics params are: speed1, scale, alpha, unused")]
		[SerializeField]
		private Vector4 underwaterCausticsParams = new Vector4(0.075f, 0.075f, 1f, 0f);

		private static readonly int shaderParam_GlobalUnderwaterCausticsTex = Shader.PropertyToID("_GlobalUnderwaterCausticsTex");

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode underwaterCausticsTexture_overrideMode;

		[SerializeField]
		private Texture2D underwaterCausticsTexture;

		private static readonly int shaderParam_GlobalUnderwaterEffectsDistanceToSurfaceFade = Shader.PropertyToID("_GlobalUnderwaterEffectsDistanceToSurfaceFade");

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode underwaterEffectsDistanceToSurfaceFade_overrideMode;

		[SerializeField]
		private Vector2 underwaterEffectsDistanceToSurfaceFade = new Vector2(0.0001f, 50f);

		private const string kEdTooltip_liquidResidueTex = "This is used for things like the charred surface effect when lava burns static geo.";

		private static readonly int shaderParam_GlobalLiquidResidueTex = Shader.PropertyToID("_GlobalLiquidResidueTex");

		[SerializeField]
		[Tooltip("This is used for things like the charred surface effect when lava burns static geo.")]
		private ZoneShaderSettings.EOverrideMode liquidResidueTex_overrideMode;

		[SerializeField]
		[Tooltip("This is used for things like the charred surface effect when lava burns static geo.")]
		private Texture2D liquidResidueTex;

		private readonly int shaderParam_GlobalMainWaterSurfacePlane = Shader.PropertyToID("_GlobalMainWaterSurfacePlane");

		private bool hasMainWaterSurfacePlane;

		private bool hasDynamicWaterSurfacePlane;

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode mainWaterSurfacePlane_overrideMode;

		[Tooltip("TODO: remove this when there is a way to precalculate the nearest triangle plane per vertex so it will work better for rivers.")]
		[SerializeField]
		private Transform mainWaterSurfacePlane;

		private static readonly int shaderParam_ZoneWeatherMapDissolveProgress = Shader.PropertyToID("_ZoneWeatherMapDissolveProgress");

		[SerializeField]
		private ZoneShaderSettings.EOverrideMode zoneWeatherMapDissolveProgress_overrideMode;

		[Tooltip("Fog params are: start, distance (end - start), unused, unused")]
		[Range(0f, 1f)]
		[SerializeField]
		private float zoneWeatherMapDissolveProgress = 1f;

		public enum EOverrideMode
		{
			LeaveUnchanged,
			ApplyNewValue,
			ApplyDefaultValue
		}

		public enum EZoneLiquidType
		{
			None,
			Water,
			Lava
		}

		public enum ELiquidShape
		{
			Plane,
			Cylinder
		}
	}
}
