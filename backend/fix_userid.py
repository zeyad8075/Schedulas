import os

files_to_fix = [
    "/home/zeyad/Downloads/Schedulas/backend/src/Schedulas.Application/Features/People/Commands/ProfileCommands.cs",
    "/home/zeyad/Downloads/Schedulas/backend/src/Schedulas.Application/Features/People/Queries/ProfileQueries.cs"
]

for file_path in files_to_fix:
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    new_content = content.replace("_currentUser.Id", "_currentUser.UserId!.Value")
    
    if new_content != content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Fixed UserId in {file_path}")

