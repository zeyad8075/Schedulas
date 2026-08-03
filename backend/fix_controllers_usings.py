import os
import glob

base_dir = "/home/zeyad/Downloads/Schedulas/backend/src/Schedulas.API/Controllers"
files = ["ProfilesController.cs", "StudentsController.cs", "TeachersController.cs", "ParentsController.cs"]

for file_name in files:
    file_path = os.path.join(base_dir, file_name)
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    if "using Asp.Versioning;" not in content:
        content = "using Asp.Versioning;\n" + content
    
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f"Fixed {file_name}")

