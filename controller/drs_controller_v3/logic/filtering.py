import math

def filter_exp(last_state, x, y, speed_y, alpha=0.8):
    if not last_state:
        return x, y, speed_y

    new_x = alpha * x + (1 - alpha) * last_state.x
    new_y = alpha * y + (1 - alpha) * last_state.y
    new_speed_y = alpha * speed_y + (1 - alpha) * last_state.speed_y

    return new_x, new_y, new_speed_y

def alpha(dt, cutoff):
    tau = 1.0 / (2.0 * math.pi * cutoff)
    return 1.0 / (1.0 + tau / dt)


def filter_1_euro(
    last_state,
    x, y, speed_y,
    freq=60.0,
    min_cutoff=1.5,
    beta=0.02,
    d_cutoff=1.0
):
    if not last_state or last_state.last_raw_y is None:
        if last_state:
            last_state.last_raw_x = x
            last_state.last_raw_y = y
        return x, y, 0.0

    dt = last_state.dt

    dy = (y - last_state.last_raw_y) / dt
    last_state.last_raw_y = y
    last_state.last_raw_x = x

    alpha_d = alpha(dt, d_cutoff)
    dy_hat = alpha_d * dy + (1 - alpha_d) * last_state.dy_hat

    cutoff = min_cutoff + beta * abs(dy_hat)

    alpha_pos = alpha(dt, cutoff)

    x_hat = alpha_pos * x + (1 - alpha_pos) * last_state.x
    y_hat = alpha_pos * y + (1 - alpha_pos) * last_state.y

    return x_hat, y_hat, dy_hat