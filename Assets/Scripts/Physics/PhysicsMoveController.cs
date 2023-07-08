using System;
using UnityEngine;

namespace Physics {

	[RequireComponent(typeof(RaycastController))]
	public class PhysicsMoveController : MonoBehaviour {
		[SerializeField] private int maxStepIterations = 5;
		[SerializeField] private float maxSlopeAngle = 80;
		[SerializeField] private float stepHeight = 0.6f;

		/// <summary>
		/// Mass controls how Physics-Objects push each other.
		/// <p> If the moving Object has a high mass and the pushed Object has a low mass, the move will be shortened slightly </p>
		/// <p> If the moving Object has a low mass and the pushed Object has a high mass, the move will be shortened strongly </p>
		/// <p> You can use a negative mass to have this Object be unaffected by pushing other Objects. </p>
		/// <p> A mass of 0 will cause this Object to be unable to push other Objects. </p>
		/// </summary>
		[SerializeField] private float mass = 10;

		private RaycastController raycastController;

		// Stored as field to avoid excess memory allocations
		private MoveResult moveResult;

		private void Awake() {
			raycastController = GetComponent<RaycastController>();
		}

		public MoveResult Move(Vector2 moveVector) {
			return Move(moveVector, false);
		}

		/// <summary>
		/// This Method works very similarly to the 'Move' Method, except:
		/// <ul><li>
		///   The Move Method works by assuming "I want to move by n-units", so if it encounters a slope while moving horizontally,
		///   it will move n-units along that slope, as a result, the final x-delta will be less than the x-delta of the moveVector.
		/// </li><li>
		///   The Push Method instead assumes "I am forced to move n-units in x and y", if it encounters a slope,
		///   it will move up along that slope until the requested delta is reached. The resulting total move distance thus can be larger than the moveVector.
		/// </li></ul>
		/// </summary>
		public MoveResult Push(Vector2 moveVector, float pushingMass) {
			if (pushingMass == 0) {
				return moveResult; // return previous result, should still be valid.
			}

			return pushingMass < 0
					? Move(moveVector, true) // negative mass, push without slowing down
					: Move(moveVector * ((mass + pushingMass) / pushingMass), true);
		}

		private MoveResult Move(Vector2 moveVector, bool isForced) {
			// steps:
			//   1. apply vertical movement
			// :[move/slope/step]
			//   2. try moving down a slope
			//   3. try moving horizontally
			//   4. try moving up a slope
			//   5. try stepping up a step
			//   6. if moveDistance not depleted, goto [move/slope/step]

			raycastController.UpdateBounds();
			TargetMove resultMove = new() {
					isForced = isForced,
					appliedMove = Vector2.zero,
					remainingDistance = Math.Abs(moveVector.x),
					requestedMove = moveVector,
			};

			moveResult.isGrounded = false;
			moveResult.isSliding = false;
			moveResult.hitCeiling = false;
			moveResult.wasSteppingUp = moveResult.isSteppingUp;
			moveResult.isSteppingUp = false;

			// ugly hack to unstuck the player
			for (int i = 0; i < 3; i++) {
				RaycastHit2D stuckHit = raycastController.CastBox(resultMove.appliedMove, Vector2.down, 0);
				if (stuckHit) { // looks like we're stuck in... something
					resultMove.appliedMove += (stuckHit.normal * RaycastController.skinWidth);
				} else {
					break;
				}
			}

			resultMove.UpdateVerticalMove(ApplyVerticalMovement(ref resultMove, moveVector.y, isForced, moveVector.x));

			Vector2 moveDirection = moveVector.x >= 0 ? Vector2.right : Vector2.left;
			bool stuck = false;
			for (int iteration = 0; iteration < maxStepIterations && resultMove.remainingDistance > 0; iteration++) {
				float previousDistanceLeft = resultMove.remainingDistance;

				TrySlopeDescend(ref resultMove, moveDirection);
				TryMoveStraight(ref resultMove, moveDirection);
				TrySlopeAscend(ref resultMove, moveDirection);

				if (Math.Abs(previousDistanceLeft - resultMove.remainingDistance) < 1e-5f) {
					// cannot move any further (e.g. walking into a wall)
					stuck = true;
					break;
				}
			}

			if (!stuck && resultMove.remainingDistance > 0) {
				Debug.LogWarning("ran out of step iterations, distance left: " + resultMove.remainingDistance);
			}

			moveResult.isGrounded = moveResult.isSteppingUp || CheckIsGrounded(resultMove.appliedMove);
			transform.Translate(resultMove.appliedMove);

			return moveResult;
		}

		private bool CheckIsGrounded(Vector2 positionOffset) {
			return raycastController.CastBox(positionOffset, Vector2.down, 0.05f);
		}

		private Vector2 ApplyVerticalMovement(ref TargetMove resultMove, float moveDistance, bool isForced, float xDirection) {
			float absMoveDistance = Math.Abs(moveDistance);
			RaycastHit2D groundedHit = raycastController.CastBox(resultMove.appliedMove, Vector2.down, absMoveDistance + 0.1f);
			if (groundedHit) {
				moveResult.isGrounded = true;
				float slopeAngle = Vector2.Angle(groundedHit.normal, Vector2.up);

				if (isForced) {
					// if normal.x is positive -> positive x movement takes us down the slope
					bool isDownSlopeMovement = xDirection != 0 && (groundedHit.normal.x > 0 == xDirection > 0);
					// being forced up a slope means we can't slide down the slope
					moveResult.isSliding = isDownSlopeMovement && slopeAngle > maxSlopeAngle;
				} else {
					moveResult.isSliding = slopeAngle > maxSlopeAngle;
				}

				moveResult.slideSlopeNormal = groundedHit.normal;
			}

			if (moveDistance > 0) {
				return raycastController.GetMaxMove(resultMove.appliedMove, Vector2.up, moveDistance, out moveResult.hitCeiling);
			}

			if (!moveResult.isGrounded) {
				return raycastController.GetMaxMove(groundedHit, Vector2.down, absMoveDistance);
			}

			if (moveResult.isSliding) {
				Vector2 slopeDownDirection = Vector2.Perpendicular(groundedHit.normal) * (groundedHit.normal.x >= 0 ? -1 : 1);
				Vector2 maxMove = raycastController.GetMaxMove(resultMove.appliedMove, slopeDownDirection, absMoveDistance, out _);
				return maxMove;
			}

			return raycastController.GetMaxMove(groundedHit, Vector2.down, absMoveDistance);
		}

		private void TrySlopeDescend(ref TargetMove move, Vector2 moveDirection) {
			if (move.remainingDistance <= 0 || moveResult.isSliding) {
				return;
			}

			RaycastHit2D slopeHit = moveDirection.x >= 0
					? raycastController.CastRayBottomLeft(move.appliedMove, Vector2.down, 0.05f)
					: raycastController.CastRayBottomRight(move.appliedMove, Vector2.down, 0.05f);

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
				Vector2 maxMove = raycastController.GetMaxMove(move.appliedMove, slopeDownDirection, move.remainingDistance, out _);
				move.Update(maxMove);
			}
		}

		private void TryMoveStraight(ref TargetMove move, Vector2 moveDirection) {
			if (move.remainingDistance <= 0) {
				return;
			}

			Vector2 maxMove = raycastController.GetMaxMove(move.appliedMove, moveDirection, move.remainingDistance, out _);
			move.Update(maxMove);
		}

		private void TrySlopeAscend(ref TargetMove move, Vector2 moveDirection) {
			if (move.remainingDistance <= 0 || moveResult.isSliding) {
				return;
			}

			RaycastHit2D slopeHit = moveDirection.x >= 0
					? raycastController.CastRayBottomRight(move.appliedMove, Vector2.right, move.remainingDistance)
					: raycastController.CastRayBottomLeft(move.appliedMove, Vector2.left, move.remainingDistance);

			if (!slopeHit) {
				return;
			}

			Vector2 slopeNormal = slopeHit.normal;
			float slopeAngle = Vector2.Angle(slopeNormal, Vector2.up);
			if (slopeAngle > 0 && ((move.isForced && slopeAngle < 89f) || slopeAngle <= maxSlopeAngle)) {
				// can ascend normally
				Vector2 upOffset = new(0f, 0.1f); // dirty hack to ensure the player is pushed up slopes, I hate it, but it works.
				Vector2 slopeUpDirection = (Vector2.Perpendicular(slopeNormal) * (moveDirection.x >= 0 ? -1 : 1)) + upOffset;
				float moveDistance = move.isForced
						? (move.remainingDistance / slopeUpDirection.x)
						: move.remainingDistance;
				Vector2 maxMove = raycastController.GetMaxMove(move.appliedMove + upOffset, slopeUpDirection, moveDistance, out _);
				move.Update(maxMove);
			} else if (moveResult.wasSteppingUp || CheckIsGrounded(move.appliedMove)) {
				TryStepAscend(ref move, moveDirection);
			}
		}

		private void TryStepAscend(ref TargetMove move, Vector2 moveDirection) {
			Vector2 stepCastOffset = new(
					(moveDirection.x >= 0 ? 1 : -1) * (RaycastController.skinWidth * 3),
					stepHeight
			);
			RaycastHit2D stepHit = moveDirection.x >= 0
					? raycastController.CastRayBottomRight(move.appliedMove + stepCastOffset, Vector2.down, stepHeight)
					: raycastController.CastRayBottomLeft(move.appliedMove + stepCastOffset, Vector2.down, stepHeight);

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
					? raycastController.CastRayBottomRight(move.appliedMove + stepCastOffset, Vector2.up, requiredHeadHeight)
					: raycastController.CastRayBottomLeft(move.appliedMove + stepCastOffset, Vector2.up, requiredHeadHeight);

			if (stepUpHit) {
				// not enough space for player to fit
				return;
			}

			float ascendHeight = Math.Min(stepHitHeight, move.remainingDistance) + 0.01f;
			RaycastHit2D stepAscendHit = raycastController.CastBox(move.appliedMove, Vector2.up, ascendHeight);
			if (stepAscendHit) {
				// cannot ascend step because ceiling is blocking us
				return;
			}

			move.Update(new Vector2(0, ascendHeight));
			moveResult.isSteppingUp = true;
		}

		private struct TargetMove {
			public Vector2 appliedMove;
			public bool isForced;
			public Vector2 requestedMove; // relevant if forced
			public float remainingDistance;

			public void UpdateVerticalMove(Vector2 move) {
				appliedMove += move;
				if (isForced) {
					RecomputeRemaining();
				}
			}

			public void Update(Vector2 move) {
				appliedMove += move;
				if (isForced) {
					RecomputeRemaining();
				} else {
					remainingDistance -= move.magnitude;
				}
			}

			private void RecomputeRemaining() {
				remainingDistance = Math.Abs(requestedMove.x - appliedMove.x);
			}
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
