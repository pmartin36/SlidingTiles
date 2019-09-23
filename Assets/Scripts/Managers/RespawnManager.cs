using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnManager
{
	public Player Player;
	public Star[] Stars;

	public Vector3 PlayerSpawnPosition;

	public ActionButtons ActionButtons;

    public RespawnManager(Scene scene, bool highlight = true) {
		Player = GameObject.FindObjectsOfType<Player>().First(g => g.gameObject.scene == scene);
		PlayerSpawnPosition = Player.transform.position;
		Player.SetRespawnManager(this);
		Player.SetAlive(false);

		Stars = GameObject.FindObjectsOfType<Star>().Where(g => g.gameObject.scene == scene).ToArray();
		ActionButtons = GameObject.FindObjectsOfType<ActionButtons>().First(g => g.gameObject.scene == scene);
		ActionButtons.HighlightSpawn(highlight);

		Player.aliveChanged += PlayerAliveChange;
    }

	public void RespawnPlayer() {
		Player.SetAlive(true);
		ActionButtons.HighlightSpawn(false);
	}

	public void PlayerAliveChange(object player, bool alive) {
		if(!alive) {
			foreach (Star s in Stars) {
				s.gameObject.SetActive(true);
			}

			ActionButtons.HighlightSpawn(true);
		}
	}

	public void Destroy() {
		Player.gameObject.Destroy();
		Player.aliveChanged -= PlayerAliveChange;
		foreach (Star s in Stars) {
			s.gameObject.Destroy();
		}	
	}
}
