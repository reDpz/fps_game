// vim:fileencoding=utf-8:foldmethod=marker
#define RDEBUG
#define TESTS
using Godot;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static math;
using static Settings;

// : TODO: {{{
// - [x] Get lurching working
// - [ ] redo wish_dir source engine style by calculating cross product etc
// - [ ] Get a better understanding of what the air_* values do
// - [ ] allow for the use of scrollwheel
// = [ ] Implement fast square root for some extra performance
// : }}}

// : NOTE: {{{
// - [ ] lurches do feel a little off, maybe there is camera interpolation?
// : }}}


// : FIXME: {{{
// - [x] Fix Yuki strafes
// : }}}




public partial class Player : CharacterBody3D
{
	// : Properties {{{

	// : Helper Values {{{
	// 89 degrees, this is used so settings_instance'm not running math.f to degrees every frame
	private const float X_ROTATION_LOCK_RAD = 1.55334f;
	// : }}}


	// : Settings Properties {{{
	[Export]
	public float mouse_sensitivity = 0.003f;
	[Export]
	public bool auto_hop = true;
	// : }}}

	// : Physics Properties {{{

	public float max_health = 10.0f;
	public float c_health = 10.0f;
	// : }}}


	// : Physics Properties {{{
	// setting a default, only to ensure that we dont run into null
	private Vector3 gravity = Vector3.Zero;
	private Vector3 velocity = Vector3.Zero;
	private Vector2 input_vector = Vector2.Zero;
	private Vector2 world_vector = Vector2.Zero;
	private Vector3 wish_dir = Vector3.Zero;
	private float wish_speed = 0.0f;

	// jumping
	public float jump_velocity = 4.0f;
	public float last_jump_pressed = float.PositiveInfinity;
	public float lurch_timer = 0.0f;
	public float jump_buffer_min = 0.1f;
	public float last_jumped = float.PositiveInfinity;
	public float jump_fatigue_time = 0.9f;





	// Apex/Titanfall movement
	private bool new_input_pressed = false;

	// : }}}

	// : Node Properties {{{
	private Node3D head;
	private Camera3D camera;
	// : }}}

	// : }}}
	//
#if TESTS
	private float[] test_array = new float[1_000_000];
#endif


	public override void _Ready()
	{
		// it should be kept in this layout in the project
		head = GetNode<Node3D>("./head");
		camera = GetNode<Camera3D>("./head/camera");

		// lock mouse
		Input.MouseMode = Input.MouseModeEnum.Captured;

		// test
#if TESTS
		GD.Print($"{settings.sv.walk_speed}");
#endif
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
		/* 	else if (@event is InputEventMouseButton mouse_button)
			{
				switch (mouse_button.ButtonIndex)
				{
					case MouseButton.WheelDown:
						// Input.ActionPress("jump");
						break;
					case MouseButton.WheelUp:
						// Input.ActionPress("forward");
						new_input_pressed = true;
						break;

				}
			} */
		else if (@event is InputEventKey key_event)
		{

			if (key_event.Pressed == true)
			{

				switch (key_event.Keycode)
				{
#if RDEBUG
					case Key.F11:
						DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
						break;

					// basic reset feature
					case Key.T:
						Position = new Vector3(0.0f, 20f, 0.0f);
						// Velocity = Vector3.Zero;
						break;
#endif

				}
			}
		}

		// : }}}
	}

	public override void _Process(double delta)
	{
#if RDEBUG
		if (Input.IsActionJustPressed("debug_bot_reset"))
		{
			Position = new Vector3(-1.0f, 0.0f, -18.5f);
			head.Rotation = Vector3.Zero;
			this.Rotation = Vector3.Zero;

		}
#endif

		// update all values
		gravity = GetGravity();
		float felta = (float)delta;
		// TODO: check if this is actually needed
		velocity = Velocity;

		input_vector = Vector2.Zero;

		input_vector.X += Input.IsActionPressed("strafe_right") ? 1.0f : 0.0f;
		input_vector.X -= Input.IsActionPressed("strafe_left") ? 1.0f : 0.0f;
		input_vector.Y += Input.IsActionPressed("backward") ? 1.0f : 0.0f;
		input_vector.Y -= Input.IsActionPressed("forward") || Input.IsActionJustReleased("mwheelup") ? 1.0f : 0.0f;

		world_vector = input_vector.Normalized();
		world_vector.X *= world_vector.X > 0.0 ? settings.sv.strafe_speed : settings.sv.strafe_speed;
		world_vector.Y *= get_speed();
		world_vector = world_vector.Rotated(-Rotation.Y);
		wish_dir = new Vector3(world_vector.X, 0.0f, world_vector.Y);
		wish_speed = vec3_normalize(ref wish_dir);
		bool on_ground = IsOnFloor();

		// this is a bit slow, since we're querying the same information twice;  TODO: optimize later
		new_input_pressed = Input.IsActionJustPressed("strafe_left") || Input.IsActionJustPressed("strafe_right") ||
			(Input.IsActionJustPressed("forward") || Input.IsActionJustReleased("mwheelup")) || Input.IsActionJustPressed("backward");

		// input handling
		if (is_jump_pressed() || Input.IsActionJustReleased("mwheeldown"))
		{
			last_jump_pressed = 0;
		}

		// Add the gravity.
		if (on_ground)
		{
			ground_move(felta);
			ground_process(felta);
		}
		else
		{
			air_move(felta);
		}

		common_physics(felta);

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.

		Velocity = velocity;
		MoveAndSlide();



		// calculate camera tilt, should be in _Process
		Vector3 relative_velocity = Velocity * Transform.Basis;
		if (settings.cl.camera_z_rotation_enabled)
		{
			Vector3 camera_rotation = camera.Rotation;
			// WARN: division by 0 if strafe speed is 0 (why would it be?)
			camera_rotation.Z = on_ground ? (-settings.cl.camera_max_z_rotation * relative_velocity.X / settings.sv.strafe_speed) : 0.0f;
			camera.Rotation = camera.Rotation.Lerp(camera_rotation, felta * 13f);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// INFO: this is in physics process to avoid writing too many times here
		// WARN: do not remove this
		settings.pi.position = this.Position;
		settings.pi.velocity = this.Velocity;
		settings.pi.angles = new Vector3(
				head.Rotation.X,
				this.Rotation.Y,
				camera.Rotation.Z
				);

	}

	// these functions are only called from Process and PhysicsProcess
	// : Physics {{{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ground_move(float delta)
	{
		// jump if all conditions are valid
		if (attempt_jump())
		{
			return;
		}

		// TODO: replace walk_speed with sprint whenever needed, with an inline function. Yeah you could do 2 function calls but it doesnt hurt to only do 1
		var cur_speed_in_wish_dir = velocity.Dot(wish_dir);
		var add_speed_till_cap = wish_speed - cur_speed_in_wish_dir;

		if (add_speed_till_cap > 0.0f)
		{
			var accel_speed = settings.sv.ground_accel * delta * wish_speed;
			accel_speed = Mathf.Min(accel_speed, add_speed_till_cap);
			velocity += accel_speed * wish_dir;
		}

		// friction
		float len = velocity.Length();
		float control = Mathf.Max(len, settings.sv.ground_decel);
		float drop = control * settings.sv.ground_friction * delta;
		float new_speed = Mathf.Max(len - drop, 0.0f);
		if (len > 0.0f)
		{
			new_speed /= len;
		}

		velocity *= new_speed;

	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ground_process(float delta)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void air_move(float delta)
	{
		// apply gravity
		velocity += gravity * delta;

		air_accelerate(delta);
		attempt_lurch(delta);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void air_accelerate(float delta)
	{
		// TODO: this isn't 1:1 to what valve did in the 2013 source sdk
		var cur_speed_in_wish_dir = velocity.Dot(wish_dir);

		// settings_instance feel like there is some unnecessary math going on here but it'settings still useful for controller settings_instance guess
		var capped_speed = Mathf.Min((settings.sv.air_move_speed * world_vector).Length(), settings.sv.air_cap);

		// this is essentially how much speed to add until we reach cap
		var add_speed_till_cap = capped_speed - cur_speed_in_wish_dir;

		if (add_speed_till_cap > 0.0f)
		{
			var accel_speed = settings.sv.airaccelerate * settings.sv.air_move_speed * delta;
			accel_speed = Mathf.Min(add_speed_till_cap, accel_speed);
			velocity += accel_speed * wish_dir;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void common_physics(float delta)
	{
		if (IsOnWall())
		{
			var wall_normal = GetWallNormal();
			// this fixes jittering but broken for now
			if (is_surface_too_steep(wall_normal))
			{
				MotionMode = CharacterBody3D.MotionModeEnum.Floating;
			}
			else
			{
				MotionMode = CharacterBody3D.MotionModeEnum.Grounded;
			}

			// this allows for surfing
			clip_velocity(wall_normal, 1.0f, delta);
		}
		last_jump_pressed += delta;
		last_jumped += delta;
		lurch_timer -= delta;
	}
	// : }}}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void set_fov(float desired_fov)
	{
		camera.Fov = desired_fov;
	}

	// prevents clipping into walls essentially, but causes the "surf" bug
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void clip_velocity(Vector3 normal, float overbounce, float delta)
	{
		float backoff = velocity.Dot(normal) * overbounce;

		if (backoff >= 0.0f)
		{
			return;
		}

		velocity -= normal * backoff;

		float adjust = velocity.Dot(normal);
		if (adjust < 0.0f)
		{
			velocity -= normal * adjust;
		}


	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool is_surface_too_steep(Vector3 normal)
	{
		float max_slope_ang_dot = Vector3.Up.Rotated(Vector3.Right, settings.sv.floor_max_angle).Dot(Vector3.Up);
		return normal.Dot(Vector3.Up) < max_slope_ang_dot;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool is_jump_pressed()
	{
		return (Input.IsActionJustPressed("jump") || Input.IsActionJustReleased("jump")) || (auto_hop && Input.IsActionPressed("jump"));
	}

	private void jump()
	{
		float jump_propulsion = jump_velocity;
#if RDEBUG
		GD.Print($"{last_jumped} <= {jump_fatigue_time} : {last_jumped <= jump_fatigue_time}");
#endif
		if (last_jumped <= jump_fatigue_time)
		{
			jump_propulsion *= 0.6f;
		}
		velocity.Y += jump_propulsion;
		last_jumped = 0;
		lurch_timer = settings.sv.keyboard_graceperiod_max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool attempt_jump()
	{
		if (last_jump_pressed < jump_buffer_min)
		{
			jump();
			return true;
		}
		return false;
	}

	// Fzzy'settings algorithm from momentum mod
	// TODO: remove this label if used in too many sections
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	// returns whether a successful lurch was made or not
	public bool attempt_lurch(float delta)
	{
		// this is essentially copied from momentum mod

		// Using lenght squared instead of length to avoid sqrt
		if (new_input_pressed && (lurch_timer > 0.0f && wish_dir.LengthSquared() > 0.01f))
		{
			// Original code normalized it simply because they did not normalize it before
			// vec3_normalize(ref wish_dir);

			// this is how much lurch the player can receieve based on how long it has been since they jumped
			float fall_off = Mathf.Min(lurch_timer / (settings.sv.keyboard_graceperiod_max - settings.sv.keyboard_graceperiod_min), 1.0f);
			// the 1.1f here was an unnammed literal in fzzy's code set to 0.7f, I'm compensating here for my lower sprint speed
			float max = settings.sv.sprint_speed * 1.1f * fall_off;

			Vector3 current_direction = velocity;
			current_direction.Y = 0.0f;
			float before_speed = vec3_normalize(ref current_direction);

			// from current_direction to new direction?
			Vector3 lurch_direction = current_direction.Lerp(wish_dir * 1.5f, settings.sv.lurch_strength) - current_direction;
			vec3_normalize(ref lurch_direction);

			// not very sure of what on earth this line is calculating
			Vector3 lurch_vector = current_direction * before_speed + lurch_direction * max;

			// original algorithm essentially did an xz_length calculation
			if (xz_length_vec3(lurch_vector) > before_speed)
			{
				lurch_vector.Y = 0.0f;
				vec3_normalize(ref lurch_vector);
				lurch_vector *= before_speed;
			}
#if RDEBUG
			GD.Print($"lurch wish_dir: {wish_dir.Rotated(-Vector3.Up, Rotation.Y)}"); // relative wish_dir
#endif


			velocity.X = lurch_vector.X;
			velocity.Z = lurch_vector.Z;

			return true;
		}
		return false;
	}

	private float get_speed()
	{
		if (Input.IsActionPressed("sprint"))
		{
			return settings.sv.sprint_speed;
		}
		else
		{
			return settings.sv.walk_speed;
		}
	}


	// helper functions

}
