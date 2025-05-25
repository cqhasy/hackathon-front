extends Node

func _ready():
	# 初始按钮禁用
	$Button.disabled = true
	
	## 监听 LineEdit 的文本变化信号
	#$NameInput/LineEdit.connect("text_changed", self._on_line_edit_text_changed)
	
	# 监听 Button 的 pressed 信号
	$Button.connect("pressed",self._on_Button_pressed)
	# 停止音乐
	GlobalAudio.stop_music()
	# 播放音乐
	GlobalAudio.play_music(load("res://assets/music/Mr_JX - 英雄主义（没错，我的名字就是堂吉诃德）.ogg"))

	

func _on_line_edit_text_changed(new_text):
	# 非空时启用按钮，空时禁用按钮
	$Button.disabled = new_text.strip_edges() == ""
	PlayerStates.UserName = new_text.strip_edges()

func _on_Button_pressed():
	GlobalAudio.stop_music()
	GlobalAudio.play_music(load("res://assets/music/653718__josefpres__8-bit-game-loop-003-simple-mix-3-long-120-bpm.ogg"))
	get_tree().change_scene_to_file("res://scenes/main.tscn")
	
