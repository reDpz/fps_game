using Godot;
using System;

public partial class plaza_bot : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public Vector3 gravity = -Vector3.Up;
	public float direction_change_timer = 0.5f;
	public float x_movement = 1.0f;
	public Random rand = new Random();

	public override void _Ready()
	{
		gravity = GetGravity();
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		float felta = (float)delta;
		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += gravity * felta;
		}

		Vector3 direction = (Transform.Basis * new Vector3(x_movement, 0.0f, 0.0f)).Normalized();
		velocity.X = direction.X * Speed;
		velocity.Z = direction.Z * Speed;

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
