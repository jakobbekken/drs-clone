import time

import cv2
import mediapipe as mp
from drs_controller_v3.vision.camera import Camera
from drs_controller_v3.vision.tracking import Tracker


class PressDetector:
    def __init__(self, alpha=0.3, debounce=0.2, speed_threshold=0.01):
        """
        alpha: smoothing factor for speed (0=very smooth, 1=raw)
        debounce: minimum seconds between presses
        speed_threshold: minimum absolute downward speed to count as a press
        """
        self.prev_speed = {"left": 0.0, "right": 0.0}
        self.smooth_speed = {"left": 0.0, "right": 0.0}
        self.alpha = alpha
        self.last_press_time = {"left": 0.0, "right": 0.0}
        self.debounce = debounce
        self.speed_threshold = speed_threshold

    def update(self, speeds):
        events = {"left": False, "right": False}
        now = time.time()

        for side in ["left", "right"]:
            raw = speeds[side]

            # Exponential smoothing
            self.smooth_speed[side] = (
                self.alpha * raw + (1 - self.alpha) * self.smooth_speed[side]
            )

            prev = self.prev_speed[side]
            current = self.smooth_speed[side]

            # Check zero-crossing AND threshold
            if prev < -self.speed_threshold and current >= -self.speed_threshold:
                if now - self.last_press_time[side] > self.debounce:
                    events[side] = True
                    self.last_press_time[side] = now

            self.prev_speed[side] = current

        return events


class Controller:
    def __init__(self):
        self.camera = Camera()
        self.tracker = Tracker()
        self.detector = PressDetector()
        self.drawer = mp.solutions.drawing_utils

    def start(self):
        self.camera.start()
        while True:
            frame = self.camera.read_frame()
            if frame is None:
                break

            rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            results = self.tracker.process_frame(rgb_frame)

            if results:
                left, right = results["left"], results["right"]

                events = self.detector.update(
                    {"left": left["speed_y"], "right": right["speed_y"]}
                )

                if events["left"]:
                    print("👣 Left foot press detected")
                    cv2.putText(
                        frame,
                        "Left Press",
                        (50, 50),
                        cv2.FONT_HERSHEY_SIMPLEX,
                        1,
                        (255, 0, 0),
                        2,
                    )

                if events["right"]:
                    print("👣 Right foot press detected")
                    cv2.putText(
                        frame,
                        "Right Press",
                        (350, 50),
                        cv2.FONT_HERSHEY_SIMPLEX,
                        1,
                        (0, 0, 255),
                        2,
                    )

                h, w, _ = frame.shape
                lx, ly = int(left["x"] * w), int(left["y"] * h)
                rx, ry = int(right["x"] * w), int(right["y"] * h)
                cv2.circle(frame, (lx, ly), 6, (255, 0, 0), -1)
                cv2.circle(frame, (rx, ry), 6, (0, 0, 255), -1)

            cv2.imshow("Pose Tracking (Press Detection)", frame)
            if cv2.waitKey(1) & 0xFF == ord("q"):
                break

        self.camera.stop()
