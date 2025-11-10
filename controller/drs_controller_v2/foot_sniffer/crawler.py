import os
from typing import Literal

PopIdx = Literal[-1, 0]

default_ignore_list = ['.vscode', '__pycache__']

def crawl(fname: str, pop_idx: PopIdx = -1, ignore: list[str] = default_ignore_list):
    """
    Crawls cwd, returns path to first file/dir matching `fname`, `None` if none is found.
    
    `pop_idx=0` for BFS, `pop_idx=-1` for DFS
    
    skips items in `ignore` (useful for avoiding redundant folders like `.vscode/`)
    """
    cwd = os.getcwd()
    search_list = [cwd]

    while search_list:
        path = search_list.pop(pop_idx)

        for item in os.listdir(path):
            if item == fname:
                return os.path.join(path, item)
            
            if item not in ignore and os.path.isdir(item_path := os.path.join(path, item)):
                search_list.append(item_path)

    return None

if __name__ == '__main__':
    crawl(fname='crawler.py', pop_idx=0, ignore=[])