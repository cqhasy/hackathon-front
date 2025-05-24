extends AudioStreamPlayer2D

func _ready():
	set_process_mode(Node.PROCESS_MODE_ALWAYS)
	volume_db = 0
	play()
	connect("finished", Callable(self, "_on_music_finished"))

func _on_finished():
	await get_tree().create_timer(2.0).timeout  # 等待两秒
	play()  # 重新播放
