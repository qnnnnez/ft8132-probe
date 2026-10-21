"""Elevated FT8132Debugger pass.

Default: Find, then Download-tab GenCode / Erase / Write / Verify.
Pass --debug for Find / Exit I2C / Find / Debug / Stop instead.

    uv run --python 3.12 --with pywinauto --with pillow python win_gui_probe.py
    uv run --python 3.12 --with pywinauto --with pillow python win_gui_probe.py --debug
"""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
import time
from pathlib import Path

EXE = Path(r"C:\ft8132\FT8132Debugger_V1.0.6\FT8132Debugger.exe")
OUT_DIR = Path(r"C:\Users\ft8132\ft8132-gui")
TITLE_RE = r".*FT8132Debugger.*"

# XAML names: FT8215Debugger.MainWindow.xaml
BTN = {
    "find": ("btn_ReadID", "Find"),
    "exit_i2c": ("btn_WriteID", "Exit I2C"),
    "debug": ("btnDebug", "Debug"),
    "stop": ("btnStartBw", "Stop"),
    "gencode": ("btn_Generate", "GenCode"),
    "erase": ("btn_Erase", "Erase"),
    "write": ("btn_WriteEfuse", "Write"),
    "verify": ("btn_Verify", "Verify"),
}


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


def _field(texts: list[dict[str, str]], auto_id: str) -> str | None:
    for row in texts:
        if row.get("auto_id") == auto_id:
            return row.get("text") or row.get("name") or ""
    return None


def _status_blob(texts: list[dict[str, str]]) -> str:
    return " ".join((row["text"] or row["name"]) for row in texts)


def _connected(texts: list[dict[str, str]]) -> bool:
    blob = _status_blob(texts)
    if "Disconnected" in blob and "Connected" not in blob.replace("Disconnected", ""):
        return False
    return "Connected" in blob and "Disconnected" not in blob


def _dismiss_dialogs() -> None:
    from pywinauto import Desktop
    from pywinauto.keyboard import send_keys

    try:
        desk = Desktop(backend="uia")
        for w in desk.windows():
            title = (w.window_text() or "").strip()
            if title in {"Warning", "CF", "CF Comm", "SMF", "Error"}:
                clicked = False
                for btn_title in ("OK", "确定", "Cancel", "取消"):
                    try:
                        w.child_window(title=btn_title, control_type="Button").click_input()
                        clicked = True
                        break
                    except Exception:
                        continue
                if not clicked:
                    send_keys("{ENTER}")
    except Exception:
        send_keys("{ESC}")


def _button(win, key: str):
    auto_id, title = BTN[key]
    btn = win.child_window(auto_id=auto_id, control_type="Button")
    try:
        if btn.exists(timeout=2):
            return btn
    except Exception:
        pass
    return win.child_window(title=title, control_type="Button")


def _shot(win, name: str) -> str:
    path = OUT_DIR / name
    win.capture_as_image().save(path)
    return str(path)


def _snapshot(win, step: str, extra: dict | None = None) -> dict:
    texts = _dump_texts(win)
    rec = {
        "step": step,
        "chip_id": _field(texts, "textBox_SetChipID"),
        "i2c_addr": _field(texts, "textBox_SetI2CAddr"),
        "motor_status": _field(texts, "textBox_MotorStatus"),
        "idle_label": _field(texts, "lblMessage"),
        "target_speed": _field(texts, "textBox_TargetSpeed"),
        "connected": _connected(texts),
        "status_words": [
            row["text"] or row["name"]
            for row in texts
            if (row["text"] or row["name"])
            in {"Disconnected", "Connected", "Connecting", "Debugging...", "Device:"}
            or "Connected" in (row["text"] or "")
            or "Disconnect" in (row["text"] or "")
            or "Debugging" in (row["text"] or "")
            or (row["text"] or row["name"]) in {
                "IDLE",
                "FAULT",
                "PAUSE",
                "Pause...",
                "PASS",
                "FAIL",
                "Success",
                "Fail",
            }
        ],
        "code_file": _field(texts, "textBox_CodeFile"),
        "src_crc": _field(texts, "label_SrcCrc"),
        "tgt_crc": _field(texts, "label_TargetCrc"),
        "verify_status": _field(texts, "label_VerifyStatus"),
    }
    if extra:
        rec.update(extra)
    return rec


def _click(win, key: str, wait_s: float) -> dict:
    btn = _button(win, key)
    rec: dict = {"button": key, "enabled": None, "error": None}
    try:
        rec["enabled"] = bool(btn.is_enabled())
    except Exception as exc:
        rec["error"] = f"enabled: {exc}"
        return rec
    try:
        btn.click_input()
    except Exception as exc:
        rec["error"] = f"click: {exc}"
        return rec
    time.sleep(wait_s)
    _dismiss_dialogs()
    return rec


def _select_download_tab(win) -> dict:
    rec: dict = {"error": None}
    tab = win.child_window(auto_id="tabItem_Download", control_type="TabItem")
    try:
        if not tab.exists(timeout=5):
            tab = win.child_window(title="Download", control_type="TabItem")
        try:
            tab.select()
        except Exception:
            tab.click_input()
        time.sleep(1)
    except Exception as exc:
        rec["error"] = f"Download tab: {exc}"
    return rec


def _enable_writing(win) -> dict:
    rec: dict = {"error": None, "checked": None}
    cb = win.child_window(auto_id="checkBox_EnableWritingMode", control_type="CheckBox")
    try:
        if not cb.exists(timeout=5):
            cb = win.child_window(title="Enable Writing Code Mode", control_type="CheckBox")
        try:
            rec["checked"] = bool(cb.get_toggle_state())
        except Exception:
            rec["checked"] = False
        if not rec["checked"]:
            cb.click_input()
            time.sleep(0.5)
            try:
                rec["checked"] = bool(cb.get_toggle_state())
            except Exception:
                rec["checked"] = True
    except Exception as exc:
        rec["error"] = f"writing-mode: {exc}"
    return rec


def _download_ok(snap: dict, *, want_pass: bool = False) -> bool:
    click = snap.get("click") or {}
    if click.get("error"):
        return False
    blob = " ".join(
        str(x)
        for x in [
            *(snap.get("status_words") or []),
            snap.get("verify_status"),
            snap.get("tgt_crc"),
        ]
        if x
    )
    if "FAIL" in blob.upper() and "PASS" not in blob.upper():
        return False
    if "Fail" in blob:
        return False
    if want_pass:
        vs = (snap.get("verify_status") or "").upper()
        return vs == "PASS" or "PASS" in blob.upper()
    return True


def run_download(win, result: dict) -> None:
    tab = _select_download_tab(win)
    snap = _snapshot(win, "download_tab", extra={"tab": tab, "src": "tabItem_Download"})
    snap["screenshot"] = _shot(win, "download_tab.png")
    snap["ok"] = tab.get("error") is None
    result["steps"]["download_tab"] = snap

    en = _enable_writing(win)
    snap = _snapshot(
        win, "enable_write", extra={"enable": en, "src": "checkBox_EnableWritingMode"}
    )
    snap["screenshot"] = _shot(win, "enable_write.png")
    snap["ok"] = en.get("error") is None and bool(en.get("checked"))
    result["steps"]["enable_write"] = snap

    click = _click(win, "gencode", 3)
    snap = _snapshot(win, "gencode", extra={"click": click, "src": "btn_Generate_Click ~6235"})
    snap["screenshot"] = _shot(win, "after_gencode.png")
    snap["ok"] = click.get("error") is None and bool(snap.get("code_file"))
    result["steps"]["gencode"] = snap

    click = _click(win, "erase", 15)
    snap = _snapshot(win, "erase", extra={"click": click, "src": "btn_Erase_Click ChipErase1 ~5894"})
    snap["screenshot"] = _shot(win, "after_erase.png")
    snap["ok"] = _download_ok(snap)
    result["steps"]["erase"] = snap

    click = _click(win, "write", 45)
    snap = _snapshot(win, "write", extra={"click": click, "src": "btn_WriteEfuse_Click ~5048"})
    snap["screenshot"] = _shot(win, "after_write.png")
    snap["ok"] = _download_ok(snap)
    result["steps"]["write"] = snap

    click = _click(win, "verify", 25)
    snap = _snapshot(win, "verify", extra={"click": click, "src": "btn_Verify_Click ~5484"})
    snap["screenshot"] = _shot(win, "after_verify.png")
    snap["ok"] = _download_ok(snap, want_pass=True)
    result["steps"]["verify"] = snap


def run_debug(win, result: dict) -> None:
    click = _click(win, "exit_i2c", 3)
    snap = _snapshot(win, "exit_i2c", extra={"click": click, "src": "btn_WriteID_Click ~7766"})
    snap["screenshot"] = _shot(win, "after_exit.png")
    snap["ok"] = click.get("error") is None
    result["steps"]["exit_i2c"] = snap

    click = _click(win, "find", 12)
    snap = _snapshot(
        win, "find_again", extra={"click": click, "src": "btn_ReadID_Click ~7824 after exit"}
    )
    snap["screenshot"] = _shot(win, "after_find2.png")
    snap["ok"] = bool(snap.get("connected")) and (
        (snap.get("chip_id") or "").upper() in {"0X02", "0x02"}
        or (snap.get("chip_id") or "").endswith("02")
    )
    result["steps"]["find_again"] = snap

    click = _click(win, "debug", 2.5)
    snap = _snapshot(win, "debug", extra={"click": click, "src": "btnDebug ExecuteDebugOpt ~4249"})
    snap["screenshot"] = _shot(win, "after_debug.png")
    snap["ok"] = click.get("error") is None
    snap["gauges_live"] = bool(
        (snap.get("idle_label") or "").upper() not in {"", "IDLE"}
        or (snap.get("motor_status") not in {None, "", "0", "0x00"})
    )
    result["steps"]["debug"] = snap

    click = _click(win, "stop", 3)
    snap = _snapshot(
        win, "stop", extra={"click": click, "src": "btnStartBw ExecuteStopOperation ~4541"}
    )
    snap["screenshot"] = _shot(win, "after_stop.png")
    snap["ok"] = click.get("error") is None
    result["steps"]["stop"] = snap


def main() -> int:
    from pywinauto import Application
    from pywinauto.keyboard import send_keys

    ap = argparse.ArgumentParser()
    ap.add_argument(
        "--debug",
        action="store_true",
        help="Find / Exit / Find / Debug / Stop (skip Download Erase/Write/Verify)",
    )
    args = ap.parse_args()

    OUT_DIR.mkdir(parents=True, exist_ok=True)
    result: dict = {
        "exe": str(EXE),
        "ok": False,
        "elevated": _is_elevated(),
        "mode": "debug" if args.debug else "download",
        "steps": {},
        "error": None,
        "log_tail": "",
    }

    if not result["elevated"]:
        result["error"] = "not High Mandatory (need S-1-16-12288)"
        (OUT_DIR / "probe_result.json").write_text(
            json.dumps(result, indent=2), encoding="utf-8"
        )
        print(json.dumps(result, indent=2))
        return 2

    if not EXE.is_file():
        result["error"] = f"missing {EXE}"
        (OUT_DIR / "probe_result.json").write_text(
            json.dumps(result, indent=2), encoding="utf-8"
        )
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
    send_keys("{ESC}")
    time.sleep(0.5)

    find = _button(win, "find")
    try:
        find.wait("exists", timeout=20)
    except Exception as exc:
        result["error"] = f"Find button: {exc}"
        result["texts"] = _dump_texts(win)
        _shot(win, "error.png")
        (OUT_DIR / "probe_result.json").write_text(
            json.dumps(result, indent=2), encoding="utf-8"
        )
        print(json.dumps({k: v for k, v in result.items() if k != "texts"}, indent=2))
        return 3

    deadline = time.time() + 45
    while time.time() < deadline:
        texts = _dump_texts(win)
        enabled = False
        try:
            enabled = bool(find.is_enabled())
        except Exception:
            pass
        if enabled or "Connected" in _status_blob(texts):
            break
        time.sleep(1)

    # Find
    click = _click(win, "find", 12)
    snap = _snapshot(win, "find", extra={"click": click, "src": "btn_ReadID_Click ~7824"})
    snap["screenshot"] = _shot(win, "after_find.png")
    snap["ok"] = bool(snap.get("connected")) and (
        (snap.get("chip_id") or "").upper() in {"0X02", "0x02"}
        or (snap.get("chip_id") or "").endswith("02")
    )
    result["steps"]["find"] = snap

    if args.debug:
        run_debug(win, result)
    else:
        run_download(win, result)

    log_path = EXE.parent / "log" / "info.log"
    if log_path.is_file():
        result["log_tail"] = log_path.read_text(encoding="utf-8", errors="replace")[-6000:]

    failed = [name for name, rec in result["steps"].items() if not rec.get("ok")]
    result["failed"] = failed
    result["ok"] = not failed and result["steps"].get("find", {}).get("ok") is True

    (OUT_DIR / "probe_result.json").write_text(
        json.dumps(result, indent=2), encoding="utf-8"
    )
    slim = {k: v for k, v in result.items() if k != "log_tail"}
    print(json.dumps(slim, indent=2))
    return 0 if result["ok"] else 4


if __name__ == "__main__":
    sys.exit(main())
