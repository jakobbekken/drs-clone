import cv2


class Camera:
    def __init__(self, index=0):
        self.cap = None
        self.index = index

    def start(self):
        self.cap = cv2.VideoCapture(self.index)
        if not self.cap.isOpened():
            raise RuntimeError("Cannot open camera")

        print("Camera opened successfully")
        print("Press 'q' to quit")

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
