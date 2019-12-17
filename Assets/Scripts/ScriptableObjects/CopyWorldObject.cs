using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "CopyObject", menuName = "ScriptableObjects/CopyWorldObject", order = 1)]
public class CopyWorldObject : ScriptableObject {
	public CopyObject Back;
	public CopyObject Background;
	public CopyObject Grid;
	public CopyObject Tile;
	public CopyObject TileText;
}


