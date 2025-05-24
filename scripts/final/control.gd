extends Control


@onready var score_label = $ScoreLabel
@onready var retry_button = $VBoxContainer/again
@onready var quit_button = $VBoxContainer/exit

var final_score: int = 0

func _ready():
  
	score_label.text = "你的得分：%d" % final_score
	retry_button.pressed.connect(_on_retry_pressed)
	quit_button.pressed.connect(_on_quit_pressed)

func set_score(score: int):
	final_score = score

func _on_retry_pressed():
	get_tree().change_scene_to_file("res://Main.tscn")  # 改成你的主场景路径

func _on_quit_pressed():
	get_tree().quit()
