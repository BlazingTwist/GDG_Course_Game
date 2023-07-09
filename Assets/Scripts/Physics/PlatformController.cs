using UnityEngine;

namespace Physics {

	[RequireComponent(typeof(PhysicsMoveController))]
	public class PlatformController : MonoBehaviour {

		[SerializeField] private Vector2 velocity;

		private PhysicsMoveController moveController;

		private void Awake() {
			moveController = GetComponent<PhysicsMoveController>();
		}

		private void FixedUpdate() {
			Vector2 moveAmount = velocity * Time.fixedDeltaTime;
			moveController.Move(moveAmount);
		}

	}

}
