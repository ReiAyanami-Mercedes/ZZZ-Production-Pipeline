# =======================================================================
# 🌌 ZZZ-Pipeline Module B: Maya 资产校验与导出工具 (V2.0)
# -----------------------------------------------------------------------
# 核心功能：
# 1. 强制命名规范 (SK_ / SM_)
# 2. 强制面数预算 (Polycount Budget)
# 3. 提供清晰的 Debug 信息和用户弹窗
# =======================================================================

import maya.cmds as cmds

# --- 全局配置 (以后可以在这里统一修改规则) ---
REQUIRED_PREFIXES = ["SK_", "SM_"]
MAX_POLYCOUNT = 50000


def run_export_validation():
    """
    这是我们将要绑定到按钮上的主函数。
    它负责调用核心的校验逻辑。
    """
    print("--- [ZZZ Pipeline] 资产海关：启动导出前校验流程... ---")

    # 1. 获取当前选中的所有对象
    selection = cmds.ls(selection=True, long=True)

    # 2. 【第一道关卡】检查是否有选中物体
    if not selection:
        _show_error_dialog("未选择对象", "请先在场景中选择您要导出的模型！")
        return  # 直接中断流程

    # 3. 【第二道关卡】检查命名规范
    is_naming_valid, error_message = _validate_naming(selection)
    if not is_naming_valid:
        _show_error_dialog("命名规范错误", error_message)
        return

    # 4. 【第三道关卡】检查面数预算
    is_polycount_valid, error_message = _validate_polycount(selection)
    if not is_polycount_valid:
        # 对于面数超标，我们可以给用户一个选择
        response = cmds.confirmDialog(
            title="[ZZZ 性能警告]",
            message=error_message + "\n\n是否仍然继续导出？",
            button=['继续导出', '取消'],
            defaultButton='取消',
            cancelButton='取消',
            dismissString='取消'
        )
        if response == '取消':
            print("--- [ZZZ Pipeline] 用户取消了导出。 ---")
            return

    # 5. 【最终放行】如果所有检查都通过了
    _show_success_dialog("校验通过", "恭喜！您的资产符合所有规范，可以进行导出了！")

    # 在这里，未来我们会加入真正的 FBX 导出代码
    # For now, we just print a success message.
    print("--- [ZZZ Pipeline] 所有校验通过！未来将在这里执行 FBX 导出。 ---")


def _validate_naming(objects):
    """
    内部函数：检查所有选中的根节点的命名。
    返回 (bool, str) -> (是否通过, 错误信息)
    """
    for obj in objects:
        # 获取物体的短名 (去掉路径)
        short_name = obj.split('|')[-1]

        # 检查名字是否以任何一个合法前缀开头
        if not any(short_name.startswith(prefix) for prefix in REQUIRED_PREFIXES):
            error_msg = f'对象 "{short_name}" 命名不规范！\n\n必须以下列前缀之一开头: {", ".join(REQUIRED_PREFIXES)}'
            return (False, error_msg)

    return (True, "")  # 全部通过


def _validate_polycount(objects):
    """
    内部函数：计算总面数并检查是否超标。
    返回 (bool, str) -> (是否通过, 错误/警告信息)
    """
    total_faces = 0
    # 遍历所有选中的物体及其子物体，计算总面数
    all_meshes = cmds.listRelatives(objects, allDescendents=True, type='mesh', fullPath=True) or []
    # 加上根节点自身（如果也是 mesh）
    for obj in objects:
        if cmds.objectType(obj, isA='mesh'):
            all_meshes.append(obj)

    for mesh in set(all_meshes):  # 用 set 去重
        total_faces += cmds.polyEvaluate(mesh, face=True)

    if total_faces > MAX_POLYCOUNT:
        error_msg = f'总面数 ({total_faces}) 已超过项目预算 ({MAX_POLYCOUNT})！\n\n请优化模型，或与技术总监确认。'
        return (False, error_msg)

    print(f"--- [ZZZ Pipeline] 面数检查通过: {total_faces} / {MAX_POLYCOUNT} ---")
    return (True, "")


# --- 辅助函数：封装弹窗，让代码更干净 ---

def _show_error_dialog(title, message):
    """显示一个标准的错误弹窗"""
    cmds.confirmDialog(title=f'[ZZZ 错误] {title}', message=f'❌ {message}', button=['好的'])
    print(f"--- [ZZZ Pipeline] 校验失败: {message} ---")


def _show_success_dialog(title, message):
    """显示一个标准的成功弹窗"""
    cmds.confirmDialog(title=f'[ZZZ 成功] {title}', message=f'✅ {message}', button=['太棒了'])

# =======================================================================
# --- 如何在 Maya 中测试 ---
# 1. 打开 Script Editor (Python Tab)。
# 2. 粘贴以上全部代码。
# 3. 在场景中创建一个 Cube。
# 4. 在 Script Editor 的输入框中，输入并执行 `run_export_validation()`
# 5. 观察弹出的窗口和 Script Editor 打印的信息。
# =======================================================================