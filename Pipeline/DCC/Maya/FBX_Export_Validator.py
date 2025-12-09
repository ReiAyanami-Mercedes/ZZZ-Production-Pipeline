# =======================================================================
# 🛡️ ZZZ-Pipeline Module B: 资产校验与溯源系统 (V3.4 架构师版)
# -----------------------------------------------------------------------
# [核心升级]
# 1. 原子性 (Atomicity): 失败即回滚，绝不残留脏数据。
# 2. 上下文 (Context): 生成 .json 元数据文件，记录身世。
# 3. 溯源 (Traceability): 记录原始 Maya 场景路径，方便 Unity 反查。
# =======================================================================

import maya.cmds as cmds
import os
import json
import datetime
import uuid
import getpass

# --- 全局配置 ---
REQUIRED_PREFIXES = ["SK_", "SM_"]
MAX_POLYCOUNT = 50000
LOG_FILE_PATH = r"D:/ZZZ_Pipeline_Log.txt"
EXPORT_FOLDER = r"D:/ZZZ_Project_Exports"


def run_export_validation():
    print("\n" + "=" * 60)
    print("--- [ZZZ Pipeline] 启动全链路导出程序... ---")

    selection = cmds.ls(selection=True, long=True)
    if not selection:
        _show_error_dialog("No Selection", "Please select objects to export first!")
        return

    is_naming_valid, name_err = _validate_naming(selection)
    if not is_naming_valid:
        _show_error_dialog("Naming Violation", name_err)
        return

    is_polycount_valid, poly_err, poly_count = _validate_polycount(selection)
    if not is_polycount_valid:
        if cmds.confirmDialog(title="Performance Warning", message=f"{poly_err}\n\nForce Export?", button=['Yes', 'No'],
                              defaultButton='No') == 'No':
            return

    # --- 原子性操作 & 上下文生成 ---

    asset_name = selection[0].split('|')[-1]

    if not os.path.exists(EXPORT_FOLDER):
        os.makedirs(EXPORT_FOLDER)

    fbx_path = os.path.join(EXPORT_FOLDER, f"{asset_name}.fbx").replace("\\", "/")
    json_path = os.path.join(EXPORT_FOLDER, f"{asset_name}.meta.json").replace("\\", "/")  # 伴生文件

    original_scene_path = cmds.file(q=True, sceneName=True) or "Unsaved_Scene"

    metadata = {
        "asset_uid": str(uuid.uuid4()),
        "asset_name": asset_name,
        "author": getpass.getuser(),
        "export_time": datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
        "source_maya_file": original_scene_path,  # 关键：回溯之线
        "polycount": poly_count,
        "is_force_exported": not is_polycount_valid
    }

    try:
        if not cmds.pluginInfo('fbxmaya', query=True, loaded=True):
            cmds.loadPlugin('fbxmaya')

        print(f"--- [ZZZ IO] Exporting FBX to: {fbx_path} ...")
        cmds.file(fbx_path, force=True, options="v=0;", type="FBX export", exportSelected=True)

        print(f"--- [ZZZ IO] Generating Metadata to: {json_path} ...")
        with open(json_path, 'w', encoding='utf-8') as f:
            json.dump(metadata, f, indent=4, ensure_ascii=False)

        _write_security_log(metadata)
        _show_success_dialog("Export Complete",
                             f"✅ Asset & Metadata Generated!\n\nSource: {os.path.basename(original_scene_path)}")

    except Exception as e:
        print(f"--- [ZZZ ERROR] Rolling back due to exception... ---")
        if os.path.exists(fbx_path): os.remove(fbx_path)
        if os.path.exists(json_path): os.remove(json_path)
        _show_error_dialog("Export Failed", f"A critical error occurred. Rollback executed.\n\nError: {e}")

    print("=" * 60 + "\n")


def _validate_naming(objects):
    for obj in objects:
        short_name = obj.split('|')[-1]
        if not any(short_name.startswith(prefix) for prefix in REQUIRED_PREFIXES):
            return (False, f'Object "{short_name}" violates naming convention!')
    return (True, "")


def _validate_polycount(objects):
    total_faces = 0
    all_meshes = cmds.listRelatives(objects, allDescendents=True, type='mesh', fullPath=True) or []
    for obj in objects:
        if cmds.objectType(obj, isType='mesh'): all_meshes.append(obj)

    for mesh in set(all_meshes):
        if cmds.objExists(mesh): total_faces += cmds.polyEvaluate(mesh, face=True)

    if total_faces > MAX_POLYCOUNT:
        return (False, f'Polycount ({total_faces}) exceeds budget ({MAX_POLYCOUNT})!', total_faces)

    return (True, "", total_faces)


def _write_security_log(meta):
    log_entry = f"[{meta['export_time']}] | USER:{meta['author']} | ASSET:{meta['asset_name']} | SOURCE:{meta['source_maya_file']} | UUID:{meta['asset_uid']}\n"
    try:
        with open(LOG_FILE_PATH, 'a', encoding='utf-8') as f:
            f.write(log_entry)
    except:
        pass


def _show_error_dialog(title, msg):
    cmds.confirmDialog(title=f'[ZZZ Error] {title}', message=f'❌ {msg}', button=['OK'])


def _show_success_dialog(title, msg):
    cmds.confirmDialog(title=f'[ZZZ Success] {title}', message=msg, button=['OK'])


if __name__ == "__main__":
    run_export_validation()