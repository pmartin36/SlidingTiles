using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World3Seeder : MonoBehaviour
{
	public List<SpriteRenderer> SeededRenderers;

    void Start() {
		foreach(var sr in SeededRenderers) {
			sr.material.SetFloat("_Seed", Random.value + 0.1f);
		}
	}

    void Update() {
        
    }
}
