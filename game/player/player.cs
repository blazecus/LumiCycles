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
	private float trail_timer = 0.0f;
	private Vector3 move_direction = new Vector3(0.0f, 0.0f, 1.0f);
	private float wheel_position = 0.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	private float controller_left_x = 0.0f;
	private float controller_left_y = 0.0f;
	private float total_forward_rotation = 0.0f;

	private Vector3 last_pos = Vector3.Zero;

	public override void _Ready(){
		rotators = GetNode<Node3D>("rotators");
		mesh = GetNode<MeshInstance3D>("rotators/mesh");
		hurtbox = GetNode<CollisionShape3D>("hurtbox");
		trailtop = GetNode<Marker3D>("rotators/mesh/trailtop");
		trailbottom = GetNode<Marker3D>("rotators/mesh/trailbottom");
		camera = GetNode<player_camera>("PlayerCamera");
		player_trail = (trail) trail_scene.Instantiate();
		player_trail.setup(this);
		player_trail.set_last_points(trailbottom.GlobalPosition, trailtop.GlobalPosition);
		GetParent().GetNode("Trails").AddChild(player_trail);
		camera.toggle_zoomed_in(false);
	}
	public override void _PhysicsProcess(double delta)
	{
		float deltaf = (float) delta;
		Vector3 velocity = Velocity;
		
		trail_timer += deltaf;
		if(trail_timer > TRAIL_CHECK_INTERVAL){
			if((Position - last_pos).Length() > TRAIL_LENGTH_INTERVAL){
				player_trail.add_section(trailbottom.GlobalPosition, trailtop.GlobalPosition);
				last_pos = Position;
			}
			trail_timer = 0.0f;
		}

		// Add the gravity.
		if (!IsOnFloor()){
			velocity.Y -= gravity * (float)delta;
			Vector3 axis = -move_direction.Cross(Vector3.Up);
			float added_rotation = deltaf * FALLING_FORWARD_ROTATION_SPEED;
			if(total_forward_rotation + added_rotation > MAX_FALLING_FORWARD_ROTATION){
				added_rotation = MAX_FALLING_FORWARD_ROTATION - total_forward_rotation;
			}
			rotators.Rotate(axis, added_rotation);
			total_forward_rotation += added_rotation;
		}
		else if(total_forward_rotation != 0.0){
			Vector3 axis = -move_direction.Cross(Vector3.Up);
			rotators.Rotate(axis, -total_forward_rotation);
			total_forward_rotation = 0.0f;
		}
		//keyboard - ADD IN SETTINGS
		//int wheel_movement_direction = (Input.IsActionPressed("right") ? 1 : 0) - (Input.IsActionPressed("left") ? 1 : 0);
		//controller
		//MOVING WHEEL
		// int wheel_movement_direction = -Mathf.Sign(controller_left_x);
		// float wheel_movement_speed = WHEEL_SPEED;
		// if(wheel_movement_direction != 0 && controller_left_x != 0){
		// 	wheel_movement_speed *= Mathf.Abs(controller_left_x);
		// }
		// wheel_position = Mathf.MoveToward(wheel_position, wheel_movement_direction, wheel_movement_speed);

		//fixed wheel
		wheel_position = -controller_left_x;

		if(IsOnFloor()){
			float rotation_amount = wheel_position * deltaf * (velocity.Length() / TOP_SPEED) * MOVE_DIRECTION_ROTATION_SPEED;
			if(velocity.Length() < 1){
				rotation_amount = wheel_position * deltaf * MOVE_DIRECTION_ROTATION_SPEED * 0.5f;
			}
			move_direction = move_direction.Rotated(Vector3.Up, rotation_amount);
			rotators.Rotate(Vector3.Up, rotation_amount);
			hurtbox.Rotate(Vector3.Up, rotation_amount);
			//Rotate(Vector3.Up, rotation_amount);

			//ABLE TO STOP
			// if (velocity.Dot(-move_direction) <= 0 && Input.IsActionPressed("forward")){
			// 	if(velocity.Length() >= TOP_SPEED){
			// 		velocity = move_direction.Normalized() * TOP_SPEED;
			// 	}
			// 	else{
			// 		velocity = move_direction * (velocity.Length() + ACCELERATION * deltaf);
			// 	}
			// }
			// else if(velocity.Dot(-move_direction) >= 0 && Input.IsActionPressed("brake")){
			// 	if(velocity.Length() >= TOP_SPEED){
			// 		velocity = -move_direction.Normalized() * TOP_SPEED;
			// 	}
			// 	else{
			// 		velocity = -move_direction * (velocity.Length() + ACCELERATION * deltaf);
			// 	}
			// }
			// else{
			// 	if(velocity.Length() < 1.0f){
			// 		velocity = Vector3.Zero;
			// 	}
			// 	else{
			// 		float drag_speed = Mathf.Max(0.1f, velocity.LengthSquared() * DRAG_COEFF * deltaf * (Input.IsActionPressed("brake") ? 5 : 1));
			// 		velocity -= velocity.Normalized() * drag_speed;
			// 		velocity = -Mathf.Sign(velocity.Dot(-move_direction)) * move_direction.Normalized() * velocity.Length();
			// 	}
			// }

			bool boosting = Input.IsActionPressed("boost");
			//UNABLE TO STOP
			if(velocity.Length() >= TOP_SPEED){
				velocity = move_direction.Normalized() * (boosting ? BOOST_TOP_SPEED : TOP_SPEED);
			}
			else{
			 	velocity = move_direction * (velocity.Length() + (boosting ? BOOST_ACCELERATION : ACCELERATION) * deltaf);
			}
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor()){
			velocity.Y = JUMP_VELOCITY;
			Vector3 axis = -move_direction.Cross(Vector3.Up);
			rotators.Rotate(axis, -Mathf.Pi/4.0f);
			total_forward_rotation -= Mathf.Pi/4.0f;
		}

		Velocity = velocity;
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
		GD.Print(body);
	}
}
