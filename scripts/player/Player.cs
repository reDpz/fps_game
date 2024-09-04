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
	[Export]
	public float mouse_sensitivity = 0.003f;
	// : }}}

	// : Physics Properties {{{
	// setting a default, only to ensure that we dont run into null
	public Vector3 gravity = Vector3.Zero;
	private Vector3 velocity = Vector3.Zero;
	private Vector2 input_vector = Vector2.Zero;
	private Vector2 world_vector = Vector2.Zero;
	private Vector3 wish_dir = Vector3.Zero;

	// source movement
	// : AIR Movement {{{
	public float air_cap = 0.25f;
	public float air_accel = 100.0f;
	public float air_move_speed = 55.0f;
	// : }}}

	// : GROUND Movement {{{
	public float walk_speed = 7.0f;
	public float sprint_speed = 8.5f;
	public float ground_accel = 14.0f;
	public float ground_decel = 10.0f;
	public float ground_friction = 6.0f;
	// : }}}

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
		else if (@event is InputEventKey key_event)
		{
			// basic reset feature
			if (key_event.Keycode == Key.T)
			{
				Position = new Vector3(0.0f, 20f, 0.0f);
				Velocity = Vector3.Zero;
			}
		}

		// : }}}
	}

	public override void _Process(double delta)
	{
		// update all values
		gravity = GetGravity();
		float felta = (float)delta;
		// TODO: check if this is actually needed
		velocity = Velocity;

		input_vector = Input.GetVector("strafe_left", "strafe_right", "forward", "backward");
		// Wish dir is in worldspace
		world_vector = input_vector.Rotated(-Rotation.Y);
		wish_dir = new Vector3(world_vector.X, 0.0f, world_vector.Y);


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

		// TODO: replace walk_speed with sprint whenever needed, with an inline function. Yeah you could do 2 function calls but it doesnt hurt to only do 1
		var cur_speed_in_wish_dir = velocity.Dot(wish_dir);
		var add_speed_till_cap = walk_speed - cur_speed_in_wish_dir;

		if (add_speed_till_cap > 0.0f)
		{
			var accel_speed = ground_accel * delta * walk_speed;
			accel_speed = Mathf.Min(accel_speed, add_speed_till_cap);
			velocity += accel_speed * wish_dir;
		}

		// FIXME: this is broken
		// friction
		float len = velocity.Length();
		float control = Mathf.Max(len, ground_decel);
		float drop = control * ground_friction * delta;
		float new_speed = Mathf.Min(len - drop, 0.0f);
		if (len > 0.0f)
		{
			new_speed /= len;
		}

		velocity *= new_speed;

	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void air_physics(float delta)
	{
		// apply gravity
		velocity += gravity * delta;

		// ITS THIS SHORT?
		var cur_speed_in_wish_dir = velocity.Dot(wish_dir);

		// i feel like there is some unnecessary math going on here but it's still useful for controller i guess
		var capped_speed = Mathf.Min((air_move_speed * world_vector).Length(), air_cap);

		// this is essentially how much speed to add until we reach cap
		var add_speed_till_cap = capped_speed - cur_speed_in_wish_dir;

		if (add_speed_till_cap > 0.0f)
		{
			var accel_speed = air_accel * air_move_speed * delta;
			accel_speed = Mathf.Min(add_speed_till_cap, accel_speed);
			velocity += accel_speed * wish_dir;
		}
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
