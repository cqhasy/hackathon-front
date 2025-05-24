extends Control

func _ready():
	print(111)
	process_mode = Node.PROCESS_MODE_ALWAYS
	#get_tree().paused = false
	$Panel/continue.connect("pressed", Callable(self, "_on_continue_pressed"))
	$Panel/exit.connect("pressed", Callable(self, "_on_exit_pressed"))

func _on_continue_pressed():
	get_tree().paused = false
	queue_free()

func _on_exit_pressed():
	get_tree().paused = false
	call_deferred("_go_to_menu")

func _go_to_menu():
	get_tree().change_scene_to_file("res://scenes/menu.tscn")
	
