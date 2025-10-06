import time

import mediapipe as mp


class Tracker:
    def __init__(self, min_detection_confidence=0.5, min_tracking_confidence=0.5):
        self.pose = mp.solutions.pose.Pose(
            min_detection_confidence=min_detection_confidence,
            min_tracking_confidence=min_tracking_confidence,
        )

        self.last_y = {"left": None, "right": None}
        self.last_time = {"left": None, "right": None}

    def _compute_speed(self, side, y, current_time):
        speed_y = 0
        if self.last_y[side] is not None and self.last_time[side] is not None:
            dt = current_time - self.last_time[side]
            if dt > 0:
                speed_y = (y - self.last_y[side]) / dt

        self.last_y[side] = y
        self.last_time[side] = current_time
        return speed_y

    def process_frame(self, frame):
        results = self.pose.process(frame)

        if not results.pose_landmarks:
            return None

        lm = results.pose_landmarks.landmark
        current_time = time.time()

        right = lm[mp.solutions.pose.PoseLandmark.RIGHT_ANKLE]
        left = lm[mp.solutions.pose.PoseLandmark.LEFT_ANKLE]

        right_data = {
            "x": right.x,
            "y": right.y,
            "speed_y": self._compute_speed("right", right.y, current_time),
        }

        left_data = {
            "x": left.x,
            "y": left.y,
            "speed_y": self._compute_speed("left", left.y, current_time),
        }

        return {"left": left_data, "right": right_data}
