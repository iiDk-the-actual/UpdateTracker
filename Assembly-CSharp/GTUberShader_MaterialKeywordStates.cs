using System;
using UnityEngine;

public struct GTUberShader_MaterialKeywordStates
{
	public GTUberShader_MaterialKeywordStates(Material mat)
	{
		this.material = mat;
		this.STEREO_INSTANCING_ON = mat.IsKeywordEnabled("STEREO_INSTANCING_ON");
		this.UNITY_SINGLE_PASS_STEREO = mat.IsKeywordEnabled("UNITY_SINGLE_PASS_STEREO");
		this.STEREO_MULTIVIEW_ON = mat.IsKeywordEnabled("STEREO_MULTIVIEW_ON");
		this.STEREO_CUBEMAP_RENDER_ON = mat.IsKeywordEnabled("STEREO_CUBEMAP_RENDER_ON");
		this._GLOBAL_ZONE_LIQUID_TYPE__WATER = mat.IsKeywordEnabled("_GLOBAL_ZONE_LIQUID_TYPE__WATER");
		this._GLOBAL_ZONE_LIQUID_TYPE__LAVA = mat.IsKeywordEnabled("_GLOBAL_ZONE_LIQUID_TYPE__LAVA");
		this._ZONE_LIQUID_SHAPE__CYLINDER = mat.IsKeywordEnabled("_ZONE_LIQUID_SHAPE__CYLINDER");
		this._ZONE_DYNAMIC_LIGHTS__CUSTOMVERTEX = mat.IsKeywordEnabled("_ZONE_DYNAMIC_LIGHTS__CUSTOMVERTEX");
		this._USE_TEXTURE = mat.IsKeywordEnabled("_USE_TEXTURE");
		this.USE_TEXTURE__AS_MASK = mat.IsKeywordEnabled("USE_TEXTURE__AS_MASK");
		this._UV_SOURCE__UV0 = mat.IsKeywordEnabled("_UV_SOURCE__UV0");
		this._UV_SOURCE__WORLD_PLANAR_Y = mat.IsKeywordEnabled("_UV_SOURCE__WORLD_PLANAR_Y");
		this._USE_VERTEX_COLOR = mat.IsKeywordEnabled("_USE_VERTEX_COLOR");
		this._USE_WEATHER_MAP = mat.IsKeywordEnabled("_USE_WEATHER_MAP");
		this._ALPHA_DETAIL_MAP = mat.IsKeywordEnabled("_ALPHA_DETAIL_MAP");
		this._HALF_LAMBERT_TERM = mat.IsKeywordEnabled("_HALF_LAMBERT_TERM");
		this._WATER_EFFECT = mat.IsKeywordEnabled("_WATER_EFFECT");
		this._HEIGHT_BASED_WATER_EFFECT = mat.IsKeywordEnabled("_HEIGHT_BASED_WATER_EFFECT");
		this._WATER_CAUSTICS = mat.IsKeywordEnabled("_WATER_CAUSTICS");
		this._ALPHATEST_ON = mat.IsKeywordEnabled("_ALPHATEST_ON");
		this._MAINTEX_ROTATE = mat.IsKeywordEnabled("_MAINTEX_ROTATE");
		this._UV_WAVE_WARP = mat.IsKeywordEnabled("_UV_WAVE_WARP");
		this._LIQUID_VOLUME = mat.IsKeywordEnabled("_LIQUID_VOLUME");
		this._LIQUID_CONTAINER = mat.IsKeywordEnabled("_LIQUID_CONTAINER");
		this._GT_RIM_LIGHT = mat.IsKeywordEnabled("_GT_RIM_LIGHT");
		this._GT_RIM_LIGHT_FLAT = mat.IsKeywordEnabled("_GT_RIM_LIGHT_FLAT");
		this._GT_RIM_LIGHT_USE_ALPHA = mat.IsKeywordEnabled("_GT_RIM_LIGHT_USE_ALPHA");
		this._SPECULAR_HIGHLIGHT = mat.IsKeywordEnabled("_SPECULAR_HIGHLIGHT");
		this._EMISSION = mat.IsKeywordEnabled("_EMISSION");
		this._EMISSION_USE_UV_WAVE_WARP = mat.IsKeywordEnabled("_EMISSION_USE_UV_WAVE_WARP");
		this._USE_DEFORM_MAP = mat.IsKeywordEnabled("_USE_DEFORM_MAP");
		this._USE_DAY_NIGHT_LIGHTMAP = mat.IsKeywordEnabled("_USE_DAY_NIGHT_LIGHTMAP");
		this._USE_TEX_ARRAY_ATLAS = mat.IsKeywordEnabled("_USE_TEX_ARRAY_ATLAS");
		this._GT_BASE_MAP_ATLAS_SLICE_SOURCE__PROPERTY = mat.IsKeywordEnabled("_GT_BASE_MAP_ATLAS_SLICE_SOURCE__PROPERTY");
		this._GT_BASE_MAP_ATLAS_SLICE_SOURCE__UV1_Z = mat.IsKeywordEnabled("_GT_BASE_MAP_ATLAS_SLICE_SOURCE__UV1_Z");
		this._CRYSTAL_EFFECT = mat.IsKeywordEnabled("_CRYSTAL_EFFECT");
		this._EYECOMP = mat.IsKeywordEnabled("_EYECOMP");
		this._MOUTHCOMP = mat.IsKeywordEnabled("_MOUTHCOMP");
		this._ALPHA_BLUE_LIVE_ON = mat.IsKeywordEnabled("_ALPHA_BLUE_LIVE_ON");
		this._GRID_EFFECT = mat.IsKeywordEnabled("_GRID_EFFECT");
		this._REFLECTIONS = mat.IsKeywordEnabled("_REFLECTIONS");
		this._REFLECTIONS_BOX_PROJECT = mat.IsKeywordEnabled("_REFLECTIONS_BOX_PROJECT");
		this._REFLECTIONS_MATCAP = mat.IsKeywordEnabled("_REFLECTIONS_MATCAP");
		this._REFLECTIONS_MATCAP_PERSP_AWARE = mat.IsKeywordEnabled("_REFLECTIONS_MATCAP_PERSP_AWARE");
		this._REFLECTIONS_ALBEDO_TINT = mat.IsKeywordEnabled("_REFLECTIONS_ALBEDO_TINT");
		this._REFLECTIONS_USE_NORMAL_TEX = mat.IsKeywordEnabled("_REFLECTIONS_USE_NORMAL_TEX");
		this._VERTEX_ROTATE = mat.IsKeywordEnabled("_VERTEX_ROTATE");
		this._VERTEX_ANIM_FLAP = mat.IsKeywordEnabled("_VERTEX_ANIM_FLAP");
		this._VERTEX_ANIM_WAVE = mat.IsKeywordEnabled("_VERTEX_ANIM_WAVE");
		this._VERTEX_ANIM_WAVE_DEBUG = mat.IsKeywordEnabled("_VERTEX_ANIM_WAVE_DEBUG");
		this._GRADIENT_MAP_ON = mat.IsKeywordEnabled("_GRADIENT_MAP_ON");
		this._PARALLAX = mat.IsKeywordEnabled("_PARALLAX");
		this._PARALLAX_AA = mat.IsKeywordEnabled("_PARALLAX_AA");
		this._PARALLAX_PLANAR = mat.IsKeywordEnabled("_PARALLAX_PLANAR");
		this._MASK_MAP_ON = mat.IsKeywordEnabled("_MASK_MAP_ON");
		this._FX_LAVA_LAMP = mat.IsKeywordEnabled("_FX_LAVA_LAMP");
		this._INNER_GLOW = mat.IsKeywordEnabled("_INNER_GLOW");
		this._STEALTH_EFFECT = mat.IsKeywordEnabled("_STEALTH_EFFECT");
		this._UV_SHIFT = mat.IsKeywordEnabled("_UV_SHIFT");
		this._TEXEL_SNAP_UVS = mat.IsKeywordEnabled("_TEXEL_SNAP_UVS");
		this._UNITY_EDIT_MODE = mat.IsKeywordEnabled("_UNITY_EDIT_MODE");
		this._GT_EDITOR_TIME = mat.IsKeywordEnabled("_GT_EDITOR_TIME");
		this._DEBUG_PAWN_DATA = mat.IsKeywordEnabled("_DEBUG_PAWN_DATA");
		this._COLOR_GRADE_PROTANOMALY = mat.IsKeywordEnabled("_COLOR_GRADE_PROTANOMALY");
		this._COLOR_GRADE_PROTANOPIA = mat.IsKeywordEnabled("_COLOR_GRADE_PROTANOPIA");
		this._COLOR_GRADE_DEUTERANOMALY = mat.IsKeywordEnabled("_COLOR_GRADE_DEUTERANOMALY");
		this._COLOR_GRADE_DEUTERANOPIA = mat.IsKeywordEnabled("_COLOR_GRADE_DEUTERANOPIA");
		this._COLOR_GRADE_TRITANOMALY = mat.IsKeywordEnabled("_COLOR_GRADE_TRITANOMALY");
		this._COLOR_GRADE_TRITANOPIA = mat.IsKeywordEnabled("_COLOR_GRADE_TRITANOPIA");
		this._COLOR_GRADE_ACHROMATOMALY = mat.IsKeywordEnabled("_COLOR_GRADE_ACHROMATOMALY");
		this._COLOR_GRADE_ACHROMATOPSIA = mat.IsKeywordEnabled("_COLOR_GRADE_ACHROMATOPSIA");
		this.LIGHTMAP_ON = mat.IsKeywordEnabled("LIGHTMAP_ON");
		this.DIRLIGHTMAP_COMBINED = mat.IsKeywordEnabled("DIRLIGHTMAP_COMBINED");
		this.INSTANCING_ON = mat.IsKeywordEnabled("INSTANCING_ON");
	}

	public void Refresh()
	{
		Material material = this.material;
		this.STEREO_INSTANCING_ON = material.IsKeywordEnabled("STEREO_INSTANCING_ON");
		this.UNITY_SINGLE_PASS_STEREO = material.IsKeywordEnabled("UNITY_SINGLE_PASS_STEREO");
		this.STEREO_MULTIVIEW_ON = material.IsKeywordEnabled("STEREO_MULTIVIEW_ON");
		this.STEREO_CUBEMAP_RENDER_ON = material.IsKeywordEnabled("STEREO_CUBEMAP_RENDER_ON");
		this._GLOBAL_ZONE_LIQUID_TYPE__WATER = material.IsKeywordEnabled("_GLOBAL_ZONE_LIQUID_TYPE__WATER");
		this._GLOBAL_ZONE_LIQUID_TYPE__LAVA = material.IsKeywordEnabled("_GLOBAL_ZONE_LIQUID_TYPE__LAVA");
		this._ZONE_LIQUID_SHAPE__CYLINDER = material.IsKeywordEnabled("_ZONE_LIQUID_SHAPE__CYLINDER");
		this._ZONE_DYNAMIC_LIGHTS__CUSTOMVERTEX = material.IsKeywordEnabled("_ZONE_DYNAMIC_LIGHTS__CUSTOMVERTEX");
		this._USE_TEXTURE = material.IsKeywordEnabled("_USE_TEXTURE");
		this.USE_TEXTURE__AS_MASK = material.IsKeywordEnabled("USE_TEXTURE__AS_MASK");
		this._UV_SOURCE__UV0 = material.IsKeywordEnabled("_UV_SOURCE__UV0");
		this._UV_SOURCE__WORLD_PLANAR_Y = material.IsKeywordEnabled("_UV_SOURCE__WORLD_PLANAR_Y");
		this._USE_VERTEX_COLOR = material.IsKeywordEnabled("_USE_VERTEX_COLOR");
		this._USE_WEATHER_MAP = material.IsKeywordEnabled("_USE_WEATHER_MAP");
		this._ALPHA_DETAIL_MAP = material.IsKeywordEnabled("_ALPHA_DETAIL_MAP");
		this._HALF_LAMBERT_TERM = material.IsKeywordEnabled("_HALF_LAMBERT_TERM");
		this._WATER_EFFECT = material.IsKeywordEnabled("_WATER_EFFECT");
		this._HEIGHT_BASED_WATER_EFFECT = material.IsKeywordEnabled("_HEIGHT_BASED_WATER_EFFECT");
		this._WATER_CAUSTICS = material.IsKeywordEnabled("_WATER_CAUSTICS");
		this._ALPHATEST_ON = material.IsKeywordEnabled("_ALPHATEST_ON");
		this._MAINTEX_ROTATE = material.IsKeywordEnabled("_MAINTEX_ROTATE");
		this._UV_WAVE_WARP = material.IsKeywordEnabled("_UV_WAVE_WARP");
		this._LIQUID_VOLUME = material.IsKeywordEnabled("_LIQUID_VOLUME");
		this._LIQUID_CONTAINER = material.IsKeywordEnabled("_LIQUID_CONTAINER");
		this._GT_RIM_LIGHT = material.IsKeywordEnabled("_GT_RIM_LIGHT");
		this._GT_RIM_LIGHT_FLAT = material.IsKeywordEnabled("_GT_RIM_LIGHT_FLAT");
		this._GT_RIM_LIGHT_USE_ALPHA = material.IsKeywordEnabled("_GT_RIM_LIGHT_USE_ALPHA");
		this._SPECULAR_HIGHLIGHT = material.IsKeywordEnabled("_SPECULAR_HIGHLIGHT");
		this._EMISSION = material.IsKeywordEnabled("_EMISSION");
		this._EMISSION_USE_UV_WAVE_WARP = material.IsKeywordEnabled("_EMISSION_USE_UV_WAVE_WARP");
		this._USE_DEFORM_MAP = material.IsKeywordEnabled("_USE_DEFORM_MAP");
		this._USE_DAY_NIGHT_LIGHTMAP = material.IsKeywordEnabled("_USE_DAY_NIGHT_LIGHTMAP");
		this._USE_TEX_ARRAY_ATLAS = material.IsKeywordEnabled("_USE_TEX_ARRAY_ATLAS");
		this._GT_BASE_MAP_ATLAS_SLICE_SOURCE__PROPERTY = material.IsKeywordEnabled("_GT_BASE_MAP_ATLAS_SLICE_SOURCE__PROPERTY");
		this._GT_BASE_MAP_ATLAS_SLICE_SOURCE__UV1_Z = material.IsKeywordEnabled("_GT_BASE_MAP_ATLAS_SLICE_SOURCE__UV1_Z");
		this._CRYSTAL_EFFECT = material.IsKeywordEnabled("_CRYSTAL_EFFECT");
		this._EYECOMP = material.IsKeywordEnabled("_EYECOMP");
		this._MOUTHCOMP = material.IsKeywordEnabled("_MOUTHCOMP");
		this._ALPHA_BLUE_LIVE_ON = material.IsKeywordEnabled("_ALPHA_BLUE_LIVE_ON");
		this._GRID_EFFECT = material.IsKeywordEnabled("_GRID_EFFECT");
		this._REFLECTIONS = material.IsKeywordEnabled("_REFLECTIONS");
		this._REFLECTIONS_BOX_PROJECT = material.IsKeywordEnabled("_REFLECTIONS_BOX_PROJECT");
		this._REFLECTIONS_MATCAP = material.IsKeywordEnabled("_REFLECTIONS_MATCAP");
		this._REFLECTIONS_MATCAP_PERSP_AWARE = material.IsKeywordEnabled("_REFLECTIONS_MATCAP_PERSP_AWARE");
		this._REFLECTIONS_ALBEDO_TINT = material.IsKeywordEnabled("_REFLECTIONS_ALBEDO_TINT");
		this._REFLECTIONS_USE_NORMAL_TEX = material.IsKeywordEnabled("_REFLECTIONS_USE_NORMAL_TEX");
		this._VERTEX_ROTATE = material.IsKeywordEnabled("_VERTEX_ROTATE");
		this._VERTEX_ANIM_FLAP = material.IsKeywordEnabled("_VERTEX_ANIM_FLAP");
		this._VERTEX_ANIM_WAVE = material.IsKeywordEnabled("_VERTEX_ANIM_WAVE");
		this._VERTEX_ANIM_WAVE_DEBUG = material.IsKeywordEnabled("_VERTEX_ANIM_WAVE_DEBUG");
		this._GRADIENT_MAP_ON = material.IsKeywordEnabled("_GRADIENT_MAP_ON");
		this._PARALLAX = material.IsKeywordEnabled("_PARALLAX");
		this._PARALLAX_AA = material.IsKeywordEnabled("_PARALLAX_AA");
		this._PARALLAX_PLANAR = material.IsKeywordEnabled("_PARALLAX_PLANAR");
		this._MASK_MAP_ON = material.IsKeywordEnabled("_MASK_MAP_ON");
		this._FX_LAVA_LAMP = material.IsKeywordEnabled("_FX_LAVA_LAMP");
		this._INNER_GLOW = material.IsKeywordEnabled("_INNER_GLOW");
		this._STEALTH_EFFECT = material.IsKeywordEnabled("_STEALTH_EFFECT");
		this._UV_SHIFT = material.IsKeywordEnabled("_UV_SHIFT");
		this._TEXEL_SNAP_UVS = material.IsKeywordEnabled("_TEXEL_SNAP_UVS");
		this._UNITY_EDIT_MODE = material.IsKeywordEnabled("_UNITY_EDIT_MODE");
		this._GT_EDITOR_TIME = material.IsKeywordEnabled("_GT_EDITOR_TIME");
		this._DEBUG_PAWN_DATA = material.IsKeywordEnabled("_DEBUG_PAWN_DATA");
		this._COLOR_GRADE_PROTANOMALY = material.IsKeywordEnabled("_COLOR_GRADE_PROTANOMALY");
		this._COLOR_GRADE_PROTANOPIA = material.IsKeywordEnabled("_COLOR_GRADE_PROTANOPIA");
		this._COLOR_GRADE_DEUTERANOMALY = material.IsKeywordEnabled("_COLOR_GRADE_DEUTERANOMALY");
		this._COLOR_GRADE_DEUTERANOPIA = material.IsKeywordEnabled("_COLOR_GRADE_DEUTERANOPIA");
		this._COLOR_GRADE_TRITANOMALY = material.IsKeywordEnabled("_COLOR_GRADE_TRITANOMALY");
		this._COLOR_GRADE_TRITANOPIA = material.IsKeywordEnabled("_COLOR_GRADE_TRITANOPIA");
		this._COLOR_GRADE_ACHROMATOMALY = material.IsKeywordEnabled("_COLOR_GRADE_ACHROMATOMALY");
		this._COLOR_GRADE_ACHROMATOPSIA = material.IsKeywordEnabled("_COLOR_GRADE_ACHROMATOPSIA");
		this.LIGHTMAP_ON = material.IsKeywordEnabled("LIGHTMAP_ON");
		this.DIRLIGHTMAP_COMBINED = material.IsKeywordEnabled("DIRLIGHTMAP_COMBINED");
		this.INSTANCING_ON = material.IsKeywordEnabled("INSTANCING_ON");
	}

	public Material material;

	public bool STEREO_INSTANCING_ON;

	public bool UNITY_SINGLE_PASS_STEREO;

	public bool STEREO_MULTIVIEW_ON;

	public bool STEREO_CUBEMAP_RENDER_ON;

	public bool _GLOBAL_ZONE_LIQUID_TYPE__WATER;

	public bool _GLOBAL_ZONE_LIQUID_TYPE__LAVA;

	public bool _ZONE_LIQUID_SHAPE__CYLINDER;

	public bool _ZONE_DYNAMIC_LIGHTS__CUSTOMVERTEX;

	public bool _USE_TEXTURE;

	public bool USE_TEXTURE__AS_MASK;

	public bool _UV_SOURCE__UV0;

	public bool _UV_SOURCE__WORLD_PLANAR_Y;

	public bool _USE_VERTEX_COLOR;

	public bool _USE_WEATHER_MAP;

	public bool _ALPHA_DETAIL_MAP;

	public bool _HALF_LAMBERT_TERM;

	public bool _WATER_EFFECT;

	public bool _HEIGHT_BASED_WATER_EFFECT;

	public bool _WATER_CAUSTICS;

	public bool _ALPHATEST_ON;

	public bool _MAINTEX_ROTATE;

	public bool _UV_WAVE_WARP;

	public bool _LIQUID_VOLUME;

	public bool _LIQUID_CONTAINER;

	public bool _GT_RIM_LIGHT;

	public bool _GT_RIM_LIGHT_FLAT;

	public bool _GT_RIM_LIGHT_USE_ALPHA;

	public bool _SPECULAR_HIGHLIGHT;

	public bool _EMISSION;

	public bool _EMISSION_USE_UV_WAVE_WARP;

	public bool _USE_DEFORM_MAP;

	public bool _USE_DAY_NIGHT_LIGHTMAP;

	public bool _USE_TEX_ARRAY_ATLAS;

	public bool _GT_BASE_MAP_ATLAS_SLICE_SOURCE__PROPERTY;

	public bool _GT_BASE_MAP_ATLAS_SLICE_SOURCE__UV1_Z;

	public bool _CRYSTAL_EFFECT;

	public bool _EYECOMP;

	public bool _MOUTHCOMP;

	public bool _ALPHA_BLUE_LIVE_ON;

	public bool _GRID_EFFECT;

	public bool _REFLECTIONS;

	public bool _REFLECTIONS_BOX_PROJECT;

	public bool _REFLECTIONS_MATCAP;

	public bool _REFLECTIONS_MATCAP_PERSP_AWARE;

	public bool _REFLECTIONS_ALBEDO_TINT;

	public bool _REFLECTIONS_USE_NORMAL_TEX;

	public bool _VERTEX_ROTATE;

	public bool _VERTEX_ANIM_FLAP;

	public bool _VERTEX_ANIM_WAVE;

	public bool _VERTEX_ANIM_WAVE_DEBUG;

	public bool _GRADIENT_MAP_ON;

	public bool _PARALLAX;

	public bool _PARALLAX_AA;

	public bool _PARALLAX_PLANAR;

	public bool _MASK_MAP_ON;

	public bool _FX_LAVA_LAMP;

	public bool _INNER_GLOW;

	public bool _STEALTH_EFFECT;

	public bool _UV_SHIFT;

	public bool _TEXEL_SNAP_UVS;

	public bool _UNITY_EDIT_MODE;

	public bool _GT_EDITOR_TIME;

	public bool _DEBUG_PAWN_DATA;

	public bool _COLOR_GRADE_PROTANOMALY;

	public bool _COLOR_GRADE_PROTANOPIA;

	public bool _COLOR_GRADE_DEUTERANOMALY;

	public bool _COLOR_GRADE_DEUTERANOPIA;

	public bool _COLOR_GRADE_TRITANOMALY;

	public bool _COLOR_GRADE_TRITANOPIA;

	public bool _COLOR_GRADE_ACHROMATOMALY;

	public bool _COLOR_GRADE_ACHROMATOPSIA;

	public bool LIGHTMAP_ON;

	public bool DIRLIGHTMAP_COMBINED;

	public bool INSTANCING_ON;
}
