import json
import socket
import time

HOST = "0.0.0.0"
PORT = 6969


def start_sending():
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.bind((HOST, PORT))
    server.listen(1)
    print(f"Waiting for game on port {PORT}...")
    conn, addr = server.accept()
    print(f"Connected by {addr}")

    try:
        while True:
            data = {"x": 4, "y": 5}

            msg = json.dumps(data) + "\n"
            conn.sendall(msg.encode())

            time.sleep(0.01)
    except KeyboardInterrupt:
        print("Stopping controller...")
    finally:
        conn.close()
        server.close()
