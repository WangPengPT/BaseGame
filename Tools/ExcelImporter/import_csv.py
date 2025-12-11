#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Unity Excel/CSV 导入工具（Python 版）
独立运行，避免 Unity 编译冲突
"""

import os
import sys
import csv
import re
import uuid
from pathlib import Path
from typing import List, Dict, Tuple, Optional


class CSVImporter:
    """CSV 导入工具"""
    
    C_SHARP_KEYWORDS = {
        'abstract', 'as', 'base', 'bool', 'break', 'byte', 'case', 'catch',
        'char', 'checked', 'class', 'const', 'continue', 'decimal', 'default',
        'delegate', 'do', 'double', 'else', 'enum', 'event', 'explicit',
        'extern', 'false', 'finally', 'fixed', 'float', 'for', 'foreach',
        'goto', 'if', 'implicit', 'in', 'int', 'interface', 'internal',
        'is', 'lock', 'long', 'namespace', 'new', 'null', 'object', 'operator',
        'out', 'override', 'params', 'private', 'protected', 'public', 'readonly',
        'ref', 'return', 'sbyte', 'sealed', 'short', 'sizeof', 'stackalloc',
        'static', 'string', 'struct', 'switch', 'this', 'throw', 'true',
        'try', 'typeof', 'uint', 'ulong', 'unchecked', 'unsafe', 'ushort',
        'using', 'virtual', 'void', 'volatile', 'while'
    }
    
    def __init__(self, document_path: str, unity_project_path: str, 
                 script_output: str = "Assets/Scripts/ExcelData",
                 asset_output: str = "Assets/Resources/ExcelData"):
        self.document_path = Path(document_path)
        self.unity_project_path = Path(unity_project_path)
        self.script_output = Path(unity_project_path) / script_output
        self.asset_output = Path(unity_project_path) / asset_output
        
    def sanitize_field_name(self, name: str) -> str:
        """清理字段名，转换为 PascalCase"""
        if not name:
            return "Field"
        
        # 移除空格和特殊字符
        sanitized = name.strip()
        parts = re.split(r'[\s_\-\.\(\)\[\]]+', sanitized)
        
        result = ""
        for part in parts:
            if part:
                result += part[0].upper() + part[1:].lower()
        
        if not result or not result[0].isalpha():
            result = "Field" + result
        
        # 检查 C# 关键字
        if result.lower() in self.C_SHARP_KEYWORDS:
            result = "@" + result
        
        return result
    
    def infer_field_type(self, rows: List[Dict[str, str]], header: str) -> str:
        """推断字段类型"""
        if not rows:
            return "string"
        
        all_int = True
        all_float = True
        all_bool = True
        
        for row in rows:
            raw_value = row.get(header, "")
            if raw_value is None:
                continue
            value = str(raw_value).strip()
            if not value:
                continue
            
            if all_int:
                try:
                    int(value)
                except ValueError:
                    all_int = False
            
            if all_float:
                try:
                    float(value)
                except ValueError:
                    all_float = False
            
            if all_bool:
                lower = value.lower()
                if lower not in ('true', 'false', '1', '0', 'yes', 'no'):
                    all_bool = False
        
        # 检查是否有非空值
        has_value = any(
            str(row.get(header, "") or "").strip() 
            for row in rows 
            if row.get(header) is not None
        )
        
        if all_bool and has_value:
            return "bool"
        if all_int:
            return "int"
        if all_float:
            return "float"
        
        return "string"
    
    def find_id_field(self, headers: List[str]) -> Tuple[str, str]:
        """查找 ID 字段"""
        id_field_names = ['id', 'Id', 'ID']
        
        for header in headers:
            field_name = self.sanitize_field_name(header)
            lower_field = field_name.lower()
            
            for id_name in id_field_names:
                if lower_field == id_name.lower():
                    return field_name, header
        
        return None, None
    
    def read_csv(self, file_path: Path) -> Tuple[List[str], List[Dict[str, str]]]:
        """读取 CSV 文件"""
        headers = []
        rows = []
        
        try:
            with open(file_path, 'r', encoding='utf-8-sig') as f:
                reader = csv.DictReader(f)
                headers = reader.fieldnames or []
                
                for row in reader:
                    rows.append(dict(row))
        except Exception as e:
            print(f"  错误: 读取 CSV 失败: {e}")
            raise
        
        return headers, rows
    
    def generate_class(self, class_name: str, headers: List[str], rows: List[Dict[str, str]]) -> str:
        """生成 C# 类文件"""
        lines = []
        
        lines.append("using System;")
        lines.append("using System.Collections.Generic;")
        lines.append("using System.Linq;")
        lines.append("using UnityEngine;")
        lines.append("")
        lines.append("namespace ExcelData")
        lines.append("{")
        lines.append("")
        
        # 数据行类
        lines.append("    [Serializable]")
        lines.append(f"    public class {class_name}Row")
        lines.append("    {")
        
        for header in headers:
            field_name = self.sanitize_field_name(header)
            field_type = self.infer_field_type(rows, header)
            lines.append(f"        public {field_type} {field_name};")
        
        lines.append("    }")
        lines.append("")
        
        # ScriptableObject 类
        lines.append(f'    [CreateAssetMenu(fileName = "{class_name}", menuName = "ExcelData/{class_name}")]')
        lines.append(f"    public class {class_name} : ScriptableObject, ExcelImporter.IExcelDataTable")
        lines.append("    {")
        lines.append(f"        public List<{class_name}Row> rows = new List<{class_name}Row>();")
        lines.append("")
        
        # GetRow 方法
        lines.append(f"        public {class_name}Row GetRow(int index)")
        lines.append("        {")
        lines.append("            if (index >= 0 && index < rows.Count)")
        lines.append("                return rows[index];")
        lines.append("            return null;")
        lines.append("        }")
        lines.append("")
        
        # GetById 方法
        id_field_name, id_header = self.find_id_field(headers)
        if id_field_name:
            id_field_type = self.infer_field_type(rows, id_header)
            
            lines.append(f"        /// <summary>")
            lines.append(f"        /// 根据 {id_field_name} 查找数据行")
            lines.append(f"        /// </summary>")
            
            if id_field_type == "int":
                lines.append(f"        public {class_name}Row GetById(int {id_field_name.lower()})")
                lines.append("        {")
                lines.append(f"            return rows.FirstOrDefault(r => r.{id_field_name} == {id_field_name.lower()});")
                lines.append("        }")
            elif id_field_type == "string":
                lines.append(f"        public {class_name}Row GetById(string {id_field_name.lower()})")
                lines.append("        {")
                lines.append(f"            if (string.IsNullOrEmpty({id_field_name.lower()}))")
                lines.append("                return null;")
                lines.append(f"            return rows.FirstOrDefault(r => r.{id_field_name} == {id_field_name.lower()});")
                lines.append("        }")
            else:
                lines.append(f"        public {class_name}Row GetById({id_field_type} {id_field_name.lower()})")
                lines.append("        {")
                lines.append(f"            return rows.FirstOrDefault(r => r.{id_field_name}.Equals({id_field_name.lower()}));")
                lines.append("        }")
            
            lines.append("")
        
        lines.append(f"        public int Count => rows.Count;")
        lines.append("")
        lines.append(f'        public string TableName => "{class_name}";')
        lines.append("")
        lines.append("    }")
        lines.append("}")
        
        return "\n".join(lines)
    
    def get_script_guid(self, class_name: str) -> Optional[str]:
        """从 .cs.meta 文件获取脚本 GUID"""
        script_meta_path = self.script_output / f"{class_name}.cs.meta"
        if script_meta_path.exists():
            try:
                content = script_meta_path.read_text(encoding='utf-8')
                # 查找 guid: 行
                for line in content.split('\n'):
                    if line.strip().startswith('guid:'):
                        guid = line.split(':', 1)[1].strip()
                        return guid
            except Exception as e:
                print(f"  警告: 无法读取脚本 GUID: {e}")
        return None
    
    def generate_asset_file(self, class_name: str, headers: List[str], rows: List[Dict[str, str]]) -> str:
        """生成 Unity 资源文件"""
        script_guid = self.get_script_guid(class_name)
        if not script_guid:
            # 如果找不到 GUID，使用占位符（Unity 会在导入时自动修复）
            script_guid = "00000000000000000000000000000000"
        
        lines = []
        lines.append("%YAML 1.1")
        lines.append("%TAG !u! tag:unity3d.com,2011:")
        lines.append("--- !u!114 &11400000")
        lines.append("MonoBehaviour:")
        lines.append("  m_ObjectHideFlags: 0")
        lines.append("  m_CorrespondingSourceObject: {fileID: 0}")
        lines.append("  m_PrefabInstance: {fileID: 0}")
        lines.append("  m_PrefabAsset: {fileID: 0}")
        lines.append("  m_GameObject: {fileID: 0}")
        lines.append("  m_Enabled: 1")
        lines.append("  m_EditorHideFlags: 0")
        lines.append(f"  m_Script: {{fileID: 11500000, guid: {script_guid}, type: 3}}")
        lines.append(f"  m_Name: {class_name}")
        lines.append("  m_EditorClassIdentifier: Assembly-CSharp::ExcelData." + class_name)
        lines.append("  rows:")
        if len(rows) == 0:
            lines.append("    m_Size: 0")
        else:
            lines.append(f"    m_Size: {len(rows)}")
            for i, row in enumerate(rows):
                lines.append(f"    - m_Item_{i}:")
                for header in headers:
                    field_name = self.sanitize_field_name(header)
                    raw_value = row.get(header, "")
                    value = str(raw_value) if raw_value is not None else ""
                    field_type = self.infer_field_type(rows, header)
                    yaml_value = self.format_yaml_value(value, field_type)
                    lines.append(f"        {field_name}: {yaml_value}")
        
        return "\n".join(lines)
    
    def generate_asset_meta_file(self) -> str:
        """生成 Unity 资源 .meta 文件"""
        guid = str(uuid.uuid4()).replace('-', '')
        lines = []
        lines.append("fileFormatVersion: 2")
        lines.append(f"guid: {guid}")
        lines.append("NativeFormatImporter:")
        lines.append("  externalObjects: {}")
        lines.append("  mainObjectFileID: 11400000")
        lines.append("  userData: ")
        lines.append("  assetBundleName: ")
        lines.append("  assetBundleVariant: ")
        return "\n".join(lines)
    
    def format_yaml_value(self, value: str, field_type: str) -> str:
        """格式化 YAML 值"""
        if value is None:
            value = ""
        value = str(value).strip()
        
        if not value:
            if field_type == "string":
                return ""
            elif field_type == "int":
                return "0"
            elif field_type == "float":
                return "0"
            elif field_type == "bool":
                return "0"
        
        if field_type == "string":
            # 转义特殊字符
            escaped = value.replace('\\', '\\\\').replace('"', '\\"').replace('\n', '\\n').replace('\r', '\\r')
            return f'"{escaped}"'
        elif field_type == "bool":
            lower = value.lower()
            return "1" if lower in ('true', '1', 'yes') else "0"
        
        return value
    
    def process_file(self, csv_file: Path) -> bool:
        """处理单个 CSV 文件"""
        try:
            file_name = csv_file.stem
            print(f"处理: {csv_file.name} -> {file_name}")
            
            # 读取 CSV
            headers, rows = self.read_csv(csv_file)
            
            if not headers:
                print(f"  警告: 文件没有表头，跳过")
                return False
            
            # 生成 C# 脚本
            script_content = self.generate_class(file_name, headers, rows)
            script_path = self.script_output / f"{file_name}.cs"
            script_path.parent.mkdir(parents=True, exist_ok=True)
            script_path.write_text(script_content, encoding='utf-8')
            print(f"  ✓ 生成脚本: {script_path.relative_to(self.unity_project_path)}")
            
            # 等待 Unity 生成 .cs.meta 文件（如果不存在）
            # 注意：如果脚本是新生成的，需要 Unity 先编译才能获取正确的 GUID
            # 这里先尝试读取，如果不存在会在生成 asset 时使用占位符
            
            # 生成 Unity 资源文件
            asset_content = self.generate_asset_file(file_name, headers, rows)
            asset_path = self.asset_output / f"{file_name}.asset"
            asset_path.parent.mkdir(parents=True, exist_ok=True)
            asset_path.write_text(asset_content, encoding='utf-8')
            print(f"  ✓ 生成资源: {asset_path.relative_to(self.unity_project_path)}")
            
            # 生成 .meta 文件（如果不存在）
            asset_meta_path = self.asset_output / f"{file_name}.asset.meta"
            if not asset_meta_path.exists():
                meta_content = self.generate_asset_meta_file()
                asset_meta_path.write_text(meta_content, encoding='utf-8')
                print(f"  ✓ 生成资源元数据: {asset_meta_path.relative_to(self.unity_project_path)}")
            
            return True
            
        except Exception as e:
            print(f"  ✗ 错误: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def run(self):
        """运行导入工具"""
        print("=== Unity Excel/CSV Importer Tool (Python) ===")
        print()
        
        if not self.document_path.exists():
            print(f"错误: Document 目录不存在: {self.document_path}")
            return
        
        if not self.unity_project_path.exists():
            print(f"错误: Unity 项目目录不存在: {self.unity_project_path}")
            return
        
        # 扫描 CSV 文件
        csv_files = list(self.document_path.glob("*.csv"))
        print(f"找到 {len(csv_files)} 个 CSV 文件")
        print()
        
        success_count = 0
        fail_count = 0
        
        for csv_file in csv_files:
            if self.process_file(csv_file):
                success_count += 1
            else:
                fail_count += 1
            print()
        
        print("=== 完成 ===")
        print(f"成功: {success_count}, 失败: {fail_count}")


def main():
    """主函数"""
    # 默认路径（相对于脚本目录）
    script_dir = Path(__file__).parent
    base_dir = script_dir.parent.parent
    
    document_path = base_dir / "Document" / "Config"
    unity_project_path = base_dir / "Game"
    script_output = "Assets/Scripts/ExcelData"
    asset_output = "Assets/Resources/ExcelData"
    
    # 解析命令行参数
    if len(sys.argv) > 1:
        document_path = Path(sys.argv[1])
    if len(sys.argv) > 2:
        unity_project_path = Path(sys.argv[2])
    if len(sys.argv) > 3:
        script_output = sys.argv[3]
    if len(sys.argv) > 4:
        asset_output = sys.argv[4]
    
    # 转换为绝对路径
    document_path = document_path.resolve()
    unity_project_path = unity_project_path.resolve()
    
    importer = CSVImporter(
        str(document_path),
        str(unity_project_path),
        script_output,
        asset_output
    )
    
    importer.run()


if __name__ == "__main__":
    main()

