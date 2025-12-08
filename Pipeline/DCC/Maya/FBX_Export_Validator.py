# =======================================================================
# 🛡️ ZZZ-Pipeline Module B: Asset Validator & Exporter (V3.3)
# -----------------------------------------------------------------------
# [版本更新 / Version Log]
# - V3.3: UI 国际化 (English UI) 以解决 Windows 编码乱码问题
# - V3.2: 集成真实 FBX 导出命令
# - V3.1: 修复对象类型检测 Bug (isType)
# =======================================================================

import maya.cmds as cmds
import maya.mel as mel
import os
import datetime
import uuid
import getpass

# --- ⚙️ Global Configuration (全局配置) ---
REQUIRED_PREFIXES = ["SK_", "SM_"]
MAX_POLYCOUNT = 50000

# 📝 Log Path (日志路径)
LOG_FILE_PATH = r"D:/ZZZ_Pipeline_Log.txt"
# 📂 Export Path (导出路径)
EXPORT_FOLDER = r"D:/ZZZ_Project_Exports"


def run_export_validation():
    """
    Main Entry Point (主入口函数)
    """
    print("\n" + "=" * 60)
    print("--- [ZZZ Pipeline] Starting Asset Validation Sequence... ---")

    # 1. Get Selection (获取选中)
    selection = cmds.ls(selection=True, long=True)

    # 2. Check Selection (基础检查)
    if not selection:
        _show_error_dialog("No Selection", "Please select objects to export first!")
        return

    # 3. Check Naming (命名检查)
    is_naming_valid, error_message = _validate_naming(selection)
    if not is_naming_valid:
        _show_error_dialog("Naming Violation", error_message)
        return

    # 4. Check Polycount (面数检查)
    is_polycount_valid, error_message = _validate_polycount(selection)
    if not is_polycount_valid:
        # Allow force export but log it (允许强行导出，但记录日志)
        response = cmds.confirmDialog(
            title="[ZZZ Performance Warning]",
            message=f"{error_message}\n\nDo you want to FORCE EXPORT?\n(This violation will be logged)",
            button=['Force Export', 'Cancel'],
            defaultButton='Cancel',
            cancelButton='Cancel',
            dismissString='Cancel'
        )
        if response == 'Cancel':
            print("--- [ZZZ Pipeline] Export Cancelled by User. ---")
            return

    # =================================================================
    # 🚀 5. Real Export Execution (真实导出阶段)
    # =================================================================

    # A. Data Prep (准备数据)
    asset_uid = str(uuid.uuid4())
    operator_name = getpass.getuser()
    # Get asset name from the first selected object
    asset_name = selection[0].split('|')[-1]

    # B. Path Prep (准备路径)
    if not os.path.exists(EXPORT_FOLDER):
        os.makedirs(EXPORT_FOLDER)

    final_export_path = os.path.join(EXPORT_FOLDER, f"{asset_name}.fbx")
    final_export_path = final_export_path.replace("\\", "/")  # Path fix

    # C. Execute Export (执行导出)
    try:
        # Load FBX plugin if needed
        if not cmds.pluginInfo('fbxmaya', query=True, loaded=True):
            cmds.loadPlugin('fbxmaya')

        print(f"--- [ZZZ IO] Exporting to: {final_export_path} ...")

        # Core Export Command
        cmds.file(final_export_path, force=True, options="v=0;", type="FBX export", exportSelected=True)

        # D. Write Log (写入日志)
        _write_security_log(asset_name, asset_uid, operator_name, is_polycount_valid, final_export_path)

        # E. Success Dialog (成功弹窗)
        _show_success_dialog(
            "Export Successful",
            f"✅ Asset Exported Successfully!\n\n📂 Path: {final_export_path}\n🔑 UUID: {asset_uid}"
        )
        print(f"--- [ZZZ IO] Export Complete. UUID: {asset_uid} ---")

    except Exception as e:
        _show_error_dialog("Export Failed", f"Maya Export Command Failed:\n{e}")
        print(f"--- [ZZZ Error] {e} ---")

    print("=" * 60 + "\n")


def _validate_naming(objects):
    """ Validate Naming Convention (检查命名规范) """
    for obj in objects:
        short_name = obj.split('|')[-1]
        if not any(short_name.startswith(prefix) for prefix in REQUIRED_PREFIXES):
            return (False, f'Object "{short_name}" violates naming convention!\nMust start with: {REQUIRED_PREFIXES}')
    return (True, "")


def _validate_polycount(objects):
    """ Validate Polycount (检查面数) """
    total_faces = 0
    all_meshes = []

    # Find all mesh children
    children = cmds.listRelatives(objects, allDescendents=True, type='mesh', fullPath=True)
    if children:
        all_meshes.extend(children)

    # Check roots
    for obj in objects:
        if cmds.objectType(obj, isType='mesh'):
            all_meshes.append(obj)

    # Calculate unique faces
    for mesh in set(all_meshes):
        if cmds.objExists(mesh):
            total_faces += cmds.polyEvaluate(mesh, face=True)

    if total_faces > MAX_POLYCOUNT:
        return (False, f'Total Polycount ({total_faces}) exceeds budget ({MAX_POLYCOUNT})!')

    return (True, "")


def _write_security_log(asset_name, uid, user, is_clean, path):
    """ Write to local log file (写入本地日志) """
    timestamp = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    status = "CLEAN" if is_clean else "WARNING_OVERRIDE"
    log_entry = f"[{timestamp}] | {status} | User:{user} | Asset:{asset_name} | Path:{path} | UUID:{uid}\n"

    try:
        with open(LOG_FILE_PATH, 'a', encoding='utf-8') as f:
            f.write(log_entry)
        return True
    except:
        return False


def _show_error_dialog(title, message):
    cmds.confirmDialog(title=f'[ZZZ Error] {title}', message=f'❌ {message}', button=['OK'])


def _show_success_dialog(title, message):
    cmds.confirmDialog(title=f'[ZZZ Success] {title}', message=message, button=['Done'])


# =================================================================
# 👇 Auto Execution (自动执行入口)
# =================================================================
if __name__ == "__main__":
    run_export_validation()