// vim:fileencoding=utf-8:foldmethod=marker
using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Player : CharacterBody3D
{
	// : Properties {{{

	// : Helper Values {{{
	// 89 degrees, this is used so i'm not running math.f to degrees every frame
	private const float X_ROTATION_LOCK_RAD = 1.55334f;
	// : }}}

	public const float speed = 5.0f;
	public const float jump_velocity = 4.5f;

	// : Input Properties {{{
	public float mouse_sensitivity = 0.003f;
	// : }}}

	// : Physics Properties {{{
	// setting a default, only to ensure that we dont run into null
	public Vector3 gravity = Vector3.Zero;
	private Vector3 velocity = Vector3.Zero;
	private Vector3 input_vector = Vector3.Zero;
	private Vector3 world_vector = Vector3.Zero;
	// : }}}

	// : Node Properties {{{
	private Node3D head;
	private Camera3D camera;
	// : }}}

	// : }}}


	public override void _Ready()
	{
		// it should be kept in this layout in the project
		head = GetNode<Node3D>("./head");
		camera = GetNode<Camera3D>("./head/camera");

		// lock mouse
		Input.MouseMode = Input.MouseModeEnum.Captured;


	}

	public override void _Input(InputEvent @event)
	{
		// : Camera Motions {{{
		if (@event is InputEventMouseMotion mouse_motion)
		{
			mouse_motion.Relative *= mouse_sensitivity;
			if (mouse_motion.Relative.X != 0.0)
			{
				this.RotateY(-mouse_motion.Relative.X);
			}

			if (mouse_motion.Relative.Y != 0.0)
			{
				head.RotateX(-mouse_motion.Relative.Y);

				// lock the rotation on X
				Vector3 rotation = new Vector3(
						Math.Clamp(head.Rotation.X, -X_ROTATION_LOCK_RAD, X_ROTATION_LOCK_RAD),
						head.Rotation.Y,
						head.Rotation.Z
						);
				head.Rotation = rotation;

			}


		}

		// : }}}
	}

	public override void _Process(double delta)
	{
		// update all values
		gravity = GetGravity();
		float felta = (float)delta;

		Vector2 input_dir = Input.GetVector("strafe_left", "strafe_right", "forward", "backward");
		input_vector.X = input_dir.X;
		input_vector.Z = input_dir.Y;
		world_vector = Transform.Basis * input_vector;

		// Add the gravity.
		if (IsOnFloor())
		{
			ground_physics(felta);
		}
		else
		{
			air_physics(felta);
		}

		common_physics(felta);


		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.


		Velocity = velocity;
		MoveAndSlide();
	}

	// these functions are only called from Process and PhysicsProcess
	// : Physics {{{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ground_physics(float delta)
	{
		// attempt to jump
		if (Input.IsActionPressed("jump"))
		{
			velocity.Y = jump_velocity;
			return;
		}

		if (input_vector != Vector3.Zero)
		{
			velocity.X = world_vector.X * speed;
			velocity.Z = world_vector.Z * speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
		}


	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void air_physics(float delta)
	{
		// apply gravity
		velocity += gravity * delta;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void common_physics(float delta)
	{

	}
	// : }}}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void set_fov(float desired_fov)
	{
		camera.Fov = desired_fov;
	}
}
