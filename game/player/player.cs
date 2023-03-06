using Godot;
using System;

public partial class player : CharacterBody3D
{
	public const float TOP_SPEED = 10.0f;
	public const float ACCELERATION = 6.0f;
	public const float JUMP_VELOCITY = 4.5f;
	public const float WHEEL_SPEED = 0.8f;
	public const float MOVE_DIRECTION_ROTATION_SPEED = 1.5f;
	public const float DRAG_COEFF = 0.4f;
	public const float TRAIL_CHECK_INTERVAL = 0.1f;
	public const float TRAIL_LENGTH_INTERVAL = 0.5f;

	public PackedScene trail_scene = ResourceLoader.Load<PackedScene>("res://game/world/trail.tscn"); 

	private trail player_trail;
	public Marker3D trailtop;
	public Marker3D trailbottom;
	private MeshInstance3D mesh;
	private CollisionShape3D hurtbox;
	private float trail_timer = 0.0f;
	private Vector3 move_direction = new Vector3(0.0f, 0.0f, 1.0f);
	private float wheel_position = 0.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	private float controller_left_x = 0.0f;
	private float controller_left_y = 0.0f;

	private Vector3 last_pos = Vector3.Zero;

	public override void _Ready(){
		mesh = GetNode<MeshInstance3D>("mesh");
		hurtbox = GetNode<CollisionShape3D>("hurtbox");
		trailtop = GetNode<Marker3D>("mesh/trailtop");
		trailbottom = GetNode<Marker3D>("mesh/trailbottom");
		player_trail = (trail) trail_scene.Instantiate();
		player_trail.setup(this);
		player_trail.set_last_points(trailbottom.GlobalPosition, trailtop.GlobalPosition);
		GetParent().GetNode("Trails").AddChild(player_trail);
	}
	public override void _PhysicsProcess(double delta)
	{
		float deltaf = (float) delta;
		Vector3 velocity = Velocity;

		/*trail_timer += deltaf;
		if(trail_timer > TRAIL_INTERVAL){
			player_trail.add_section(trailbottom.GlobalPosition, trailtop.GlobalPosition);
			trail_timer = 0.0f;
			GD.Print((Position - last_pos).Length());
			last_pos = Position;
		}*/
		trail_timer += deltaf;
		if(trail_timer > TRAIL_CHECK_INTERVAL){
			if((Position - last_pos).Length() > TRAIL_LENGTH_INTERVAL){
				player_trail.add_section(trailbottom.GlobalPosition, trailtop.GlobalPosition);
				last_pos = Position;
			}
			trail_timer = 0.0f;
		}

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			//GD.Print("ASDfa");
			velocity.Y = JUMP_VELOCITY;

		//keyboard - ADD IN SETTINGS
		//int wheel_movement_direction = (Input.IsActionPressed("right") ? 1 : 0) - (Input.IsActionPressed("left") ? 1 : 0);
		//controller
		int wheel_movement_direction = -Mathf.Sign(controller_left_x);
		float wheel_movement_speed = WHEEL_SPEED;
		if(wheel_movement_direction != 0 && controller_left_x != 0){
			wheel_movement_speed *= Mathf.Abs(controller_left_x);
		}
		wheel_position = Mathf.MoveToward(wheel_position, wheel_movement_direction, wheel_movement_speed);

		if(IsOnFloor()){
			float rotation_amount = wheel_position * deltaf * (velocity.Length() / TOP_SPEED) * MOVE_DIRECTION_ROTATION_SPEED;
			if(velocity.Length() < 1){
				rotation_amount = wheel_position * deltaf * MOVE_DIRECTION_ROTATION_SPEED * 0.5f;
			}
			move_direction = move_direction.Rotated(Vector3.Up, rotation_amount);
			mesh.Rotate(Vector3.Up, rotation_amount);
			hurtbox.Rotate(Vector3.Up, rotation_amount);
			//Rotate(Vector3.Up, rotation_amount);
			if (velocity.Dot(-move_direction) <= 0 && Input.IsActionPressed("forward")){
				if(velocity.Length() >= TOP_SPEED){
					velocity = move_direction.Normalized() * TOP_SPEED;
				}
				else{
					velocity = move_direction * (velocity.Length() + ACCELERATION * deltaf);
				}
			}
			else if(velocity.Dot(-move_direction) >= 0 && Input.IsActionPressed("brake")){
				if(velocity.Length() >= TOP_SPEED){
					velocity = -move_direction.Normalized() * TOP_SPEED;
				}
				else{
					velocity = -move_direction * (velocity.Length() + ACCELERATION * deltaf);
				}
			}
			else{
				if(velocity.Length() < 1.0f){
					velocity = Vector3.Zero;
				}
				else{
					float drag_speed = Mathf.Max(0.1f, velocity.LengthSquared() * DRAG_COEFF * deltaf * (Input.IsActionPressed("brake") ? 5 : 1));
					velocity -= velocity.Normalized() * drag_speed;
					velocity = -Mathf.Sign(velocity.Dot(-move_direction)) * move_direction.Normalized() * velocity.Length();
				}
			}
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
}
