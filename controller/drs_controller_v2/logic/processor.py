import copy
from datetime import datetime
import time
from typing import Optional

from drs_controller_v2.foot_sniffer.sniffer import FootSniffer  # it kinda sux icl
from drs_controller_v2.logic.filtering import filter_exp
from drs_controller_v2.logic.foot_state import FootState
from drs_controller_v2.vision.camera import Camera


class CamProcessor:
    def __init__(self, threshold: float = 0.1):
        self.camera = Camera()
        self.tracker = FootSniffer()

        self.foot_left = FootState(threshold)
        self.prev_left = None
        self.foot_right = FootState(threshold)
        self.prev_right = None

        self.prev_time = None

    @staticmethod
    def speed_y(curr: Optional[dict], prev: Optional[FootState], dt: Optional[float]):
        if curr is None or prev is None or dt is None:
            return 0
        return (curr["x"] - prev.y) / dt

    def process_frame(self, frame):
        data = self.tracker.process_frame(frame)
        if not data:
            return None

        left, right = data["left"], data["right"]
        t = time.time()
        dt = t - self.prev_time if self.prev_time else None

        self.prev_left = copy.deepcopy(self.foot_left)
        self.prev_right = copy.deepcopy(self.foot_right)

        new_left = filter_exp(self.prev_left, left["x"], left["y"], self.speed_y(left, self.prev_left, dt))
        new_right = filter_exp(self.prev_right, right["x"], right["y"], self.speed_y(right, self.prev_right, dt))

        self.foot_left.update(*new_left, t)
        self.foot_right.update(*new_right, t)

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
