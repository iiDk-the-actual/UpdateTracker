using System;
using UnityEngine;
using UnityEngine.Rendering;

public static class UberShader
{
	public static Material ReferenceMaterial
	{
		get
		{
			UberShader.InitDependencies();
			return UberShader.kReferenceMaterial;
		}
	}

	public static Shader ReferenceShader
	{
		get
		{
			UberShader.InitDependencies();
			return UberShader.kReferenceShader;
		}
	}

	public static Material ReferenceMaterialNonSRP
	{
		get
		{
			UberShader.InitDependencies();
			return UberShader.kReferenceMaterialNonSRP;
		}
	}

	public static Shader ReferenceShaderNonSRP
	{
		get
		{
			UberShader.InitDependencies();
			return UberShader.kReferenceShaderNonSRP;
		}
	}

	public static UberShaderProperty[] AllProperties
	{
		get
		{
			UberShader.InitDependencies();
			return UberShader.kProperties;
		}
	}

	public static bool IsAnimated(Material m)
	{
		if (m == null)
		{
			return false;
		}
		if ((double)UberShader.UvShiftToggle.GetValue<float>(m) <= 0.5)
		{
			return false;
		}
		Vector2 value = UberShader.UvShiftRate.GetValue<Vector2>(m);
		return value.x > 0f || value.y > 0f;
	}

	private static UberShaderProperty GetProperty(int i)
	{
		UberShader.InitDependencies();
		return UberShader.kProperties[i];
	}

	private static UberShaderProperty GetProperty(int i, string expectedName)
	{
		UberShader.InitDependencies();
		return UberShader.kProperties[i];
	}

	private static void InitDependencies()
	{
		if (UberShader.gInitialized)
		{
			return;
		}
		UberShader.kReferenceShader = Shader.Find("GorillaTag/UberShader");
		UberShader.kReferenceMaterial = new Material(UberShader.kReferenceShader);
		UberShader.kReferenceShaderNonSRP = Shader.Find("GorillaTag/UberShaderNonSRP");
		UberShader.kReferenceMaterialNonSRP = new Material(UberShader.kReferenceShaderNonSRP);
		UberShader.kProperties = UberShader.EnumerateAllProperties(UberShader.kReferenceShader);
		UberShader.gInitialized = true;
	}

	public static Shader GetShader()
	{
		UberShader.InitDependencies();
		return UberShader.kReferenceShader;
	}

	private static UberShaderProperty[] EnumerateAllProperties(Shader uberShader)
	{
		int propertyCount = uberShader.GetPropertyCount();
		UberShaderProperty[] array = new UberShaderProperty[propertyCount];
		for (int i = 0; i < propertyCount; i++)
		{
			UberShaderProperty uberShaderProperty = new UberShaderProperty
			{
				index = i,
				flags = uberShader.GetPropertyFlags(i),
				type = uberShader.GetPropertyType(i),
				nameID = uberShader.GetPropertyNameId(i),
				name = uberShader.GetPropertyName(i),
				attributes = uberShader.GetPropertyAttributes(i)
			};
			if (uberShaderProperty.type == ShaderPropertyType.Range)
			{
				uberShaderProperty.rangeLimits = uberShader.GetPropertyRangeLimits(uberShaderProperty.index);
			}
			string[] attributes = uberShaderProperty.attributes;
			if (attributes != null && attributes.Length != 0)
			{
				foreach (string text in attributes)
				{
					if (!string.IsNullOrWhiteSpace(text))
					{
						bool flag = text.StartsWith("Toggle(");
						uberShaderProperty.isKeywordToggle = flag;
						if (flag)
						{
							string text2 = text.Split('(', StringSplitOptions.RemoveEmptyEntries)[1].RemoveEnd(")", StringComparison.InvariantCulture);
							uberShaderProperty.keyword = text2;
						}
					}
				}
			}
			array[i] = uberShaderProperty;
		}
		return array;
	}

	private static Shader kReferenceShader;

	private static Material kReferenceMaterial;

	private static Shader kReferenceShaderNonSRP;

	private static Material kReferenceMaterialNonSRP;

	private static UberShaderProperty[] kProperties;

	private static bool gInitialized = false;

	public static UberShaderProperty TransparencyMode = UberShader.GetProperty(0);

	public static UberShaderProperty Cutoff = UberShader.GetProperty(1);

	public static UberShaderProperty ColorSource = UberShader.GetProperty(2);

	public static UberShaderProperty BaseColor = UberShader.GetProperty(3);

	public static UberShaderProperty GChannelColor = UberShader.GetProperty(4);

	public static UberShaderProperty BChannelColor = UberShader.GetProperty(5);

	public static UberShaderProperty AChannelColor = UberShader.GetProperty(6);

	public static UberShaderProperty BaseMap = UberShader.GetProperty(7);

	public static UberShaderProperty BaseMap_WH = UberShader.GetProperty(8);

	public static UberShaderProperty TexelSnapToggle = UberShader.GetProperty(9);

	public static UberShaderProperty TexelSnap_Factor = UberShader.GetProperty(10);

	public static UberShaderProperty UVSource = UberShader.GetProperty(11);

	public static UberShaderProperty AlphaDetailToggle = UberShader.GetProperty(12);

	public static UberShaderProperty AlphaDetail_ST = UberShader.GetProperty(13);

	public static UberShaderProperty AlphaDetail_Opacity = UberShader.GetProperty(14);

	public static UberShaderProperty AlphaDetail_WorldSpace = UberShader.GetProperty(15);

	public static UberShaderProperty MaskMapToggle = UberShader.GetProperty(16);

	public static UberShaderProperty MaskMap = UberShader.GetProperty(17);

	public static UberShaderProperty MaskMap_WH = UberShader.GetProperty(18);

	public static UberShaderProperty LavaLampToggle = UberShader.GetProperty(19);

	public static UberShaderProperty GradientMapToggle = UberShader.GetProperty(20);

	public static UberShaderProperty GradientMap = UberShader.GetProperty(21);

	public static UberShaderProperty DoTextureRotation = UberShader.GetProperty(22);

	public static UberShaderProperty RotateAngle = UberShader.GetProperty(23);

	public static UberShaderProperty RotateAnim = UberShader.GetProperty(24);

	public static UberShaderProperty UseWaveWarp = UberShader.GetProperty(25);

	public static UberShaderProperty WaveAmplitude = UberShader.GetProperty(26);

	public static UberShaderProperty WaveFrequency = UberShader.GetProperty(27);

	public static UberShaderProperty WaveScale = UberShader.GetProperty(28);

	public static UberShaderProperty WaveTimeScale = UberShader.GetProperty(29);

	public static UberShaderProperty UseWeatherMap = UberShader.GetProperty(30);

	public static UberShaderProperty WeatherMap = UberShader.GetProperty(31);

	public static UberShaderProperty WeatherMapDissolveEdgeSize = UberShader.GetProperty(32);

	public static UberShaderProperty ReflectToggle = UberShader.GetProperty(33);

	public static UberShaderProperty ReflectBoxProjectToggle = UberShader.GetProperty(34);

	public static UberShaderProperty ReflectBoxCubePos = UberShader.GetProperty(35);

	public static UberShaderProperty ReflectBoxSize = UberShader.GetProperty(36);

	public static UberShaderProperty ReflectBoxRotation = UberShader.GetProperty(37);

	public static UberShaderProperty ReflectMatcapToggle = UberShader.GetProperty(38);

	public static UberShaderProperty ReflectMatcapPerspToggle = UberShader.GetProperty(39);

	public static UberShaderProperty ReflectNormalToggle = UberShader.GetProperty(40);

	public static UberShaderProperty ReflectTex = UberShader.GetProperty(41);

	public static UberShaderProperty ReflectNormalTex = UberShader.GetProperty(42);

	public static UberShaderProperty ReflectAlbedoTint = UberShader.GetProperty(43);

	public static UberShaderProperty ReflectTint = UberShader.GetProperty(44);

	public static UberShaderProperty ReflectOpacity = UberShader.GetProperty(45);

	public static UberShaderProperty ReflectExposure = UberShader.GetProperty(46);

	public static UberShaderProperty ReflectOffset = UberShader.GetProperty(47);

	public static UberShaderProperty ReflectScale = UberShader.GetProperty(48);

	public static UberShaderProperty ReflectRotate = UberShader.GetProperty(49);

	public static UberShaderProperty HalfLambertToggle = UberShader.GetProperty(50);

	public static UberShaderProperty ZFightOffset = UberShader.GetProperty(51);

	public static UberShaderProperty ParallaxPlanarToggle = UberShader.GetProperty(52);

	public static UberShaderProperty ParallaxToggle = UberShader.GetProperty(53);

	public static UberShaderProperty ParallaxAAToggle = UberShader.GetProperty(54);

	public static UberShaderProperty ParallaxAABias = UberShader.GetProperty(55);

	public static UberShaderProperty DepthMap = UberShader.GetProperty(56);

	public static UberShaderProperty ParallaxAmplitude = UberShader.GetProperty(57);

	public static UberShaderProperty ParallaxSamplesMinMax = UberShader.GetProperty(58);

	public static UberShaderProperty UvShiftToggle = UberShader.GetProperty(59);

	public static UberShaderProperty UvShiftSteps = UberShader.GetProperty(60);

	public static UberShaderProperty UvShiftRate = UberShader.GetProperty(61);

	public static UberShaderProperty UvShiftOffset = UberShader.GetProperty(62);

	public static UberShaderProperty UseGridEffect = UberShader.GetProperty(63);

	public static UberShaderProperty UseCrystalEffect = UberShader.GetProperty(64);

	public static UberShaderProperty CrystalPower = UberShader.GetProperty(65);

	public static UberShaderProperty CrystalRimColor = UberShader.GetProperty(66);

	public static UberShaderProperty LiquidVolume = UberShader.GetProperty(67);

	public static UberShaderProperty LiquidFill = UberShader.GetProperty(68);

	public static UberShaderProperty LiquidFillNormal = UberShader.GetProperty(69);

	public static UberShaderProperty LiquidSurfaceColor = UberShader.GetProperty(70);

	public static UberShaderProperty LiquidSwayX = UberShader.GetProperty(71);

	public static UberShaderProperty LiquidSwayY = UberShader.GetProperty(72);

	public static UberShaderProperty LiquidContainer = UberShader.GetProperty(73);

	public static UberShaderProperty LiquidPlanePosition = UberShader.GetProperty(74);

	public static UberShaderProperty LiquidPlaneNormal = UberShader.GetProperty(75);

	public static UberShaderProperty VertexFlapToggle = UberShader.GetProperty(76);

	public static UberShaderProperty VertexFlapAxis = UberShader.GetProperty(77);

	public static UberShaderProperty VertexFlapDegreesMinMax = UberShader.GetProperty(78);

	public static UberShaderProperty VertexFlapSpeed = UberShader.GetProperty(79);

	public static UberShaderProperty VertexFlapPhaseOffset = UberShader.GetProperty(80);

	public static UberShaderProperty VertexWaveToggle = UberShader.GetProperty(81);

	public static UberShaderProperty VertexWaveDebug = UberShader.GetProperty(82);

	public static UberShaderProperty VertexWaveEnd = UberShader.GetProperty(83);

	public static UberShaderProperty VertexWaveParams = UberShader.GetProperty(84);

	public static UberShaderProperty VertexWaveFalloff = UberShader.GetProperty(85);

	public static UberShaderProperty VertexWaveSphereMask = UberShader.GetProperty(86);

	public static UberShaderProperty VertexWavePhaseOffset = UberShader.GetProperty(87);

	public static UberShaderProperty VertexWaveAxes = UberShader.GetProperty(88);

	public static UberShaderProperty VertexRotateToggle = UberShader.GetProperty(89);

	public static UberShaderProperty VertexRotateAngles = UberShader.GetProperty(90);

	public static UberShaderProperty VertexRotateAnim = UberShader.GetProperty(91);

	public static UberShaderProperty VertexLightToggle = UberShader.GetProperty(92);

	public static UberShaderProperty InnerGlowOn = UberShader.GetProperty(93);

	public static UberShaderProperty InnerGlowColor = UberShader.GetProperty(94);

	public static UberShaderProperty InnerGlowParams = UberShader.GetProperty(95);

	public static UberShaderProperty InnerGlowTap = UberShader.GetProperty(96);

	public static UberShaderProperty InnerGlowSine = UberShader.GetProperty(97);

	public static UberShaderProperty InnerGlowSinePeriod = UberShader.GetProperty(98);

	public static UberShaderProperty InnerGlowSinePhaseShift = UberShader.GetProperty(99);

	public static UberShaderProperty StealthEffectOn = UberShader.GetProperty(100);

	public static UberShaderProperty UseEyeTracking = UberShader.GetProperty(101);

	public static UberShaderProperty EyeTileOffsetUV = UberShader.GetProperty(102);

	public static UberShaderProperty EyeOverrideUV = UberShader.GetProperty(103);

	public static UberShaderProperty EyeOverrideUVTransform = UberShader.GetProperty(104);

	public static UberShaderProperty UseMouthFlap = UberShader.GetProperty(105);

	public static UberShaderProperty MouthMap = UberShader.GetProperty(106);

	public static UberShaderProperty MouthMap_Atlas = UberShader.GetProperty(107);

	public static UberShaderProperty MouthMap_AtlasSlice = UberShader.GetProperty(108);

	public static UberShaderProperty UseVertexColor = UberShader.GetProperty(109);

	public static UberShaderProperty WaterEffect = UberShader.GetProperty(110);

	public static UberShaderProperty HeightBasedWaterEffect = UberShader.GetProperty(111);

	public static UberShaderProperty UseDayNightLightmap = UberShader.GetProperty(112);

	public static UberShaderProperty UseSpecular = UberShader.GetProperty(113);

	public static UberShaderProperty UseSpecularAlphaChannel = UberShader.GetProperty(114);

	public static UberShaderProperty Smoothness = UberShader.GetProperty(115);

	public static UberShaderProperty UseSpecHighlight = UberShader.GetProperty(116);

	public static UberShaderProperty SpecularDir = UberShader.GetProperty(117);

	public static UberShaderProperty SpecularPowerIntensity = UberShader.GetProperty(118);

	public static UberShaderProperty SpecularColor = UberShader.GetProperty(119);

	public static UberShaderProperty SpecularUseDiffuseColor = UberShader.GetProperty(120);

	public static UberShaderProperty EmissionToggle = UberShader.GetProperty(121);

	public static UberShaderProperty EmissionColor = UberShader.GetProperty(122);

	public static UberShaderProperty EmissionMap = UberShader.GetProperty(123);

	public static UberShaderProperty EmissionMaskByBaseMapAlpha = UberShader.GetProperty(124);

	public static UberShaderProperty EmissionUVScrollSpeed = UberShader.GetProperty(125);

	public static UberShaderProperty EmissionDissolveProgress = UberShader.GetProperty(126);

	public static UberShaderProperty EmissionDissolveAnimation = UberShader.GetProperty(127);

	public static UberShaderProperty EmissionDissolveEdgeSize = UberShader.GetProperty(128);

	public static UberShaderProperty EmissionUseUVWaveWarp = UberShader.GetProperty(129);

	public static UberShaderProperty GreyZoneException = UberShader.GetProperty(130);

	public static UberShaderProperty Cull = UberShader.GetProperty(131);

	public static UberShaderProperty StencilReference = UberShader.GetProperty(132);

	public static UberShaderProperty StencilComparison = UberShader.GetProperty(133);

	public static UberShaderProperty StencilPassFront = UberShader.GetProperty(134);

	public static UberShaderProperty USE_DEFORM_MAP = UberShader.GetProperty(135);

	public static UberShaderProperty DeformMap = UberShader.GetProperty(136);

	public static UberShaderProperty DeformMapIntensity = UberShader.GetProperty(137);

	public static UberShaderProperty DeformMapMaskByVertColorRAmount = UberShader.GetProperty(138);

	public static UberShaderProperty DeformMapScrollSpeed = UberShader.GetProperty(139);

	public static UberShaderProperty DeformMapUV0Influence = UberShader.GetProperty(140);

	public static UberShaderProperty DeformMapObjectSpaceOffsetsU = UberShader.GetProperty(141);

	public static UberShaderProperty DeformMapObjectSpaceOffsetsV = UberShader.GetProperty(142);

	public static UberShaderProperty DeformMapWorldSpaceOffsetsU = UberShader.GetProperty(143);

	public static UberShaderProperty DeformMapWorldSpaceOffsetsV = UberShader.GetProperty(144);

	public static UberShaderProperty RotateOnYAxisBySinTime = UberShader.GetProperty(145);

	public static UberShaderProperty USE_TEX_ARRAY_ATLAS = UberShader.GetProperty(146);

	public static UberShaderProperty BaseMap_Atlas = UberShader.GetProperty(147);

	public static UberShaderProperty BaseMap_AtlasSlice = UberShader.GetProperty(148);

	public static UberShaderProperty EmissionMap_Atlas = UberShader.GetProperty(149);

	public static UberShaderProperty EmissionMap_AtlasSlice = UberShader.GetProperty(150);

	public static UberShaderProperty DeformMap_Atlas = UberShader.GetProperty(151);

	public static UberShaderProperty DeformMap_AtlasSlice = UberShader.GetProperty(152);

	public static UberShaderProperty DEBUG_PAWN_DATA = UberShader.GetProperty(153);

	public static UberShaderProperty SrcBlend = UberShader.GetProperty(154);

	public static UberShaderProperty DstBlend = UberShader.GetProperty(155);

	public static UberShaderProperty SrcBlendAlpha = UberShader.GetProperty(156);

	public static UberShaderProperty DstBlendAlpha = UberShader.GetProperty(157);

	public static UberShaderProperty ZWrite = UberShader.GetProperty(158);

	public static UberShaderProperty AlphaToMask = UberShader.GetProperty(159);

	public static UberShaderProperty Color = UberShader.GetProperty(160);

	public static UberShaderProperty Surface = UberShader.GetProperty(161);

	public static UberShaderProperty Metallic = UberShader.GetProperty(162);

	public static UberShaderProperty SpecColor = UberShader.GetProperty(163);

	public static UberShaderProperty DayNightLightmapArray = UberShader.GetProperty(164);

	public static UberShaderProperty DayNightLightmapArray_AtlasSlice = UberShader.GetProperty(165);

	public static UberShaderProperty SingleLightmap = UberShader.GetProperty(166);
}
