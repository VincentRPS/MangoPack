using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 10.0f;
	public const float JumpVelocity = 4.5f;
	public const float MouseSensitivity = 0.001f;
	Vector2 CameraRotation;
	Marker3D CameraPivot;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		// set the initial rotation of the camera
		CameraRotation = new Vector2(0, 0);
		CameraPivot = GetNode<Marker3D>("CameraPivot");
	}

    public override void _Input(InputEvent @event)
    {
		// set mouse mode if Input is from the mouse
        if (Input.IsActionPressed("ui_cancel"))
			// exiting mouse interface. ESC
			Input.MouseMode = Input.MouseModeEnum.Visible;
		else if (Input.IsActionPressed("enter"))
			// On first click of the program
			Input.MouseMode = Input.MouseModeEnum.Captured;

		if (@event is InputEventMouseMotion event_mouse_motion) {
			// firstly copy the current Transforms
			Transform3D transform = Transform;
			Transform3D camera_pivot = CameraPivot.Transform;

			// Multiply the relative motion of the user's mouse with the sensitivity
			Vector2 mov = event_mouse_motion.Relative * MouseSensitivity;
			// rotate the Camera based on `mov`'s value
			CameraRotation += mov;
			// Clamp the new Y camera rotation to not flip the Camera
			CameraRotation.Y = (float) Mathf.Clamp(CameraRotation.Y, -1.5, 1.2);
			// assign new Basis to both Transforms
			transform.Basis = Basis.Identity;
			camera_pivot.Basis = Basis.Identity;
			// assign our copied Transforms to their actual counterparts
			Transform = transform;
			CameraPivot.Transform = camera_pivot;

			// Rotate the Player and the Camera based on the Camera's new rotation
			RotateObjectLocal(new Vector3(0, 1, 0), -CameraRotation.X);
			CameraPivot.RotateObjectLocal(new Vector3(1, 0, 0), -CameraRotation.Y);
		}
    }

    public override void _PhysicsProcess(double delta)
	{
		// copy the Player's current velocity
		Vector3 velocity = Velocity;

		// make sure the player falls if they aren't on the floor
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// if the player jumps, and hasn't already jumped, bring their velocity
		// to the default JumpVelocity
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// get vectors for controlling the player
		Vector2 input_dir = Input.GetVector("left", "right", "forward", "backward");
		// get the normalized direction which the player is now going
		// based on the Player's transform basis
		Vector3 direction = (Transform.Basis * new Vector3(input_dir.X, 0, input_dir.Y)).Normalized();

		// if direction is Zero, we multiply by the Player's movement speed
		// otherwise, use Mathf.MoveToward to get a new value for X & Z instead
		if (direction != Vector3.Zero) {
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		} else {
			velocity.X = Mathf.MoveToward(direction.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(direction.Z, 0, Speed);
		}

		// set our copied Velocity value to the actual player velocity
		Velocity = velocity;

		// apply changes
		MoveAndSlide();
	}
}
