using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowMillisecondsToggle : Toggle
{
    public void OnChange() {
		GameManager.Instance.SetShowMilliseconds(this.isOn);
	}
}
