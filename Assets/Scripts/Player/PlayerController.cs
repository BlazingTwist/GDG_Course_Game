using System;
using UnityEngine;

namespace Player {

	[RequireComponent(typeof(RaycastController))]
	public class PlayerController : MonoBehaviour {

		[SerializeField] private int maxStepIterations = 5;
		[SerializeField] private float maxSlopeAngle = 80;
		[SerializeField] private float stepHeight = 0.6f;

		private RaycastController raycastController;

		// Stored as field to avoid excess memory allocations
		private MoveResult moveResult;

		private void Awake() {
			raycastController = GetComponent<RaycastController>();
		}

		public MoveResult Move(Vector2 moveVector) {
			// steps:
			//   1. apply vertical movement
			// :[move/slope/step]
			//   2. try moving down a slope
			//   3. try moving horizontally
			//   4. try moving up a slope
			//   5. try stepping up a step
			//   6. if moveDistance not depleted, goto [move/slope/step]

			raycastController.UpdateBounds();
			Vector2 resultMove = Vector2.zero;

			moveResult.isGrounded = false;
			moveResult.isSliding = false;
			moveResult.hitCeiling = false;
			moveResult.wasSteppingUp = moveResult.isSteppingUp;
			moveResult.isSteppingUp = false;

			resultMove += ApplyVerticalMovement(moveVector.y);

			float distanceLeft = Math.Abs(moveVector.x);
			Vector2 moveDirection = moveVector.x >= 0 ? Vector2.right : Vector2.left;
			bool stuck = false;
			for (int iteration = 0; iteration < maxStepIterations && distanceLeft > 0; iteration++) {
				float previousDistanceLeft = distanceLeft;

				TrySlopeDescend(ref resultMove, moveDirection, ref distanceLeft);
				TryMoveStraight(ref resultMove, moveDirection, ref distanceLeft);
				TrySlopeAscend(ref resultMove, moveDirection, ref distanceLeft);

				if (Math.Abs(previousDistanceLeft - distanceLeft) < 1e-5f) {
					// cannot move any further (e.g. walking into a wall)
					stuck = true;
					break;
				}
			}

			if (!stuck && distanceLeft > 0) {
				Debug.LogWarning("ran out of step iterations, distance left: " + distanceLeft);
			}

			moveResult.isGrounded = moveResult.isSteppingUp || CheckIsGrounded(resultMove);
			transform.Translate(resultMove);
			return moveResult;
		}

		private bool CheckIsGrounded(Vector2 positionOffset) {
			return raycastController.CastBox(positionOffset, Vector2.down, 0.05f);
		}

		private Vector2 ApplyVerticalMovement(float moveDistance) {
			float absMoveDistance = Math.Abs(moveDistance);
			RaycastHit2D groundedHit = raycastController.CastBox(Vector2.zero, Vector2.down, absMoveDistance + 0.1f);
			if (groundedHit) {
				moveResult.isGrounded = true;
				float slopeAngle = Vector2.Angle(groundedHit.normal, Vector2.up);
				moveResult.isSliding = slopeAngle > maxSlopeAngle;
				moveResult.slideSlopeNormal = groundedHit.normal;
			}

			if (moveDistance > 0) {
				return raycastController.GetMaxMove(Vector2.up, moveDistance, out moveResult.hitCeiling);
			}

			if (!moveResult.isGrounded) {
				return raycastController.GetMaxMove(groundedHit, Vector2.down, absMoveDistance);
			}

			if (moveResult.isSliding) {
				Vector2 slopeDownDirection = Vector2.Perpendicular(groundedHit.normal) * (groundedHit.normal.x >= 0 ? -1 : 1);
				Vector2 maxMove = raycastController.GetMaxMove(slopeDownDirection, absMoveDistance, out _);
				return maxMove;
			}

			return raycastController.GetMaxMove(groundedHit, Vector2.down, absMoveDistance);
		}

		private void TrySlopeDescend(ref Vector2 positionOffset, Vector2 moveDirection, ref float distanceLeft) {
			if (distanceLeft <= 0 || moveResult.isSliding) {
				return;
			}

			RaycastHit2D slopeHit = moveDirection.x >= 0
					? raycastController.CastRayBottomLeft(positionOffset, Vector2.down, 0.05f)
					: raycastController.CastRayBottomRight(positionOffset, Vector2.down, 0.05f);

			if (!slopeHit) {
				return;
			}

			Vector2 slopeNormal = slopeHit.normal;
			// ignore slope if it leads uphill
			if (slopeNormal.x >= 0 ^ moveDirection.x >= 0) {
				return;
			}

			// TODO technically, we should check for slope-sliding here as well

			float slopeAngle = Vector2.Angle(slopeNormal, Vector2.up);
			if (slopeAngle > 0 && slopeAngle <= maxSlopeAngle) {
				Vector2 slopeDownDirection = Vector2.Perpendicular(slopeNormal) * (moveDirection.x >= 0 ? -1 : 1);
				Vector2 maxMove = raycastController.GetMaxMove(positionOffset, slopeDownDirection, distanceLeft, out _);
				distanceLeft -= maxMove.magnitude;
				positionOffset += maxMove;
			}
		}

		private void TryMoveStraight(ref Vector2 positionOffset, Vector2 moveDirection, ref float distanceLeft) {
			if (distanceLeft <= 0) {
				return;
			}

			Vector2 maxMove = raycastController.GetMaxMove(positionOffset, moveDirection, distanceLeft, out _);
			distanceLeft -= maxMove.magnitude;
			positionOffset += maxMove;
		}

		private void TrySlopeAscend(ref Vector2 positionOffset, Vector2 moveDirection, ref float distanceLeft) {
			if (distanceLeft <= 0 || moveResult.isSliding) {
				return;
			}

			RaycastHit2D slopeHit = moveDirection.x >= 0
					? raycastController.CastRayBottomRight(positionOffset, Vector2.right, distanceLeft)
					: raycastController.CastRayBottomLeft(positionOffset, Vector2.left, distanceLeft);

			if (!slopeHit) {
				return;
			}

			Vector2 slopeNormal = slopeHit.normal;
			float slopeAngle = Vector2.Angle(slopeNormal, Vector2.up);
			if (slopeAngle > 0 && slopeAngle <= maxSlopeAngle) {
				// can ascend normally
				Vector2 slopeUpDirection = Vector2.Perpendicular(slopeNormal) * (moveDirection.x >= 0 ? -1 : 1);
				Vector2 maxMove = raycastController.GetMaxMove(positionOffset, slopeUpDirection, distanceLeft, out _);
				distanceLeft -= maxMove.magnitude;
				positionOffset += maxMove;
			} else if (moveResult.wasSteppingUp || CheckIsGrounded(positionOffset)) {
				TryStepAscend(ref positionOffset, moveDirection, ref distanceLeft);
			}
		}

		private void TryStepAscend(ref Vector2 positionOffset, Vector2 moveDirection, ref float distanceLeft) {
			Vector2 stepCastOffset = new(
					(moveDirection.x >= 0 ? 1 : -1) * (RaycastController.skinWidth * 3),
					stepHeight
			);
			RaycastHit2D stepHit = moveDirection.x >= 0
					? raycastController.CastRayBottomRight(positionOffset + stepCastOffset, Vector2.down, stepHeight)
					: raycastController.CastRayBottomLeft(positionOffset + stepCastOffset, Vector2.down, stepHeight);

			if (!stepHit) {
				return;
			}

			float stepHitHeight = stepHeight - stepHit.distance;
			if (stepHitHeight <= 0) {
				return;
			}

			float stepAngle = Vector2.Angle(stepHit.normal, Vector2.up);
			if (stepAngle > maxSlopeAngle) {
				return;
			}

			float requiredHeadHeight = raycastController.GetControllerHeight() - (stepHeight - stepHitHeight);
			RaycastHit2D stepUpHit = moveDirection.x >= 0
					? raycastController.CastRayBottomRight(positionOffset + stepCastOffset, Vector2.up, requiredHeadHeight)
					: raycastController.CastRayBottomLeft(positionOffset + stepCastOffset, Vector2.up, requiredHeadHeight);

			if (stepUpHit) {
				// not enough space for player to fit
				return;
			}

			float ascendHeight = Math.Min(stepHitHeight, distanceLeft) + 0.01f;
			RaycastHit2D stepAscendHit = raycastController.CastBox(positionOffset, Vector2.up, ascendHeight);
			if (stepAscendHit) {
				// cannot ascend step because ceiling is blocking us
				return;
			}

			distanceLeft -= ascendHeight;
			positionOffset.y += ascendHeight;
			moveResult.isSteppingUp = true;
		}

		public struct MoveResult {

			public bool hitCeiling;
			public bool isGrounded;
			public bool isSliding;
			public Vector2 slideSlopeNormal;

			public bool isSteppingUp;
			public bool wasSteppingUp;

		}

	}

}
