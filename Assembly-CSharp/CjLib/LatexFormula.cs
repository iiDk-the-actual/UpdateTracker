using System;
using UnityEngine;

namespace CjLib
{
	[ExecuteInEditMode]
	public class LatexFormula : MonoBehaviour
	{
		public static readonly string BaseUrl = "http://tex.s2cms.ru/svg/f(x) ";

		private int m_hash = LatexFormula.BaseUrl.GetHashCode();

		[SerializeField]
		private string m_formula = "";

		private Texture m_texture;
	}
}
