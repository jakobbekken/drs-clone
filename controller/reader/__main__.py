import asyncio
import json

import websockets


async def listen():
    uri = "ws://127.0.0.1:6969"
    async with websockets.connect(uri) as ws:
        print("Connected to controller!")
        async for message in ws:
            try:
                msg = json.loads(message)
                print("Received:", msg)
            except json.JSONDecodeError:
                print("Bad JSON:", message)


asyncio.run(listen())
