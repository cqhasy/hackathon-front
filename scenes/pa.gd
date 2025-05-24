extends Node2D

var pause_scene = preload("res://scenes/pause.tscn")
var pause_instance = null

func _process(_delta):
	if Input.is_action_just_pressed("pa"):
		if not pause_instance:
			get_tree().paused = true
			pause_instance = pause_scene.instantiate()
			add_child(pause_instance)
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
		else:
			get_tree().paused = false
			pause_instance.queue_free()
			pause_instance = null
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
