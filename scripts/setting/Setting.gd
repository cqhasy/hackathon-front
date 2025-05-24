extends Window

@onready var volume_slider = $Volumn  # 请修改为你音量条节点的路径

func _ready():
	set_process_mode(Node.PROCESS_MODE_ALWAYS)
	hide()
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)  # 默认隐藏鼠标

	# 监听Slider值变化
	volume_slider.connect("value_changed", Callable(self, "_on_volumn_value_changed"))

func _process(_delta):
	if Input.is_action_just_pressed("ui_cancel"):
		if visible:
			_close_settings()
		else:
			_open_settings()

func _open_settings():
	show()
	get_tree().paused = true
	Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)

	# 读取当前主音量并更新滑块
	var bus_index = AudioServer.get_bus_index("Master")
	var db = AudioServer.get_bus_volume_db(bus_index)
	var linear_vol = db_to_linear(db)  # 转换为线性值（0~1）
	volume_slider.value = linear_vol * 100  # 映射到 0~100 范围

func _close_settings():
	hide()
	get_tree().paused = false
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _on_close_requested():
	_close_settings()

func _on_volumn_value_changed(value: float) -> void:
	var volume = value / 100.0
	var db = linear_to_db(volume)
	AudioServer.set_bus_volume_db(AudioServer.get_bus_index("Master"), db)



	
