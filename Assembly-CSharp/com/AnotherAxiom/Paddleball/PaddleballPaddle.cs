using System;
using UnityEngine;

namespace com.AnotherAxiom.Paddleball
{
	public class PaddleballPaddle : MonoBehaviour
	{
		public bool Right
		{
			get
			{
				return this.right;
			}
		}

		[SerializeField]
		private bool right;
	}
}
