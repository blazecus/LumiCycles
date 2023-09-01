using System.Security.Principal;
using Godot;
using System;

public partial class player : CharacterBody3D
{
	public const float SPEED = 25.0f;
	public const float ACCELERATION = 0.8f;
	public const float BOOST_ADDITIONAL_SPEED = 17.0f;
	public const float BOOST_ADDITIONAL_ACCELERATION = 0.3f;
	public const float SKATE_ADDITIONAL_SPEED = 20.0f;
	public const float SKATE_ADDITIONAL_ACCELERATION = -0.3f;
	public const float JUMP_VELOCITY = 5.9f;
	public const float WHEEL_SPEED = 1.2f;
	public const float MOVE_DIRECTION_ROTATION_SPEED = 3.3f;
	public const float DRAG_COEFF = 0.4f;
	public const float TRAIL_CHECK_INTERVAL = 0.1f;
	public const float TRAIL_LENGTH_INTERVAL = 0.5f;
	public const float FALLING_FORWARD_ROTATION_SPEED = 1.2f;
	public const float MAX_FALLING_FORWARD_ROTATION = Mathf.Pi/4.0f;
	public const float JUMP_BUFFER = 0.2f;
	public const float AIR_ROTATE_BUFFER = 0.1f;
	public const float NORMAL_ROTATION_SPEED = 4.5f;
	public const float SPEED_BOOST_DURATION = 3.0f;
	public PackedScene trail_scene = ResourceLoader.Load<PackedScene>("res://game/world/trail.tscn"); 
	private world world_node;
	private trail player_trail;
	public Marker3D trailtop;
	public Marker3D trailbottom;
	public Marker3D topfront;
	public Marker3D bottomfront;
	private MeshInstance3D mesh;
	private CollisionShape3D hurtbox;
	private Node3D rotators;
	private player_camera camera;
	private Node3D slope_check;
	private trail skating_trail;
	private Vector3 move_direction = new Vector3(0.0f, 0.0f, 1.0f);
	private float wheel_position = 0.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	private float controller_left_x = 0.0f;
	private float controller_left_y = 0.0f;
	public Vector3 last_pos = Vector3.Zero;
	private Vector3 current_normal = new Vector3(0.0f, 1.0f, 0.0f);
	private float velocity_magnitude = 1.0f;
	private float jump_timer = 0.0f;
	private float air_timer = 0.0f;
	private float boosting = 0.02f;
	private float current_speed = SPEED;
	public Color color = new Color(0.0f, 0.0f, 0.0f, 1.0f);

	[Export]
	public bool active = false;
	[Export]
	public bool alive = true;

	public override void _EnterTree(){
		SetMultiplayerAuthority(Int32.Parse(Name));
	}

	public override void _Ready(){
		rotators = GetNode<Node3D>("rotators");
		mesh = GetNode<MeshInstance3D>("rotators/bike_frame");
		hurtbox = GetNode<CollisionShape3D>("hurtbox");
		trailtop = GetNode<Marker3D>("rotators/bike_frame/trailtop");
		trailbottom = GetNode<Marker3D>("rotators/bike_frame/trailbottom");
		topfront = GetNode<Marker3D>("rotators/bike_frame/topfront");
		bottomfront = GetNode<Marker3D>("rotators/bike_frame/bottomfront");
		camera = GetNode<player_camera>("PlayerCamera");
		slope_check = hurtbox.GetNode<Node3D>("slope_check");
		player_trail = GetNode<trail>("trail");
		//player_trail = (trail) trail_scene.Instantiate();
		//player_trail.setup(this);
		world_node = GetParent().GetParent<world>();
		//world_node.GetNode("Trails").AddChild(player_trail);

		camera.camera.Current = IsMultiplayerAuthority();
	}
	public override void _PhysicsProcess(double delta)
	{
		//only run process if authority
		if(!IsMultiplayerAuthority()){
			return;
		}

		if(!active || !alive){
			return;
		}

		world_node.authority_player_position = Position;

		float deltaf = (float) delta;

		//trail logic moved to trail script
		//add_trail(deltaf);

		jump_timer += deltaf;

		//DEBUGGING
		GetNode<MeshInstance3D>("move_direction_check").Position = move_direction * 3;
		GetNode<MeshInstance3D>("normal_check").Position = current_normal * 3;

		//check for new normal - only below is relevant if already on ground, else it will check around too (for weird landing angles)
		Vector3 next_normal = current_normal;
		//GD.Print(jump_timer);
		if(jump_timer > JUMP_BUFFER){
			foreach(RayCast3D raycast in slope_check.GetChildren()){
				if(raycast.IsColliding() && raycast.GetCollisionPoint().Y < Position.Y){
					next_normal = raycast.GetCollisionNormal().Normalized();
					break;
				}
			}
		}
		//if new normal is found, rotate appropriately
		if((next_normal - current_normal).Length() < 0.02f){
			current_normal = next_normal;
		}
		
		if(next_normal != current_normal){
			Vector3 rotate_axis = current_normal.Cross(next_normal).Normalized();
			float rotate_angle = current_normal.SignedAngleTo(next_normal, rotate_axis);
			float fixed_speed = deltaf * Mathf.Sign(rotate_angle) * NORMAL_ROTATION_SPEED;
			float final_speed = Mathf.Sign(rotate_angle) * Mathf.Min(Mathf.Abs(fixed_speed), Mathf.Abs(rotate_angle));
			//if(IsOnFloor() || air_timer > JUMP_BUFFER){
			//move_direction = move_direction.Rotated(rotate_axis, final_speed).Normalized();
			//current_normal = next_normal;
			current_normal = current_normal.Rotated(rotate_axis, final_speed).Normalized();

			//}
		}
		Vector3 md_axis = current_normal.Cross(move_direction).Normalized();
		float md_angle = current_normal.SignedAngleTo(move_direction, md_axis);
		move_direction = move_direction.Rotated(md_axis, Mathf.Pi/2.0f - md_angle).Normalized();
		
		Vector3 velocity = Velocity;

		// rotate forwards while falling, but only to a certain point
		if (!IsOnFloor() && air_timer > AIR_ROTATE_BUFFER){
			float added_rotation = deltaf * FALLING_FORWARD_ROTATION_SPEED;
			Vector3 axis = move_direction.Cross(current_normal).Normalized();
			float current_total_rotation = current_normal.SignedAngleTo(Vector3.Up, axis);
			if(current_total_rotation + added_rotation > MAX_FALLING_FORWARD_ROTATION){
				added_rotation = MAX_FALLING_FORWARD_ROTATION - current_total_rotation;
			}
			current_normal = current_normal.Rotated(-axis, added_rotation).Normalized();
			move_direction = move_direction.Rotated(-axis, added_rotation).Normalized();
		}
		else if(IsOnFloor() && air_timer > AIR_ROTATE_BUFFER){
			//hit ground after falling/jumping - set current forward rotation to slope of ground
		}

		//airtime
		if(IsOnFloor()){
			air_timer = 0.0f;
		}
		else{
			air_timer += deltaf;
		}

		//CONTROLLER WHEEL CONTROL
		wheel_position = -controller_left_x;

		//turning
		//consider changing this algorithm - seems to work decently but using velocity.length is weird + it ranges froms omething like .5 to 1 which is also weird
		float rotation_amount = wheel_position * deltaf * (velocity.Length() / (SPEED + SKATE_ADDITIONAL_SPEED + BOOST_ADDITIONAL_SPEED)) * MOVE_DIRECTION_ROTATION_SPEED;
		if(velocity.Length() < 1 || !IsOnFloor()){
			rotation_amount = wheel_position * deltaf * MOVE_DIRECTION_ROTATION_SPEED * 0.5f;
		}
		if(!settings.Instance.controller_toggle){
			if(Input.IsActionPressed("left")){
				rotation_amount = -deltaf * MOVE_DIRECTION_ROTATION_SPEED * 0.5f;
			}
			else if(Input.IsActionPressed("right")){
				rotation_amount = deltaf * MOVE_DIRECTION_ROTATION_SPEED * 0.5f;
			}
		}
		//control where to move based on wheel position
		move_direction = move_direction.Rotated(current_normal, rotation_amount).Normalized();
		
		//rotate hurtbox and model to look at move direction
		Vector3 lookat_pos = GlobalPosition - move_direction * 3;
		rotators.LookAt(lookat_pos, current_normal);
		hurtbox.LookAt(lookat_pos, current_normal); 
		
		//determine speed
		float goal_speed = SPEED + (Input.IsActionPressed("boost") ? 1 : 0) * BOOST_ADDITIONAL_SPEED + (skating_trail != null ? 1 : 0) * SKATE_ADDITIONAL_SPEED;
		float current_acceleration = ACCELERATION + (Input.IsActionPressed("boost") ? 1 : 0) * BOOST_ADDITIONAL_ACCELERATION + (skating_trail != null ? 1 : 0) * SKATE_ADDITIONAL_ACCELERATION;
		current_speed = Mathf.Lerp(current_speed, goal_speed, current_acceleration * deltaf);

		//movement!
		if(IsOnFloor() && jump_timer > JUMP_BUFFER){
			rotators.Rotate(move_direction, -rotation_amount * 8.0f);
			velocity = move_direction * current_speed;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor()){
			velocity += current_normal * JUMP_VELOCITY;
			Vector3 axis = -move_direction.Cross(current_normal).Normalized();
			current_normal = current_normal.Rotated(axis, -Mathf.Pi/4.0f).Normalized();
			move_direction = move_direction.Rotated(axis, -Mathf.Pi/4.0f).Normalized();
			jump_timer = 0.0f;
		}

		//gravity
		velocity.Y -= gravity * (float)delta;
		
		Velocity = velocity;
		velocity_magnitude = Velocity.Length();
		MoveAndSlide();

		for(int i = 0; i < GetSlideCollisionCount(); i++){
			KinematicCollision3D collision = GetSlideCollision(i);
			bool check_trail_collision = (collision.GetColliderShapeIndex() < ((Node3D)collision.GetCollider()).GetChildCount() - 5)
			 						  || (((Node3D)collision.GetCollider()).GetChildCount() == 1);
			if(collision.GetCollider().HasMethod("can_kill") && check_trail_collision){
				//dead?
				die();
			}
		}
		camera.goal_rotation = new Vector3(rotators.Rotation.X, rotators.Rotation.Y - Mathf.Pi, rotators.Rotation.Z);
	}

	public override void _Input(InputEvent inputEvent){
		if(!IsMultiplayerAuthority()){
			return;
		}

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
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]	
	public void die(){
		if(IsMultiplayerAuthority()){
			Rpc("die");
		}
		alive = false;
		active = false;
		rotators.Visible = false;
		hurtbox.Disabled = true;
		world_node.player_died();
		hurtbox.GetNode<GpuParticles3D>("death_particles").Emitting = true;
	}

	public void spawn_player(Vector3 position){
		reset_player();
		active = true;
		alive = true;
		Position = position;
	}

	public void reset_player(){
		Position = Vector3.Zero;
		rotators.Rotation = Vector3.Zero;
		hurtbox.Rotation = Vector3.Zero;
		move_direction = new Vector3(0,0,1);
		wheel_position = 0.0f;
		last_pos = Vector3.Zero;
		current_normal = new Vector3(0,1,0);
		Velocity = Vector3.Zero;
		velocity_magnitude = 1;
		active = false;
		alive = false;
		Visible = true;
		rotators.Visible = true;
		hurtbox.Disabled = false;
		jump_timer = 0.0f;
		air_timer = 0.0f;
		player_trail.reset_trail();
		hurtbox.GetNode<GpuParticles3D>("death_particles").Restart();
		hurtbox.GetNode<GpuParticles3D>("death_particles").Emitting = false;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]	
	public void set_color(Color input_color){
		if(IsMultiplayerAuthority()){
			Rpc("set_color", input_color);
		}
		color = input_color;
		player_trail.set_color(input_color);
		mesh.SetInstanceShaderParameter("input_color", color);
		//hurtbox.GetNode<GpuParticles3D>("GPUParticles3D").DrawPass1.SetInstanceShaderParameter("input_color", color);
	}

	public void _on_skate_check_body_entered(Node3D body){
		if(body.HasMethod("reset_trail") && skating_trail == null){
			skating_trail = (trail)body;
		}
	}

	public void _on_skate_check_body_exited(Node3D body){
		if(body.HasMethod("reset_trail") && (trail)body == skating_trail){
			skating_trail = null;
		}
	}

	public float get_current_zoom(float min_zoom, float max_zoom){
		return max_zoom - ((current_speed - SPEED) / (BOOST_ADDITIONAL_SPEED + SKATE_ADDITIONAL_SPEED)) * (max_zoom - min_zoom);
	}

	public void prepare_destruction(){
		player_trail.QueueFree();
	}

	public bool can_kill(){
		return true;
	}

	public bool speed_up(){
		boosting = 1;
		return true;
	}
}
