using System;
using UnityEngine;

namespace BoingKit
{
	[CreateAssetMenu(fileName = "BoingParams", menuName = "Boing Kit/Shared Boing Params", order = 550)]
	public class SharedBoingParams : ScriptableObject
	{
		public SharedBoingParams()
		{
			this.Params.Init();
		}

		public BoingWork.Params Params;
	}
}
