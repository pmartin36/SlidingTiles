using MoreMountains.NiceVibrations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowMillisecondsToggle : Toggle
{
    public void OnChange() {
		MMVibrationManager.Haptic(HapticTypes.Selection);
		GameManager.Instance.SetShowMilliseconds(this.isOn);
	}
}
