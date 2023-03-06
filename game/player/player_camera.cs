using Godot;
using System;
public partial class player_camera : Node3D
{
	[Export]
	private const float MIN_ZOOM = 2.0f;
	[Export]
	private const float MAX_ZOOM = 10.0f;
	[Export]
	private const float CONTROLLER_SENSITIVITY = 10.0f;
	private CharacterBody3D player;
	private Node3D h;
	private Node3D v;
	private Camera3D camera;
	private float crh = 0.0f;
	private float crv = 0.0f;
	private float crv_max = 20.0f;
	private float crv_min = -55.0f;
	private float hs = 0.1f;
	private float vs = 0.1f;
	private float ha = 10.0f;
	private float va = 10.0f;
	private float camera_zoom_speed = 10.0f;
	private float zoom_direction = 0.0f;

	private float controller_right_x = 0.0f;
	private float controller_right_y = 0.0f;

	public override void _Ready()
	{
		player = GetParent<CharacterBody3D>();
		h = GetNode<Node3D>("h");
		v = GetNode<Node3D>("h/v");
		camera = GetNode<Camera3D>("h/v/Camera");
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent inputEvent){
		//CAMERA CONTROLS
		//MOUSE
		if(inputEvent is InputEventMouseMotion){
			InputEventMouseMotion p = (InputEventMouseMotion) inputEvent;
			crh += -p.Relative.X * hs;
			crv += -p.Relative.Y * vs;
		}
		//CONTROLLER
		else if (inputEvent is InputEventJoypadMotion) {
			InputEventJoypadMotion j = (InputEventJoypadMotion) inputEvent;
			if(j.Axis == JoyAxis.RightX){
				controller_right_x = j.AxisValue;
				if(Mathf.Abs(controller_right_x) < 0.1f){
					controller_right_x = 0.0f;
				}
			}
			else if (j.Axis == JoyAxis.RightY){
				controller_right_y = j.AxisValue;
				if(Mathf.Abs(controller_right_y) < 0.1f){
					controller_right_y = 0.0f;
				}
			}
		}
		//ZOOM CONTROLS
		else if (inputEvent is InputEventMouseButton){
			InputEventMouseButton emb = (InputEventMouseButton) inputEvent;
			if (emb.IsPressed()){
				if (emb.ButtonIndex == MouseButton.WheelUp){
					zoom_direction = 1.0f;
				}
				else if (emb.ButtonIndex == MouseButton.WheelDown){
					zoom_direction = -1.0f;
				}
			}
		}
	}

	public override void _Process(double delta)
	{
		//CONTROLLER CONTROLS
		crh += controller_right_x * hs * CONTROLLER_SENSITIVITY;
		crv -= controller_right_y * vs * CONTROLLER_SENSITIVITY;

		//CAMERA ROTATION
		float fdelta = (float) delta;
		crv = Mathf.Clamp(crv, crv_min, crv_max);

		Vector3 replace_rotation_h = h.Rotation;
		replace_rotation_h.Y = Mathf.DegToRad(Mathf.Lerp(Mathf.RadToDeg(h.Rotation.Y), crh, fdelta * ha));
		h.Rotation = replace_rotation_h;

		Vector3 replace_rotation_v = v.Rotation;
		replace_rotation_v.X = Mathf.DegToRad(Mathf.Lerp(Mathf.RadToDeg(v.Rotation.X), crv, fdelta * va));
		v.Rotation = replace_rotation_v;

		//ZOOM
		zoom_direction = Mathf.Clamp(zoom_direction - Mathf.Sign(zoom_direction) * fdelta * 2.0f, (float) Mathf.Min(0, Mathf.Sign(zoom_direction)), (float) Mathf.Max(0, Mathf.Sign(zoom_direction)));
		float camera_z = Mathf.Clamp(camera.Position.Z + zoom_direction * camera_zoom_speed * fdelta, MIN_ZOOM, MAX_ZOOM);
		camera.Position = new Vector3(camera.Position.X, camera.Position.Y, camera_z);
	}
}
