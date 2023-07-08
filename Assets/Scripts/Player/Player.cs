using System;
using Input;
using Ui;
using UnityEngine;

namespace Player {

	[RequireComponent(typeof(PlayerController))]
	public class Player : MonoBehaviour {

		[Header("Jumping")]
		[SerializeField] private float maxJumpHeight = 4;
		[SerializeField] private float minJumpHeight = 1;
		[SerializeField] private float timeToJumpApex = 0.4f;
		[SerializeField] private float maxFallSpeed = -18f;
		[SerializeField] private float coyoteTime = 0.1f;
		[SerializeField] private float jumpBufferTime = 0.1f;
		[SerializeField] private float jumpCooldownTime = 0.2f;

		[Header("Horizontal Movement")]
		[SerializeField] private float accelerationTimeAirborne = .2f;
		[SerializeField] private float accelerationTimeGrounded = .1f;
		[SerializeField] private float moveSpeed = 6;

		private EFSInputManager inputManager;
		private UiController uiController;
		private PlayerController playerController;

		private float pauseCooldownLeft; // prevents pause buffering

		private float gravity;
		private float maxJumpVelocity;
		private float minJumpVelocity;
		private JumpInfo jumpInfo;

		private Vector3 velocity;
		private float velocityXSmoothing;


		private void Awake() {
			playerController = GetComponent<PlayerController>();
		}

		private void Start() {
			GameController gameController = GameController.GetInstance();
			inputManager = gameController.GetInputManager();
			uiController = gameController.GetUiController();

			gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
			maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
			minJumpVelocity = Mathf.Sqrt(2 * Math.Abs(gravity) * minJumpHeight);
		}

		private void Update() {
			if (pauseCooldownLeft > 0) {
				pauseCooldownLeft -= Time.deltaTime;
			} else {
				if (inputManager.GetButton(EGameplay_Button.Pause).IsTriggered(0)) {
					pauseCooldownLeft = 1f;
					uiController.OpenPauseMenu();
				}
			}
		}

		private void FixedUpdate() {
			PlayerController.MoveResult moveResult = playerController.Move(velocity * Time.deltaTime);
			bool jumpPressed = inputManager.GetButton(EGameplay_Button.Jump).IsTriggered();

			UpdateJumpInfo(moveResult, jumpPressed);

			float targetVelocityX = inputManager.GetAxis(EGameplay_Axis.MoveHorizontal).GetValue() * moveSpeed;
			velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
					moveResult.isGrounded ? accelerationTimeGrounded : accelerationTimeAirborne);

			if ((moveResult.isGrounded && !moveResult.isSliding) || moveResult.hitCeiling) {
				velocity.y = 0;
			} else {
				velocity.y = Math.Max(velocity.y + (gravity * Time.deltaTime), maxFallSpeed);
			}

			if (jumpInfo.ShouldJump) {
				jumpInfo.OnJump(jumpCooldownTime);
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

			jumpInfo.jumpCooldownTimeLeft -= Time.fixedDeltaTime;
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

			public float jumpCooldownTimeLeft;

			public bool ShouldJump => coyoteTimeLeft > 0 && jumpBufferTimeLeft > 0 && jumpCooldownTimeLeft <= 0;

			public void OnJump(float cooldownTime) {
				coyoteTimeLeft = 0; // ensure we can only jump once in air
				jumpBufferTimeLeft = 0; // avoid jumping twice by doing some weird slope interaction
				jumpCooldownTimeLeft = cooldownTime;
			}

		}

	}

}
