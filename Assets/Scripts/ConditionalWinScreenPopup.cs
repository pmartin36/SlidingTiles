using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalWinScreenPopup : MonoBehaviour
{
	public WinScreenPopup PopupType;

    public bool ShouldShow(int world, int level) {
		switch (PopupType) {
			case WinScreenPopup.Compare:
				return world > 0 && level > 0 && level % 5 == 0 && !GameManager.Instance.SaveData.HasComparedWithFriends;
			case WinScreenPopup.Rate:
				return world > 1 && level % 9 == 0 && !GameManager.Instance.SaveData.HasClickedToRate;
		}
		return false;
	}
}

public enum WinScreenPopup {
	Compare,
	Rate
}
