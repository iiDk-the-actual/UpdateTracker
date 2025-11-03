using System;
using UnityEngine;

namespace GorillaTag
{
	[CreateAssetMenu(fileName = "New String List", menuName = "String List")]
	public class StringList : ScriptableObject
	{
		public string[] Strings
		{
			get
			{
				return this.strings;
			}
		}

		[SerializeField]
		private string[] strings;
	}
}
