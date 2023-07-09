using System;
using UnityEngine;

namespace Physics {

	[RequireComponent(typeof(PhysicsMoveController))]
	public class MovableBoxController : MonoBehaviour {
		/*
		 * Objectives for this Class:
		 * Player can run into the Box to push it.
		 *   This class should receive the players intended move and return a modified move vector
		 * The Player can grab the Box to pull it.
		 *   Same as above, modify the players move vector.
		 * The Player may not push the box while standing on it.
		 * The Player may be pushed by the box, if it is moving by gravity or other forces.
		 *
		 * Something else: Boxes should be able to push boxes.
		 * So the PhysicsMoveController should check for boxes in its path.
		 */

		[SerializeField] private float velocityDampingFactor = 0.999f;
		[SerializeField] private float velocityFrictionLoss = 1f;
		[SerializeField] private float maxFallSpeed = -18;
		[SerializeField] private float gravity = -50;

		private PhysicsMoveController moveController;
		private Vector3 velocity = Vector3.zero;
		private bool wasPushed;
		private float pushXVelocity;

		private void Awake() {
			moveController = GetComponent<PhysicsMoveController>();
		}

		private void Start() {
			moveController.SetPushListener(OnPushed);
		}

		private void FixedUpdate() {
			// wait until the player stops pushing for momentum to apply
			if (!wasPushed && pushXVelocity != 0f) {
				velocity.x = pushXVelocity / Time.fixedDeltaTime;
				pushXVelocity = 0f;
			}
			wasPushed = false;
			
			PhysicsMoveController.MoveResult moveResult = moveController.Move(velocity * Time.fixedDeltaTime);

			velocity.x *= velocityDampingFactor;
			if (moveResult.isGrounded || moveResult.isSliding) {
				float frictionLoss = velocityFrictionLoss * Time.fixedDeltaTime;
				velocity.x = Math.Abs(velocity.x) < frictionLoss
						? 0f
						: velocity.x - (Math.Sign(velocity.x) * frictionLoss);
			}

			if (moveResult.isGrounded && !moveResult.isSliding) {
				velocity.y = 0;
			} else {
				velocity.y = Math.Max(velocity.y + (gravity * Time.fixedDeltaTime), maxFallSpeed);
			}
		}

		private void OnPushed(PhysicsMoveController.MoveResult moveResult) {
			wasPushed = true;
			pushXVelocity = moveResult.moveDelta.x;
		}
	}

}
