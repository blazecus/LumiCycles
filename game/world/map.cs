using Godot;
using System;

public partial class map : Node3D
{
	const int GRID_SIZE = 12;
	const float GRID_CELL_SIZE = 50.0f;
	const float GRID_EDGE_WIDTH = 2.5f;
	const int BLOCK_COUNT = 8;
	const int BLOCK_SCALE_MIN = 40;
	const int BLOCK_SCALE_MAX = 160;
	const int BLOCK_X_BOUND = 500;
	const int BLOCK_Y_BOUND = 500;
	const int BLOCK_Z_MIN = 100;
	const int BLOCK_Z_MAX = 200;
	public int network_authority_id = 0;
	private RandomNumberGenerator rng;
	public Godot.Collections.Array<Vector3> block_position_array;
	public Godot.Collections.Array<Vector3> block_scale_array;
	public Godot.Collections.Array<Vector3> block_dir_array;
	public Godot.Collections.Array<float> block_speed_array;
	private Material grid_material = GD.Load<Material>("res://assets/materials/grid_material.tres");
	private Node3D grid_meshes;
	private MeshInstance3D mesh;
	private ImmediateMesh imesh;
	private world world_node;

	public override void _Ready()
	{
		GD.Seed(2397458);
		rng = new RandomNumberGenerator();

		world_node = GetParent<world>();
		grid_meshes = GetNode<Node3D>("grid_meshes");
		mesh = GetNode<MeshInstance3D>("grid");
		imesh = new ImmediateMesh();
		mesh.Mesh = imesh;
		mesh.SetInstanceShaderParameter("xwidth", GRID_EDGE_WIDTH);
		for(int i = 0; i < BLOCK_COUNT; i++){
			block_position_array.Add(Vector3.Zero);
			block_scale_array.Add(Vector3.Zero);
			block_dir_array.Add(Vector3.Zero);
			block_speed_array.Add(0);
			reset_block(i);
		}

		//set_up_imesh_grid();
		//set_up_cube_mesh_grid();
	}

	public void reset_block(int i){
		int seed = rng.RandiRange(0,3);
		int dsign = Mathf.Sign(seed - 1.5f);
		block_dir_array[i] = new Vector3(seed%2==0 ? dsign : 0, seed%2==1 ? dsign : 0, 0);
		
	}

	public void update_blocks(float delta){
		//blocks controlled here to make sync easy
		for(int i = 0; i < BLOCK_COUNT; i++){
			block_position_array[i] += block_dir_array[i] * block_speed_array[i] * delta;

			//out of bounds
			if( block_position_array[i].X < -BLOCK_X_BOUND || block_position_array[i].X > BLOCK_X_BOUND || 
			    block_position_array[i].Y < -BLOCK_Y_BOUND || block_position_array[i].Y > BLOCK_Y_BOUND){
					reset_block(i);
			}

			block cblock = (block)GetNode<Node>("Platforms").GetChildren()[i];
			cblock.GlobalPosition = block_position_array[i];
		}
	}
	public void multiplayer_setup(int net_id){
		network_authority_id = net_id;
		SetMultiplayerAuthority(network_authority_id);
	}

	public override void _Process(double delta)
	{
		mesh.SetInstanceShaderParameter("player_position", world_node.authority_player_position);
	}


	private void set_up_cube_mesh_grid(){
		BoxMesh z_box_mesh = new BoxMesh();
		z_box_mesh.Size = new Vector3(GRID_EDGE_WIDTH, GRID_EDGE_WIDTH, GRID_CELL_SIZE * GRID_SIZE);
		z_box_mesh.Material = grid_material;
		
		/*BoxMesh x_box_mesh = new BoxMesh();
		x_box_mesh.Size = new Vector3(GRID_CELL_SIZE * GRID_SIZE, GRID_EDGE_WIDTH, GRID_EDGE_WIDTH);
		x_box_mesh.Material = grid_material;

		BoxMesh y_box_mesh = new BoxMesh();
		y_box_mesh.Size = new Vector3(GRID_EDGE_WIDTH, GRID_CELL_SIZE * GRID_SIZE, GRID_EDGE_WIDTH);
		y_box_mesh.Material = grid_material;*/

		for(int i = -GRID_SIZE + 1; i < GRID_SIZE; i++){
			for(int j = -GRID_SIZE + 1; j < GRID_SIZE; j++){
				MeshInstance3D new_box = new MeshInstance3D();
				new_box.Mesh = z_box_mesh;
				new_box.Position = new Vector3(i * GRID_CELL_SIZE, j * GRID_CELL_SIZE, 0);
				grid_meshes.AddChild(new_box);

				MeshInstance3D new_box2 = new MeshInstance3D();
				new_box2.Mesh = z_box_mesh;
				new_box2.Position = new Vector3(0, i *GRID_CELL_SIZE, j * GRID_CELL_SIZE);
				new_box2.Rotate(Vector3.Up, Mathf.Pi/2.0f);
				grid_meshes.AddChild(new_box2);

				MeshInstance3D new_box3 = new MeshInstance3D();
				new_box3.Mesh = z_box_mesh;
				new_box3.Position = new Vector3(i * GRID_CELL_SIZE, 0, j * GRID_CELL_SIZE);
				new_box3.Rotate(Vector3.Right, Mathf.Pi/2.0f);
				grid_meshes.AddChild(new_box3);
			}
		}
	}

	private void set_up_imesh_grid(){
		imesh.SurfaceBegin(Mesh.PrimitiveType.Triangles, grid_material);

		for(int x = -GRID_SIZE + 1; x < GRID_SIZE; x++){
			for(int y = -GRID_SIZE + 1; y < GRID_SIZE; y++){
				//draw_grid_line(generate_grid_line(new Vector3(x * GRID_CELL_SIZE, y * GRID_CELL_SIZE, -GRID_SIZE * GRID_CELL_SIZE), new Vector3(0,0,1)));

				//draw_grid_line(generate_grid_line(new Vector3(-GRID_SIZE * GRID_CELL_SIZE, x * GRID_CELL_SIZE, y * GRID_CELL_SIZE), new Vector3(1,0,0)));
				//draw_grid_line(generate_grid_line(new Vector3(x * GRID_CELL_SIZE, -GRID_SIZE * GRID_CELL_SIZE, y * GRID_CELL_SIZE), new Vector3(0,1,0)));
			}
		}

		for(int y = -GRID_SIZE + 1; y < GRID_SIZE; y++){
			for(int z = -GRID_SIZE + 1; z < GRID_SIZE; z++){
				//draw_grid_line(generate_grid_line(new Vector3(-GRID_SIZE * GRID_CELL_SIZE, y * GRID_CELL_SIZE, z * GRID_CELL_SIZE), new Vector3(1,0,0)));
			}
		}

		for(int x = -GRID_SIZE + 1; x < GRID_SIZE; x++){
			for(int z = -GRID_SIZE + 1; z < GRID_SIZE; z++){
				draw_grid_line(generate_grid_line(new Vector3(x * GRID_CELL_SIZE, -GRID_SIZE * GRID_CELL_SIZE, z * GRID_CELL_SIZE), new Vector3(0,1,0)));
			}
		}

		imesh.SurfaceEnd();
	}

	private Godot.Collections.Array<Vector3> generate_grid_line(Vector3 start_pos, Vector3 dir){
		float total_grid_size = GRID_SIZE * GRID_CELL_SIZE * 2;

		Godot.Collections.Array<Vector3> points = new Godot.Collections.Array<Vector3>();
		
		points.Add(start_pos);

		if(dir.X == 1){
			points.Add(start_pos + new Vector3(total_grid_size,0,0));
			points.Add(start_pos + new Vector3(0,GRID_EDGE_WIDTH,0));
			points.Add(start_pos + new Vector3(total_grid_size,GRID_EDGE_WIDTH,0));

			points.Add(start_pos + new Vector3(0,0,GRID_EDGE_WIDTH));
			points.Add(start_pos + new Vector3(total_grid_size,0,GRID_EDGE_WIDTH));
			points.Add(start_pos + new Vector3(0,GRID_EDGE_WIDTH,GRID_EDGE_WIDTH));
			points.Add(start_pos + new Vector3(total_grid_size,GRID_EDGE_WIDTH,GRID_EDGE_WIDTH));
		}
		else if(dir.Y == 1){
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,0,0));
			points.Add(start_pos + new Vector3(0,total_grid_size,0));
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,total_grid_size,0));

			points.Add(start_pos + new Vector3(0,0,GRID_EDGE_WIDTH));
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,0,GRID_EDGE_WIDTH));
			points.Add(start_pos + new Vector3(0,total_grid_size,GRID_EDGE_WIDTH));
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,total_grid_size,GRID_EDGE_WIDTH));
		}
		else{
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,0,0));
			points.Add(start_pos + new Vector3(0,GRID_EDGE_WIDTH,0));
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,GRID_EDGE_WIDTH,0));

			points.Add(start_pos + new Vector3(0,0,total_grid_size));
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,0,total_grid_size));
			points.Add(start_pos + new Vector3(0,GRID_EDGE_WIDTH,total_grid_size));
			points.Add(start_pos + new Vector3(GRID_EDGE_WIDTH,GRID_EDGE_WIDTH,total_grid_size));
		}

		return points;
	}

	private void draw_grid_line(Godot.Collections.Array<Vector3> points){
		//uvs need testing

		//bottom face
		imesh.SurfaceSetUV(Vector2.Zero);
		imesh.SurfaceAddVertex(points[0]);
		imesh.SurfaceSetUV(new Vector2(0,points[4].Z - points[0].Z));
		imesh.SurfaceAddVertex(points[4]);
		imesh.SurfaceSetUV(new Vector2(points[1].X - points[0].X, 0));
		imesh.SurfaceAddVertex(points[1]);

		imesh.SurfaceSetUV(new Vector2(points[1].X - points[0].X, points[4].Z - points[0].Z));
		imesh.SurfaceAddVertex(points[5]);
		imesh.SurfaceSetUV(new Vector2(points[1].X - points[0].X, 0));
		imesh.SurfaceAddVertex(points[1]);
		imesh.SurfaceSetUV(new Vector2(0,points[4].Z - points[0].Z));
		imesh.SurfaceAddVertex(points[4]);

		//side faces
		imesh.SurfaceSetUV(new Vector2(points[4].Z - points[0].Z, 0));
		imesh.SurfaceAddVertex(points[0]);
		imesh.SurfaceSetUV(new Vector2(points[4].Z - points[0].Z, points[2].Y - points[0].Y));
		imesh.SurfaceAddVertex(points[2]);
		imesh.SurfaceSetUV(Vector2.Zero);
		imesh.SurfaceAddVertex(points[4]);

		imesh.SurfaceSetUV(new Vector2(0, points[2].Y - points[0].Y));
		imesh.SurfaceAddVertex(points[6]);
		imesh.SurfaceSetUV(Vector2.Zero);
		imesh.SurfaceAddVertex(points[4]);
		imesh.SurfaceSetUV(new Vector2(points[4].Z - points[0].Z, points[2].Y - points[0].Y));
		imesh.SurfaceAddVertex(points[2]);


		imesh.SurfaceSetUV(new Vector2(points[5].X - points[4].X, 0));
		imesh.SurfaceAddVertex(points[4]);
		imesh.SurfaceSetUV(new Vector2(points[5].X - points[4].X, points[7].Y - points[5].Y));
		imesh.SurfaceAddVertex(points[6]);
		imesh.SurfaceSetUV(Vector2.Zero);
		imesh.SurfaceAddVertex(points[5]);

		imesh.SurfaceSetUV(new Vector2(0, points[7].Y - points[5].Y));
		imesh.SurfaceAddVertex(points[7]);
		imesh.SurfaceSetUV(Vector2.Zero);
		imesh.SurfaceAddVertex(points[5]);
		imesh.SurfaceSetUV(new Vector2(points[5].X - points[4].X, points[7].Y - points[5].Y));
		imesh.SurfaceAddVertex(points[6]);	


		imesh.SurfaceSetUV(new Vector2(points[5].Z - points[1].Z, 0));
		imesh.SurfaceAddVertex(points[5]);
		imesh.SurfaceSetUV(new Vector2(points[5].Z - points[1].Z, points[3].Y - points[1].Y));
		imesh.SurfaceAddVertex(points[7]);
		imesh.SurfaceSetUV(Vector2.Zero);
		imesh.SurfaceAddVertex(points[1]);

		imesh.SurfaceSetUV(new Vector2(0, points[3].Y - points[1].Y));
		imesh.SurfaceAddVertex(points[3]);
		imesh.SurfaceSetUV(Vector2.Zero);
		imesh.SurfaceAddVertex(points[1]);
		imesh.SurfaceSetUV(new Vector2(points[5].Z - points[1].Z, points[3].Y - points[1].Y));
		imesh.SurfaceAddVertex(points[7]);
		
		
		imesh.SurfaceSetUV(new Vector2(points[1].X - points[0].X, 0));
		imesh.SurfaceAddVertex(points[1]);
		imesh.SurfaceSetUV(new Vector2(points[1].X - points[0].X, points[2].Y - points[0].Y));
		imesh.SurfaceAddVertex(points[3]);
		imesh.SurfaceSetUV(Vector2.Zero);
		imesh.SurfaceAddVertex(points[0]);

		imesh.SurfaceSetUV(new Vector2(0, points[2].Y - points[0].Y));
		imesh.SurfaceAddVertex(points[2]);
		imesh.SurfaceSetUV(Vector2.Zero);
		imesh.SurfaceAddVertex(points[0]);
		imesh.SurfaceSetUV(new Vector2(points[1].X - points[0].X, points[2].Y - points[0].Y));
		imesh.SurfaceAddVertex(points[3]);

		//top face
		imesh.SurfaceSetUV(new Vector2(0, points[6].Z - points[2].Z));
		imesh.SurfaceAddVertex(points[6]);
		imesh.SurfaceSetUV(Vector2.Zero);
		imesh.SurfaceAddVertex(points[2]);
		imesh.SurfaceSetUV(new Vector2(points[3].X - points[2].X, points[6].Z - points[2].Z));
		imesh.SurfaceAddVertex(points[7]);

		imesh.SurfaceSetUV(new Vector2(points[3].X - points[2].X, 0));
		imesh.SurfaceAddVertex(points[3]);
		imesh.SurfaceSetUV(new Vector2(points[3].X - points[2].X, points[6].Z - points[2].Z));
		imesh.SurfaceAddVertex(points[7]);
		imesh.SurfaceSetUV(Vector2.Zero);
		imesh.SurfaceAddVertex(points[2]);
	}
}
