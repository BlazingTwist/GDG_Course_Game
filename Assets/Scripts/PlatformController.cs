using System.Collections.Generic;
using Player;
using UnityEngine;

[RequireComponent(typeof(RaycastController))]
public class PlatformController : MonoBehaviour {

	[SerializeField] private Vector2 velocity;

	private RaycastController raycastController;

	private readonly Queue<MoveInfo> movesBeforePlatform = new(10);
	private readonly Queue<MoveInfo> movesAfterPlatform = new(10);
	private readonly Dictionary<Transform, TransformComponents> transformDictionary = new(10);

	private void Awake() {
		raycastController = GetComponent<RaycastController>();
	}

	private void FixedUpdate() {
		raycastController.UpdateBounds();
		Vector2 moveAmount = velocity * Time.fixedDeltaTime;
		QueuePassengerMovements(moveAmount);

		ApplyMoves(movesBeforePlatform);
		transform.Translate(moveAmount);
		ApplyMoves(movesAfterPlatform);
	}

	private void QueuePassengerMovements(Vector2 move) {
		if (move.y != 0f) {
			float upFactor = move.y > 0 ? 1 : -1;
			RaycastHit2D hit = raycastController.CastBox(Vector2.zero, Vector2.up * upFactor, Mathf.Abs(move.y));
			if (hit) {
				float pushX = move.y > 0 ? move.x : 0;
				float pushY = move.y - ((hit.distance - RaycastController.skinWidth) * upFactor);
				movesBeforePlatform.Enqueue(new MoveInfo(hit.transform, new Vector2(pushX, pushY)));
			}
		}

		if (move.x != 0f) {
			float rightFactor = move.x > 0 ? 1 : -1;
			RaycastHit2D hit = raycastController.CastBox(Vector2.zero, Vector2.right * rightFactor, Mathf.Abs(move.x));
			if (hit) {
				float pushX = move.x - ((hit.distance - RaycastController.skinWidth) * rightFactor);
				movesBeforePlatform.Enqueue(new MoveInfo(hit.transform, new Vector2(pushX, 0)));
			}
		}

		// if the platform is moving downwards or strictly horizontally
		// then check for players standing on the platform and move them
		if (move.y < 0 || (move.y == 0 && move.x != 0)) {
			RaycastHit2D hit = raycastController.CastBox(Vector2.zero, Vector2.up, RaycastController.skinWidth * 3);
			if (hit) {
				movesAfterPlatform.Enqueue(new MoveInfo(hit.transform, move));
			}
		}
	}

	private void ApplyMoves(Queue<MoveInfo> moves) {
		while (moves.Count > 0) {
			MoveInfo moveInfo = moves.Dequeue();
			EnsureTransformCached(moveInfo.movedTransform);

			TransformComponents transformComponents = transformDictionary[moveInfo.movedTransform];
			Debug.Log("moving transform, playerController ? " + (transformComponents.playerController != null) + " | move: " + moveInfo.moveVector);
			if (transformComponents.playerController != null) {
				transformComponents.playerController.Move(moveInfo.moveVector);
			} else {
				moveInfo.movedTransform.Translate(moveInfo.moveVector);
			}
		}
	}

	private void EnsureTransformCached(Transform movedTransform) {
		if (!transformDictionary.ContainsKey(movedTransform)) {
			transformDictionary[movedTransform] = new TransformComponents(movedTransform.GetComponent<PlayerController>());
		}
	}

	private readonly struct MoveInfo {

		public readonly Transform movedTransform;
		public readonly Vector2 moveVector;

		public MoveInfo(Transform movedTransform, Vector2 moveVector) {
			this.movedTransform = movedTransform;
			this.moveVector = moveVector;
		}

	}

	private readonly struct TransformComponents {

		public readonly PlayerController playerController;

		public TransformComponents(PlayerController playerController) {
			this.playerController = playerController;
		}

	}

}
