using UnityEngine;

namespace Player {

	public class FixedPlayerCamera : MonoBehaviour {

		public GameObject player;

		// Update is called once per frame
		private void Update() {
			Transform cameraTransform = transform;
			Vector3 cameraPosition = cameraTransform.position;
			Vector3 playerPosition = player.transform.position;
			cameraPosition.x = playerPosition.x;
			cameraPosition.y = playerPosition.y;
			cameraTransform.position = cameraPosition;
		}

	}

}
