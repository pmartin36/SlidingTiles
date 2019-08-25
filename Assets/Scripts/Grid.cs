using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
	public List<List<Tilespace>> Tilespaces;

    void Start() {
		Tilespaces = new List<List<Tilespace>>();

		var spaces = GetComponentsInChildren<Tilespace>();
		int max = spaces.Max(s => s.Position.x);
		var spacesByRow = spaces.GroupBy(t => t.Position.y).OrderBy(g => g.Key);
		foreach(var rawrow in spacesByRow) {
			Tilespaces.Add(rawrow.OrderBy(r => r.Position.x).ToList());
		}	

		foreach(Tilespace s in spaces) {
			s.Init(this);
		}

		// transform.position = new Vector3(-max * 5, -(Tilespaces.Count - 1) * 5, 0);
		Camera.main.transform.position = new Vector3(max * 5, (Tilespaces.Count - 1) * 5, -50);
	}

    void Update() {
        
    }

	public Dictionary<Direction, Tilespace> GetNeighboringSpaces(TilePosition p) {
		Dictionary<Direction, Tilespace> tiles = new Dictionary<Direction, Tilespace>();
		foreach(Direction d in Enum.GetValues(typeof(Direction))) {
			Tilespace t = GetTilespaceInDirection(p, d);
			if(t != null) {
				tiles.Add(d, t);
			}
		}
		return tiles;
	}

	public Tilespace GetTilespaceInDirection(TilePosition p, Direction d) {
		int nx = p.x + Mathf.RoundToInt(d.Value.x);
		int ny = p.y + Mathf.RoundToInt(d.Value.y);
		if(ny >= 0 && ny < Tilespaces.Count &&
			nx >= 0 && nx < Tilespaces[ny].Count) {
			return Tilespaces[ny][nx];
		}
		return null;
	}
}
