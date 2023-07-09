using UnityEngine;

namespace Physics {

	[RequireComponent(typeof(BoxCollider2D))]
	public class RaycastController : MonoBehaviour {

		/// <summary>
		/// The inset into the bounding box used when casting rays
		/// </summary>
		public const float skinWidth = 0.01f;

		/// <summary>
		/// If enabled, draws traced rays for debugging purposes.
		/// </summary>
		public bool drawDebug;

		public LayerMask noCollisionLayer;
		public LayerMask collisionMask;
		public LayerMask pushableMask;

		private readonly RaycastHit2D[] hitArray = new RaycastHit2D[4];
		private new BoxCollider2D collider;
		private RaycastOrigins raycastOrigin;
		private int previousLayer;

		private void Awake() {
			collider = GetComponent<BoxCollider2D>();
			Physics2D.IgnoreCollision(collider, collider);
		}

		public void DisableCollisions() {
			GameObject go = gameObject;
			int layer = go.layer;
			if (layer == noCollisionLayer.value) {
				return;
			}
			previousLayer = layer;
			go.layer = noCollisionLayer.value;
		}

		public void RestoreCollisions() {
			gameObject.layer = previousLayer;
		}

		/// <summary>
		/// Update the bounds of this collider.
		/// This method allows you to run the setup only on those frames where you need the bounds.
		/// </summary>
		public void UpdateBounds() {
			Bounds bounds = collider.bounds;
			bounds.Expand(skinWidth * -2);

			raycastOrigin.centerPosition = bounds.center;
			raycastOrigin.boxSize = bounds.size;

			raycastOrigin.bottomLeft.x = bounds.min.x;
			raycastOrigin.bottomLeft.y = bounds.min.y;

			raycastOrigin.bottomRight.x = bounds.max.x;
			raycastOrigin.bottomRight.y = bounds.min.y;
		}

		public float GetControllerHeight() {
			return collider.size.y;
		}

		public void DrawLine(Vector2 positionOffset, Vector2 direction, Color color, float duration) {
			Vector2 startPosition = raycastOrigin.centerPosition + positionOffset;
			Debug.DrawLine(startPosition, startPosition + direction, color, duration);
		}

		/// <summary>
		/// Find the maximum move distance without intersecting with a collider
		/// </summary>
		/// <param name="targetDirection">direction to check for collisions</param>
		/// <param name="targetDistance">distance to check</param>
		/// <param name="didCollide">true if the move is limited by a collision</param>
		/// <returns>farthest possible move</returns>
		public Vector2 GetMaxMove(Vector2 targetDirection, float targetDistance, out bool didCollide) {
			return GetMaxMove(Vector2.zero, targetDirection, targetDistance, out didCollide);
		}

		/// <summary>
		/// Find the maximum move distance without intersecting with a collider
		/// </summary>
		/// <param name="positionOffset">offset to apply to the current collider position</param>
		/// <param name="targetDirection">direction to check for collisions</param>
		/// <param name="targetDistance">distance to check</param>
		/// <param name="didCollide">true if the move is limited by a collision</param>
		/// <returns>farthest possible move</returns>
		public Vector2 GetMaxMove(Vector2 positionOffset, Vector2 targetDirection, float targetDistance, out bool didCollide) {
			RaycastHit2D hit = Physics2D.BoxCast(
					raycastOrigin.centerPosition + positionOffset,
					raycastOrigin.boxSize,
					0,
					targetDirection,
					targetDistance + skinWidth,
					collisionMask
			);

			didCollide = hit;
			return GetMaxMove(hit, targetDirection, targetDistance);
		}

		/// <summary>
		/// Find the maximum move distance without intersecting with a collider
		/// </summary>
		/// <param name="hit">collider hit that inhibits this movement</param>
		/// <param name="targetDirection">direction to check for collisions</param>
		/// <param name="targetDistance">distance to check</param>
		/// <returns>farthest possible move</returns>
		public Vector2 GetMaxMove(RaycastHit2D hit, Vector2 targetDirection, float targetDistance) {
			if (drawDebug) {
				Debug.DrawRay(raycastOrigin.centerPosition, targetDirection.normalized, Color.red, 0.1f);
			}
			
			if (!hit) {
				return targetDirection * targetDistance;
			}

			if (hit.distance <= (skinWidth * 2f)) {
				return Vector2.zero;
			}

			float moveDistance = hit.distance - (skinWidth * 2f);
			return targetDirection * moveDistance;
		}

		public RaycastHit2D CastRayBottomLeft(Vector2 positionOffset, Vector2 direction, float distance) {
			return Physics2D.Raycast(
					raycastOrigin.bottomLeft + positionOffset,
					direction,
					distance + skinWidth,
					collisionMask
			);
		}

		public RaycastHit2D CastRayBottomRight(Vector2 positionOffset, Vector2 direction, float distance) {
			return Physics2D.Raycast(
					raycastOrigin.bottomRight + positionOffset,
					direction,
					distance + skinWidth,
					collisionMask
			);
		}

		public RaycastHit2D CastBox(Vector2 positionOffset, Vector2 direction, float distance) {
			return Physics2D.BoxCast(
					raycastOrigin.centerPosition + positionOffset,
					raycastOrigin.boxSize,
					0,
					direction,
					distance + skinWidth,
					collisionMask
			);
		}

		public RaycastHit2D[] CastBoxPushLayer(Vector2 positionOffset, Vector2 direction, float distance, out int hits) {
			hits = Physics2D.BoxCastNonAlloc(
					raycastOrigin.centerPosition + positionOffset,
					raycastOrigin.boxSize,
					0,
					direction,
					hitArray,
					distance + skinWidth,
					pushableMask
			);
			return hitArray;
		}

		private struct RaycastOrigins {

			public Vector2 centerPosition;
			public Vector2 boxSize;

			public Vector2 bottomLeft;
			public Vector2 bottomRight;

		}

	}

}
