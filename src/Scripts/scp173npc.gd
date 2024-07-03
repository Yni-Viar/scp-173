extends CharacterBody3D


@export var speed: float = 40.0;
# Check if human is watching
@export var is_watching: bool = false

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _physics_process(delta):
	# If is watching, set velocity to zero, else - go to player.
	if !is_watching:
		var player_direction: Vector3 = global_position.direction_to(get_tree().root.get_node("Game/Player").global_position)
		velocity += speed * player_direction * delta
		# Look at player
		look_at(get_tree().root.get_node("Game/Player").global_position)
	else:
		velocity = Vector3.ZERO
	# Necessary for moving
	move_and_slide()
	
	# BONUS: crunching player mechanic.
	for i in range(get_slide_collision_count()):
		# check collision, and if collision is player (PlayerScript), crunch it.
		var check_collision: KinematicCollision3D = get_slide_collision(i)
		var collided_with = check_collision.get_collider()
		if collided_with is PlayerScript:
			$InteractSound.play()
			collided_with.call("health_manage", -16777216)
			# disable statue function
			set_physics_process(false)


func _on_dont_look_at_me_screen_entered():
	is_watching = true


func _on_dont_look_at_me_screen_exited():
	is_watching = false
