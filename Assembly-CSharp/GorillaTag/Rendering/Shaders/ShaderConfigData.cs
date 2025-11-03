using System;
using UnityEngine;

namespace GorillaTag.Rendering.Shaders
{
	public class ShaderConfigData
	{
		public static ShaderConfigData.MatPropInt[] convertInts(string[] names, int[] vals)
		{
			ShaderConfigData.MatPropInt[] array = new ShaderConfigData.MatPropInt[names.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ShaderConfigData.MatPropInt
				{
					intName = names[i],
					intVal = vals[i]
				};
			}
			return array;
		}

		public static ShaderConfigData.MatPropFloat[] convertFloats(string[] names, float[] vals)
		{
			ShaderConfigData.MatPropFloat[] array = new ShaderConfigData.MatPropFloat[names.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ShaderConfigData.MatPropFloat
				{
					floatName = names[i],
					floatVal = vals[i]
				};
			}
			return array;
		}

		public static ShaderConfigData.MatPropMatrix[] convertMatrices(string[] names, Matrix4x4[] vals)
		{
			ShaderConfigData.MatPropMatrix[] array = new ShaderConfigData.MatPropMatrix[names.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ShaderConfigData.MatPropMatrix
				{
					matrixName = names[i],
					matrixVal = vals[i]
				};
			}
			return array;
		}

		public static ShaderConfigData.MatPropVector[] convertVectors(string[] names, Vector4[] vals)
		{
			ShaderConfigData.MatPropVector[] array = new ShaderConfigData.MatPropVector[names.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ShaderConfigData.MatPropVector
				{
					vectorName = names[i],
					vectorVal = vals[i]
				};
			}
			return array;
		}

		public static ShaderConfigData.MatPropTexture[] convertTextures(string[] names, Texture[] vals)
		{
			ShaderConfigData.MatPropTexture[] array = new ShaderConfigData.MatPropTexture[names.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ShaderConfigData.MatPropTexture
				{
					textureName = names[i],
					textureVal = vals[i]
				};
			}
			return array;
		}

		public static string GetShaderPropertiesStringFromMaterial(Material mat, bool excludeMainTexData)
		{
			string text = "";
			string[] array = mat.GetPropertyNames(MaterialPropertyType.Int);
			int[] array2 = new int[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = mat.GetInteger(array[i]);
				text += array2[i].ToString();
			}
			array = mat.GetPropertyNames(MaterialPropertyType.Float);
			float[] array3 = new float[array.Length];
			for (int j = 0; j < array.Length; j++)
			{
				if (excludeMainTexData || !array[j].Contains("_BaseMap"))
				{
					array3[j] = mat.GetFloat(array[j]);
					text += array3[j].ToString();
				}
			}
			array = mat.GetPropertyNames(MaterialPropertyType.Matrix);
			Matrix4x4[] array4 = new Matrix4x4[array.Length];
			for (int k = 0; k < array.Length; k++)
			{
				array4[k] = mat.GetMatrix(array[k]);
				text += array4[k].ToString();
			}
			array = mat.GetPropertyNames(MaterialPropertyType.Vector);
			Vector4[] array5 = new Vector4[array.Length];
			for (int l = 0; l < array.Length; l++)
			{
				if (excludeMainTexData || !array[l].Contains("_BaseMap"))
				{
					array5[l] = mat.GetVector(array[l]);
					text += array5[l].ToString();
				}
			}
			array = mat.GetPropertyNames(MaterialPropertyType.Texture);
			Texture[] array6 = new Texture[array.Length];
			for (int m = 0; m < array.Length; m++)
			{
				if (!array[m].Contains("_BaseMap"))
				{
					array6[m] = mat.GetTexture(array[m]);
					if (array6[m] != null)
					{
						text += array6[m].ToString();
					}
				}
			}
			return text;
		}

		public static ShaderConfigData.ShaderConfig GetConfigDataFromMaterial(Material mat, bool includeMainTexData)
		{
			string[] array = mat.GetPropertyNames(MaterialPropertyType.Int);
			string[] array2 = array;
			int[] array3 = new int[array2.Length];
			bool flag = mat.IsKeywordEnabled("_WATER_EFFECT");
			bool flag2 = mat.IsKeywordEnabled("_MAINTEX_ROTATE");
			bool flag3 = mat.IsKeywordEnabled("_UV_WAVE_WARP");
			bool flag4 = mat.IsKeywordEnabled("_EMISSION_USE_UV_WAVE_WARP");
			bool flag5 = flag3 || flag4;
			bool flag6 = mat.IsKeywordEnabled("_LIQUID_CONTAINER");
			bool flag7 = mat.IsKeywordEnabled("_LIQUID_VOLUME") && !flag6;
			bool flag8 = mat.IsKeywordEnabled("_CRYSTAL_EFFECT");
			bool flag9 = mat.IsKeywordEnabled("_EMISSION") || flag8;
			bool flag10 = mat.IsKeywordEnabled("_REFLECTIONS");
			mat.IsKeywordEnabled("_REFLECTIONS_MATCAP");
			bool flag11 = mat.IsKeywordEnabled("_UV_SHIFT");
			for (int i = 0; i < array2.Length; i++)
			{
				array3[i] = mat.GetInteger(array[i]);
				if (!flag11 && (array[i] == "_UvShiftSteps" || array[i] == "_UvShiftOffset"))
				{
					array3[i] = 0;
				}
			}
			array = mat.GetPropertyNames(MaterialPropertyType.Float);
			string[] array4 = array;
			float[] array5 = new float[array4.Length];
			for (int j = 0; j < array.Length; j++)
			{
				if (includeMainTexData || !array[j].Contains("_BaseMap"))
				{
					array5[j] = mat.GetFloat(array[j]);
				}
				if ((!flag && array[j] == "_HeightBasedWaterEffect") || (!flag2 && array[j] == "_RotateSpeed") || (!flag5 && (array[j] == "_WaveAmplitude" || array[j] == "_WaveFrequency" || array[j] == "_WaveScale")) || (!flag7 && (array[j] == "_LiquidFill" || array[j] == "_LiquidSwayX" || array[j] == "_LiquidSwayY")) || (!flag8 && array[j] == "_CrystalPower") || (!flag9 && array[j].StartsWith("_Emission")) || (!flag10 && (array[j] == "_ReflectOpacity" || array[j] == "_ReflectExposure" || array[j] == "_ReflectRotate")) || (!flag11 && array[j] == "_UvShiftRate"))
				{
					array5[j] = 0f;
				}
			}
			array = mat.GetPropertyNames(MaterialPropertyType.Matrix);
			string[] array6 = array;
			Matrix4x4[] array7 = new Matrix4x4[array6.Length];
			for (int k = 0; k < array.Length; k++)
			{
				array7[k] = mat.GetMatrix(array[k]);
			}
			array = mat.GetPropertyNames(MaterialPropertyType.Vector);
			string[] array8 = array;
			Vector4[] array9 = new Vector4[array8.Length];
			for (int l = 0; l < array.Length; l++)
			{
				if (includeMainTexData || !array[l].Contains("_BaseMap"))
				{
					array9[l] = mat.GetVector(array[l]);
				}
				if ((!flag7 && (array[l] == "_LiquidFillNormal" || array[l] == "_LiquidSurfaceColor")) || (!flag6 && (array[l] == "_LiquidPlanePosition" || array[l] == "_LiquidPlaneNormal")) || (!flag8 && array[l] == "_CrystalRimColor") || (!flag9 && array[l].StartsWith("_Emission")) || (!flag10 && (array[l] == "_ReflectTint" || array[l] == "_ReflectOffset" || array[l] == "_ReflectScale")))
				{
					array9[l] = Vector4.zero;
				}
			}
			array = mat.GetPropertyNames(MaterialPropertyType.Texture);
			string[] array10 = array;
			Texture[] array11 = new Texture[array10.Length];
			for (int m = 0; m < array.Length; m++)
			{
				if (!array[m].Contains("_BaseMap"))
				{
					array11[m] = mat.GetTexture(array[m]);
				}
			}
			return new ShaderConfigData.ShaderConfig(mat.shader.name, mat, array2, array3, array4, array5, array6, array7, array8, array9, array10, array11);
		}

		[Serializable]
		public struct ShaderConfig
		{
			public ShaderConfig(string shadName, Material fMat, string[] intNames, int[] intVals, string[] floatNames, float[] floatVals, string[] matrixNames, Matrix4x4[] matrixVals, string[] vectorNames, Vector4[] vectorVals, string[] textureNames, Texture[] textureVals)
			{
				this.shaderName = shadName;
				this.firstMat = fMat;
				this.ints = ShaderConfigData.convertInts(intNames, intVals);
				this.floats = ShaderConfigData.convertFloats(floatNames, floatVals);
				this.matrices = ShaderConfigData.convertMatrices(matrixNames, matrixVals);
				this.vectors = ShaderConfigData.convertVectors(vectorNames, vectorVals);
				this.textures = ShaderConfigData.convertTextures(textureNames, textureVals);
			}

			public string shaderName;

			public Material firstMat;

			public ShaderConfigData.MatPropInt[] ints;

			public ShaderConfigData.MatPropFloat[] floats;

			public ShaderConfigData.MatPropMatrix[] matrices;

			public ShaderConfigData.MatPropVector[] vectors;

			public ShaderConfigData.MatPropTexture[] textures;
		}

		[Serializable]
		public struct MatPropInt
		{
			public string intName;

			public int intVal;
		}

		[Serializable]
		public struct MatPropFloat
		{
			public string floatName;

			public float floatVal;
		}

		[Serializable]
		public struct MatPropMatrix
		{
			public string matrixName;

			public Matrix4x4 matrixVal;
		}

		[Serializable]
		public struct MatPropVector
		{
			public string vectorName;

			public Vector4 vectorVal;
		}

		[Serializable]
		public struct MatPropTexture
		{
			public string textureName;

			public Texture textureVal;
		}

		[Serializable]
		public struct RenderersForShaderWithSameProperties
		{
			public MeshRenderer[] renderers;
		}
	}
}
