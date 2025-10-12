import asyncio
import json

import websockets


class Server:
    def __init__(self, host="0.0.0.0", port=6969):
        self.host = host
        self.port = port
        self.clients = set()
        self.loop = None
        self.server = None

    async def handler(self, websocket):
        print(f"Client connected: {websocket.remote_address}")
        self.clients.add(websocket)
        try:
            await websocket.wait_closed()
        finally:
            print(f"Client disconnected: {websocket.remote_address}")
            self.clients.remove(websocket)

    async def start_server(self):
        self.server = await websockets.serve(self.handler, self.host, self.port)
        print(f"Waiting for client on ws://{self.host}:{self.port}...")
        await self.server.wait_closed()

    def start(self):
        self.loop = asyncio.new_event_loop()
        asyncio.set_event_loop(self.loop)
        self.loop.create_task(self.start_server())
        import threading

        threading.Thread(target=self.loop.run_forever, daemon=True).start()
        print("Server started")

    def send(self, payload: dict):
        if not self.clients:
            return
        msg = json.dumps(payload)
        asyncio.run_coroutine_threadsafe(self.broadcast(msg), self.loop)

    async def broadcast(self, message: str):
        to_remove = set()
        for client in self.clients:
            try:
                await client.send(message)
            except Exception:
                to_remove.add(client)
        self.clients.difference_update(to_remove)

    def stop(self):
        if self.server:
            self.server.close()
            print("Server closed")
        if self.loop:
            self.loop.stop()
