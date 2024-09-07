using Godot;
using static Settings;

public partial class Showpos : Label
{
	// private ColorRect rect;

	public override void _Ready()
	{
		// this.rect = GetNode<ColorRect>("../bg");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		// first we are going to format our text
		Vector3 position = settings.pi.position;
		Vector3 angles = settings.pi.angles;
		//Vector3 velocity = settings.pi.velocity.Rotated(-Vector3.Up, angles.Y); // this is in relative space
		ref Vector3 velocity = ref settings.pi.velocity;
		float speed = math.xz_length_vec3(velocity);

		Text = $@"pos: {position.X:n2}, {position.Y:n2}, {position.Z:n2}
vel: {velocity.X:n2}, {velocity.Y:n2}, {velocity.Z:n2} ({speed:n2})
rot: {Mathf.RadToDeg(angles.X):n2}, {Mathf.RadToDeg(angles.Y):n2}, {Mathf.RadToDeg(angles.Z):n2}
";


	}
}
