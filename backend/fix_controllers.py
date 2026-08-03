import os
import glob

base_dir = "/home/zeyad/Downloads/Schedulas/backend/src/Schedulas.API/Controllers"
files = ["ProfilesController.cs", "StudentsController.cs", "TeachersController.cs", "ParentsController.cs"]

for file_name in files:
    file_path = os.path.join(base_dir, file_name)
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Replace ApiControllerBase with ControllerBase
    content = content.replace("ApiControllerBase", "ControllerBase")
    
    # Add Asp.Versioning, Route stuff
    if "[ApiController]" not in content:
        content = content.replace("[Route(", "[ApiController]\n[ApiVersion(\"1.0\")]\n[Route(")
        content = content.replace("api/v1/", "api/v{version:apiVersion}/")

    # Add constructor
    class_name = file_name.replace(".cs", "")
    if f"public class {class_name}" in content:
        content = content.replace(f"public class {class_name} : ControllerBase\n{{", 
                                  f"public class {class_name} : ControllerBase\n{{\n    private readonly ISender _mediator;\n    public {class_name}(ISender mediator) => _mediator = mediator;\n")

    # Replace Mediator.Send with _mediator.Send
    content = content.replace("Mediator.Send", "_mediator.Send")
    
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f"Fixed {file_name}")

