using Godot;
using System;
public partial class player_camera : Node3D
{
	private const float MIN_ZOOM = 2.0f;
	private const float MAX_ZOOM = 10.0f;
	private const float CONTROLLER_SENSITIVITY = 3000.0f;
	private const float DEFAULT_ZOOM = 10.0f;
	private const float BOOST_ZOOM = 6.0f;
	private const float CAMERA_CORRECTION_SPEED = 1.5f;
	private const float DEFAULT_CAMERA_X_ROTATION = -Mathf.Pi/6.0f;
	private CharacterBody3D player;
	private Node3D h;
	private Node3D v;
	public Camera3D camera;
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

	private float goal_zoom = 4.0f;
	private float current_zoom  = 4.0f;
	private float zoom_speed = 10.0f;

	public Vector3 goal_rotation = Vector3.Zero;
	public bool follow_toggle = true;

	public override void _EnterTree(){
		SetMultiplayerAuthority(Int32.Parse(GetParent().Name));
	}
	public override void _Ready()
	{
		player = GetParent<CharacterBody3D>();
		h = GetNode<Node3D>("h");
		v = GetNode<Node3D>("h/v");
		camera = GetNode<Camera3D>("h/v/Camera");
		current_zoom = goal_zoom;
	}

	public override void _Input(InputEvent inputEvent){
		if(!IsMultiplayerAuthority()){
			return;
		}
		player parent = GetParent<player>();
		if(parent.active && !settings.Instance.controller_toggle){
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		//MOUSE CAMERA CONTROLS
		if(inputEvent is InputEventMouseMotion){
			if(!settings.Instance.controller_toggle){
				InputEventMouseMotion p = (InputEventMouseMotion) inputEvent;
				crh += -p.Relative.X * hs;
				crv += -p.Relative.Y * vs;
			}
		}
		//CONTROLLER
		else if (inputEvent is InputEventJoypadMotion) {
			if(settings.Instance.controller_toggle){
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
		}
		//ZOOM CONTROLS
		// else if (inputEvent is InputEventMouseButton){
			// InputEventMouseButton emb = (InputEventMouseButton) inputEvent;
			// if (emb.IsPressed()){
				// if (emb.ButtonIndex == MouseButton.WheelUp){
					// zoom_direction = 1.0f;
				// }
				// else if (emb.ButtonIndex == MouseButton.WheelDown){
					// zoom_direction = -1.0f;
				// }
			// }
		// }
	}

	public override void _Process(double delta)
	{
		if(!IsMultiplayerAuthority()){
			return;
		}

		camera.GetNode<MeshInstance3D>("post_processing").SetInstanceShaderParameter("camera_position", GlobalPosition);

		if(goal_zoom != camera.Position.Z){
			int zoom_goal_direction = MathF.Sign(goal_zoom - camera.Position.Z);
			current_zoom += zoom_goal_direction * (float) delta * zoom_speed;
			if(zoom_goal_direction * current_zoom > zoom_goal_direction * goal_zoom){
				current_zoom = goal_zoom;
			}
			camera.Position = new Vector3(camera.Position.X, camera.Position.Y, current_zoom);
		}

		if(settings.Instance.controller_toggle){
			//CONTROLLER CONTROLS
			crh += controller_right_x * hs * CONTROLLER_SENSITIVITY * (float) delta;
			crv -= controller_right_y * vs * CONTROLLER_SENSITIVITY * (float) delta;
		}

		//CAMERA ROTATION
		float fdelta = (float) delta;
		crv = Mathf.Clamp(crv, crv_min, crv_max);

		if(Input.IsActionJustPressed("camera_reset")){
			follow_toggle = true;
		}
		else if ((new Vector2(controller_right_x, controller_right_y).Length() > 0.1f)){
			follow_toggle = false;
		}

		if(follow_toggle){
			Vector3 replace_rotation_h = h.Rotation;
			replace_rotation_h.Y = Mathf.LerpAngle(replace_rotation_h.Y, goal_rotation.Y, CAMERA_CORRECTION_SPEED * (float) delta);
			h.Rotation = replace_rotation_h;
			crh = Mathf.RadToDeg(h.Rotation.Y);

			Vector3 replace_rotation_v = v.Rotation;
			replace_rotation_v.X = Mathf.LerpAngle(replace_rotation_v.X, -goal_rotation.X + DEFAULT_CAMERA_X_ROTATION, CAMERA_CORRECTION_SPEED * (float) delta);
			v.Rotation = replace_rotation_v;
			crv = Mathf.RadToDeg(v.Rotation.X);
		}
		else{
			Vector3 replace_rotation_h = h.Rotation;
			replace_rotation_h.Y = Mathf.DegToRad(Mathf.Lerp(Mathf.RadToDeg(h.Rotation.Y), crh, fdelta * ha));
			h.Rotation = replace_rotation_h;

			Vector3 replace_rotation_v = v.Rotation;
			replace_rotation_v.X = Mathf.DegToRad(Mathf.Lerp(Mathf.RadToDeg(v.Rotation.X), crv, fdelta * va));
			v.Rotation = replace_rotation_v;
		}

		//ZOOM
		// zoom_direction = Mathf.Clamp(zoom_direction - Mathf.Sign(zoom_direction) * fdelta * 2.0f, (float) Mathf.Min(0, Mathf.Sign(zoom_direction)), (float) Mathf.Max(0, Mathf.Sign(zoom_direction)));
		// float camera_z = Mathf.Clamp(camera.Position.Z + zoom_direction * camera_zoom_speed * fdelta, MIN_ZOOM, MAX_ZOOM);
		// camera.Position = new Vector3(camera.Position.X, camera.Position.Y, camera_z);
	}

	public void toggle_zoomed_in(bool zoomed){
		goal_zoom = zoomed ? BOOST_ZOOM : DEFAULT_ZOOM;
	}
}
