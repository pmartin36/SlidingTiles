using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticky : MonoBehaviour
{
    void Start() {
		var tilespace = transform.parent.GetComponent<Tilespace>();
		tilespace.Sticky = true;
    }
}
