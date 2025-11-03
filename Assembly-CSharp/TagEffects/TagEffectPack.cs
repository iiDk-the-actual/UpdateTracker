using System;
using UnityEngine;

namespace TagEffects
{
	[CreateAssetMenu(fileName = "New Tag Effect Pack", menuName = "Tag Effect Pack")]
	public class TagEffectPack : ScriptableObject
	{
		public GameObject thirdPerson;

		public bool thirdPersonParentEffect = true;

		public GameObject firstPerson;

		public bool firstPersonParentEffect = true;

		public GameObject highFive;

		public bool highFiveParentEffect;

		public GameObject fistBump;

		public bool fistBumpParentEffect;

		public bool shouldFaceTagger;
	}
}
