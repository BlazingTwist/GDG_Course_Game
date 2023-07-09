using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Physics {

	[RequireComponent(typeof(RaycastController))]
	public class PhysicsMoveController : MonoBehaviour {
		[SerializeField] private int maxStepIterations = 5;
		[SerializeField] private float maxSlopeAngle = 80;
		[SerializeField] private float stepHeight = 0.6f;
		[SerializeField] private float slideSpeedFactor = 0.25f;

		/// <summary>
		/// Mass controls how Physics-Objects push each other.
		/// <p> If the moving Object has a high mass and the pushed Object has a low mass, the move will be shortened slightly </p>
		/// <p> If the moving Object has a low mass and the pushed Object has a high mass, the move will be shortened strongly </p>
		/// <p> You can use a negative mass to have this Object be unaffected by pushing other Objects. </p>
		/// <p> A mass of 0 will cause this Object to be unable to push other Objects. </p>
		/// </summary>
		[SerializeField] private float mass = 10;

		/// <summary>
		/// Contains the transform IDs that were already pushed during this move operation to avoid recursive bombs...
		/// </summary>
		private static ConcurrentDictionary<int, byte> pushChain = new();

		private RaycastController raycastController;

		private readonly Queue<PushInfo> pushBeforePlatform = new(10);
		private readonly Queue<PushInfo> pushAfterPlatform = new(10);
		private readonly Dictionary<Transform, TransformComponents> transformDictionary = new(10);

		// Stored as field to avoid excess memory allocations
		private MoveResult moveResult;
		private Action<MoveResult> pushListener;

		private void Awake() {
			raycastController = GetComponent<RaycastController>();
		}

		public void PrepareMove() {
			pushChain.Clear(); // normal move operation
			pushChain[transform.GetInstanceID()] = 0;
		}

		public void EnqueueGrabbedPushable(Collider2D pushable, Vector2 expectedMove) {
			float dot = Vector2.Dot(expectedMove, pushable.transform.position - transform.position);
			// if dot product is positive, pushable is in front of us, push before move
			EnqueuePush(dot > 0 ? pushBeforePlatform : pushAfterPlatform, pushable.transform);
		}

		public void SetPushListener(Action<MoveResult> listener) {
			pushListener = listener;
		}

		public MoveResult Move(Vector2 moveVector) {
			return Move(moveVector, false, mass);
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

			MoveResult pushResult = Move(moveVector, true, pushingMass);
			pushListener?.Invoke(pushResult);
			return pushResult;
		}

		private MoveResult Move(Vector2 moveVector, bool isForced, float pushMass) {
			raycastController.UpdateBounds();
			raycastController.DisableCollisions();
			float pushedMass = QueuePushes(moveVector);
			moveVector *= (pushMass / (pushMass + pushedMass));
			ApplyPushes(pushBeforePlatform, moveVector, pushMass);

			// steps:
			//   1. apply vertical movement
			// :[move/slope/step]
			//   2. try moving down a slope
			//   3. try moving horizontally
			//   4. try moving up a slope
			//   5. try stepping up a step
			//   6. if moveDistance not depleted, goto [move/slope/step]

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
			moveResult.moveDelta = resultMove.appliedMove;
			ApplyPushes(pushAfterPlatform, resultMove.appliedMove, pushMass);

			raycastController.RestoreCollisions();
			return moveResult;
		}

		private float QueuePushes(Vector2 move) {
			float totalPushWeight = 0;

			if (move.y != 0f) {
				float upFactor = move.y > 0 ? 1 : -1;
				RaycastHit2D[] hitArray = raycastController.CastBoxPushLayer(Vector2.zero, Vector2.up * upFactor,
						Mathf.Abs(move.y), out int numHits);
				for (int i = 0; i < numHits; i++) {
					RaycastHit2D hit = hitArray[i];
					totalPushWeight += EnqueuePush(pushBeforePlatform, hit.transform);
				}
			}

			if (move.x != 0f) {
				float rightFactor = move.x > 0 ? 1 : -1;
				RaycastHit2D[] hitArray = raycastController.CastBoxPushLayer(Vector2.zero, Vector2.right * rightFactor,
						Mathf.Abs(move.x), out int numHits);
				for (int i = 0; i < numHits; i++) {
					RaycastHit2D hit = hitArray[i];
					totalPushWeight += EnqueuePush(pushBeforePlatform, hit.transform);
				}
			}

			// if the platform is moving downwards or strictly horizontally
			// then check for players standing on the platform and move them
			if (move.y < 0 || (move.y == 0 && move.x != 0)) {
				RaycastHit2D[] hitArray = raycastController.CastBoxPushLayer(Vector2.zero, Vector2.up,
						RaycastController.skinWidth * 3, out int numHits);
				for (int i = 0; i < numHits; i++) {
					RaycastHit2D hit = hitArray[i];
					totalPushWeight += EnqueuePush(pushBeforePlatform, hit.transform);
				}
			}

			return totalPushWeight;
		}

		private float EnqueuePush(Queue<PushInfo> queue, Transform pushTransform) {
			int pushedId = pushTransform.GetInstanceID();
			if (pushChain.ContainsKey(pushedId)) {
				return 0;
			}

			pushChain[pushedId] = 0;
			queue.Enqueue(new PushInfo(pushTransform));
			EnsureTransformCached(pushTransform);
			return transformDictionary[pushTransform].moveController.mass;
		}

		private void ApplyPushes(Queue<PushInfo> pushes, Vector2 move, float pushingMass) {
			while (pushes.Count > 0) {
				PushInfo push = pushes.Dequeue();
				TransformComponents transformComponents = transformDictionary[push.movedTransform];
				if (transformComponents.moveController != null) {
					transformComponents.moveController.Push(move, pushingMass);
				} else {
					push.movedTransform.Translate(move);
				}
			}
		}

		private void EnsureTransformCached(Transform movedTransform) {
			if (!transformDictionary.ContainsKey(movedTransform)) {
				transformDictionary[movedTransform] = new TransformComponents(movedTransform.GetComponent<PhysicsMoveController>());
			}
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
				Vector2 maxMove = raycastController.GetMaxMove(
						resultMove.appliedMove, slopeDownDirection, absMoveDistance * slideSpeedFactor, out _
				);
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

			public Vector2 moveDelta;

		}

		private readonly struct PushInfo {
			public readonly Transform movedTransform;

			public PushInfo(Transform movedTransform) {
				this.movedTransform = movedTransform;
			}
		}

		private readonly struct TransformComponents {
			public readonly PhysicsMoveController moveController;

			public TransformComponents(PhysicsMoveController moveController) {
				this.moveController = moveController;
			}
		}
	}

}
