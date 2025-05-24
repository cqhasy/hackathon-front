extends Control


@onready var score_label = $ScoreLabel
@onready var retry_button = $again
@onready var quit_button = $exit

var final_score: int = 0

func _ready():
	final_score = PlayerStates.Score;
	score_label.text = "你的得分：%d" % final_score
	retry_button.pressed.connect(_on_retry_pressed)
	quit_button.pressed.connect(_on_quit_pressed)

func set_score(score: int):
	final_score = score

func _on_retry_pressed():
	get_tree().change_scene_to_file("res://scenes/main.tscn") 

func _on_quit_pressed():
	get_tree().change_scene_to_file("res://scenes/menu.tscn")
