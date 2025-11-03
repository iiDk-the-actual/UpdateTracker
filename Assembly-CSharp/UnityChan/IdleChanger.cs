using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityChan
{
	public class IdleChanger : MonoBehaviour
	{
		private void Start()
		{
			this.currentState = this.UnityChanA.GetCurrentAnimatorStateInfo(0);
			this.previousState = this.currentState;
			base.StartCoroutine("RandomChange");
			this.kb = Keyboard.current;
		}

		private void Update()
		{
			if (this.kb.upArrowKey.wasPressedThisFrame || this.kb.spaceKey.wasPressedThisFrame)
			{
				this.UnityChanA.SetBool("Next", true);
				this.UnityChanB.SetBool("Next", true);
			}
			if (this.kb.downArrowKey.wasPressedThisFrame)
			{
				this.UnityChanA.SetBool("Back", true);
				this.UnityChanB.SetBool("Back", true);
			}
			if (this.UnityChanA.GetBool("Next"))
			{
				this.currentState = this.UnityChanA.GetCurrentAnimatorStateInfo(0);
				if (this.previousState.fullPathHash != this.currentState.fullPathHash)
				{
					this.UnityChanA.SetBool("Next", false);
					this.UnityChanB.SetBool("Next", false);
					this.previousState = this.currentState;
				}
			}
			if (this.UnityChanA.GetBool("Back"))
			{
				this.currentState = this.UnityChanA.GetCurrentAnimatorStateInfo(0);
				if (this.previousState.fullPathHash != this.currentState.fullPathHash)
				{
					this.UnityChanA.SetBool("Back", false);
					this.UnityChanB.SetBool("Back", false);
					this.previousState = this.currentState;
				}
			}
		}

		private void OnGUI()
		{
			if (this.isGUI)
			{
				GUI.Box(new Rect((float)(Screen.width - 110), 10f, 100f, 90f), "Change Motion");
				if (GUI.Button(new Rect((float)(Screen.width - 100), 40f, 80f, 20f), "Next"))
				{
					this.UnityChanA.SetBool("Next", true);
					this.UnityChanB.SetBool("Next", true);
				}
				if (GUI.Button(new Rect((float)(Screen.width - 100), 70f, 80f, 20f), "Back"))
				{
					this.UnityChanA.SetBool("Back", true);
					this.UnityChanB.SetBool("Back", true);
				}
			}
		}

		private IEnumerator RandomChange()
		{
			for (;;)
			{
				if (this._random)
				{
					float num = Random.Range(0f, 1f);
					if (num < this._threshold)
					{
						this.UnityChanA.SetBool("Back", true);
						this.UnityChanB.SetBool("Back", true);
					}
					else if (num >= this._threshold)
					{
						this.UnityChanA.SetBool("Next", true);
						this.UnityChanB.SetBool("Next", true);
					}
				}
				yield return new WaitForSeconds(this._interval);
			}
			yield break;
		}

		private AnimatorStateInfo currentState;

		private AnimatorStateInfo previousState;

		public bool _random;

		public float _threshold = 0.5f;

		public float _interval = 10f;

		public bool isGUI = true;

		public Animator UnityChanA;

		public Animator UnityChanB;

		private Keyboard kb;
	}
}
