# DRS-clone

## Controller environment

The controller-project uses `uv` to manage dependencies. This is how you use it:

```bash
cd controller
uv venv
uv pip sync requirements.lock
uv run -m controller_version
```

It can be downloaded here: [https://docs.astral.sh/uv/](https://docs.astral.sh/uv/)

To lock a new requirements file from the `.txt` file, run:
```bash
uv pip compile requirements.txt -o requirements.lock
```

## DRS Game 
The game is built using Godot Engine - .NET (4.5) and as such it is required to test the code. 
Windows is recomended as that is what this guide is ment for.

#### Installing Godot
1. Install godot Godot Engine - .NET (4.5) from here: https://godotengine.org/download/windows/

2. Unzip it and launch "Godot_v4.5-stable_mono_win64.exe". 

3. That opens up a window to select a project. Should be empt if you have never worked with Godot before.

#### Copying and running the game
1. Clone the git repo with everything inside it to some place on your computer.

2. Go to the project select window in godot and click on "import" in the top right corner.

3. Find the project and open it. 

4. If the project does not launch immediatly, select it in the window and click "Edit"

5. After the editor launches click on the play button in the top left to launch the game in the editor. NOTE: Only run the game after the controller is already running or it wont work. 

## Playing the game
1. Ensure the camera is capturing your whole body or just your feet from the waist down. All that is important is that your feet are in frame.

2. Stand back 1-2 meters from the camera.

3. Step around making clear steps. Tiny taps may not be recognized as of yet. You should be able to see the blue blocks move around. They are blue when the foot is off the ground and red when the foot is stepping. 
