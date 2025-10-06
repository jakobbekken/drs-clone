# DRS-clone

## Controller environment

The controller-project uses `uv` to manage dependencies. This is how you use it:

```bash
cd controller
uv venv
uv pip sync requirements.txt
uv run -m controller_version
```

To lock a new requirements file from the `.in` file, run:
```bash
uv pip compile requirements.in -o requirements.txt
```
