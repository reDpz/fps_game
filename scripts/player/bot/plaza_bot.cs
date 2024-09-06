#define RDEBUG
using Godot;
using System;
using static Settings;


public partial class plaza_bot : CharacterBody3D
{
	public float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public Vector3 gravity = -Vector3.Up;
	public float direction_change_timer = 0.5f;
	public float x_movement = 1.0f;
	public Random rand = new Random();

	public override void _Ready()
	{
		gravity = GetGravity();
		Speed = settings.sv.walk_speed;
	}

	public override void _PhysicsProcess(double delta)
	{
#if RDEBUG
		if (Input.IsActionJustPressed("debug_bot_reset"))
		{
			Position = new Vector3(-1.0f, 0.0f, -24.0f);
		}
#endif



		// start by facing the player
		Vector3 p2b = Position - settings.pi.position;
		// use Y as a buffer
		p2b.Y = -p2b.X;
		// swap
		p2b.X = p2b.Z;
		p2b.Z = p2b.Y;
		// this p2b vector is now perpendicular to the player

		// zero out Y because it was just a buffer
		p2b.Y = 0.0f;

		// normalize
		math.vec3_normalize(ref p2b);

		Vector3 velocity = Velocity;
		float felta = (float)delta;

		velocity = p2b * x_movement * Speed;

		Velocity = velocity;
		MoveAndSlide();

		// consider the timer
		direction_change_timer -= felta;
		if (direction_change_timer <= 0 || IsOnWall())
		{
			// change direction
			x_movement *= -1.0f;
			direction_change_timer = 0.2f + (float)(rand.NextDouble());
		}
	}
}
