using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RespawnManager
{
	public Player Player;
	public Star[] Stars;

	public Vector3 PlayerSpawnPosition;

	public ActionButtons ActionButtons;

    public RespawnManager(bool highlight = true) {
		Player = GameObject.FindObjectOfType<Player>();
		PlayerSpawnPosition = Player.transform.position;
		Player.gameObject.SetActive(false);	

		Stars = GameObject.FindObjectsOfType<Star>();
		ActionButtons = GameObject.FindObjectOfType<ActionButtons>();
		ActionButtons.HighlightSpawn(highlight);

		Player.aliveChanged += PlayerAliveChange;
    }

	public void RespawnPlayer() {	
		Player.SetAlive(true, PlayerSpawnPosition);
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
