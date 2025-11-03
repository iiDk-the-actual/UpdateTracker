using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct TextComponentLegacySupportStore
{
	public TextComponentLegacySupportStore(Transform objRef)
	{
		this._objectReference = objRef;
		this._legacyTextReference = null;
		this._legacyTextMeshReference = null;
		this._tmpTextReference = objRef.GetComponent<TMP_Text>();
		if (this._tmpTextReference != null)
		{
			return;
		}
		this._legacyTextReference = objRef.GetComponent<Text>();
		if (this._legacyTextReference)
		{
			return;
		}
		this._legacyTextMeshReference = objRef.GetComponent<TextMesh>();
		if (this._legacyTextMeshReference)
		{
			return;
		}
		Debug.LogError("[LOCALIZATION::TEXT_COMPONENT_LEGACY_SUPPORT_STORE] Could not find either a [TMP_Text], Legacy-[Text], or Legacy-[TextMesh] component on object [" + objRef.name + "]", this._objectReference);
	}

	public bool IsValid
	{
		get
		{
			return this._tmpTextReference || this._legacyTextReference || this._legacyTextMeshReference;
		}
	}

	public float characterSpacing
	{
		get
		{
			if (this._tmpTextReference)
			{
				return this._tmpTextReference.characterSpacing;
			}
			return 0f;
		}
		set
		{
			if (this._tmpTextReference)
			{
				this._tmpTextReference.characterSpacing = value;
				return;
			}
		}
	}

	public void SetFont(TMP_FontAsset font, Font legacyFont)
	{
		if (font != null && this._tmpTextReference)
		{
			this.SetFont(font);
			return;
		}
		if (legacyFont != null && (this._legacyTextReference || this._legacyTextMeshReference))
		{
			this.SetFont(legacyFont);
			return;
		}
		if (!this.IsValid)
		{
			Debug.LogError("[LOCALIZATION::TEXT_COMPONENT_LEGACY_SUPPORT_STORE] Trying to change font but both text references are NULL.");
		}
	}

	public void SetFont(Font font)
	{
		if (this._legacyTextReference)
		{
			this._legacyTextReference.font = font;
			return;
		}
		if (this._legacyTextMeshReference)
		{
			this._legacyTextMeshReference.font = font;
			return;
		}
		Debug.LogError("[LOCALIZATION::TEXT_COMPONENT_LEGACY_SUPPORT_STORE] Trying to change font for non-legacy reference but passed in a legacy font.", font);
	}

	public void SetFont(TMP_FontAsset font)
	{
		if (this._tmpTextReference == null)
		{
			return;
		}
		this._tmpTextReference.font = font;
	}

	public void SetFontSize(float fontSize)
	{
		if (!this._tmpTextReference)
		{
			return;
		}
		TMP_Text tmpTextReference = this._tmpTextReference;
		this._tmpTextReference.fontSizeMax = fontSize;
		tmpTextReference.fontSize = fontSize;
	}

	public string text
	{
		get
		{
			if (this._tmpTextReference)
			{
				return this._tmpTextReference.text;
			}
			if (this._legacyTextReference)
			{
				return this._legacyTextReference.text;
			}
			if (this._legacyTextMeshReference)
			{
				return this._legacyTextMeshReference.text;
			}
			Debug.LogError("[LOCALIZATION::TEXT_COMPONENT_LEGACY_SUPPORT_STORE] Both Legacy Text ref and TMP text ref are null!");
			return "";
		}
		set
		{
			if (this._tmpTextReference != null)
			{
				this._tmpTextReference.text = value;
				return;
			}
			if (this._legacyTextReference != null)
			{
				this._legacyTextReference.text = value;
				return;
			}
			if (this._legacyTextMeshReference)
			{
				this._legacyTextMeshReference.text = value;
				return;
			}
			Debug.LogError("[LOCALIZATION::TEXT_COMPONENT_LEGACY_SUPPORT_STORE] Both Legacy Text ref and TMP text ref are null and cannot be set!", this._objectReference);
		}
	}

	public void SetText(string newText)
	{
		this.text = newText;
	}

	public void SetCharSpacing(float spacing)
	{
		this.characterSpacing = spacing;
	}

	private Transform _objectReference;

	private TMP_Text _tmpTextReference;

	private Text _legacyTextReference;

	private TextMesh _legacyTextMeshReference;
}
