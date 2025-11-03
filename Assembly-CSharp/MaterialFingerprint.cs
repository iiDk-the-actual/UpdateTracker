using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

[StructLayout(LayoutKind.Auto)]
public struct MaterialFingerprint
{
	public MaterialFingerprint(UberShaderMatUsedProps used)
	{
		Material material = used.material;
		this._TransparencyMode = MaterialFingerprint.GetMatTransparencyMode(material);
		this._Cutoff = MaterialFingerprint._Round(material.GetFloat(ShaderProps._Cutoff), 100, used._Cutoff);
		this._ColorSource = ((used._ColorSource > 0) ? material.GetInt(ShaderProps._ColorSource) : 0);
		this._BaseColor = MaterialFingerprint._Round(material.GetColor(ShaderProps._BaseColor), 100, used._BaseColor);
		this._GChannelColor = MaterialFingerprint._Round(material.GetColor(ShaderProps._GChannelColor), 100, used._GChannelColor);
		this._BChannelColor = MaterialFingerprint._Round(material.GetColor(ShaderProps._BChannelColor), 100, used._BChannelColor);
		this._AChannelColor = MaterialFingerprint._Round(material.GetColor(ShaderProps._AChannelColor), 100, used._AChannelColor);
		this._TexMipBias = MaterialFingerprint._Round(material.GetFloat(ShaderProps._TexMipBias), 100, used._TexMipBias);
		this._BaseMap = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._BaseMap, used._BaseMap);
		this._BaseMap_ST = MaterialFingerprint._Round(material.GetVector(ShaderProps._BaseMap_ST), 100, used._BaseMap_ST);
		this._BaseMap_WH = MaterialFingerprint._Round(material.GetVector(ShaderProps._BaseMap_WH), 100, used._BaseMap_WH);
		this._TexelSnapToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._TexelSnapToggle), 100, used._TexelSnapToggle);
		this._TexelSnap_Factor = MaterialFingerprint._Round(material.GetFloat(ShaderProps._TexelSnap_Factor), 100, used._TexelSnap_Factor);
		this._UVSource = ((used._UVSource > 0) ? material.GetInt(ShaderProps._UVSource) : 0);
		this._AlphaDetailToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._AlphaDetailToggle), 100, used._AlphaDetailToggle);
		this._AlphaDetail_ST = MaterialFingerprint._Round(material.GetVector(ShaderProps._AlphaDetail_ST), 100, used._AlphaDetail_ST);
		this._AlphaDetail_Opacity = MaterialFingerprint._Round(material.GetFloat(ShaderProps._AlphaDetail_Opacity), 100, used._AlphaDetail_Opacity);
		this._AlphaDetail_WorldSpace = MaterialFingerprint._Round(material.GetFloat(ShaderProps._AlphaDetail_WorldSpace), 100, used._AlphaDetail_WorldSpace);
		this._MaskMapToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._MaskMapToggle), 100, used._MaskMapToggle);
		this._MaskMap = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._MaskMap, used._MaskMap);
		this._MaskMap_ST = MaterialFingerprint._Round(material.GetVector(ShaderProps._MaskMap_ST), 100, used._MaskMap_ST);
		this._MaskMap_WH = MaterialFingerprint._Round(material.GetVector(ShaderProps._MaskMap_WH), 100, used._MaskMap_WH);
		this._LavaLampToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._LavaLampToggle), 100, used._LavaLampToggle);
		this._GradientMapToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._GradientMapToggle), 100, used._GradientMapToggle);
		this._GradientMap = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._GradientMap, used._GradientMap);
		this._DoTextureRotation = MaterialFingerprint._Round(material.GetFloat(ShaderProps._DoTextureRotation), 100, used._DoTextureRotation);
		this._RotateAngle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._RotateAngle), 100, used._RotateAngle);
		this._RotateAnim = MaterialFingerprint._Round(material.GetFloat(ShaderProps._RotateAnim), 100, used._RotateAnim);
		this._UseWaveWarp = MaterialFingerprint._Round(material.GetFloat(ShaderProps._UseWaveWarp), 100, used._UseWaveWarp);
		this._WaveAmplitude = MaterialFingerprint._Round(material.GetFloat(ShaderProps._WaveAmplitude), 100, used._WaveAmplitude);
		this._WaveFrequency = MaterialFingerprint._Round(material.GetFloat(ShaderProps._WaveFrequency), 100, used._WaveFrequency);
		this._WaveScale = MaterialFingerprint._Round(material.GetFloat(ShaderProps._WaveScale), 100, used._WaveScale);
		this._WaveTimeScale = MaterialFingerprint._Round(material.GetFloat(ShaderProps._WaveTimeScale), 100, used._WaveTimeScale);
		this._UseWeatherMap = MaterialFingerprint._Round(material.GetFloat(ShaderProps._UseWeatherMap), 100, used._UseWeatherMap);
		this._WeatherMap = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._WeatherMap, used._WeatherMap);
		this._WeatherMapDissolveEdgeSize = MaterialFingerprint._Round(material.GetFloat(ShaderProps._WeatherMapDissolveEdgeSize), 100, used._WeatherMapDissolveEdgeSize);
		this._ReflectToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ReflectToggle), 100, used._ReflectToggle);
		this._ReflectBoxProjectToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ReflectBoxProjectToggle), 100, used._ReflectBoxProjectToggle);
		this._ReflectBoxCubePos = MaterialFingerprint._Round(material.GetVector(ShaderProps._ReflectBoxCubePos), 100, used._ReflectBoxCubePos);
		this._ReflectBoxSize = MaterialFingerprint._Round(material.GetVector(ShaderProps._ReflectBoxSize), 100, used._ReflectBoxSize);
		this._ReflectBoxRotation = MaterialFingerprint._Round(material.GetVector(ShaderProps._ReflectBoxRotation), 100, used._ReflectBoxRotation);
		this._ReflectMatcapToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ReflectMatcapToggle), 100, used._ReflectMatcapToggle);
		this._ReflectMatcapPerspToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ReflectMatcapPerspToggle), 100, used._ReflectMatcapPerspToggle);
		this._ReflectNormalToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ReflectNormalToggle), 100, used._ReflectNormalToggle);
		this._ReflectTex = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._ReflectTex, used._ReflectTex);
		this._ReflectNormalTex = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._ReflectNormalTex, used._ReflectNormalTex);
		this._ReflectAlbedoTint = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ReflectAlbedoTint), 100, used._ReflectAlbedoTint);
		this._ReflectTint = MaterialFingerprint._Round(material.GetColor(ShaderProps._ReflectTint), 100, used._ReflectTint);
		this._ReflectOpacity = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ReflectOpacity), 100, used._ReflectOpacity);
		this._ReflectExposure = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ReflectExposure), 100, used._ReflectExposure);
		this._ReflectOffset = MaterialFingerprint._Round(material.GetVector(ShaderProps._ReflectOffset), 100, used._ReflectOffset);
		this._ReflectScale = MaterialFingerprint._Round(material.GetVector(ShaderProps._ReflectScale), 100, used._ReflectScale);
		this._ReflectRotate = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ReflectRotate), 100, used._ReflectRotate);
		this._HalfLambertToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._HalfLambertToggle), 100, used._HalfLambertToggle);
		this._ParallaxPlanarToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ParallaxPlanarToggle), 100, used._ParallaxPlanarToggle);
		this._ParallaxToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ParallaxToggle), 100, used._ParallaxToggle);
		this._ParallaxAAToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ParallaxAAToggle), 100, used._ParallaxAAToggle);
		this._ParallaxAABias = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ParallaxAABias), 100, used._ParallaxAABias);
		this._DepthMap = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._DepthMap, used._DepthMap);
		this._ParallaxAmplitude = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ParallaxAmplitude), 100, used._ParallaxAmplitude);
		this._ParallaxSamplesMinMax = MaterialFingerprint._Round(material.GetVector(ShaderProps._ParallaxSamplesMinMax), 100, used._ParallaxSamplesMinMax);
		this._UvShiftToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._UvShiftToggle), 100, used._UvShiftToggle);
		this._UvShiftSteps = MaterialFingerprint._Round(material.GetVector(ShaderProps._UvShiftSteps), 100, used._UvShiftSteps);
		this._UvShiftRate = MaterialFingerprint._Round(material.GetVector(ShaderProps._UvShiftRate), 100, used._UvShiftRate);
		this._UvShiftOffset = MaterialFingerprint._Round(material.GetVector(ShaderProps._UvShiftOffset), 100, used._UvShiftOffset);
		this._UseGridEffect = MaterialFingerprint._Round(material.GetFloat(ShaderProps._UseGridEffect), 100, used._UseGridEffect);
		this._UseCrystalEffect = MaterialFingerprint._Round(material.GetFloat(ShaderProps._UseCrystalEffect), 100, used._UseCrystalEffect);
		this._CrystalPower = MaterialFingerprint._Round(material.GetFloat(ShaderProps._CrystalPower), 100, used._CrystalPower);
		this._CrystalRimColor = MaterialFingerprint._Round(material.GetColor(ShaderProps._CrystalRimColor), 100, used._CrystalRimColor);
		this._LiquidVolume = MaterialFingerprint._Round(material.GetFloat(ShaderProps._LiquidVolume), 100, used._LiquidVolume);
		this._LiquidFill = MaterialFingerprint._Round(material.GetFloat(ShaderProps._LiquidFill), 100, used._LiquidFill);
		this._LiquidFillNormal = MaterialFingerprint._Round(material.GetVector(ShaderProps._LiquidFillNormal), 100, used._LiquidFillNormal);
		this._LiquidSurfaceColor = MaterialFingerprint._Round(material.GetColor(ShaderProps._LiquidSurfaceColor), 100, used._LiquidSurfaceColor);
		this._LiquidSwayX = MaterialFingerprint._Round(material.GetFloat(ShaderProps._LiquidSwayX), 100, used._LiquidSwayX);
		this._LiquidSwayY = MaterialFingerprint._Round(material.GetFloat(ShaderProps._LiquidSwayY), 100, used._LiquidSwayY);
		this._LiquidContainer = MaterialFingerprint._Round(material.GetFloat(ShaderProps._LiquidContainer), 100, used._LiquidContainer);
		this._LiquidPlanePosition = MaterialFingerprint._Round(material.GetVector(ShaderProps._LiquidPlanePosition), 100, used._LiquidPlanePosition);
		this._LiquidPlaneNormal = MaterialFingerprint._Round(material.GetVector(ShaderProps._LiquidPlaneNormal), 100, used._LiquidPlaneNormal);
		this._VertexFlapToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._VertexFlapToggle), 100, used._VertexFlapToggle);
		this._VertexFlapAxis = MaterialFingerprint._Round(material.GetVector(ShaderProps._VertexFlapAxis), 100, used._VertexFlapAxis);
		this._VertexFlapDegreesMinMax = MaterialFingerprint._Round(material.GetVector(ShaderProps._VertexFlapDegreesMinMax), 100, used._VertexFlapDegreesMinMax);
		this._VertexFlapSpeed = MaterialFingerprint._Round(material.GetFloat(ShaderProps._VertexFlapSpeed), 100, used._VertexFlapSpeed);
		this._VertexFlapPhaseOffset = MaterialFingerprint._Round(material.GetFloat(ShaderProps._VertexFlapPhaseOffset), 100, used._VertexFlapPhaseOffset);
		this._VertexWaveToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._VertexWaveToggle), 100, used._VertexWaveToggle);
		this._VertexWaveDebug = MaterialFingerprint._Round(material.GetFloat(ShaderProps._VertexWaveDebug), 100, used._VertexWaveDebug);
		this._VertexWaveEnd = MaterialFingerprint._Round(material.GetVector(ShaderProps._VertexWaveEnd), 100, used._VertexWaveEnd);
		this._VertexWaveParams = MaterialFingerprint._Round(material.GetVector(ShaderProps._VertexWaveParams), 100, used._VertexWaveParams);
		this._VertexWaveFalloff = MaterialFingerprint._Round(material.GetVector(ShaderProps._VertexWaveFalloff), 100, used._VertexWaveFalloff);
		this._VertexWaveSphereMask = MaterialFingerprint._Round(material.GetVector(ShaderProps._VertexWaveSphereMask), 100, used._VertexWaveSphereMask);
		this._VertexWavePhaseOffset = MaterialFingerprint._Round(material.GetFloat(ShaderProps._VertexWavePhaseOffset), 100, used._VertexWavePhaseOffset);
		this._VertexWaveAxes = MaterialFingerprint._Round(material.GetVector(ShaderProps._VertexWaveAxes), 100, used._VertexWaveAxes);
		this._VertexRotateToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._VertexRotateToggle), 100, used._VertexRotateToggle);
		this._VertexRotateAngles = MaterialFingerprint._Round(material.GetVector(ShaderProps._VertexRotateAngles), 100, used._VertexRotateAngles);
		this._VertexRotateAnim = MaterialFingerprint._Round(material.GetFloat(ShaderProps._VertexRotateAnim), 100, used._VertexRotateAnim);
		this._VertexLightToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._VertexLightToggle), 100, used._VertexLightToggle);
		this._InnerGlowOn = MaterialFingerprint._Round(material.GetFloat(ShaderProps._InnerGlowOn), 100, used._InnerGlowOn);
		this._InnerGlowColor = MaterialFingerprint._Round(material.GetColor(ShaderProps._InnerGlowColor), 100, used._InnerGlowColor);
		this._InnerGlowParams = MaterialFingerprint._Round(material.GetVector(ShaderProps._InnerGlowParams), 100, used._InnerGlowParams);
		this._InnerGlowTap = MaterialFingerprint._Round(material.GetFloat(ShaderProps._InnerGlowTap), 100, used._InnerGlowTap);
		this._InnerGlowSine = MaterialFingerprint._Round(material.GetFloat(ShaderProps._InnerGlowSine), 100, used._InnerGlowSine);
		this._InnerGlowSinePeriod = MaterialFingerprint._Round(material.GetFloat(ShaderProps._InnerGlowSinePeriod), 100, used._InnerGlowSinePeriod);
		this._InnerGlowSinePhaseShift = MaterialFingerprint._Round(material.GetFloat(ShaderProps._InnerGlowSinePhaseShift), 100, used._InnerGlowSinePhaseShift);
		this._StealthEffectOn = MaterialFingerprint._Round(material.GetFloat(ShaderProps._StealthEffectOn), 100, used._StealthEffectOn);
		this._UseEyeTracking = MaterialFingerprint._Round(material.GetFloat(ShaderProps._UseEyeTracking), 100, used._UseEyeTracking);
		this._EyeTileOffsetUV = MaterialFingerprint._Round(material.GetVector(ShaderProps._EyeTileOffsetUV), 100, used._EyeTileOffsetUV);
		this._EyeOverrideUV = MaterialFingerprint._Round(material.GetFloat(ShaderProps._EyeOverrideUV), 100, used._EyeOverrideUV);
		this._EyeOverrideUVTransform = MaterialFingerprint._Round(material.GetVector(ShaderProps._EyeOverrideUVTransform), 100, used._EyeOverrideUVTransform);
		this._UseMouthFlap = MaterialFingerprint._Round(material.GetFloat(ShaderProps._UseMouthFlap), 100, used._UseMouthFlap);
		this._MouthMap = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._MouthMap, used._MouthMap);
		this._MouthMap_ST = MaterialFingerprint._Round(material.GetVector(ShaderProps._MouthMap_ST), 100, used._MouthMap_ST);
		this._UseVertexColor = MaterialFingerprint._Round(material.GetFloat(ShaderProps._UseVertexColor), 100, used._UseVertexColor);
		this._WaterEffect = MaterialFingerprint._Round(material.GetFloat(ShaderProps._WaterEffect), 100, used._WaterEffect);
		this._HeightBasedWaterEffect = MaterialFingerprint._Round(material.GetFloat(ShaderProps._HeightBasedWaterEffect), 100, used._HeightBasedWaterEffect);
		this._WaterCaustics = MaterialFingerprint._Round(material.GetFloat(ShaderProps._WaterCaustics), 100, used._WaterCaustics);
		this._UseDayNightLightmap = MaterialFingerprint._Round(material.GetFloat(ShaderProps._UseDayNightLightmap), 100, used._UseDayNightLightmap);
		this._UseSpecular = MaterialFingerprint._Round(material.GetFloat(ShaderProps._UseSpecular), 100, used._UseSpecular);
		this._UseSpecularAlphaChannel = MaterialFingerprint._Round(material.GetFloat(ShaderProps._UseSpecularAlphaChannel), 100, used._UseSpecularAlphaChannel);
		this._Smoothness = MaterialFingerprint._Round(material.GetFloat(ShaderProps._Smoothness), 100, used._Smoothness);
		this._UseSpecHighlight = MaterialFingerprint._Round(material.GetFloat(ShaderProps._UseSpecHighlight), 100, used._UseSpecHighlight);
		this._SpecularDir = MaterialFingerprint._Round(material.GetVector(ShaderProps._SpecularDir), 100, used._SpecularDir);
		this._SpecularPowerIntensity = MaterialFingerprint._Round(material.GetVector(ShaderProps._SpecularPowerIntensity), 100, used._SpecularPowerIntensity);
		this._SpecularColor = MaterialFingerprint._Round(material.GetColor(ShaderProps._SpecularColor), 100, used._SpecularColor);
		this._SpecularUseDiffuseColor = MaterialFingerprint._Round(material.GetFloat(ShaderProps._SpecularUseDiffuseColor), 100, used._SpecularUseDiffuseColor);
		this._EmissionToggle = MaterialFingerprint._Round(material.GetFloat(ShaderProps._EmissionToggle), 100, used._EmissionToggle);
		this._EmissionColor = MaterialFingerprint._Round(material.GetColor(ShaderProps._EmissionColor), 100, used._EmissionColor);
		this._EmissionMap = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._EmissionMap, used._EmissionMap);
		this._EmissionMaskByBaseMapAlpha = MaterialFingerprint._Round(material.GetFloat(ShaderProps._EmissionMaskByBaseMapAlpha), 100, used._EmissionMaskByBaseMapAlpha);
		this._EmissionUVScrollSpeed = MaterialFingerprint._Round(material.GetVector(ShaderProps._EmissionUVScrollSpeed), 100, used._EmissionUVScrollSpeed);
		this._EmissionDissolveProgress = MaterialFingerprint._Round(material.GetFloat(ShaderProps._EmissionDissolveProgress), 100, used._EmissionDissolveProgress);
		this._EmissionDissolveAnimation = MaterialFingerprint._Round(material.GetVector(ShaderProps._EmissionDissolveAnimation), 100, used._EmissionDissolveAnimation);
		this._EmissionDissolveEdgeSize = MaterialFingerprint._Round(material.GetFloat(ShaderProps._EmissionDissolveEdgeSize), 100, used._EmissionDissolveEdgeSize);
		this._EmissionIntensityInDynamic = MaterialFingerprint._Round(material.GetFloat(ShaderProps._EmissionIntensityInDynamic), 100, used._EmissionIntensityInDynamic);
		this._EmissionUseUVWaveWarp = MaterialFingerprint._Round(material.GetFloat(ShaderProps._EmissionUseUVWaveWarp), 100, used._EmissionUseUVWaveWarp);
		this._GreyZoneException = MaterialFingerprint._Round(material.GetFloat(ShaderProps._GreyZoneException), 100, used._GreyZoneException);
		this._Cull = MaterialFingerprint._Round(material.GetFloat(ShaderProps._Cull), 100, used._Cull);
		this._StencilReference = MaterialFingerprint._Round(material.GetFloat(ShaderProps._StencilReference), 100, used._StencilReference);
		this._StencilComparison = MaterialFingerprint._Round(material.GetFloat(ShaderProps._StencilComparison), 100, used._StencilComparison);
		this._StencilPassFront = MaterialFingerprint._Round(material.GetFloat(ShaderProps._StencilPassFront), 100, used._StencilPassFront);
		this._USE_DEFORM_MAP = MaterialFingerprint._Round(material.GetFloat(ShaderProps._USE_DEFORM_MAP), 100, used._USE_DEFORM_MAP);
		this._DeformMap = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._DeformMap, used._DeformMap);
		this._DeformMapIntensity = MaterialFingerprint._Round(material.GetFloat(ShaderProps._DeformMapIntensity), 100, used._DeformMapIntensity);
		this._DeformMapMaskByVertColorRAmount = MaterialFingerprint._Round(material.GetFloat(ShaderProps._DeformMapMaskByVertColorRAmount), 100, used._DeformMapMaskByVertColorRAmount);
		this._DeformMapScrollSpeed = MaterialFingerprint._Round(material.GetVector(ShaderProps._DeformMapScrollSpeed), 100, used._DeformMapScrollSpeed);
		this._DeformMapUV0Influence = MaterialFingerprint._Round(material.GetVector(ShaderProps._DeformMapUV0Influence), 100, used._DeformMapUV0Influence);
		this._DeformMapObjectSpaceOffsetsU = MaterialFingerprint._Round(material.GetVector(ShaderProps._DeformMapObjectSpaceOffsetsU), 100, used._DeformMapObjectSpaceOffsetsU);
		this._DeformMapObjectSpaceOffsetsV = MaterialFingerprint._Round(material.GetVector(ShaderProps._DeformMapObjectSpaceOffsetsV), 100, used._DeformMapObjectSpaceOffsetsV);
		this._DeformMapWorldSpaceOffsetsU = MaterialFingerprint._Round(material.GetVector(ShaderProps._DeformMapWorldSpaceOffsetsU), 100, used._DeformMapWorldSpaceOffsetsU);
		this._DeformMapWorldSpaceOffsetsV = MaterialFingerprint._Round(material.GetVector(ShaderProps._DeformMapWorldSpaceOffsetsV), 100, used._DeformMapWorldSpaceOffsetsV);
		this._RotateOnYAxisBySinTime = MaterialFingerprint._Round(material.GetVector(ShaderProps._RotateOnYAxisBySinTime), 100, used._RotateOnYAxisBySinTime);
		this._USE_TEX_ARRAY_ATLAS = MaterialFingerprint._Round(material.GetFloat(ShaderProps._USE_TEX_ARRAY_ATLAS), 100, used._USE_TEX_ARRAY_ATLAS);
		this._BaseMap_Atlas = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._BaseMap_Atlas, used._BaseMap_Atlas);
		this._BaseMap_AtlasSlice = MaterialFingerprint._Round(material.GetFloat(ShaderProps._BaseMap_AtlasSlice), 100, used._BaseMap_AtlasSlice);
		this._BaseMap_AtlasSliceSource = MaterialFingerprint._Round(material.GetFloat(ShaderProps._BaseMap_AtlasSliceSource), 100, used._BaseMap_AtlasSliceSource);
		this._EmissionMap_Atlas = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._EmissionMap_Atlas, used._EmissionMap_Atlas);
		this._EmissionMap_AtlasSlice = MaterialFingerprint._Round(material.GetFloat(ShaderProps._EmissionMap_AtlasSlice), 100, used._EmissionMap_AtlasSlice);
		this._DeformMap_Atlas = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._DeformMap_Atlas, used._DeformMap_Atlas);
		this._DeformMap_AtlasSlice = MaterialFingerprint._Round(material.GetFloat(ShaderProps._DeformMap_AtlasSlice), 100, used._DeformMap_AtlasSlice);
		this._WeatherMap_Atlas = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._WeatherMap_Atlas, used._WeatherMap_Atlas);
		this._WeatherMap_AtlasSlice = MaterialFingerprint._Round(material.GetFloat(ShaderProps._WeatherMap_AtlasSlice), 100, used._WeatherMap_AtlasSlice);
		this._DEBUG_PAWN_DATA = MaterialFingerprint._Round(material.GetFloat(ShaderProps._DEBUG_PAWN_DATA), 100, used._DEBUG_PAWN_DATA);
		this._SrcBlend = MaterialFingerprint._Round(material.GetFloat(ShaderProps._SrcBlend), 100, used._SrcBlend);
		this._DstBlend = MaterialFingerprint._Round(material.GetFloat(ShaderProps._DstBlend), 100, used._DstBlend);
		this._SrcBlendAlpha = MaterialFingerprint._Round(material.GetFloat(ShaderProps._SrcBlendAlpha), 100, used._SrcBlendAlpha);
		this._DstBlendAlpha = MaterialFingerprint._Round(material.GetFloat(ShaderProps._DstBlendAlpha), 100, used._DstBlendAlpha);
		this._ZWrite = MaterialFingerprint._Round(material.GetFloat(ShaderProps._ZWrite), 100, used._ZWrite);
		this._AlphaToMask = MaterialFingerprint._Round(material.GetFloat(ShaderProps._AlphaToMask), 100, used._AlphaToMask);
		this._Color = MaterialFingerprint._Round(material.GetColor(ShaderProps._Color), 100, used._Color);
		this._Surface = MaterialFingerprint._Round(material.GetFloat(ShaderProps._Surface), 100, used._Surface);
		this._Metallic = MaterialFingerprint._Round(material.GetFloat(ShaderProps._Metallic), 100, used._Metallic);
		this._SpecColor = MaterialFingerprint._Round(material.GetColor(ShaderProps._SpecColor), 100, used._SpecColor);
		this._DayNightLightmapArray = MaterialFingerprint._GetTexPropGuid(material, ShaderProps._DayNightLightmapArray, used._DayNightLightmapArray);
		this._DayNightLightmapArray_ST = MaterialFingerprint._Round(material.GetVector(ShaderProps._DayNightLightmapArray_ST), 100, used._DayNightLightmapArray_ST);
		this._DayNightLightmapArray_AtlasSlice = MaterialFingerprint._Round(material.GetFloat(ShaderProps._DayNightLightmapArray_AtlasSlice), 100, used._DayNightLightmapArray_AtlasSlice);
		this.isValid = true;
	}

	private static int4 _Round(Color c, int mul, int usedCount)
	{
		if (usedCount <= 0)
		{
			return int4.zero;
		}
		return new int4(Mathf.RoundToInt(c.r * (float)mul), Mathf.RoundToInt(c.g * (float)mul), Mathf.RoundToInt(c.b * (float)mul), Mathf.RoundToInt(c.a * (float)mul));
	}

	private static int4 _Round(Vector4 v, int mul, int usedCount)
	{
		if (usedCount <= 0)
		{
			return int4.zero;
		}
		return new int4(Mathf.RoundToInt(v.x * (float)mul), Mathf.RoundToInt(v.y * (float)mul), Mathf.RoundToInt(v.z * (float)mul), Mathf.RoundToInt(v.w * (float)mul));
	}

	private static int _Round(float f, int mul, int usedCount)
	{
		if (usedCount <= 0)
		{
			return 0;
		}
		return Mathf.RoundToInt(f * (float)mul);
	}

	private static TexFormatInfo _GetTexFormatInfo(Material mat, string texPropName, int usedCount)
	{
		if (usedCount > 0)
		{
			Texture2D texture2D = mat.GetTexture(texPropName) as Texture2D;
			if (texture2D != null)
			{
				return new TexFormatInfo(texture2D);
			}
		}
		return default(TexFormatInfo);
	}

	private static string _GetTexPropGuid(Material mat, int texPropId, int usedCount)
	{
		return string.Empty;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GTShaderTransparencyMode GetMatTransparencyMode(Material mat)
	{
		return (GTShaderTransparencyMode)mat.GetInteger(ShaderProps._TransparencyMode);
	}

	public GTShaderTransparencyMode _TransparencyMode;

	public int _Cutoff;

	public int _ColorSource;

	public int4 _BaseColor;

	public int4 _GChannelColor;

	public int4 _BChannelColor;

	public int4 _AChannelColor;

	public int _TexMipBias;

	public string _BaseMap;

	public int4 _BaseMap_ST;

	public int4 _BaseMap_WH;

	public int _TexelSnapToggle;

	public int _TexelSnap_Factor;

	public int _UVSource;

	public int _AlphaDetailToggle;

	public int4 _AlphaDetail_ST;

	public int _AlphaDetail_Opacity;

	public int _AlphaDetail_WorldSpace;

	public int _MaskMapToggle;

	public string _MaskMap;

	public int4 _MaskMap_ST;

	public int4 _MaskMap_WH;

	public int _LavaLampToggle;

	public int _GradientMapToggle;

	public string _GradientMap;

	public int _DoTextureRotation;

	public int _RotateAngle;

	public int _RotateAnim;

	public int _UseWaveWarp;

	public int _WaveAmplitude;

	public int _WaveFrequency;

	public int _WaveScale;

	public int _WaveTimeScale;

	public int _UseWeatherMap;

	public string _WeatherMap;

	public int _WeatherMapDissolveEdgeSize;

	public int _ReflectToggle;

	public int _ReflectBoxProjectToggle;

	public int4 _ReflectBoxCubePos;

	public int4 _ReflectBoxSize;

	public int4 _ReflectBoxRotation;

	public int _ReflectMatcapToggle;

	public int _ReflectMatcapPerspToggle;

	public int _ReflectNormalToggle;

	public string _ReflectTex;

	public string _ReflectNormalTex;

	public int _ReflectAlbedoTint;

	public int4 _ReflectTint;

	public int _ReflectOpacity;

	public int _ReflectExposure;

	public int4 _ReflectOffset;

	public int4 _ReflectScale;

	public int _ReflectRotate;

	public int _HalfLambertToggle;

	public int _ParallaxPlanarToggle;

	public int _ParallaxToggle;

	public int _ParallaxAAToggle;

	public int _ParallaxAABias;

	public string _DepthMap;

	public int _ParallaxAmplitude;

	public int4 _ParallaxSamplesMinMax;

	public int _UvShiftToggle;

	public int4 _UvShiftSteps;

	public int4 _UvShiftRate;

	public int4 _UvShiftOffset;

	public int _UseGridEffect;

	public int _UseCrystalEffect;

	public int _CrystalPower;

	public int4 _CrystalRimColor;

	public int _LiquidVolume;

	public int _LiquidFill;

	public int4 _LiquidFillNormal;

	public int4 _LiquidSurfaceColor;

	public int _LiquidSwayX;

	public int _LiquidSwayY;

	public int _LiquidContainer;

	public int4 _LiquidPlanePosition;

	public int4 _LiquidPlaneNormal;

	public int _VertexFlapToggle;

	public int4 _VertexFlapAxis;

	public int4 _VertexFlapDegreesMinMax;

	public int _VertexFlapSpeed;

	public int _VertexFlapPhaseOffset;

	public int _VertexWaveToggle;

	public int _VertexWaveDebug;

	public int4 _VertexWaveEnd;

	public int4 _VertexWaveParams;

	public int4 _VertexWaveFalloff;

	public int4 _VertexWaveSphereMask;

	public int _VertexWavePhaseOffset;

	public int4 _VertexWaveAxes;

	public int _VertexRotateToggle;

	public int4 _VertexRotateAngles;

	public int _VertexRotateAnim;

	public int _VertexLightToggle;

	public int _InnerGlowOn;

	public int4 _InnerGlowColor;

	public int4 _InnerGlowParams;

	public int _InnerGlowTap;

	public int _InnerGlowSine;

	public int _InnerGlowSinePeriod;

	public int _InnerGlowSinePhaseShift;

	public int _StealthEffectOn;

	public int _UseEyeTracking;

	public int4 _EyeTileOffsetUV;

	public int _EyeOverrideUV;

	public int4 _EyeOverrideUVTransform;

	public int _UseMouthFlap;

	public string _MouthMap;

	public int4 _MouthMap_ST;

	public int _UseVertexColor;

	public int _WaterEffect;

	public int _HeightBasedWaterEffect;

	public int _WaterCaustics;

	public int _UseDayNightLightmap;

	public int _UseSpecular;

	public int _UseSpecularAlphaChannel;

	public int _Smoothness;

	public int _UseSpecHighlight;

	public int4 _SpecularDir;

	public int4 _SpecularPowerIntensity;

	public int4 _SpecularColor;

	public int _SpecularUseDiffuseColor;

	public int _EmissionToggle;

	public int4 _EmissionColor;

	public string _EmissionMap;

	public int _EmissionMaskByBaseMapAlpha;

	public int4 _EmissionUVScrollSpeed;

	public int _EmissionDissolveProgress;

	public int4 _EmissionDissolveAnimation;

	public int _EmissionDissolveEdgeSize;

	public int _EmissionIntensityInDynamic;

	public int _EmissionUseUVWaveWarp;

	public int _GreyZoneException;

	public int _Cull;

	public int _StencilReference;

	public int _StencilComparison;

	public int _StencilPassFront;

	public int _USE_DEFORM_MAP;

	public string _DeformMap;

	public int _DeformMapIntensity;

	public int _DeformMapMaskByVertColorRAmount;

	public int4 _DeformMapScrollSpeed;

	public int4 _DeformMapUV0Influence;

	public int4 _DeformMapObjectSpaceOffsetsU;

	public int4 _DeformMapObjectSpaceOffsetsV;

	public int4 _DeformMapWorldSpaceOffsetsU;

	public int4 _DeformMapWorldSpaceOffsetsV;

	public int4 _RotateOnYAxisBySinTime;

	public int _USE_TEX_ARRAY_ATLAS;

	public string _BaseMap_Atlas;

	public int _BaseMap_AtlasSlice;

	public int _BaseMap_AtlasSliceSource;

	public string _EmissionMap_Atlas;

	public int _EmissionMap_AtlasSlice;

	public string _DeformMap_Atlas;

	public int _DeformMap_AtlasSlice;

	public string _WeatherMap_Atlas;

	public int _WeatherMap_AtlasSlice;

	public int _DEBUG_PAWN_DATA;

	public int _SrcBlend;

	public int _DstBlend;

	public int _SrcBlendAlpha;

	public int _DstBlendAlpha;

	public int _ZWrite;

	public int _AlphaToMask;

	public int4 _Color;

	public int _Surface;

	public int _Metallic;

	public int4 _SpecColor;

	public string _DayNightLightmapArray;

	public int4 _DayNightLightmapArray_ST;

	public int _DayNightLightmapArray_AtlasSlice;

	private const bool _k_UNITY_2023_1_OR_NEWER = true;

	public bool isValid;
}
