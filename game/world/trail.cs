using Godot;
using System;

/*-------------------------------------------
Synchronization plan
Instead of using multiplayersynchronizer, synchronizations will have to be done through rpcs,
since they have to be timed on when the network authority moves to the next chunk. 
chunks should be done fairly often, which means the mesh might have to be copied fairly often
-------------------------------------------*/


public partial class trail : Node3D 
{
	private const int CHUNK_SIZE = 20;
	public const float TRAIL_CHECK_INTERVAL = 0.05f;
	public const float TRAIL_LENGTH_INTERVAL = 0.25f;
	public const int TRAIL_HITBOX_LAG = 3;
	public const float TRAIL_STARTUP = 1.5f;
	public const float UV_SCALING_FACTOR = 1.0f;

	public Material trail_material = GD.Load<Material>("res://assets/materials/trail_material.tres");

	private MeshInstance3D mesh;
	private MeshInstance3D frontmesh;
	private ImmediateMesh imesh;
	private ImmediateMesh frontimesh;
	private Area3D trail_area;
	private Area3D close_trail_area;
	public Godot.Collections.Array<Vector3> points;
	public Godot.Collections.Array<Vector3> added_points;
	public int added_count = 0;
	public player parent_player;
	private int polygon_counter = 0;
	private int start_index = 0;
	private int network_authority_id = 0;
	private float trail_timer = 0.0f;
	private Vector3 last_player_pos = Vector3.Zero;
	public Color color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
	private int current_uv = 0;

	public void setup(){

	}

	public override void _Ready()
	{
		mesh = GetNode<MeshInstance3D>("mesh");
		frontmesh = GetNode<MeshInstance3D>("frontmesh");
		trail_area = GetNode<Area3D>("trail_area");
		close_trail_area = GetNode<Area3D>("close_trail_area");
		frontimesh = new ImmediateMesh();
		frontmesh.Mesh = frontimesh;
		imesh = new ImmediateMesh();
		mesh.Mesh = imesh;
		points = new Godot.Collections.Array<Vector3>();
		added_points = new Godot.Collections.Array<Vector3>();
		parent_player = GetParent<player>();
		network_authority_id = Int32.Parse(parent_player.Name);
		SetMultiplayerAuthority(network_authority_id);
		trail_timer = -TRAIL_STARTUP;
	}

	public override void _Process(double delta)
	{
		if(!parent_player.active || !parent_player.alive){
			return;
		}
		frontmesh.SetInstanceShaderParameter("input_color", color);
		mesh.SetInstanceShaderParameter("input_color", color);
		float deltaf = (float) delta;
		trail_timer += deltaf;
		if(trail_timer > TRAIL_CHECK_INTERVAL){
			if((parent_player.Position - last_player_pos).Length() > TRAIL_LENGTH_INTERVAL){
				add_section(parent_player.trailbottom.GlobalPosition, parent_player.trailtop.GlobalPosition);
				last_player_pos = parent_player.Position;
			}
			trail_timer = 0.0f;
		}
		configure_front_quad();
		GD.Print(frontimesh.GetSurfaceCount());
		GD.Print(imesh.GetSurfaceCount());
		GD.Print("-");
	}

	public void configure_front_quad(){

		Vector3 last_point_bot;
		Vector3 last_point_top;
		if(added_points.Count >= 2){
			last_point_bot = added_points[added_points.Count-2];
			last_point_top = added_points[added_points.Count-1];
		}
		else{
			if(points.Count < 2){
				return;
			}
			last_point_bot = points[points.Count-2];
			last_point_top = points[points.Count-1];
		}

		Godot.Collections.Array<Vector3> in_points = new Godot.Collections.Array<Vector3>
        {
			last_point_bot,
			last_point_top,
			parent_player.trailbottom.GlobalPosition,
			parent_player.trailtop.GlobalPosition
        };

		frontimesh.ClearSurfaces();
		frontimesh.SurfaceBegin(Mesh.PrimitiveType.Triangles, trail_material);
		float end = (current_uv / 2.0f)/UV_SCALING_FACTOR;
		frontmesh.SetInstanceShaderParameter("player_position", parent_player.GlobalPosition);
		mesh.SetInstanceShaderParameter("player_position", parent_player.GlobalPosition);
		draw_rect(ref frontimesh, ref in_points, current_uv);
		frontimesh.SurfaceEnd();

		if(close_trail_area.HasNode("last")){
			CollisionShape3D tlast = close_trail_area.GetNode<CollisionShape3D>("last");
			close_trail_area.RemoveChild(tlast);
			tlast.QueueFree();
		}

		last_hitbox();
	}

	public void draw_rect(ref ImmediateMesh mesh, ref Godot.Collections.Array<Vector3> in_points, int i){
		//triangle 1 front
		mesh.SurfaceSetUV(new Vector2((i/2) / UV_SCALING_FACTOR, 0));
		mesh.SurfaceAddVertex(in_points[0]);

		mesh.SurfaceSetUV(new Vector2((i/2) / UV_SCALING_FACTOR, 1));
		mesh.SurfaceAddVertex(in_points[1]);

		mesh.SurfaceSetUV(new Vector2((i/2 + 1) / UV_SCALING_FACTOR,0));
		mesh.SurfaceAddVertex(in_points[2]);

		//triangle 1 back
		mesh.SurfaceSetUV(new Vector2((i/2 + 1) / UV_SCALING_FACTOR, 0));
		mesh.SurfaceAddVertex(in_points[2]);
		
		mesh.SurfaceSetUV(new Vector2((i/2) / UV_SCALING_FACTOR, 1));
		mesh.SurfaceAddVertex(in_points[1]);

		mesh.SurfaceSetUV(new Vector2((i/2) / UV_SCALING_FACTOR, 0));
		mesh.SurfaceAddVertex(in_points[0]);

		//triangle 2 front
		mesh.SurfaceSetUV(new Vector2((i/2) / UV_SCALING_FACTOR, 1));
		mesh.SurfaceAddVertex(in_points[1]);

		mesh.SurfaceSetUV(new Vector2((i/2 + 1) / UV_SCALING_FACTOR, 1));
		mesh.SurfaceAddVertex(in_points[3]);

		mesh.SurfaceSetUV(new Vector2((i/2 + 1) / UV_SCALING_FACTOR, 0));
		mesh.SurfaceAddVertex(in_points[2]);

		//triangle 2 back
		mesh.SurfaceSetUV(new Vector2((i/2 + 1) / UV_SCALING_FACTOR, 0));
		mesh.SurfaceAddVertex(in_points[2]);

		mesh.SurfaceSetUV(new Vector2((i/2 + 1) / UV_SCALING_FACTOR, 1));
		mesh.SurfaceAddVertex(in_points[3]);

		mesh.SurfaceSetUV(new Vector2((i/2) / UV_SCALING_FACTOR, 1));
		mesh.SurfaceAddVertex(in_points[1]);
	}
	public void draw_mesh(int start_index){
		if(points.Count > 4){
			imesh.SurfaceBegin(Mesh.PrimitiveType.Triangles, trail_material);
			for(int i = start_index; i < points.Count - 3; i += 2){
				Godot.Collections.Array<Vector3> in_points = new Godot.Collections.Array<Vector3>
                {
                    points[i],
                    points[i + 1],
                    points[i + 2],
                    points[i + 3]
                };
				draw_rect(ref imesh, ref in_points, i);
			}
			imesh.SurfaceEnd();
		}
	}

	public void draw_last_rect(){
		if(added_points.Count >= 4){
			imesh.SurfaceBegin(Mesh.PrimitiveType.Triangles, trail_material);
			int i = added_points.Count - 4;
			int uv_i = points.Count + added_points.Count - 4;

			Godot.Collections.Array<Vector3> in_points = new Godot.Collections.Array<Vector3>
            {
                added_points[i],
                added_points[i + 1],
                added_points[i + 2],
                added_points[i + 3]
            };
			draw_rect(ref imesh, ref in_points, uv_i);
			current_uv = uv_i + 2;
			imesh.SurfaceEnd();
		}
		else{
			if(points.Count < 2){
				return;
			}
			imesh.SurfaceBegin(Mesh.PrimitiveType.Triangles, trail_material);
			int i = points.Count - 2;
			
			Godot.Collections.Array<Vector3> in_points = new Godot.Collections.Array<Vector3>
            {
                points[points.Count - 2],
                points[points.Count - 1],
                added_points[0],
                added_points[1]
            };
			draw_rect(ref imesh, ref in_points, i);
			imesh.SurfaceEnd();
			current_uv = i + 2;
		}
	}

	public void update_collision(){
		foreach(CollisionShape3D cshape in close_trail_area.GetChildren()){
			if(cshape.Name != "last"){
				close_trail_area.RemoveChild(cshape);
				trail_area.AddChild(cshape);
			}
		}
		CollisionShape3D p = new CollisionShape3D();
		ConvexPolygonShape3D shape = new ConvexPolygonShape3D();
		Vector3[] pc = new Vector3[6];

		int i = added_points.Count - 4;
		//hard coding edge cases - definitely a better solution but not important
		if(i == -2){
			if(points.Count < 2){
				return;
			}
			//halfway in points case
			pc[0] = points[points.Count - 2];
			pc[1] = points[points.Count - 1];
			pc[2] = added_points[0];

			pc[3] = points[points.Count - 1];
			pc[4] = added_points[1];
			pc[5] = added_points[0];			
		}
		else if(i < -2){
			//all the way in points
			if(points.Count < 4){
				//dont add shape
				return;
			}
			pc[0] = points[points.Count + i];
			pc[1] = points[points.Count + i + 1];
			pc[2] = points[points.Count + i + 2];

			pc[3] = points[points.Count + i + 1];
			pc[4] = points[points.Count + i + 3];
			pc[5] = points[points.Count + i + 2];
		}
		else{
			//normal - all the way in added points
			pc[0] = added_points[i];
			pc[1] = added_points[i+1];
			pc[2] = added_points[i+2];

			pc[3] = added_points[i+1];
			pc[4] = added_points[i+3];
			pc[5] = added_points[i+2];
		}

		shape.Points = pc;
		p.Shape = shape;
		close_trail_area.AddChild(p);
	}

	public void last_hitbox(){
		CollisionShape3D p = new CollisionShape3D();
		ConvexPolygonShape3D shape = new ConvexPolygonShape3D();
		Vector3 top;
		Vector3 bot;
		if(added_points.Count < 2){
			if(points.Count < 2){
				return;
			}

			top = points[points.Count - 1];
			bot = points[points.Count - 2];
		}
		else{
			top = added_points[added_points.Count - 1];
			bot = added_points[added_points.Count - 2];
		}
		if((top - parent_player.trailtop.GlobalPosition).Length() < 0.2f || (bot - parent_player.trailbottom.GlobalPosition).Length() < 0.2f){
			return;
		}
		Vector3[] pc = new Vector3[6];
		pc[0] = bot;
		pc[1] = top;
		pc[2] = parent_player.trailbottom.GlobalPosition;

		pc[3] = top;
		pc[4] = parent_player.trailtop.GlobalPosition;
		pc[5] = parent_player.trailbottom.GlobalPosition;

		shape.Points = pc;
		p.Shape = shape;
		p.Name = "last";
		close_trail_area.AddChild(p);
	}

	public void add_section(Vector3 p1, Vector3 p2){
		if(!IsMultiplayerAuthority()){
			added_points.Add(p1);
			added_points.Add(p2);
			update_collision();
			draw_last_rect();
		}
		else{
			//Network authority
			added_points.Add(p1);
			added_points.Add(p2);
			update_collision();
			polygon_counter++;
			if(polygon_counter % CHUNK_SIZE == 0){
				authority_sync();
			}
			else{
				draw_last_rect();
			}
		}
	}

	private void authority_sync(){
		foreach(Vector3 point in added_points){
			points.Add(point);
		}
		Rpc("sync_trail", added_points);
		added_points.Clear();
		imesh.ClearSurfaces();
		draw_mesh(0);
	}
		
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]	
	public void sync_trail(Godot.Collections.Array<Vector3> sync_points){
		//remove bad client side collision shapes
		for(int i = 0; i < added_points.Count / 2; i++){
			trail_area.GetChild<CollisionShape3D>(trail_area.GetChildCount() - 1).QueueFree();
		}
		for(int i = 0; i < close_trail_area.GetChildCount(); i++){
			GD.Print(close_trail_area);
			GD.Print("-");
			close_trail_area.GetChild<CollisionShape3D>(trail_area.GetChildCount() - 1).QueueFree();
		}
		added_points.Clear();
			//use update_collision to them back
		for(int i = 0; i < sync_points.Count; i += 2){
			added_points.Add(sync_points[i]);
			added_points.Add(sync_points[i+1]);
			update_collision();
		}
		added_points.Clear();

		//update points and mesh
		foreach(Vector3 point in sync_points){
			points.Add(point);
		}

		imesh.ClearSurfaces();
		draw_mesh(0);
	}

	public void set_color(Color input_color){
		color = input_color;
		mesh.SetInstanceShaderParameter("input_color", color);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]	
	public void reset_trail(){
		trail_timer = -TRAIL_STARTUP;
		points.Clear();
		added_points.Clear();
		imesh.ClearSurfaces();
		frontimesh.ClearSurfaces();
		foreach(var child in trail_area.GetChildren()){
			if(child is CollisionShape3D){
				child.QueueFree();
			}
		}
		foreach(var child in close_trail_area.GetChildren()){
			if(child is CollisionShape3D){
				child.QueueFree();
			}
		}
		polygon_counter = 0;
	}

	public void _on_trail_area_body_entered_all(Node3D body){
		if(body.HasMethod("trail_collision")){
			((player)body).trail_collision();
		}
	}
	public void _on_trail_area_body_entered_close(Node3D body){
		if(body != parent_player){
			if(body.HasMethod("trail_collision")){
				((player)body).trail_collision();
			}
		}
	}
}
