using System.Security.Principal;
using Godot;
using System;
using System.Collections.Generic;

public partial class player : CharacterBody3D
{
	public const float SPEED = 25.0f;
	public const float ACCELERATION = 0.12f;
	public const float NEGATIVE_ACCELERATION = 0.25f;
	public const float BOOST_ADDITIONAL_SPEED = 30.0f;
	public const float BOOST_ADDITIONAL_ACCELERATION = 2.0f;
	public const float BOOST_NEGATIVE_ACCELERATION = -0.18f;
	public const float SKATE_ADDITIONAL_SPEED = 25.0f;
	public const float SKATE_ADDITIONAL_ACCELERATION = 5.0f;
	public const float SKATE_NEGATIVE_ACCELERATION = -0.04f;
	public const float TECH_ADDITIONAL_SPEED = 15.0f;
	public const float TECH_ADDITIONAL_ACCELERATION = 40.0f;
	public const float TECH_NEGATIVE_ACCELERATION = -0.03f;
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
	public const float TECH_WINDOW = 0.1f;
	public const float TECH_COOLDOWN = 0.04f;
	public const float TECH_DURATION = 0.1f;
	public const float SKATE_CORRECTION_FACTOR = 15.0f;
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
	private Node3D skate_check;
	private Node3D sparks;
	private trail skating_trail;
	private Vector3 move_direction = new Vector3(0.0f, 0.0f, 1.0f);
	private float wheel_position = 0.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public Vector3 last_pos = Vector3.Zero;
	private Vector3 current_normal = new Vector3(0.0f, 1.0f, 0.0f);
	private Vector3 last_velocity = Vector3.Zero;
	private float velocity_magnitude = 1.0f;
	private float jump_timer = 0.0f;
	private float air_timer = 0.0f;
	private float boosting = 0.02f;
	private float current_speed = SPEED;
	private float tech_counter = 0.0f;
	private float teching = 0.0f;
	private float lean_rotation_speed = 6.0f;
	private float current_lean = 0.0f;
	private float goal_lean = 0.0f;
	private float ping_counter = 0.0f;
	private float last_ping = 0.0f;	
	public Color color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
	
	//synced variables

	[Export]
	private Vector3 velocity = Vector3.Zero;
	[Export]
	private Godot.Collections.Dictionary<string, bool> input_pressed = new  Godot.Collections.Dictionary<string, bool>();
	[Export]
	private Godot.Collections.Dictionary<string, bool> input_just_pressed = new Godot.Collections.Dictionary<string, bool>();
	[Export]
	private float controller_left_x = 0.0f;
	[Export]
	private float controller_left_y = 0.0f;
	[Export]
	public bool active = false;
	[Export]
	public bool alive = true;

	public void get_input(){
		//inputs are only retrieved if authority - otherwise they are just synchronized
		if(!IsMultiplayerAuthority()){
			return;
		}
		//inputs are stored in dictionary to easily synchronize
		foreach(string action in input_pressed.Keys){
			input_pressed[action] = Input.IsActionPressed(action);
			input_just_pressed[action] = Input.IsActionJustPressed(action);
		}
	}
	public void _on_multiplayer_synchronizer_synchronized(){
		if(IsMultiplayerAuthority()){
			//check should be unnecessary
			return;
		}
		last_ping = ping_counter;
		ping_counter = 0.0f;

		//position, velocity, and inputs have been synced - now need to predict future position and velocity
		client_side_prediction();

	}

	public void client_side_prediction(){
		//simplest form of client side prediction - just move forward by last velocity and estimated velocity
		Position += move_direction * velocity * last_ping;
	}

	public override void _EnterTree(){
		//needs to be done before any syncs
		for(int i = InputMap.GetActions().Count - 1; i >= 0; i--){
			string action = InputMap.GetActions()[i];
			if( action.Substr(0,2) != "ui" ) {
				input_pressed.Add(action, false);
				input_just_pressed.Add(action, false);
			}
		}

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
		skate_check = hurtbox.GetNode<Node3D>("skate_check");
		sparks = rotators.GetNode<Node3D>("sparks");
		player_trail = GetNode<trail>("trail");
		world_node = GetParent().GetParent<world>();

		camera.camera.Current = IsMultiplayerAuthority();
	}
	public override void _PhysicsProcess(double delta)
	{
		float deltaf = (float) delta;

		if(!active || !alive){
			return;
		}

		get_input();

		world_node.authority_player_position = Position;

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
		}

		Vector3 md_axis = current_normal.Cross(move_direction).Normalized();
		float md_angle = current_normal.SignedAngleTo(move_direction, md_axis);
		move_direction = move_direction.Rotated(md_axis, Mathf.Pi/2.0f - md_angle).Normalized();
		
		velocity = Velocity;

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
			if(input_pressed["left"]){
				rotation_amount = -deltaf * MOVE_DIRECTION_ROTATION_SPEED * 0.5f;
			}
			else if(input_pressed["right"]){
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
		float goal_speed = SPEED + 
			(input_pressed["boost"] ? 1 : 0) * BOOST_ADDITIONAL_SPEED + 
			(skating_trail != null ? 1 : 0) * SKATE_ADDITIONAL_SPEED + 
			(teching > 0 ? 1 : 0) * TECH_ADDITIONAL_SPEED;
		float current_acceleration = ACCELERATION + 
			(input_pressed["boost"] ? 1 : 0) * BOOST_ADDITIONAL_ACCELERATION + 
			(skating_trail != null ? 1 : 0) * SKATE_ADDITIONAL_ACCELERATION + 
			(teching > 0 ? 1 : 0) * TECH_ADDITIONAL_ACCELERATION;
		float negative_acceleration = NEGATIVE_ACCELERATION + 
			(input_pressed["boost"] ? 1 : 0) * BOOST_NEGATIVE_ACCELERATION + 
			(skating_trail != null ? 1 : 0) * SKATE_NEGATIVE_ACCELERATION + 
			(teching > 0 ? 1 : 0) * TECH_NEGATIVE_ACCELERATION;
		
		current_acceleration = current_speed < goal_speed ? current_acceleration : negative_acceleration;

		current_speed = Mathf.Lerp(current_speed, goal_speed, current_acceleration * deltaf);

		goal_lean = 0.0f;

		//movement!
		if(IsOnFloor() && jump_timer > JUMP_BUFFER){
			//rotators.Rotate(move_direction, -rotation_amount * 8.0f);
			goal_lean = -rotation_amount * 12.0f;
			//updated after skate calculation
			velocity = move_direction * current_speed;
		}

		// Handle Jump.
		if (input_just_pressed["jump"] && IsOnFloor()){
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

		//check for tech
		tech_counter -= deltaf;
		teching -= deltaf;
		if(input_just_pressed["tech"] && tech_counter <= TECH_COOLDOWN){
			tech_counter = TECH_WINDOW;			
		}

		for(int i = 0; i < GetSlideCollisionCount(); i++){
			KinematicCollision3D collision = GetSlideCollision(i);
			bool check_trail_collision = (collision.GetColliderShapeIndex() < ((Node3D)collision.GetCollider()).GetChildCount() - 5)
			 						  || (((Node3D)collision.GetCollider()).GetChildCount() == 1);
			if(collision.GetCollider().HasMethod("can_kill") && check_trail_collision){
				//dead?
				if(tech_counter > 0 && collision.GetCollider().HasMethod("reset_trail")){
					reflect(collision.GetNormal());
				}
				else if(teching <= 0){
					die();
				}
			}
		}
		camera.goal_rotation = new Vector3(rotators.Rotation.X, rotators.Rotation.Y - Mathf.Pi, rotators.Rotation.Z);
		last_velocity = Velocity;

		//skate check - in main function now as raycast doesn't use signals
		//need normal for movement correction
		skating_trail = null;
		foreach(GpuParticles3D spark in sparks.GetChildren()){
			spark.Emitting = false;
		}

		RayCast3D left_skate_ray = skate_check.GetNode<RayCast3D>("skate_check_left");
		if(left_skate_ray.IsColliding()){
			skate_on(left_skate_ray, deltaf);
		}
		RayCast3D right_skate_ray = skate_check.GetNode<RayCast3D>("skate_check_right");
		if(right_skate_ray.IsColliding()){
			skate_on(right_skate_ray, deltaf);
		}
		float angle_change = Mathf.LerpAngle(current_lean, goal_lean, deltaf * lean_rotation_speed);
		rotators.Rotate(move_direction, angle_change);
		current_lean = angle_change;

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
		GetNode<GpuParticles3D>("death_particles").LookAt(GlobalPosition + last_velocity.Normalized());
		GetNode<GpuParticles3D>("death_particles").Emitting = true;
	}

	public void reflect(Vector3 normal){
		teching = TECH_DURATION;
		tech_counter = 0.0f;
		normal.Y = 0;
		normal = normal.Normalized();
		//Vector3 mdt = (new Vector3(move_direction.X, 0, move_direction.Z)).Normalized();
		move_direction = move_direction.Bounce(normal).Normalized();
		Velocity = move_direction * current_speed;
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
		current_speed = SPEED;
		player_trail.reset_trail();
		GetNode<GpuParticles3D>("death_particles").Restart();
		GetNode<GpuParticles3D>("death_particles").Emitting = false;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]	
	public void set_color(Color input_color){
		if(IsMultiplayerAuthority()){
			Rpc("set_color", input_color);
		}
		color = input_color;
		player_trail.set_color(input_color);
		mesh.SetInstanceShaderParameter("input_color", color);
		rotators.GetNode<GpuParticles3D>("trail_particles").SetInstanceShaderParameter("input_color", color);
		//rotators.GetNode<GpuParticles3D>("trail_particles2").SetInstanceShaderParameter("input_color", color);
		rotators.GetNode<GpuParticles3D>("trail_particles3").SetInstanceShaderParameter("input_color", color);
		GetNode<GpuParticles3D>("death_particles").SetInstanceShaderParameter("input_color", color);
	}

	public void skate_on(RayCast3D ray, float deltaf){
		Node3D body = (Node3D) ray.GetCollider();
		if(body.HasMethod("reset_trail")){
			
			Vector3 c_normal = ray.GetCollisionNormal();
			Vector3 c_point = ray.GetCollisionPoint();
			
			float distance = Mathf.Clamp((ray.GlobalPosition - c_point).Length(), 1.0f, 5.0f);
			float dist_factor = 1.0f - (distance - 1.0f) / 4.0f;
			
			//select shortest angle to be perpendicular to trail
			Vector3 perp = c_normal.Cross(current_normal).Normalized();
			float goal_angle = move_direction.SignedAngleTo(perp,current_normal);
			float goal_angle_negative = move_direction.SignedAngleTo(-perp, current_normal);
			if(Mathf.Abs(goal_angle_negative) < Mathf.Abs(goal_angle)){
				goal_angle = goal_angle_negative;
			}

			if(Mathf.Abs(goal_angle) > Mathf.Pi/6.0f){
				return; //effect is only applied when already mostly aligned to the direction of the trail
			}

			skating_trail = (trail) body;
			sparks.Rotation = Vector3.Zero; 
			sparks.Rotate(Vector3.Forward, Mathf.Pi/4.0f * (ray.Name == "skate_check_left" ? 1 : -1) );

			for(int i = 0; i < 3 && i < 4.0f * dist_factor; i++){
				sparks.GetChild<GpuParticles3D>(i).Emitting = true;
			}

			float skate_influence = Mathf.Clamp(dist_factor * dist_factor, 0.2f, 0.7f);
			float rotate_amount = Mathf.Sign(goal_angle) * Mathf.Min(Mathf.Abs(goal_angle), Mathf.Abs(goal_angle * SKATE_CORRECTION_FACTOR * deltaf * skate_influence)); 

			GetNode<MeshInstance3D>("last_velocity").Position = perp * 3;
			move_direction = move_direction.Rotated(current_normal, rotate_amount).Normalized();
			//rotators.Rotate(move_direction, -Mathf.Sign(rotate_amount) * dist_factor * 0.5f);
			goal_lean += -Mathf.Sign(rotate_amount) * dist_factor * 0.5f;
		}
	}

	public float get_current_zoom(float min_zoom, float max_zoom){
		float zoom_val = max_zoom - ((current_speed - SPEED) / (BOOST_ADDITIONAL_SPEED + SKATE_ADDITIONAL_SPEED)) * (max_zoom - min_zoom);
		if(teching > 0){
			zoom_val = max_zoom - ((current_speed - SPEED) / (BOOST_ADDITIONAL_SPEED + SKATE_ADDITIONAL_SPEED + TECH_ADDITIONAL_SPEED)) * (max_zoom - min_zoom);
		}
		zoom_val = Mathf.Clamp(zoom_val, min_zoom, max_zoom);
		return zoom_val;
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
