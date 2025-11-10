from datetime import datetime

import cv2
import mediapipe as mp
from drs_controller_v3.logic.filtering import filter_1_euro
from drs_controller_v3.logic.foot_state import FootState
from drs_controller_v3.vision.camera import Camera
from drs_controller_v3.vision.tracking import Tracker


class CamProcessor:
    def __init__(self, threshold=1):
        self.camera = Camera()
        self.tracker = Tracker()
        self.drawer = mp.solutions.drawing_utils

        self.foot_left = FootState(threshold)
        self.prev_left = None
        self.foot_right = FootState(threshold)
        self.prev_right = None

    def process_frame(self, frame):
        rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        results = self.tracker.pose.process(rgb)
        if not results.pose_landmarks:
            return None

        self.drawer.draw_landmarks(
            frame, results.pose_landmarks, mp.solutions.pose.POSE_CONNECTIONS
        )

        data = self.tracker.process_frame(rgb)
        if not data:
            return None

        left, right = data["left"], data["right"]
        t = data["time"]

        self.prev_left = self.foot_left
        self.prev_right = self.foot_right

        # new_left = filter_exp(self.prev_left, left["x"], left["y"], left["speed_y"])
        # new_left = filter_1_euro(self.prev_left, left["x"], left["y"], left["speed_y"])

        # new_right = filter_exp(
        #     self.prev_right, right["x"], right["y"], right["speed_y"]
        # )
        # new_right = filter_1_euro(self.prev_right, right["x"], right["y"], right["speed_y"])

        new_left = filter_1_euro(
            self.prev_left,
            left["x"],
            left["y"],
            left["speed_y"]
        )

        new_right = filter_1_euro(
            self.prev_right,
            right["x"],
            right["y"],
            right["speed_y"]
        )

        self.foot_left.update(*new_left, t)
        self.foot_right.update(*new_right, t)

        print(new_left, " ", new_right)

        return {
            "left": {
                "x": self.foot_left.x,
                "y": self.foot_left.y,
                "state": self.foot_left.state,
            },
            "right": {
                "x": self.foot_right.x,
                "y": self.foot_right.y,
                "state": self.foot_right.state,
            },
            "time": datetime.now().isoformat(timespec="milliseconds"),
        }
