using UnityEngine;
using System.Collections;
using Artemis;


public class ECSTest : MonoBehaviour {
	EntityWorld world;


	void Start () {
		world = new EntityWorld ();

	}
	
	void Update () {
		if (world != null) {
			world.Update();
		}
	}
}
