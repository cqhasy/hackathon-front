extends Control

@onready var coin_label = $Coin
@onready var Text_box = $Textbox
@onready var item_container = $GoodsContainer
@onready var confirm_dialog = $ConfirmDialog
@onready var exit_button = $ExitButton

var selected_item = null
var player_coins = PlayerStates.CurrentMoney;
var index = null

func _ready():
	update_coin_label()
	
	var descriptions = [
		"启用主动追踪",
		"加快护盾生成",
		"增加生命上限",
		"减少子弹时间冷却时间"
	]

	var i = 0 
	var j = 1
	for item_button in item_container.get_children():
		var price = 100 + randi() % 50 
		var Price = item_button.get_node("Price")
		Price.text = "%d金币" % price
		item_button.set_meta("price", price)
		
		var desc = descriptions[i % descriptions.size()]
		item_button.set_meta("description", desc)
		i += 1
		j += 1
		
		
		
		item_button.connect("pressed",Callable(self,"_on_item_pressed").bind(item_button,i))
		item_button.connect("mouse_entered",Callable(self,"_on_mouse_entered").bind(item_button,j))
		item_button.connect("mouse_exited",Callable(self,"_on_mouse_exited").bind(item_button,j))

	
	exit_button.connect("mouse_entered", Callable(self, "_on_button_mouse_entered").bind(exit_button))
	exit_button.connect("mouse_exited", Callable(self, "_on_button_mouse_exited").bind(exit_button))
	exit_button.connect("pressed", Callable(self, "_on_exit_button_pressed"))
	confirm_dialog.connect("confirmed", Callable(self, "_on_purchase_confirmed"))

func _on_mouse_entered(item_button,index):
	item_button.set_default_cursor_shape(Input.CURSOR_POINTING_HAND)
	var texture = load("res://assets/store/shop_icon%d.png" %(index + 7))
	item_button.icon = texture
	
	
func _on_mouse_exited(item_button,index):
	item_button.set_default_cursor_shape(Input.CURSOR_ARROW)
	var texture = load("res://assets/store/shop_icon%d.png" %(index - 1))
	item_button.icon = texture
	

func update_coin_label():
	PlayerStates.CurrentMoney = player_coins
	coin_label.text = "金币：%d" % player_coins

func _on_item_pressed(item_button,i):
	selected_item = item_button 
	index = i
	var description = selected_item.get_meta("description")
	var price = selected_item.get_meta("price")
	Text_box.text = description
	
	confirm_dialog.dialog_text = "是否花费 %d 金币购买商品？" % price
	confirm_dialog.popup_centered()


func _on_purchase_confirmed():
	if selected_item:
		var price = selected_item.get_meta("price")
		var Price = selected_item.get_node("Price")
		if player_coins >= price:
			player_coins -= price
			index-=1
			match index:
				0:
					PlayerStates.UseActiveTrace = true
				1:
					PlayerStates.ShieldCostTime = max(PlayerStates.ShieldCostTime - 1, 1)
				2:
					PlayerStates.MaxHealth = PlayerStates.MaxHealth + 10
				3:
					PlayerStates.SlowDownCostTime = max(PlayerStates.SlowDownCostTime - 1,1)
			update_coin_label()
			
			Price.text = "已经售出"
			Text_box.text = ""
			
			selected_item.disabled = true
			confirm_dialog.dialog_text = "购买成功：%s" %selected_item.text
			confirm_dialog.popup_centered()
		else:
			confirm_dialog.dialog_text = "金币不足"
			confirm_dialog.popup_centered()
		selected_item = null

func _on_button_mouse_entered(button):
	button.set_default_cursor_shape(Input.CURSOR_POINTING_HAND)

func _on_button_mouse_exited(button):
	button.set_default_cursor_shape(Input.CURSOR_ARROW)

func _on_exit_button_pressed():
	get_tree().change_scene_to_file("res://scenes/main.tscn")
	
