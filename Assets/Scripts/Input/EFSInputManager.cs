using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input {

	public class EFSInputManager : MonoBehaviour {
		[SerializeField] private PlayerInput playerInput;

		private IActions activeActions;

		[PublicAPI]
		public void SetActiveActions(IActions actions) {
			activeActions = actions;
			playerInput.SwitchCurrentActionMap(actions.InputMapName());
		}

		[PublicAPI]
		public void StartRebinding(InputHandler handler, int bindingIndex, Action callback) {
			playerInput.DeactivateInput();
			handler.inputAction.PerformInteractiveRebinding()
					.WithTargetBinding(bindingIndex)
					.OnMatchWaitForAnother(0.1f)
					.OnComplete(operation => {
						callback?.Invoke();
						operation.Dispose();
						playerInput.ActivateInput();
					})
					.Start();
		}

		[PublicAPI]
		public ButtonInput GetButton(EGameplay_Button button) {
			return GameplayActions.getButton(button);
		}

		[PublicAPI]
		public ButtonInput GetButton(EMenu_Button button) {
			return MenuActions.getButton(button);
		}

		[PublicAPI]
		public AxisInput GetAxis(EGameplay_Axis axis) {
			return GameplayActions.getAxis(axis);
		}

		[PublicAPI]
		public AxisInput GetAxis(EMenu_Axis axis) {
			return MenuActions.getAxis(axis);
		}

		[PublicAPI]
		public Axis2DInput GetAxis2D(EGameplay_Axis2D axis) {
			return GameplayActions.getAxis2D(axis);
		}

		[PublicAPI]
		public Axis2DInput GetAxis2D(EMenu_Axis2D axis) {
			return MenuActions.getAxis2D(axis);
		}

		[PublicAPI]
		public InputHandler FindInputHandler<T>(T key) where T : Enum {
			return key switch {
					EGameplay_Button x => GameplayActions.getButton(x),
					EGameplay_Axis x => GameplayActions.getAxis(x),
					EGameplay_Axis2D x => GameplayActions.getAxis2D(x),
					EMenu_Button x => MenuActions.getButton(x),
					EMenu_Axis x => MenuActions.getAxis(x),
					EMenu_Axis2D x => MenuActions.getAxis2D(x),
					_ => throw new ArgumentOutOfRangeException(nameof(key), key, null),
			};
		}

		[PublicAPI] public Actions<EGameplay_Button, EGameplay_Axis, EGameplay_Axis2D> GameplayActions { get; private set; }

		[PublicAPI] public Actions<EMenu_Button, EMenu_Axis, EMenu_Axis2D> MenuActions { get; private set; }

		private void Awake() {
			GameplayActions = new Actions<EGameplay_Button, EGameplay_Axis, EGameplay_Axis2D>(
					playerInput.actions.FindActionMap("Gameplay", true)
			);
			MenuActions = new Actions<EMenu_Button, EMenu_Axis, EMenu_Axis2D>(
					playerInput.actions.FindActionMap("Menu", true)
			);

			string activeActionName = playerInput.defaultActionMap;
			activeActions = activeActionName switch {
					"Gameplay" => GameplayActions,
					"Menu" => MenuActions,
					_ => activeActions,
			};
		}

		private void Update() {
			activeActions?.Update(Time.unscaledDeltaTime);
		}
	}

	public enum EGameplay_Button {
		Jump,
		Interact,
		Pause,
	}

	public enum EGameplay_Axis {
		MoveHorizontal,
	}

	public enum EGameplay_Axis2D { }

	public enum EMenu_Button {
		Select,
		Back,
	}

	public enum EMenu_Axis { }

	public enum EMenu_Axis2D {
		Navigate,
	}

	public interface IActions {
		string InputMapName();
		void Update(float dt);
	}

	public class Actions<T_Buttons, T_Axes, T_Axes2D> : IActions
			where T_Buttons : Enum
			where T_Axes : Enum
			where T_Axes2D : Enum {
		private readonly string inputMapName;
		private readonly Dictionary<T_Buttons, ButtonInput> buttonHandlers;
		private readonly Dictionary<T_Axes, AxisInput> axisHandlers;
		private readonly Dictionary<T_Axes2D, Axis2DInput> axis2DHandlers;

		[PublicAPI]
		public ButtonInput getButton(T_Buttons button) {
			return buttonHandlers[button];
		}

		[PublicAPI]
		public AxisInput getAxis(T_Axes axis) {
			return axisHandlers[axis];
		}

		[PublicAPI]
		public Axis2DInput getAxis2D(T_Axes2D axis) {
			return axis2DHandlers[axis];
		}

		public Actions(InputActionMap map) {
			inputMapName = map.name;
			buttonHandlers = new Dictionary<T_Buttons, ButtonInput>();
			axisHandlers = new Dictionary<T_Axes, AxisInput>();
			axis2DHandlers = new Dictionary<T_Axes2D, Axis2DInput>();

			foreach (T_Buttons button in Enum.GetValues(typeof(T_Buttons)).Cast<T_Buttons>()) {
				buttonHandlers.Add(button, new ButtonInput(Enum.GetName(typeof(T_Buttons), button), map));
			}

			foreach (T_Axes axis in Enum.GetValues(typeof(T_Axes)).Cast<T_Axes>()) {
				axisHandlers.Add(axis, new AxisInput(Enum.GetName(typeof(T_Axes), axis), map));
			}

			foreach (T_Axes2D axis in Enum.GetValues(typeof(T_Axes2D)).Cast<T_Axes2D>()) {
				axis2DHandlers.Add(axis, new Axis2DInput(Enum.GetName(typeof(T_Axes2D), axis), map));
			}
		}

		string IActions.InputMapName() {
			return inputMapName;
		}

		void IActions.Update(float dt) {
			foreach (ButtonInput handler in buttonHandlers.Values) {
				handler.Update(dt);
			}
			foreach (AxisInput handler in axisHandlers.Values) {
				handler.Update(dt);
			}
			foreach (Axis2DInput handler in axis2DHandlers.Values) {
				handler.Update(dt);
			}
		}
	}

	public abstract class InputHandler {
		[PublicAPI] public readonly string actionName;
		public readonly InputAction inputAction;

		protected InputHandler(string actionName, InputActionMap actionMap) {
			this.actionName = actionName;
			inputAction = actionMap[actionName];
		}

		public abstract void Update(float dt);
	}

	public class ButtonInput : InputHandler {
		private float releaseSinceSeconds = 1;

		[PublicAPI]
		public void AddTriggerCallback(Action<InputAction.CallbackContext> callback) {
			inputAction.started += callback;
		}

		[PublicAPI]
		public void RemoveTriggerCallback(Action<InputAction.CallbackContext> callback) {
			inputAction.started -= callback;
		}

		[PublicAPI]
		public bool IsTriggered() {
			return IsTriggered(Time.fixedUnscaledDeltaTime * 1.25f);
		}

		[PublicAPI]
		public bool IsTriggered(float bufferSeconds) {
			return releaseSinceSeconds <= bufferSeconds;
		}

		public ButtonInput(string actionName, InputActionMap actionMap) : base(actionName, actionMap) { }

		public override void Update(float dt) {
			if (inputAction.IsPressed()) {
				releaseSinceSeconds = 0;
			} else {
				releaseSinceSeconds += dt;
			}
		}
	}

	public class AxisInput : InputHandler {
		[PublicAPI]
		public float GetValue() {
			return inputAction.ReadValue<float>();
		}

		public AxisInput(string actionName, InputActionMap actionMap) : base(actionName, actionMap) { }

		public override void Update(float dt) { }
	}

	public class Axis2DInput : InputHandler {
		[PublicAPI]
		public Vector2 GetInput() {
			return inputAction.ReadValue<Vector2>();
		}

		[PublicAPI]
		public DpadButtons GetAsDpad() {
			Vector2 vector2 = GetInput();
			return new DpadButtons(
					vector2.y > 0,
					vector2.y < 0,
					vector2.x < 0,
					vector2.x > 0
			);
		}

		public Axis2DInput(string actionName, InputActionMap actionMap) : base(actionName, actionMap) { }

		public override void Update(float dt) { }

		[PublicAPI]
		public readonly struct DpadButtons {
			public readonly bool up;
			public readonly bool down;
			public readonly bool left;
			public readonly bool right;

			public DpadButtons(bool up, bool down, bool left, bool right) {
				this.up = up;
				this.down = down;
				this.left = left;
				this.right = right;
			}
		}
	}

}
