using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
	public Player Player;
	public Star[] Stars;

	public ActionButtons ActionButtons;

    void Start() {
		Player = FindObjectOfType<Player>();
		Stars = FindObjectsOfType<Star>();
		ActionButtons = FindObjectOfType<ActionButtons>();
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
}
