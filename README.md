# DRS-clone

## Controller environment

The controller-project uses `uv` to manage dependencies. This is how you use it:

```bash
cd controller
uv venv
uv pip sync requirements.lock
uv run -m controller_version
```

To lock a new requirements file from the `.txt` file, run:
```bash
uv pip compile requirements.txt -o requirements.lock
```
