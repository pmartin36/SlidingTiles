using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager
{
	public Player Player;
	public Star[] Stars;

	public ActionButtons ActionButtons;

    public RespawnManager() {
		Player = GameObject.FindObjectOfType<Player>();
		Stars = GameObject.FindObjectsOfType<Star>();
		ActionButtons = GameObject.FindObjectOfType<ActionButtons>();
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
