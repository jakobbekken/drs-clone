def filter_exp(last_state, x, y, speed_y, alpha=0.8):
    if not last_state:
        return x, y, speed_y

    new_x = alpha * x + (1 - alpha) * last_state.x
    new_y = alpha * y + (1 - alpha) * last_state.y
    new_speed_y = alpha * speed_y + (1 - alpha) * last_state.speed_y

    return new_x, new_y, new_speed_y
