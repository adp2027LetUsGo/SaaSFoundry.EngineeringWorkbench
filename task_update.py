import os

task_path = r"C:\Users\armando\.gemini\antigravity\brain\5c7726f6-290a-42e8-bba5-a32e6505f83c\task.md"

with open(task_path, "r", encoding="utf-8") as f:
    code = f.read()

code = code.replace("[ ] Extend VibeStockProduct with PI/SEO models", "[x] Extend VibeStockProduct with PI/SEO models")
code = code.replace("[ ] Wire up PI/SEO dependencies inside VibeStock.System.Cell.IntegrationTests", "[x] Wire up PI/SEO dependencies inside VibeStock.System.Cell.IntegrationTests")
code = code.replace("[ ] Verify full NativeAOT compatibility", "[x] Verify full NativeAOT compatibility")

with open(task_path, "w", encoding="utf-8") as f:
    f.write(code)

print("Task updated.")
