import os
import glob

repos_dir = 'lib/features/academic/data/repositories/'

for filepath in glob.glob(os.path.join(repos_dir, '*_repository_impl.dart')):
    with open(filepath, 'r') as f:
        content = f.read()

    # Fix body parameter
    content = content.replace('data: {', 'body: {')
    
    # Fix unnecessary cast in mapping lists
    content = content.replace(
        '.map((e) => fromJson(e as Map<String, dynamic>))',
        '.map((e) => fromJson(e))'
    )
    content = content.replace(
        '.map((e) => InstitutionDto.fromJson(e as Map<String, dynamic>))',
        '.map((e) => InstitutionDto.fromJson(e))'
    )
    content = content.replace(
        '.map((e) => DepartmentDto.fromJson(e as Map<String, dynamic>))',
        '.map((e) => DepartmentDto.fromJson(e))'
    )
    content = content.replace(
        '.map((e) => ProgramDto.fromJson(e as Map<String, dynamic>))',
        '.map((e) => ProgramDto.fromJson(e))'
    )
    content = content.replace(
        '.map((e) => CourseDto.fromJson(e as Map<String, dynamic>))',
        '.map((e) => CourseDto.fromJson(e))'
    )
    content = content.replace(
        '.map((e) => ClassDto.fromJson(e as Map<String, dynamic>))',
        '.map((e) => ClassDto.fromJson(e))'
    )

    with open(filepath, 'w') as f:
        f.write(content)
