import json
import socket

HOST = "127.0.0.1"
PORT = 6969

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((HOST, PORT))
print("Connected to controller!")

buffer = ""

try:
    while True:
        data = sock.recv(1024)
        if not data:
            break
        buffer += data.decode()

        lines = buffer.split("\n")
        buffer = lines.pop()

        for line in lines:
            if not line.strip():
                continue
            try:
                msg = json.loads(line)
                print("Received:", msg)
            except json.JSONDecodeError:
                print("Bad JSON:", line)

except KeyboardInterrupt:
    print("Stopped.")
finally:
    sock.close()
