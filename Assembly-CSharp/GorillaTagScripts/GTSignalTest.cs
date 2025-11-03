using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTagScripts
{
	public class GTSignalTest : GTSignalListener
	{
		public MeshRenderer[] targets = new MeshRenderer[0];

		[Space]
		public MeshRenderer target;

		public List<GTSignalListener> listeners = new List<GTSignalListener>(12);
	}
}
