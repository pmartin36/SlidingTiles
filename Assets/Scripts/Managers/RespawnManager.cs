using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnManager
{
	private Player Player;
	public Star[] Stars;

	public Vector3 PlayerSpawnPosition;
	public Quaternion PlayerSpawnRotation;

	public ActionButtons ActionButtons;

    public RespawnManager(Scene scene, Player player, bool playerAlive = false) {
		Player = player;
		PlayerSpawnPosition = Player.transform.position;
		PlayerSpawnRotation = Player.transform.rotation;
		Player.SetRespawnManager(this);
		Player.SetAlive(playerAlive);

		Stars = GameObject.FindObjectsOfType<Star>().Where(g => g.gameObject.scene == scene).ToArray();
		ActionButtons = GameObject.FindObjectsOfType<ActionButtons>().First(g => g.gameObject.scene == scene);
		ActionButtons.ForceSetBasedOnPlayerAlive(playerAlive);

		Player.aliveChanged += PlayerAliveChange;
    }

	public void RespawnPlayer() {
		Player.SetAlive(true);
		Player.transform.rotation = PlayerSpawnRotation;
		ActionButtons.ForceSetBasedOnPlayerAlive(true);
		foreach (Star s in Stars) {
			s.Reset();
		}
	}

	public void PlayerAliveChange(object player, bool alive) {
		if(!alive) {
			foreach (Star s in Stars) {
				s.Reset();
			}

			ActionButtons.ForceSetBasedOnPlayerAlive(false);
		}
	}

	public void Destroy() {
		if(Player != null) {
			Player.aliveChanged -= PlayerAliveChange;
		}
	}
}
