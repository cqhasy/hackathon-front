extends Control

@onready var coin_label = $Coin
@onready var item_container = $GoodsContainer
@onready var confirm_dialog = $ConfirmDialog
@onready var exit_button = $ExitButton
@onready var buy_button = $BuyButton

var selected_item = null
var player_coins = 300  
var price = null

func _ready():
	update_coin_label()

	for item_button in item_container.get_children():
		var price = 100 + randi() % 50  
		item_button.text = "%d金币" % price
		item_button.set_meta("price", price)
		item_button.connect("pressed",Callable(self,"_on_item_selected").bind(item_button))

	buy_button.connect("pressed",Callable(self,"_on_buy_button_pressed"))
	exit_button.connect("pressed", Callable(self, "_on_exit_button_pressed"))
	confirm_dialog.connect("confirmed", Callable(self, "_on_purchase_confirmed"))

func update_coin_label():
	coin_label.text = "金币：%d" % player_coins

func _on_item_selected(item_button):
	selected_item = item_button
	price = selected_item.get_meta("price")

func _on_buy_button_pressed():
	confirm_dialog.dialog_text = "是否花费 %d 金币购买商品？" % price
	confirm_dialog.popup_centered()

func _on_purchase_confirmed():
	if selected_item:
		var price = selected_item.get_meta("price")
		if player_coins >= price:
			player_coins -= price
			update_coin_label()

			
			selected_item.text = "已经售出"
			selected_item.disabled = true
			confirm_dialog.dialog_text = "购买成功：%s" %selected_item.text
			confirm_dialog.popup_centered()
		else:
			confirm_dialog.dialog_text = "金币不足，购买失败"
			confirm_dialog.popup_centered()
		selected_item = null

func _on_exit_button_pressed():
	queue_free()
