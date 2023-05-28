using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	/// <summary>
	/// The inset into the bounding box used when casting rays
	/// </summary>
	public const float skinWidth = 0.01f;

	/// <summary>
	/// If enabled, draws traced rays for debugging purposes.
	/// </summary>
	public bool drawDebug = false;

	public LayerMask collisionMask;

	private new BoxCollider2D collider;
	private RaycastOrigins raycastOrigin;

	private void Awake() {
		collider = GetComponent<BoxCollider2D>();
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

		if (drawDebug) {
			Debug.DrawRay(raycastOrigin.centerPosition + positionOffset, targetDirection, Color.red);
		}

		didCollide = hit;
		if (!didCollide) {
			return targetDirection * targetDistance;
		}

		return targetDirection * (hit.distance - (skinWidth * 2)); // twice skin width to keep a safe distance from colliders
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

	private struct RaycastOrigins {

		public Vector2 centerPosition;
		public Vector2 boxSize;

		public Vector2 bottomLeft;
		public Vector2 bottomRight;

	}

}
