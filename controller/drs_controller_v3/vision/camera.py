import cv2
import time

class Camera:
    def __init__(self, index=0):
        self.cap = None
        self.index = index
        self.fps = 60
        self.last_time = None

    def start(self):
        self.cap = cv2.VideoCapture(self.index)
        if not self.cap.isOpened():
            raise RuntimeError("Cannot open camera")

        self.fps = self.cap.get(cv2.CAP_PROP_FPS)

        print(f"Camera opened successfully ({self.fps:.1f} FPS)")
        print("Press 'q' to quit")

        self.last_time = time.time()

    def read_frame(self):
        if self.cap is None:
            return None

        ret, frame = self.cap.read()
        if not ret:
            return None

        return frame

    def stop(self):
        if self.cap:
            self.cap.release()
        cv2.destroyAllWindows()
        print("Camera stopped")
