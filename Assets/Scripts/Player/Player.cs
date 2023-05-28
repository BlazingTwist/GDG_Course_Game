using System;
using UnityEngine;

namespace Player {

	[RequireComponent(typeof(PlayerController))]
	public class Player : MonoBehaviour {

		private const KeyCode jumpKey = KeyCode.Space;

		[Header("Jumping")]
		[SerializeField] private float maxJumpHeight = 4;
		[SerializeField] private float minJumpHeight = 1;
		[SerializeField] private float timeToJumpApex = 0.4f;
		[SerializeField] private float maxFallSpeed = -18f;
		[SerializeField] private float coyoteTime = 0.1f;
		[SerializeField] private float jumpBufferTime = 0.1f;

		[Header("Horizontal Movement")]
		[SerializeField] private float accelerationTimeAirborne = .2f;
		[SerializeField] private float accelerationTimeGrounded = .1f;
		[SerializeField] private float moveSpeed = 6;

		private float gravity;
		private float maxJumpVelocity;
		private float minJumpVelocity;
		private JumpInfo jumpInfo;

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
			bool jumpPressed = Input.GetKey(jumpKey); // TODO this actually shouldn't be in fixed update, but whatever.

			UpdateJumpInfo(moveResult, jumpPressed);

			Vector2 directionalInput = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
			float targetVelocityX = directionalInput.x * moveSpeed;
			velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
					moveResult.isGrounded ? accelerationTimeGrounded : accelerationTimeAirborne);
			
			if ((moveResult.isGrounded && !moveResult.isSliding) || moveResult.hitCeiling) {
				velocity.y = 0;
			} else {
				velocity.y = Math.Max(velocity.y + (gravity * Time.deltaTime), maxFallSpeed);
			}

			if (jumpInfo.ShouldJump) {
				jumpInfo.OnJump();
				if (moveResult.isSliding) {
					velocity = moveResult.slideSlopeNormal * maxJumpVelocity;
				} else {
					velocity.y = maxJumpVelocity;					
				}
			}

			if (velocity.y > minJumpVelocity && !jumpPressed) {
				velocity.y = minJumpVelocity;
			}
		}

		private void UpdateJumpInfo(PlayerController.MoveResult moveResult, bool jumpPressed) {
			if (moveResult.isGrounded) {
				jumpInfo.coyoteTimeLeft = coyoteTime;
			} else {
				jumpInfo.coyoteTimeLeft -= Time.fixedDeltaTime;
			}

			if (jumpPressed) {
				jumpInfo.jumpBufferTimeLeft = jumpBufferTime;
			} else {
				jumpInfo.jumpBufferTimeLeft -= Time.fixedDeltaTime;
			}
		}

		private struct JumpInfo {

			/// <summary>
			/// How much time is left until the player cannot jump in air anymore
			/// </summary>
			public float coyoteTimeLeft;

			/// <summary>
			/// Time left until the jump button will be treated as 'released'
			/// This tries to ensure that the player still jumps even if he released the key slightly before landing
			/// </summary>
			public float jumpBufferTimeLeft;

			public bool ShouldJump => coyoteTimeLeft > 0 && jumpBufferTimeLeft > 0;

			public void OnJump() {
				coyoteTimeLeft = 0; // ensure we can only jump once in air
				jumpBufferTimeLeft = 0; // avoid jumping twice by doing some weird slope interaction
			}

		}

	}

}
