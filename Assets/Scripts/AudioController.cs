using MoreMountains.NiceVibrations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SoundType {
	Music,
	SFX
}

public class AudioController : MonoBehaviour
{
	public SoundType SoundType;
	private Slider slider;

	public Image Icon;

	public void Start() {
		slider = GetComponentInChildren<Slider>();

		slider.value = SoundType == SoundType.SFX
			? GameManager.Instance.SaveData.FxVolume
			: GameManager.Instance.SaveData.MusicVolume;
		SetIconColor();
	}

	public void OnSliderValueChange() {
		SetIconColor();

		// tell audio manager that value changed
		GameManager.Instance.AdjustAudio(SoundType, slider.value);
	}

	public void OnMuteButtonToggle() {
		MMVibrationManager.Haptic(HapticTypes.Selection);
		bool isMuting = slider.value > 0;
		slider.value = isMuting ? 0 : 1; // maybe should animate
		OnSliderValueChange();
	}

	public void SetIconColor() {
		Color c = Icon.color;
		c.a = slider.value * 0.66f + 0.34f;
		Icon.color = c;
	}
}
