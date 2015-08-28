using UnityEngine;
using System.Collections;
using Artemis;
using SPShared;

public class PhysicsRelay : MonoBehaviour {

	Entity e;
	SPShared.ECS.Components.PhysicsBody body;
	Vector3 desiredPosition = Vector3.zero;

	public void SetEntity(Entity e)
	{
		this.e = e;
		body = e.GetComponent<SPShared.ECS.Components.PhysicsBody> ();
	}
	
	void Update () {
		if (body != null) {
			Debug.Log (transform.position);
			desiredPosition.x = body.Body.Position.X;
			desiredPosition.y = body.Body.Position.Y;
			desiredPosition.z = body.Body.Position.Z;
			transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime);
		}
	}
}
