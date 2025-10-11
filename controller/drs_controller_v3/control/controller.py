import copy
from datetime import datetime

import cv2
import mediapipe as mp
from drs_controller_v3.logic.filtering import filter_exp
from drs_controller_v3.logic.foot_state import FootState
from drs_controller_v3.vision.camera import Camera
from drs_controller_v3.vision.tracking import Tracker


class Controller:
    def __init__(self, threshold=0.1):
        self.camera = Camera()
        self.tracker = Tracker()
        self.drawer = mp.solutions.drawing_utils

        self.foot_left = FootState(threshold)
        self.prev_left = None
        self.foot_right = FootState(threshold)
        self.prev_right = None

    def start(self):
        self.camera.start()
        while True:
            frame = self.camera.read_frame()
            if frame is None:
                break

            rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            results = self.tracker.pose.process(rgb_frame)

            if results.pose_landmarks:
                self.drawer.draw_landmarks(
                    frame,
                    results.pose_landmarks,
                    mp.solutions.pose.POSE_CONNECTIONS,
                )

                data = self.tracker.process_frame(rgb_frame)
                if data:
                    left, right = data["left"], data["right"]

                    left_x = left["x"]
                    left_y = left["y"]
                    left_speed_y = left["speed_y"]

                    right_x = right["x"]
                    right_y = right["y"]
                    right_speed_y = right["speed_y"]

                    time = data["time"]

                    self.prev_left = copy.deepcopy(self.foot_left)
                    self.prev_right = copy.deepcopy(self.foot_right)

                    new_left_x, new_left_y, new_left_speed_y = filter_exp(
                        self.prev_left, left_x, left_y, left_speed_y
                    )

                    new_right_x, new_right_y, new_right_speed_y = filter_exp(
                        self.prev_right, right_x, right_y, right_speed_y
                    )

                    self.foot_left.update(
                        new_left_x, new_left_y, new_left_speed_y, time
                    )
                    self.foot_right.update(
                        new_right_x, new_right_y, new_right_speed_y, time
                    )

                    if self.prev_left.state != self.foot_left.state:
                        print(datetime.now().strftime("%H:%M:%S.%f")[:-3])
                        print(f"New left state: {self.foot_left.state}")

                    if self.prev_right.state != self.foot_right.state:
                        print(f"New right state: {self.foot_right.state}")

                    # print(self.foot_left)

            cv2.imshow("Pose Tracking", frame)
            if cv2.waitKey(1) & 0xFF == ord("q"):
                break

        self.camera.stop()
        cv2.destroyAllWindows()
