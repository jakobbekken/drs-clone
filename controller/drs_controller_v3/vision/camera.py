import cv2


class Camera:
    def __init__(self):
        self.cap = None

    def start(self):
        self.cap = cv2.VideoCapture(0)
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

    def show_live_feed(self):
        while True:
            frame = self.read_frame()
            if frame is None:
                break

            cv2.imshow("Camera Feed", frame)

            if cv2.waitKey(1) & 0xFF == ord("q"):
                break

    def stop(self):
        if self.cap:
            self.cap.release()
        cv2.destroyAllWindows()
        print("Camera stopped")
