#extends Node2D
#
#var pause_scene = preload("res://scenes/pause.tscn")
#var pause_instance = null
#
#func _process(_delta):
	#if Input.is_action_just_pressed("pa"):
		#if not pause_instance:
			#get_tree().paused = true
			#pause_instance = pause_scene.instantiate()
			#add_child(pause_instance)
			#Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
		#else:
			#get_tree().paused = false
			#pause_instance.queue_free()
			#pause_instance = null
			#Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
extends Node2D

var pause_scene = preload("res://scenes/pause.tscn")

var pause_instance = null

func _process(_delta):
	if Input.is_action_just_pressed("pa"):
		if not pause_instance:
			get_tree().paused = true
			pause_instance = pause_scene.instantiate()

			var camera = get_tree().current_scene.get_node("Camera2D")
			if camera:
				camera.zoom = Vector2(6.667,6.667)  # 设置缩放为1
				camera.add_child(pause_instance)
				if pause_instance is Control:
					pause_instance.set_anchors_preset(Control.PRESET_FULL_RECT)
					pause_instance.position = Vector2.ZERO
			else:
				print("未找到 Camera2D 节点")

			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
		else:
			get_tree().paused = false
			pause_instance.queue_free()
			pause_instance = null
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
