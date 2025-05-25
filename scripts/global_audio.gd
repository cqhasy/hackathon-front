extends Node

var music_player: AudioStreamPlayer
var sfx_player: AudioStreamPlayer

func _ready():
	music_player = AudioStreamPlayer.new()
	add_child(music_player)
	music_player.bus = "Music"
	music_player.autoplay = false

	sfx_player = AudioStreamPlayer.new()
	add_child(sfx_player)
	sfx_player.bus = "SFX"
	sfx_player.autoplay = false

	# 可选：连接 finished 信号
	music_player.connect("finished", Callable(self, "_on_music_finished"))
	sfx_player.connect("finished", Callable(self, "_on_sfx_finished"))

func play_music(stream: AudioStream, loop := true):
	music_player.stream = stream
	music_player.play()

func stop_music():
	music_player.stop()

func play_sfx(stream: AudioStream):
	sfx_player.stream = stream
	sfx_player.play()

func _on_music_finished():
	music_player.play()

func _on_sfx_finished():
	# 可在此处实现音效播放结束后的逻辑
	pass
