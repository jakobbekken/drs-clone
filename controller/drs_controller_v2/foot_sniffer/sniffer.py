import cv2
import numpy as np
from typing import Optional, Callable
from functools import cache
from collections import deque
import time

class FootSniffer:
    def __init__(self, cap_source: Optional[int|str] = None, hook: Optional[Callable] = None) -> None:
        self.cap_source = cap_source
        self.cap = cv2.VideoCapture(cap_source) if cap_source is not None else None
        self.hook = hook

        if self.cap is not None:
            if not self.cap.isOpened():
                raise RuntimeError(f"Couldn't open video source: '{cap_source}'")
            
            # cap stats
            self.cap_fps = self.cap.get(cv2.CAP_PROP_FPS) or 30.0
            self.cap_delay = int(1000 / self.cap_fps)
            self.cap_dt = 1 / self.cap_fps
            self.cap_width = int(self.cap.get(cv2.CAP_PROP_FRAME_WIDTH))
            self.cap_height = int(self.cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
            self.start_time: Optional[float] = None

        # preprocessing
        self.resize_width: Optional[int] = None
        self.blur_sigma: float = 1.0
        self.use_clahe: bool = False
        self.clahe_clip: float = 2.0
        self.clahe_grid: int = 8
        self.gamma: Optional[float] = None  

        # Shi-Tomasi params
        self.corner_max = 10
        self.corner_quality = 0.3
        self.corner_min_dist = 10
        self.corner_block = 5
        
        # Lucas-Kanade params
        self.lk_win = 21
        self.lk_levels = 4
        self.lk_iters = 30
        self.lk_eps = 0.02
        self.reseed_thresh = 5

        # tracking state
        self.feet: Optional[tuple[int, int]] = None
        self.wait_for_movement: int = 5
        self.prev_frame: Optional[np.ndarray] = None
        self.points_prev: Optional[np.ndarray] = None
        self.bbox_xywh: Optional[tuple[int,int,int,int]] = None
        self.track_ids: Optional[np.ndarray] = None
        self.next_id: int = 0
        self.tracks: dict[int, dict] = {}
        self.static_thresh = 10.0
        self.max_from_origin_thresh = 30.0
        self.history_len = 50

    def _pick_feet(self):
        """Picks the two lowest (origin) points that have moved"""
        # return if feet already picked
        if self.feet is not None:
            return
        
        # let feet move before picking
        if self.start_time is None or time.time() - self.start_time < self.wait_for_movement:
            return
        
        candidates = []
        for tid, rec in self.tracks.items():
            if rec["alive"] and rec["moved"]:
                candidates.append((rec["origin"][1], tid))
        
        if len(candidates) < 2:
            return
        
        candidates.sort(reverse=True)  # low pos -> high y

        self.feet = (candidates[0][1], candidates[1][1])

    def _register_new_points(self, new_pts: np.ndarray):
        """Registers points to be tracked"""
        if new_pts is None or len(new_pts) == 0:
            return
        M = len(new_pts)
        new_ids = np.arange(self.next_id, self.next_id + M, dtype=np.int32)
        self.next_id += M

        # init track records
        pts_xy = new_pts.reshape(-1, 2)
        for tid, (x, y) in zip(new_ids, pts_xy):
            self.tracks[int(tid)] = {
                "path": deque([(float(x), float(y))], maxlen=self.history_len),
                "alive": True,
                "static_frames": 0,
                "moved_last": False,
                "moved_count": 0,
                "moved": False,
                "origin": (float(x), float(y)),
                "max_dist": 0.0
            }

        # append to existing arrays
        if self.points_prev is None:
            self.points_prev = new_pts.astype(np.float32)
            self.track_ids = new_ids
        else:
            self.points_prev = np.vstack([self.points_prev, new_pts]).astype(np.float32)
            self.track_ids = np.concatenate([self.track_ids, new_ids])  # type: ignore[arg-type]

    def get_tracks_status(self):
        """Returns all tracks"""
        out = []
        for tid, rec in self.tracks.items():
            last = rec["path"][-1] if rec["path"] else (None, None)
            out.append({
                "id": tid,
                "alive": rec["alive"],
                "last_x": last[0],
                "last_y": last[1],
                "moved": rec["moved"],
                "moved_last": rec.get("moved_last", False),
                "moved_count": rec.get("moved_count", 0),
                "total_dist": rec["total_dist"],
            })
        return out

    def get_track_pos(self, tid):
        """Get current position of given track"""
        return self.tracks[int(tid)]["path"][-1]

    def _shi_tomasi(self, frame: np.ndarray):
        """Key feature detector"""
        H, W = frame.shape

        if self.bbox_xywh is None:
            # bottom 40%
            y0 = int(0.6 * H)
            # middle 60%
            x0 = int(0.2 * W)
            x1 = int(0.8 * W)
            x = x0
            y = y0
            w = x1 - x0
            h = H - y0
            self.bbox_xywh = (x, y, w, h)

        x, y, w, h = self.bbox_xywh
        x = max(0, min(x, W - 1))
        y = max(0, min(y, H - 1))
        w = max(1, min(w, W - x))
        h = max(1, min(h, H - y))

        roi = frame[y:y+h, x:x+w]
        pts = cv2.goodFeaturesToTrack(
            roi,
            maxCorners=self.corner_max,
            qualityLevel=self.corner_quality,
            minDistance=self.corner_min_dist,
            blockSize=self.corner_block,
            useHarrisDetector=False
        )
        if pts is None:
            return None
        
        pts[:, 0, 0] += x
        pts[:, 0, 1] += y
        return pts.astype(np.float32)

    def _lucas_kanade(self, frame: np.ndarray):
        """Key point optical flow tracker"""
        criteria = (cv2.TERM_CRITERIA_EPS | cv2.TERM_CRITERIA_COUNT, self.lk_iters, self.lk_eps)
        p_curr, tracked, err = cv2.calcOpticalFlowPyrLK(    # type: ignore
            self.prev_frame, frame,                         # type: ignore
            self.points_prev, None,                         # type: ignore
            winSize=(self.lk_win, self.lk_win),
            maxLevel=self.lk_levels,
            criteria=criteria,
            flags=0
        )
        return p_curr, tracked, err

    def _draw_points(self, img: np.ndarray, pts: Optional[np.ndarray], color=(0, 0, 255), color_moved=(0,255,0)):
        if pts is None or self.track_ids is None:
            return
        pts_xy = pts.reshape(-1, 2)
        for (x, y), tid in zip(pts_xy, self.track_ids):
            rec = self.tracks.get(int(tid))
            if rec is None:
                continue
            c = color_moved if rec.get("moved", False) else color
            cv2.circle(img, (int(x), int(y)), 2, c, -1)

    def _draw_trails(self, img: np.ndarray, color=(0, 200, 255)):
        for _, rec in self.tracks.items():
            pts = rec["path"]
            for i in range(1, len(pts)):
                p0 = (int(pts[i-1][0]), int(pts[i-1][1]))
                p1 = (int(pts[i][0]),  int(pts[i][1]))
                cv2.line(img, p0, p1, color, 1)


    def _resize(self, frame: np.ndarray):
        "Resizes frame if `resize_width` is specified"
        if self.resize_width is None:
            return frame
        height, width = frame.shape[:2]
        if width == self.resize_width:
            return frame
        scale = self.resize_width / width
        new_size = (self.resize_width, round(height * scale))
        return cv2.resize(frame, new_size)
    
    @staticmethod
    @cache
    def _LUT(gamma):
        "Construct LUT for gamma changes"
        g = 1 / gamma
        return np.array([((i / 255) ** g) * 255 for i in range(256)], dtype=np.uint8)

    def _gamma(self, frame: np.ndarray):
        "Corrects frame gamma if `gamma` is specified"
        if self.gamma is None or self.gamma <= 0:
            return frame
        return cv2.LUT(frame, self._LUT(self.gamma))
    
    @staticmethod
    @cache
    def _create_CLAHE(clip_limit, grid_size):
        "Creates CLAHE instance"
        return cv2.createCLAHE(clipLimit=clip_limit, tileGridSize=(grid_size, grid_size))

    def _CLAHE(self, frame):
        "Applies CLAHE to improve contrast in frame if `use_clahe` is `True`"
        if not self.use_clahe:
            return frame
        return self._create_CLAHE(self.clahe_clip, self.clahe_grid).apply(frame)
    
    def _blur(self, frame):
        "Applies Gaussian blur to frame if `blur_sigma` is specified and greater than zero"
        if not self.blur_sigma or self.blur_sigma <= 0:
            return frame
        k = max(1, int(2 * np.ceil(3 * self.blur_sigma) + 1))
        return cv2.GaussianBlur(frame, (k, k), self.blur_sigma)

    def preprocess(self, frame: np.ndarray):
        "resize -> grayscale -> gamma -> clahe -> blur"
        frame = self._resize(frame) # optional
        frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        frame = self._gamma(frame)  # optional
        frame = self._CLAHE(frame)  # optional
        frame = self._blur(frame)   # optional
        return frame

    def read_frame(self) -> tuple[bool, Optional[np.ndarray]]:
        """Reads single frame from cap"""
        if self.cap is None: return False, None
        ok, frame = self.cap.read()
        if not ok:
            if isinstance(self.cap_source, str):
                self.cap.set(cv2.CAP_PROP_POS_FRAMES, 0)
                ok, frame = self.cap.read()
                if not ok:
                    return False, None
            else:
                return False, None
        return True, frame

    def process_frame(self, frame: np.ndarray) -> Optional[dict]:
        """Processes single frame"""
        if not self.start_time:
            self.start_time = time.time()

        # preprocess
        gray = self.preprocess(frame)

        # initial frame
        if self.prev_frame is None:
            self.prev_frame = gray.copy()
        
        # Shi-Tomasi feature detection
        if self.points_prev is None or len(self.points_prev) == 0:
            self._register_new_points(self._shi_tomasi(gray)) # type: ignore

        # Lucas-Kanade feature tracking + tracking logic
        if self.points_prev is not None and len(self.points_prev) > 0:
            p_curr, tracked, err = self._lucas_kanade(gray)
            
            mask = (tracked[:, 0] == 1)
            good_prev = self.points_prev[mask].reshape(-1, 2).astype(np.float32)
            good_curr = p_curr[mask].reshape(-1, 2).astype(np.float32)
            ids_alive = self.track_ids[mask] if self.track_ids is not None else None

            disp = np.linalg.norm(good_curr - good_prev, axis=1) if len(good_prev) else np.array([])
            moved = disp >= self.static_thresh if len(good_prev) else np.array([], dtype=bool)

            if ids_alive is not None:
                seen_now = set(int(t) for t in ids_alive)
                for tid, rec in self.tracks.items():
                    if rec["alive"] and tid not in seen_now:
                        rec["alive"] = False
                
                for tid, p0, p1, mv, d in zip(ids_alive, good_prev, good_curr, moved, disp):
                    rec = self.tracks[int(tid)]
                    rec["path"].append((float(p1[0]), float(p1[1])))

                    rec["moved_last"] = bool(mv)
                    if mv:
                        rec["moved_count"] += 1
                        rec["static_frames"] = 0
                    else:
                        rec["static_frames"] += 1

                    ox, oy = rec["origin"]
                    dist_from_origin = float(np.hypot(p1[0] - ox, p1[1] - oy))
                    if dist_from_origin > rec["max_dist"]:
                        rec["max_dist"] = dist_from_origin
                    
                    if rec["max_dist"] >= self.max_from_origin_thresh:
                        rec["moved"] = True
                
                    rec["alive"] = True

            self.points_prev = good_curr.reshape(-1, 1, 2).astype(np.float32)
            self.track_ids   = ids_alive.astype(np.int32) if ids_alive is not None else None

            if self.points_prev is None or len(self.points_prev) < self.reseed_thresh:
                self._register_new_points(self._shi_tomasi(gray)) # type: ignore

        self.prev_frame = gray.copy()

        self._pick_feet()

        if self.feet is not None:
            left = self.get_track_pos(self.feet[0])   # not actually left
            right = self.get_track_pos(self.feet[1])  # not actually right
            return {'left': {'x': left[0], 'y': left[1]}, 'right': {'x': right[0], 'y': right[1]}}
        return None

    def run(self, display: bool = True):  # NOTE: might have to tweak the exit conditions for usability
        "Continuously reads and processes frames until ESC is pressed"
        self.start_time = time.time()
        while True:
            ok, frame = self.read_frame()
            if not ok or frame is None:
                break

            img = frame.copy()

            self.process_frame(frame)

            if self.feet is not None and callable(self.hook):
                feet_pos = (self.get_track_pos(self.feet[0]), self.get_track_pos(self.feet[1]))
                self.hook(*np.array(feet_pos))

            if display:
                self._draw_trails(img)
                self._draw_points(img, self.points_prev)
                cv2.imshow('Foot Sniffer', img)
                if cv2.waitKey(self.cap_delay) & 0xFF == 27:
                    break
        
        if self.cap is not None:
            self.cap.release()
            cv2.destroyAllWindows()