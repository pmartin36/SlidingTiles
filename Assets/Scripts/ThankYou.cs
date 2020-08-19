using MoreMountains.NiceVibrations;
using UnityEngine;

public class ThankYou : ContextManager
{
	public bool Loaded { get; set; } = true;
	public override void HandleInput(InputPackage p) {}

	public void Menu() {
		MMVibrationManager.Haptic(HapticTypes.Selection);
		GameManager.Instance.LoadScene(
			SceneHelpers.MenuBuildIndex,
			null,
			() => GameManager.Instance.MenuManager.OpenLevelSelect(true)
		);
	}
}
