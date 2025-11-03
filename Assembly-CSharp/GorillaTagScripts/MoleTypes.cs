using System;
using UnityEngine;

namespace GorillaTagScripts
{
	public class MoleTypes : MonoBehaviour
	{
		public bool IsLeftSideMoleType { get; set; }

		public Mole MoleContainerParent { get; set; }

		private void Start()
		{
			this.MoleContainerParent = base.GetComponentInParent<Mole>();
			if (this.MoleContainerParent)
			{
				this.IsLeftSideMoleType = this.MoleContainerParent.IsLeftSideMole;
			}
		}

		public bool isHazard;

		public int scorePoint = 1;

		public MeshRenderer MeshRenderer;

		public Material monkeMoleDefaultMaterial;

		public Material monkeMoleHitMaterial;
	}
}
