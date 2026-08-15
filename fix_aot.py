import os

filepath = r"C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench\src\VibeStock.Ingestor.Cell\Generated\connection\NpgsqlConnectionSetup.cs"

with open(filepath, "r", encoding="utf-8") as f:
    code = f.read()

code = code.replace("builder.EnableDynamicJson();", "")

with open(filepath, "w", encoding="utf-8") as f:
    f.write(code)

print("AOT fixed.")
