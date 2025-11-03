using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GorillaTagScripts
{
	[CreateAssetMenu(fileName = "GorillaCaveCrystalSetup", menuName = "ScriptableObjects/GorillaCaveCrystalSetup", order = 0)]
	public class GorillaCaveCrystalSetup : ScriptableObject
	{
		public static GorillaCaveCrystalSetup Instance
		{
			get
			{
				return GorillaCaveCrystalSetup.gInstance;
			}
		}

		private void OnEnable()
		{
			if (GorillaCaveCrystalSetup.gInstance == null)
			{
				GorillaCaveCrystalSetup.gInstance = this;
			}
		}

		public GorillaCaveCrystalSetup.CrystalDef[] GetCrystalDefs()
		{
			return (from f in typeof(GorillaCaveCrystalSetup).GetRuntimeFields()
				where f != null && f.FieldType == typeof(GorillaCaveCrystalSetup.CrystalDef)
				select (GorillaCaveCrystalSetup.CrystalDef)f.GetValue(this)).ToArray<GorillaCaveCrystalSetup.CrystalDef>();
		}

		public Material SharedBase;

		public Texture2D CrystalAlbedo;

		public Texture2D CrystalDarkAlbedo;

		public GorillaCaveCrystalSetup.CrystalDef Red;

		public GorillaCaveCrystalSetup.CrystalDef Orange;

		public GorillaCaveCrystalSetup.CrystalDef Yellow;

		public GorillaCaveCrystalSetup.CrystalDef Green;

		public GorillaCaveCrystalSetup.CrystalDef Teal;

		public GorillaCaveCrystalSetup.CrystalDef DarkBlue;

		public GorillaCaveCrystalSetup.CrystalDef Pink;

		public GorillaCaveCrystalSetup.CrystalDef Dark;

		public GorillaCaveCrystalSetup.CrystalDef DarkLight;

		public GorillaCaveCrystalSetup.CrystalDef DarkLightUnderWater;

		[SerializeField]
		[TextArea(4, 10)]
		private string _notes;

		[Space]
		[SerializeField]
		private GameObject _target;

		private static GorillaCaveCrystalSetup gInstance;

		private static GorillaCaveCrystalSetup.CrystalDef[] gCrystalDefs;

		[Serializable]
		public class CrystalDef
		{
			public Material keyMaterial;

			public CrystalVisualsPreset visualPreset;

			[Space]
			public int low;

			public int mid;

			public int high;
		}
	}
}
