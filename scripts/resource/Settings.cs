using Godot;
using System;

public partial class Settings : Node
{
    public static Settings settings { get; private set; }

    public ServerSettings sv = new ServerSettings();
    public PlayerInfo pi = new PlayerInfo();
    public ClientSettings cl = new ClientSettings();


    public override void _Ready()
    {
        settings = this;
    }

    public struct ServerSettings
    {
        // source engine air
        public float airaccelerate;
        public float air_cap;
        public float air_move_speed;
        public float floor_max_angle;

        // source engine ground
        public float walk_speed;
        public float backwards_speed;
        public float strafe_speed;
        public float sprint_speed;
        public float ground_accel;
        public float ground_decel;
        public float ground_friction;

        // lurching
        public float lurch_strength;
        public float keyboard_graceperiod_min;
        public float keyboard_graceperiod_max;


        public ServerSettings()
        {
            airaccelerate = 10f;
            air_cap = 0.35f;
            air_move_speed = 2.0f;
            floor_max_angle = Mathf.DegToRad(80); // this does technically run at runtime but only when creating instances

            walk_speed = 6.0f;
            backwards_speed = 0.3f;
            strafe_speed = 6.0f;
            sprint_speed = 8.0f;
            ground_accel = 10.0f;
            ground_decel = 7.0f;
            ground_friction = 6.0f;

            lurch_strength = 0.7f; // default value
            keyboard_graceperiod_min = 0.2f;
            keyboard_graceperiod_max = 0.5f;
        }

    }

    public struct PlayerInfo
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 angles;

        public PlayerInfo()
        {
            position = Vector3.Zero;
            velocity = Vector3.Zero;
            angles = Vector3.Zero;
        }
    }

    public struct ClientSettings
    {
        public bool camera_z_rotation_enabled;
        public float camera_max_z_rotation;

        public ClientSettings()
        {
            camera_z_rotation_enabled = true;
            camera_max_z_rotation = Mathf.DegToRad(3.0f);
        }
    }

}


