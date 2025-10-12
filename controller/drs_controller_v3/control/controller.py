import cv2
from drs_controller_v3.control.sender import Server
from drs_controller_v3.logic.processor import CamProcessor


class Controller:
    def __init__(self, threshold=0.1, host="0.0.0.0", port=6969):
        self.processor = CamProcessor(threshold)
        self.server = Server(host, port)
        self.running = True

    def start(self):
        self.server.start()
        self.processor.camera.start()

        while self.running:
            frame = self.processor.camera.read_frame()
            if frame is None:
                break

            result = self.processor.process_frame(frame)
            if result:
                self.server.send(result)

            cv2.imshow("Pose Tracking", frame)
            if cv2.waitKey(1) & 0xFF == ord("q"):
                self.running = False
                break

        self.shutdown()

    def shutdown(self):
        self.processor.camera.stop()
        self.server.stop()
        cv2.destroyAllWindows()
