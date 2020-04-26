using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "CopyImageObject", menuName = "ScriptableObjects/CopyImageObject", order = 1)]
public class CopyImageObject : CopyObject {
	[Header("Image Properties")]
	public Sprite Sprite;
	public Color Color = Color.white;
}
