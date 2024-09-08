using Godot;
using System;

public partial class Settings : Node
{
    public static Settings settings { get; private set; }

    public ServerSettings sv = new ServerSettings();
    public PlayerInfo pi = new PlayerInfo();
    public ClientSettings def = ClientSettings.load_from_config_file();


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
        private static readonly string FILE_PATH = "user://client_settings.ini";
        // INPUT
        public float sensitivity;

        // CAMERA
        public bool camera_roll_enabled;
        public bool camera_pitch_enabled;
        public float max_camera_pitch;
        public float max_camera_roll;

        public ClientSettings()
        {
            sensitivity = 1.0f;
            camera_roll_enabled = true;
            camera_pitch_enabled = true;
            max_camera_roll = Mathf.DegToRad(3.0f);
            max_camera_pitch = Mathf.DegToRad(90.0f);
        }

        public static ClientSettings load_from_config_file()
        {
            var def = new ClientSettings();
            var config = new ConfigFile();
            Error err = config.Load(FILE_PATH);

            // read values
            if (err != Error.Ok)
            {
                GD.Print("Failed to load client settings");
                // overwrite corrupt config file
                def.save_config_file();
                return def;
            }

            def.sensitivity = (float)config.GetValue("input", "sensitivity", def.sensitivity);
            def.camera_roll_enabled = (bool)config.GetValue("camera", "camera_roll_enabled", def.camera_roll_enabled);
            def.camera_pitch_enabled = (bool)config.GetValue("camera", "camera_pitch_enabled", def.camera_pitch_enabled);
            def.max_camera_roll = (float)config.GetValue("camera", "max_camera_roll", def.max_camera_roll);
            def.max_camera_pitch = (float)config.GetValue("camera", "max_camera_pitch", def.max_camera_pitch);


            return def;
        }

        public void save_config_file()
        {
            // this is possibly a bit slow? but it's not running every frame or even often
            ConfigFile config = new ConfigFile();
            config.SetValue("input", "sensitivity", sensitivity);
            config.SetValue("camera", "camera_roll_enabled", camera_roll_enabled);
            config.SetValue("camera", "camera_pitch_enabled", camera_pitch_enabled);
            config.SetValue("camera", "max_camera_roll", max_camera_roll);
            config.SetValue("camera", "max_camera_pitch", max_camera_pitch);
            config.Save(FILE_PATH);

        }
    }

}


