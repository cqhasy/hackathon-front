extends Node

func _ready():
	# 初始按钮禁用
	$Button.disabled = true
	
	# 监听 LineEdit 的文本变化信号
	$NameInput/LineEdit.connect("text_changed", self._on_line_edit_text_changed)
	
	# 监听 Button 的 pressed 信号
	$Button.connect("pressed",self._on_Button_pressed)

func _on_line_edit_text_changed(new_text):
	# 非空时启用按钮，空时禁用按钮
	$Button.disabled = new_text.strip_edges() == ""
	PlayerStates.UserName = new_text.strip_edges()

func _on_Button_pressed():
	get_tree().change_scene_to_file("res://scenes/main.tscn")
	
