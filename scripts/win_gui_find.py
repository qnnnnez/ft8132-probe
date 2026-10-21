"""Drive official FT8132Debugger Find/Read ID on an interactive Windows session.

Run in the console/RDP session (not SSH session 0), elevated, e.g. a
Scheduled Task with LogonType Interactive and RunLevel HighestAvailable:

    uv run --python 3.12 --with pywinauto --with pillow python win_gui_find.py
"""

from __future__ import annotations

import json
import subprocess
import sys
import time
from pathlib import Path

EXE = Path(r"C:\ft8132\FT8132Debugger_V1.0.6\FT8132Debugger.exe")
OUT_DIR = Path(r"C:\Users\ft8132\ft8132-gui")
TITLE_RE = r".*FT8132Debugger.*"


def _is_elevated() -> bool:
    out = subprocess.run(
        ["whoami", "/groups"], capture_output=True, text=True, check=False
    )
    return "S-1-16-12288" in (out.stdout or "")


def _dump_texts(ctrl) -> list[dict[str, str]]:
    rows: list[dict[str, str]] = []

    def walk(c) -> None:
        try:
            info = c.element_info
            text = (c.window_text() or "").strip()
            name = (getattr(info, "name", None) or "").strip()
            auto_id = (getattr(info, "automation_id", None) or "").strip()
            ctype = (getattr(info, "control_type", None) or "").strip()
            if text or name or auto_id:
                rows.append(
                    {
                        "text": text,
                        "name": name,
                        "auto_id": auto_id,
                        "control_type": str(ctype),
                    }
                )
        except Exception:
            return
        try:
            children = c.children()
        except Exception:
            return
        for child in children:
            walk(child)

    walk(ctrl)
    return rows


def main() -> int:
    from pywinauto import Application
    from pywinauto.keyboard import send_keys

    OUT_DIR.mkdir(parents=True, exist_ok=True)
    result: dict[str, object] = {
        "exe": str(EXE),
        "ok": False,
        "find_enabled": None,
        "device_status": None,
        "texts": [],
        "log_tail": "",
        "error": None,
        "elevated": _is_elevated(),
    }

    if not EXE.is_file():
        result["error"] = f"missing {EXE}"
        (OUT_DIR / "result.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
        print(json.dumps(result, indent=2))
        return 2

    subprocess.run(
        ["taskkill", "/IM", "FT8132Debugger.exe", "/F"],
        capture_output=True,
        check=False,
    )
    time.sleep(1)

    cmd = f'"{EXE}" -senmode 2 -curtp 0'
    app = Application(backend="uia").start(cmd, work_dir=str(EXE.parent))
    win = app.window(title_re=TITLE_RE)
    win.wait("visible", timeout=60)
    time.sleep(3)
    try:
        win.set_focus()
    except Exception:
        pass

    # Dismiss leftover sensor-mode / UAC / AV dialogs if they stole focus.
    send_keys("{ESC}")
    time.sleep(0.5)

    find = win.child_window(title="Find", control_type="Button")
    try:
        find.wait("exists", timeout=20)
    except Exception as exc:
        result["error"] = f"Find button: {exc}"
        result["texts"] = _dump_texts(win)
        win.capture_as_image().save(OUT_DIR / "window.png")
        (OUT_DIR / "result.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
        print(json.dumps(result, indent=2))
        return 3

    # Device-arrival is easy to miss if the dongle was already plugged in.
    # Poll while the host hotplugs 1b3f:8000.
    deadline = time.time() + 45
    while time.time() < deadline:
        texts = _dump_texts(win)
        blob = " ".join((row["text"] or row["name"]) for row in texts)
        enabled = False
        try:
            enabled = bool(find.is_enabled())
        except Exception:
            pass
        result["find_enabled"] = enabled
        if enabled or "Connected" in blob:
            break
        time.sleep(1)

    try:
        find.click_input()
    except Exception as exc:
        result["error"] = f"Find click: {exc}"
    time.sleep(12)

    texts = _dump_texts(win)
    result["texts"] = texts
    status_words = [
        row["text"] or row["name"]
        for row in texts
        if (row["text"] or row["name"])
        in {"Disconnected", "Connected", "Connecting", "Device:"}
        or "Connected" in (row["text"] or "")
        or "Disconnect" in (row["text"] or "")
    ]
    result["device_status"] = status_words
    names = {(row["text"] or row["name"]).lower() for row in texts}
    result["ok"] = bool(result["find_enabled"]) and not any(
        "disconnect" in n for n in names
    )

    log_path = EXE.parent / "log" / "info.log"
    if log_path.is_file():
        result["log_tail"] = log_path.read_text(encoding="utf-8", errors="replace")[-4000:]

    win.capture_as_image().save(OUT_DIR / "window.png")
    (OUT_DIR / "result.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps({k: v for k, v in result.items() if k != "texts"}, indent=2))
    print("--- texts ---")
    print(json.dumps(texts[:80], indent=2))
    return 0 if result["ok"] else 4


if __name__ == "__main__":
    sys.exit(main())
