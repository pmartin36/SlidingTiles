using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "CopyMusicObject", menuName = "ScriptableObjects/CopyMusicObject", order = 1)]
public class CopyMusicObject : ScriptableObject {
	public AudioClip Track;
}

