import os
import glob

base_dir = "/home/zeyad/Downloads/Schedulas/backend/src/Schedulas.Application/Features"
files_to_fix = glob.glob(os.path.join(base_dir, "People/**/*.cs"), recursive=True) + glob.glob(os.path.join(base_dir, "OrgHierarchy/Queries/EnrollmentQueries.cs"))

for file_path in files_to_fix:
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    new_content = content.replace("using Schedulas.Application.Common.Exceptions;", "using Schedulas.Domain.Exceptions;\nusing Schedulas.Application.Common.Behaviors;")
    
    if new_content != content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Fixed imports in {file_path}")

