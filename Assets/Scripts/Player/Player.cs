using System;
using UnityEngine;

namespace Player {

	[RequireComponent(typeof(PlayerController))]
	public class Player : MonoBehaviour {

		private const KeyCode jumpKey = KeyCode.Space;
		
		public float maxJumpHeight = 4;
		public float minJumpHeight = 1;
		public float timeToJumpApex = 0.4f;

		private const float accelerationTimeAirborne = .2f;
		private const float accelerationTimeGrounded = .1f;
		private const float moveSpeed = 6;

		private float gravity;
		private float maxJumpVelocity;
		private float minJumpVelocity;
		private Vector3 velocity;
		private float velocityXSmoothing;

		private PlayerController playerController;

		private void Awake() {
			playerController = GetComponent<PlayerController>();
		}

		private void Start() {
			gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
			maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
			minJumpVelocity = Mathf.Sqrt(2 * Math.Abs(gravity) * minJumpHeight);
		}

		private void FixedUpdate() {
			PlayerController.MoveResult moveResult = playerController.Move(velocity * Time.deltaTime);

			// TODO head bonks, slope sliding, ...
			if (moveResult.isGrounded) {
				velocity.y = 0;
			}

			Vector2 directionalInput = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
			float targetVelocityX = directionalInput.x * moveSpeed;
			velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
					moveResult.isGrounded ? accelerationTimeGrounded : accelerationTimeAirborne);
			velocity.y += gravity * Time.deltaTime;

			if (moveResult.isGrounded && Input.GetKey(jumpKey)) {
				// TODO check slope sliding here
				velocity.y = maxJumpVelocity;
			}

			if (velocity.y > minJumpVelocity && !Input.GetKey(jumpKey)) {
				velocity.y = minJumpVelocity;
			}
		}

	}

}
