#!/usr/bin/env bash

cd controller
uv venv
uv pip sync requirements.lock
uv run -m drs_controller_v3
