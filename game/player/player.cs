using Godot;
using System;

public partial class player : CharacterBody3D
{
	public const float TOP_SPEED = 10.0f;
	public const float ACCELERATION = 6.0f;
	public const float BOOST_TOP_SPEED = 22.0f;
	public const float BOOST_ACCELERATION = 15.0f;
	public const float JUMP_VELOCITY = 6.1f;
	public const float WHEEL_SPEED = 0.8f;
	public const float MOVE_DIRECTION_ROTATION_SPEED = 1.5f;
	public const float DRAG_COEFF = 0.4f;
	public const float TRAIL_CHECK_INTERVAL = 0.1f;
	public const float TRAIL_LENGTH_INTERVAL = 0.5f;
	public const float FALLING_FORWARD_ROTATION_SPEED = 0.8f;
	public const float MAX_FALLING_FORWARD_ROTATION = Mathf.Pi/4.0f;

	public PackedScene trail_scene = ResourceLoader.Load<PackedScene>("res://game/world/trail.tscn"); 

	private trail player_trail;
	public Marker3D trailtop;
	public Marker3D trailbottom;
	private MeshInstance3D mesh;
	private CollisionShape3D hurtbox;
	private Node3D rotators;
	private player_camera camera;
	private RayCast3D slope_check;
	private float trail_timer = 0.0f;
	private Vector3 move_direction = new Vector3(0.0f, 0.0f, 1.0f);
	private float wheel_position = 0.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	private float controller_left_x = 0.0f;
	private float controller_left_y = 0.0f;
	private float total_forward_rotation = 0.0f;

	private Vector3 last_pos = Vector3.Zero;
	private Vector3 current_normal = new Vector3(0.0f, 1.0f, 0.0f);
	private float velocity_magnitude = 1.0f;

	public override void _Ready(){
		rotators = GetNode<Node3D>("rotators");
		mesh = GetNode<MeshInstance3D>("rotators/mesh");
		hurtbox = GetNode<CollisionShape3D>("hurtbox");
		trailtop = GetNode<Marker3D>("rotators/mesh/trailtop");
		trailbottom = GetNode<Marker3D>("rotators/mesh/trailbottom");
		camera = GetNode<player_camera>("PlayerCamera");
		slope_check = GetNode<RayCast3D>("slope_check");
		player_trail = (trail) trail_scene.Instantiate();
		player_trail.setup(this);
		player_trail.set_last_points(trailbottom.GlobalPosition, trailtop.GlobalPosition);
		GetParent().GetNode("Trails").AddChild(player_trail);
		camera.toggle_zoomed_in(false);
	}
	public override void _PhysicsProcess(double delta)
	{
		float deltaf = (float) delta;
		GetNode<MeshInstance3D>("move_direction_check").Position = move_direction * 3;
		GetNode<MeshInstance3D>("normal_check").Position = current_normal * 3;
		if(slope_check.IsColliding()){
			Vector3 next_normal = slope_check.GetCollisionNormal().Normalized();
			if(next_normal != current_normal){
				Vector3 rotate_axis = current_normal.Cross(next_normal).Normalized();
				//GD.Print(rotate_axis);
				move_direction = move_direction.Rotated(rotate_axis, current_normal.SignedAngleTo(next_normal, rotate_axis)).Normalized();
				current_normal = next_normal;
			}
		}

		Vector3 velocity = Velocity;

		trail_timer += deltaf;
		if(trail_timer > TRAIL_CHECK_INTERVAL){
			if((Position - last_pos).Length() > TRAIL_LENGTH_INTERVAL){
				player_trail.add_section(trailbottom.GlobalPosition, trailtop.GlobalPosition);
				last_pos = Position;
			}
			trail_timer = 0.0f;
		}

		// Add the gravity, rotate forwards
		if (!IsOnFloor()){
			velocity.Y -= gravity * (float)delta;
			Vector3 axis = -move_direction.Cross(current_normal);
			float added_rotation = deltaf * FALLING_FORWARD_ROTATION_SPEED;
			if(total_forward_rotation + added_rotation > MAX_FALLING_FORWARD_ROTATION){
				added_rotation = MAX_FALLING_FORWARD_ROTATION - total_forward_rotation;
			}
			rotators.Rotate(axis, added_rotation);
			total_forward_rotation += added_rotation;
		}
		else if(total_forward_rotation != 0.0){
			total_forward_rotation = 0.0f;
		}

		//CONTROLLER WHEEL CONTROL
		wheel_position = -controller_left_x;

		if(IsOnFloor()){
			float rotation_amount = wheel_position * deltaf * (velocity.Length() / TOP_SPEED) * MOVE_DIRECTION_ROTATION_SPEED;
			if(velocity.Length() < 1){
				rotation_amount = wheel_position * deltaf * MOVE_DIRECTION_ROTATION_SPEED * 0.5f;
			}

			//control where to move based on wheel position
			move_direction = move_direction.Rotated(current_normal, rotation_amount).Normalized();
			
			//rotate hurtbox and model
			Vector3 lookat_pos = GlobalPosition - move_direction * 3;
			rotators.LookAt(lookat_pos, current_normal);
			hurtbox.LookAt(lookat_pos, current_normal);

			bool boosting = Input.IsActionPressed("boost");
			if(velocity.Length() >= TOP_SPEED){
				velocity = move_direction * (boosting ? BOOST_TOP_SPEED : TOP_SPEED);
			}
			else{
			 	velocity = move_direction * (velocity_magnitude + (boosting ? BOOST_ACCELERATION : ACCELERATION) * deltaf);
			}
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor()){
			velocity += current_normal * JUMP_VELOCITY;
			Vector3 axis = -move_direction.Cross(current_normal);
			rotators.Rotate(axis, -Mathf.Pi/4.0f);
			total_forward_rotation -= Mathf.Pi/4.0f;
		}

		Velocity = velocity;
		velocity_magnitude = Velocity.Length();
		MoveAndSlide();
	}

	public override void _Input(InputEvent inputEvent){
		if (inputEvent is InputEventJoypadMotion) {
			InputEventJoypadMotion j = (InputEventJoypadMotion) inputEvent;

			if(j.Axis == JoyAxis.LeftX){
				controller_left_x = j.AxisValue;
				if(Mathf.Abs(controller_left_x) < 0.1f){
					controller_left_x = 0.0f;
				}
			}
			else if (j.Axis == JoyAxis.LeftY){
				controller_left_y = j.AxisValue;
				if(Mathf.Abs(controller_left_y) < 0.1f){
					controller_left_y = 0.0f;
				}
			}
		}
	}

	private void fix_rotation(Vector3 point_direction){
		hurtbox.LookAt(point_direction);
		mesh.LookAt(point_direction);
	}
	
	
	private void _on_area_3d_body_entered(Node3D body)
	{
		//GD.Print(body);
	}
}
