using UnityEngine;
using System.Collections;
using Artemis;
using SPShared.ECS.Templates;


public class ECSTest : MonoBehaviour {
	EntityWorld world;


	void Start () {
		world = new EntityWorld (false, true, true);
		world.InitializeAll (true);
		Entity e = world.CreateEntityFromTemplate ("Ship");
		GameObject obj = new GameObject("Ship");
		PhysicsRelay relay = obj.AddComponent<PhysicsRelay>();
		relay.SetEntity (e);
	}
	
	void Update () {
		if (world != null) {
			world.Update();
		}
	}
}
