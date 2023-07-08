using UnityEngine;

namespace Physics {

	[RequireComponent(typeof(RaycastController))]
	[RequireComponent(typeof(PhysicsMoveController))]
	public class MovableBoxController : MonoBehaviour
	{
		/*
		 * Objectives for this Class:
		 * Player can run into the Box to push it.
		 *   This class should receive the players intended move and return a modified move vector
		 * The Player can grab the Box to pull it.
		 *   Same as above, modify the players move vector.
		 * The Player may not push the box while standing on it.
		 * The Player may be pushed by the box, if it is moving by gravity or other forces.
		 *
		 * Something else: Boxes should be able to push boxes.
		 * So the PhysicsMoveController should check for boxes in its path.
		 *
		 * Boxes should have inertia.
		 * So if they're sliding 
		 */
	}

}
