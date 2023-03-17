using System.ComponentModel;
using Godot;
using System;

public partial class join_screen : Node2D
{
	private PackedScene game_scene = ResourceLoader.Load<PackedScene>("res://game/world/world.tscn");
	private PackedScene start_scene = ResourceLoader.Load<PackedScene>("res://game/UI/start_screen.tscn"); 
	
	private LineEdit address_edit;	

	public override void _Ready()
	{
		address_edit = GetNode<LineEdit>("address_entry");
	}

	public override void _Process(double delta)
	{
	}

	private void _on_join_button_button_up()
	{
		settings.Instance.host = false;
		settings.Instance.connection_address = address_edit.Text;
		GetTree().ChangeSceneToPacked(game_scene);
	}

	private void _on_back_button_button_up()
	{
		GetTree().ChangeSceneToPacked(start_scene);
	}
}

