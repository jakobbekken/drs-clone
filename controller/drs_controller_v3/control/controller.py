from drs_controller_v3.vision.camera import Camera


class Controller:
    def __init__(self):
        self.cam = Camera()

    def start(self):
        self.cam.start()
        self.cam.show_live_feed()
        self.cam.stop()
