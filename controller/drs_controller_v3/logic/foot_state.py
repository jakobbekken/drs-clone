class FootState:
    def __init__(self, threshold):
        self.state = "Idle"
        self.x = 0
        self.y = 0
        self.speed_y = 0
        self.dy_hat = 0
        self.last_raw_x = None
        self.last_raw_y = None
        self.dt = 1/60
        self.threshold = threshold

    def update(self, x, y, speed_y, time):
        if self.state == "Idle":
            if speed_y < -self.threshold:
                self.state = "Up"

        elif self.state == "Up":
            if speed_y > self.threshold:
                self.state = "Down"

        elif self.state == "Down":
            if speed_y < abs(self.threshold):
                self.state = "Press"

        elif self.state == "Press":
            self.state = "Idle"

        self.x = x
        self.y = y
        self.speed_y = speed_y
        self.time = time

    def __repr__(self):
        return (
            f"FootState(state={self.state}, x={self.x:.3f}, y_speed={self.speed_y:.3f})"
        )
