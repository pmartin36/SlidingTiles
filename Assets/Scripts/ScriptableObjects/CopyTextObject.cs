using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "CopyObject", menuName = "ScriptableObjects/CopyTextObject", order = 1)]
public class CopyTextObject : CopyObject {
	[Header("Text Properties")]
	public TMP_FontAsset Font;
	public FontStyles FontStyles;
	public int FontSize;
}
