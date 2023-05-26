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
		private MoveResult _moveResult;

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

			resultMove += ApplyVerticalMovement(moveVector.y);

			float distanceLeft = Math.Abs(moveVector.x);
			Vector2 moveDirection = moveVector.x >= 0 ? Vector2.right : Vector2.left;
			bool overrideIsGrounded = false;
			bool stuck = false;
			for (int iteration = 0; iteration < maxStepIterations && distanceLeft > 0; iteration++) {
				float previousDistanceLeft = distanceLeft;

				TrySlopeDescend(ref resultMove, moveDirection, ref distanceLeft);
				TryMoveStraight(ref resultMove, moveDirection, ref distanceLeft);
				TrySlopeAscend(ref resultMove, moveDirection, ref distanceLeft, ref overrideIsGrounded);

				if (Math.Abs(previousDistanceLeft - distanceLeft) < 1e-5f) {
					// cannot move any further (e.g. walking into a wall)
					stuck = true;
					break;
				}
			}

			if (!stuck && distanceLeft > 0) {
				Debug.LogWarning("ran out of step iterations, distance left: " + distanceLeft);
			}

			// TODO this is JANK
			_moveResult.isGrounded = overrideIsGrounded || raycastController.CastBox(resultMove, Vector2.down, 0.05f);
			transform.Translate(resultMove);
			return _moveResult;
		}

		private Vector2 ApplyVerticalMovement(float moveDistance) {
			// TODO slide down steep slopes
			return moveDistance switch {
					> 0 => raycastController.GetMaxMove(Vector2.up, moveDistance),
					< 0 => raycastController.GetMaxMove(Vector2.down, Math.Abs(moveDistance)),
					_ => Vector2.zero,
			};
		}

		private void TrySlopeDescend(ref Vector2 positionOffset, Vector2 moveDirection, ref float distanceLeft) {
			if (distanceLeft <= 0) {
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
				Vector2 maxMove = raycastController.GetMaxMove(positionOffset, slopeDownDirection, distanceLeft);
				distanceLeft -= maxMove.magnitude;
				positionOffset += maxMove;
			}
		}

		private void TryMoveStraight(ref Vector2 positionOffset, Vector2 moveDirection, ref float distanceLeft) {
			if (distanceLeft <= 0) {
				return;
			}

			Vector2 maxMove = raycastController.GetMaxMove(positionOffset, moveDirection, distanceLeft);
			distanceLeft -= maxMove.magnitude;
			positionOffset += maxMove;
		}

		private void TrySlopeAscend(ref Vector2 positionOffset, Vector2 moveDirection, ref float distanceLeft, ref bool overrideIsGrounded) {
			if (distanceLeft <= 0) {
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
				Vector2 maxMove = raycastController.GetMaxMove(positionOffset, slopeUpDirection, distanceLeft);
				distanceLeft -= maxMove.magnitude;
				positionOffset += maxMove;
			} else {
				TryStepAscend(ref positionOffset, moveDirection, ref distanceLeft, ref overrideIsGrounded);
			}
		}

		private void TryStepAscend(ref Vector2 positionOffset, Vector2 moveDirection, ref float distanceLeft, ref bool overrideIsGrounded) {
			Vector2 stepCastOffset = new(
					(moveDirection.x >= 0 ? 1 : -1) * (RaycastController.skinWidth * 2),
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
			overrideIsGrounded = true;
		}

		public struct MoveResult {

			public bool isGrounded;

		}

	}

}
