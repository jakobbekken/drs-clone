import json
import socket


class Server:
    def __init__(self, host="0.0.0.0", port=6969):
        self.host = host
        self.port = port
        self.conn = None
        self.server = None

    def start(self):
        self.server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.server.bind((self.host, self.port))
        self.server.listen(1)
        print(f"Waiting for client on {self.host}:{self.port}...")
        self.conn, addr = self.server.accept()
        print(f"Connected to {addr}")

    def send(self, payload: dict):
        if not self.conn:
            return
        try:
            msg = json.dumps(payload) + "\n"
            self.conn.sendall(msg.encode())
        except (BrokenPipeError, ConnectionResetError):
            print("Client disconnected.")
            self.conn = None

    def stop(self):
        if self.conn:
            self.conn.close()
        if self.server:
            self.server.close()
        print("[Socket] Server closed.")
