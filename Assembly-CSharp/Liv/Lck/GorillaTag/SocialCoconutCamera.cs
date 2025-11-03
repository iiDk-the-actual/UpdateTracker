using System;
using UnityEngine;

namespace Liv.Lck.GorillaTag
{
	public class SocialCoconutCamera : MonoBehaviour
	{
		private void Awake()
		{
			if (this._propertyBlock == null)
			{
				this._propertyBlock = new MaterialPropertyBlock();
			}
			this._propertyBlock.SetInt(this.IS_RECORDING, 0);
			this._bodyRenderer.SetPropertyBlock(this._propertyBlock);
		}

		public void SetVisualsActive(bool active)
		{
			this._isActive = active;
			this._visuals.SetActive(active);
		}

		public void SetRecordingState(bool isRecording)
		{
			if (!this._isActive)
			{
				return;
			}
			this._propertyBlock.SetInt(this.IS_RECORDING, isRecording ? 1 : 0);
			this._bodyRenderer.SetPropertyBlock(this._propertyBlock);
		}

		[SerializeField]
		private GameObject _visuals;

		[SerializeField]
		private MeshRenderer _bodyRenderer;

		private bool _isActive;

		private MaterialPropertyBlock _propertyBlock;

		private string IS_RECORDING = "_Is_Recording";
	}
}
