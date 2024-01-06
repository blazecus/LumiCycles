using System.Security.Cryptography;
using System.Reflection.Metadata;
using System.Linq;
using Godot;
using System;

public partial class world : Node
{
	const int PORT = 7788;
	const float END_ROUND_TIMER = 3.0f;

	private PackedScene start_screen = GD.Load<PackedScene>("res://game/UI/start_screen.tscn");
	private PackedScene player_scene = GD.Load<PackedScene>("res://game/player/player.tscn");

	private ENetMultiplayerPeer peer = new ENetMultiplayerPeer();

	private CanvasLayer lobby_menu;
	private CanvasLayer hud;
	private Label info_label;
	private Node3D players;
	private map world_map;

	[Export]
	public Godot.Collections.Array<int> player_win_count = new Godot.Collections.Array<int>();
	
	[Export]
	public string winner_name = "";
	private bool host = true;
	[Export]
	private int rounds_left = 1;
	[Export]
	private int total_rounds = 1;
	[Export]
	private int player_count = 1;
	[Export]
	private int alive_players = 1;
	private float end_round_time = 0.0f;
	public Vector3 authority_player_position = Vector3.Zero;

	public override void _EnterTree(){
		lobby_menu = GetNode<CanvasLayer>("menu_layer");
		hud = GetNode<CanvasLayer>("hud_layer");
		players = GetNode<Node3D>("Players");
		info_label = lobby_menu.GetNode<Label>("Label");
		world_map = GetNode<map>("Map");
	}

	public override void _Ready()
	{
		if(settings.Instance.host){
			start_host();
		}
		else{
			start_client(settings.Instance.connection_address);
		}
	}
	
	public override void _Process(double delta)
	{
		if(rounds_left <= 0){
			lobby_menu.GetNode<Label>("debug").Text = GetNode<Node3D>("Players").GetChildren().Count().ToString();
			lobby_menu.GetNode<Label>("numplayers").Text = Multiplayer.GetPeers().Count().ToString();
		}
		else{
			if(IsMultiplayerAuthority() && end_round_time > 0){
				end_round_time -= (float) delta;
				//GD.Print(end_round_time);
				if(end_round_time <= 0){
					rounds_left -= 1;
					if(rounds_left == 0){
						end_game();
					}
					else{
						start_round();
					}
				}
			}
		}
	}

	public void start_host(){
		peer.CreateServer(PORT);
		Multiplayer.MultiplayerPeer = peer;
		// listen to peer connections, and create new player for them
		Multiplayer.PeerConnected += CreatePlayer;
		// listen to peer disconnections, and destroy their players
		Multiplayer.PeerDisconnected += DestroyPlayer;	

		int temp_id = Multiplayer.GetUniqueId();
		CreatePlayer(temp_id);
		//GetNode<map>("Map").set_mu
		
		upnp_setup();
	}

	public void start_client(string address){
		lobby_menu.GetNode<Button>("start_button").Visible = false;
		lobby_menu.GetNode<SpinBox>("round_count_selector").Visible = false;
		lobby_menu.GetNode<SpinBox>("player_count_selector").Visible = false;
		lobby_menu.GetNode<ColorPickerButton>("ColorPickerButton").Visible = true;
		Error connection = peer.CreateClient(address, PORT);
		if(connection != Error.Ok){
			lobby_menu.GetNode<Label>("Label").Text = "connection failed!";
			return;
		}
		Multiplayer.MultiplayerPeer = peer;
		lobby_menu.GetNode<Label>("Label").Text = "lobby " + peer.Host.ToString();
	}

	private void CreatePlayer(long id){
		player player = (player) player_scene.Instantiate();
		player.Name = id.ToString();
		GetNode<Node3D>("Players").AddChild(player);
		player_win_count.Add(0);
	}

	private void DestroyPlayer(long id){
		for(int i = 0; i < players.GetChildCount(); i++){
			if(players.GetChild<player>(i).Name == id.ToString()){
				players.GetChild<player>(i).prepare_destruction();
				players.GetChild<player>(i).QueueFree();
				player_win_count.RemoveAt(i);
			}
		}
		//GetNode<Node3D>("Players").GetNode<player>(id.ToString()).QueueFree();
		GD.Print(Multiplayer.GetPeers().Count());
	}

	private void upnp_setup(){
		Upnp upnp = new Upnp();
		int discover_result = upnp.Discover();
		//Debug.Assert(discover_result == (int) Upnp.UpnpResult.Success);
		if(discover_result != (int) Upnp.UpnpResult.Success){
			throw new ApplicationException("discovery failed!");
		}
		//Debug.Assert(upnp.GetGateway() != null && upnp.GetGateway().IsValidGateway());
		if(!(upnp.GetGateway() != null && upnp.GetGateway().IsValidGateway())){
			throw new ApplicationException("gateway invalid!");
		}
		int map_result = upnp.AddPortMapping(PORT);
		//Debug.Assert(map_result == (int) Upnp.UpnpResult.Success);
		if(map_result != (int) Upnp.UpnpResult.Success){
			throw new ApplicationException("port mapping failed!");
		}
		GD.Print("success, join address: " + upnp.QueryExternalAddress());
		info_label.Text = "Address: " + upnp.QueryExternalAddress();
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]	
	private void start_game(){
		total_rounds = (int) lobby_menu.GetNode<SpinBox>("round_count_selector").Value;
		rounds_left = total_rounds;
		lobby_menu.Visible = false;
		hud.Visible = true;
		if(IsMultiplayerAuthority()){
			Rpc("start_game");
			start_round();
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]	
	private void start_round(){
		if(IsMultiplayerAuthority()){
			Rpc("start_round");
		}
		hud.GetNode<Label>("winner_label").Visible = false;
		hud.GetNode<Label>("current_round").Text = "Round " + (total_rounds - rounds_left + 1).ToString();
		for(int i = 0; i < players.GetChildCount(); i++){
			player p = players.GetChild<player>(i);
			//set up better spawn system later along with map selection
			p.spawn_player(world_map.GetNode<stage3>("stage").GetNode<Node3D>("spawns").GetChild<Marker3D>(i).GlobalPosition);
		}
	}
	
	private void _on_start_button_button_up()
	{
		if(IsMultiplayerAuthority()){
			start_game();
		}
	}
	
	private void _on_back_button_button_up()
	{
		GetTree().ChangeSceneToPacked(start_screen);
	}

	public void _on_multiplayer_spawner_spawned(Node spawned_node){
		lobby_menu.GetNode<Label>("debug").Text = Multiplayer.GetPeers().Count().ToString();
	}

	public void _on_color_picker_button_color_changed(Color color){
		foreach(player p in GetNode<Node3D>("Players").GetChildren()){
			if(p.IsMultiplayerAuthority()){
				p.set_color(color);
				//p.set_color(lobby_menu.GetNode<ColorPicker>("ColorPicker").Color);
			}
		}
	}

	public void player_died(){
		if(!IsMultiplayerAuthority()){
			return;
		}

		//check if round is over
		check_end_round();
	}

	
	public void check_end_round(){
		if(players.GetChildCount() == 1){
			//winner_name = players.GetChild<player>(0).Name;
			player_win_count[0]++;
			end_round(players.GetChild<player>(0).Name);
		}
		else{
			int alive = 0;
			int alive_idx = 0;
			for(int i = 0; i < players.GetChildCount(); i++){
				if(players.GetChild<player>(i).alive){
					alive++;
					alive_idx = i;
				}
			}
			if(alive == 1){
				//winner_name = players.GetChild<player>(alive_idx).Name;
				player_win_count[alive_idx]++;
				end_round(players.GetChild<player>(alive_idx).Name);
			}
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]	
	public void end_round(string winner_name){
		if(IsMultiplayerAuthority()){
			Rpc("end_round", winner_name);
		}
		end_round_time = END_ROUND_TIMER;

		hud.GetNode<Label>("winner_label").Visible = true;
		hud.GetNode<Label>("winner_label").Text = winner_name + " wins!";
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]	
	public void end_game(){
		if(IsMultiplayerAuthority()){
			Rpc("end_game");
		}

		lobby_menu.Visible = true;
		hud.Visible = false;
		reset_players();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]	
	public void reset_players(){
		foreach(player p in players.GetChildren()){
			p.reset_player();
		}
	}
}

