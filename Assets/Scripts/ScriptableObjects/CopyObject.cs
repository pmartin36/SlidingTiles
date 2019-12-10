using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "CopyObject", menuName = "ScriptableObjects/CopyObject", order = 1)]
public class CopyObject : ScriptableObject {
	public Material Material;

	[Header("Text Properties")]
	public TMP_FontAsset Font;
	public bool Bold;
	public int FontSize;
}
