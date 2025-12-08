# =======================================================================
# 🛡️ ZZZ-Pipeline Module B: 资产校验与导出系统 (V3.2 最终执行版)
# -----------------------------------------------------------------------
# [功能清单]
# 1. 强制命名规范 (SK_/SM_)
# 2. 强制面数预算 (<50k)
# 3. 生成 UUID 数字指纹 (IP 保护)
# 4. 记录黑匣子日志 (Log)
# 5. 【新增】真实执行 FBX 导出 (不再是模拟)
# =======================================================================

import maya.cmds as cmds
import maya.mel as mel
import os
import datetime
import uuid
import getpass

# --- ⚙️ 全局配置 (请根据你的电脑修改路径) ---
REQUIRED_PREFIXES = ["SK_", "SM_"]
MAX_POLYCOUNT = 50000

# 📝 日志存哪里？
LOG_FILE_PATH = r"D:/ZZZ_Pipeline_Log.txt"
# 📂 模型导出的文件夹存哪里？(会自动创建)
EXPORT_FOLDER = r"D:/ZZZ_Project_Exports"


def run_export_validation():
    """
    主入口函数：点击按钮时执行
    """
    print("\n" + "=" * 50)
    print("--- [ZZZ Pipeline] 启动资产导出流程... ---")

    # 1. 获取选中对象
    selection = cmds.ls(selection=True, long=True)

    # 2. 基础检查
    if not selection:
        _show_error_dialog("未选择对象", "请先选择要导出的模型！")
        return

    # 3. 命名规范检查
    is_naming_valid, error_message = _validate_naming(selection)
    if not is_naming_valid:
        _show_error_dialog("命名违规", error_message)
        return

    # 4. 面数预算检查
    is_polycount_valid, error_message = _validate_polycount(selection)
    if not is_polycount_valid:
        # 允许强行导出，但会记录
        response = cmds.confirmDialog(
            title="[ZZZ 性能警告]",
            message=error_message + "\n\n是否强行导出？(违规操作将被记录)",
            button=['强行导出', '取消'],
            defaultButton='取消',
            cancelButton='取消',
            dismissString='取消'
        )
        if response == '取消':
            print("--- [ZZZ Pipeline] 导出已取消 ---")
            return

    # =================================================================
    # 🚀 5. 真实导出阶段 (Real Export Execution)
    # =================================================================

    # A. 准备数据
    asset_uid = str(uuid.uuid4())
    operator_name = getpass.getuser()
    # 取第一个物体的名字作为文件名
    asset_name = selection[0].split('|')[-1]

    # B. 准备路径
    if not os.path.exists(EXPORT_FOLDER):
        os.makedirs(EXPORT_FOLDER)

    # 最终文件路径
    final_export_path = os.path.join(EXPORT_FOLDER, f"{asset_name}.fbx")
    # 统一路径斜杠 (防止 Windows/Mac 路径报错)
    final_export_path = final_export_path.replace("\\", "/")

    # C. 执行导出命令
    try:
        # 确保 FBX 插件已加载
        if not cmds.pluginInfo('fbxmaya', query=True, loaded=True):
            cmds.loadPlugin('fbxmaya')

        print(f"--- [ZZZ IO] 正在导出到: {final_export_path} ...")

        # 核心导出指令：
        # -v=0: 关闭详细日志
        # -exportSelected: 只导出选中的
        cmds.file(final_export_path, force=True, options="v=0;", type="FBX export", exportSelected=True)

        # D. 写入日志 (导出成功后才记)
        _write_security_log(asset_name, asset_uid, operator_name, is_polycount_valid, final_export_path)

        # E. 成功弹窗
        _show_success_dialog(
            "导出成功 (Success)",
            f"✅ 资产已落地！\n\n📂 路径: {final_export_path}\n🔑 UUID: {asset_uid}"
        )
        print(f"--- [ZZZ IO] 导出完成。UUID: {asset_uid} ---")

    except Exception as e:
        _show_error_dialog("导出崩溃", f"Maya 导出命令执行失败：\n{e}")
        print(f"--- [ZZZ Error] {e} ---")

    print("=" * 50 + "\n")


def _validate_naming(objects):
    """检查命名规范"""
    for obj in objects:
        short_name = obj.split('|')[-1]
        if not any(short_name.startswith(prefix) for prefix in REQUIRED_PREFIXES):
            return (False, f'对象 "{short_name}" 命名不规范！\n必须以 {REQUIRED_PREFIXES} 开头')
    return (True, "")


def _validate_polycount(objects):
    """检查面数"""
    total_faces = 0
    all_meshes = []
    children = cmds.listRelatives(objects, allDescendents=True, type='mesh', fullPath=True)
    if children:
        all_meshes.extend(children)
    for obj in objects:
        if cmds.objectType(obj, isType='mesh'):
            all_meshes.append(obj)

    for mesh in set(all_meshes):
        if cmds.objExists(mesh):
            total_faces += cmds.polyEvaluate(mesh, face=True)

    if total_faces > MAX_POLYCOUNT:
        return (False, f'总面数 ({total_faces}) 超过预算 ({MAX_POLYCOUNT})！')

    return (True, "")


def _write_security_log(asset_name, uid, user, is_clean, path):
    """写入日志"""
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
    cmds.confirmDialog(title=f'[ZZZ 拦截] {title}', message=f'❌ {message}', button=['好的'])


def _show_success_dialog(title, message):
    cmds.confirmDialog(title=f'[ZZZ 放行] {title}', message=message, button=['完成'])


# =================================================================
# 👇 自动执行入口
# =================================================================
if __name__ == "__main__":
    run_export_validation()