using System;
using UnityEngine;

namespace World {

	[RequireComponent(typeof(Collider2D))]
	[RequireComponent(typeof(Rigidbody2D))]
	public class LevelEndTrigger : MonoBehaviour {
		private void OnTriggerEnter2D(Collider2D other) {
			if (other.gameObject.CompareTag("Player")) {
				GameManager.GetInstance().GetLevelProgressManager().ShowLevelComplete();
			}
		}
	}

}
