using Godot;
using System;

public partial class settings_screen : Node2D
{
	private PackedScene start_scene = ResourceLoader.Load<PackedScene>("res://game/UI/start_screen.tscn"); 

	private Button toggle_controller_button;
	public override void _Ready()
	{
		toggle_controller_button = GetNode<Button>("toggle_controls");
	}

	public override void _Process(double delta)
	{
	}
	
	private void _on_back_button_button_up(){
		GetTree().ChangeSceneToPacked(start_scene);
	}

	private void _on_toggle_controls_button_up(){
		settings.Instance.controller_toggle = !settings.Instance.controller_toggle;
		if(settings.Instance.controller_toggle){
			toggle_controller_button.Text = "Switch to keyboard/mouse";
		}
		else{
			toggle_controller_button.Text = "Switch to controller";
		}
	}
}
