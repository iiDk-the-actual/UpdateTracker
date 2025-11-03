using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace GorillaTagScripts.VirtualStumpCustomMaps.UI
{
	public class CustomMapsKeyButton : GorillaKeyButton<CustomMapKeyboardBinding>
	{
		protected override void OnEnableEvents()
		{
			base.OnEnableEvents();
			if (!this._isLocalized)
			{
				return;
			}
			this.OnLanguageChanged();
			LocalisationManager.RegisterOnLanguageChanged(new Action(this.OnLanguageChanged));
		}

		protected override void OnDisableEvents()
		{
			base.OnDisableEvents();
			if (!this._isLocalized)
			{
				return;
			}
			LocalisationManager.UnregisterOnLanguageChanged(new Action(this.OnLanguageChanged));
		}

		public static string BindingToString(CustomMapKeyboardBinding binding)
		{
			if (binding < CustomMapKeyboardBinding.up || (binding > CustomMapKeyboardBinding.option3 && binding < CustomMapKeyboardBinding.at))
			{
				if (binding >= CustomMapKeyboardBinding.up)
				{
					return binding.ToString();
				}
				int num = (int)binding;
				return num.ToString();
			}
			else
			{
				switch (binding)
				{
				case CustomMapKeyboardBinding.at:
					return "@";
				case CustomMapKeyboardBinding.dash:
					return "-";
				case CustomMapKeyboardBinding.period:
					return ".";
				case CustomMapKeyboardBinding.underscore:
					return "_";
				case CustomMapKeyboardBinding.plus:
					return "+";
				case CustomMapKeyboardBinding.space:
					return " ";
				default:
					return "";
				}
			}
		}

		protected override void OnButtonPressedEvent()
		{
		}

		private void OnLanguageChanged()
		{
			if (!this._isLocalized)
			{
				return;
			}
			if (this._buttonDisplayNameTxt == null)
			{
				Debug.LogError("[LOCALIZATION::CUSTOM_MAPS_KEY_BUTTON] [_buttonDisplayNameTxt] has not been assigned and is NULL", this);
				return;
			}
			if (this._localizedName == null || this._localizedName.IsEmpty)
			{
				Debug.LogError("[LOCALIZATION::CUSTOM_MAPS_KEY_BUTTON] [_localizedName] has not been assigned", this);
				return;
			}
			this._buttonDisplayNameTxt.text = this._localizedName.GetLocalizedString();
		}

		[SerializeField]
		private bool _isLocalized;

		[SerializeField]
		private LocalizedString _localizedName;

		[SerializeField]
		private TMP_Text _buttonDisplayNameTxt;
	}
}
