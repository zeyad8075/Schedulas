import os

def fix_file(file_path, state_name):
    with open(file_path, 'r') as f:
        content = f.read()

    # Find where the class _...State begins
    idx = content.find(f'class {state_name} extends ConsumerState')
    if idx == -1: return

    # find the end of `static const int _pageSize = 20;`
    idx2 = content.find('static const int _pageSize = 20;')
    if idx2 == -1: return

    # From idx2, replace everything up to `Widget build(BuildContext context) {`
    # but keep `static const int _pageSize = 20;`
    
    start_replace = idx2 + len('static const int _pageSize = 20;')
    end_replace = content.find('  Widget build(BuildContext context) {', start_replace)
    
    if end_replace != -1:
        # replace with just `\n\n  @override\n`
        new_content = content[:start_replace] + '\n\n  @override\n' + content[end_replace:]
        with open(file_path, 'w') as f:
            f.write(new_content)

fix_file('lib/features/people/students/presentation/screens/students_list_screen.dart', '_StudentsListScreenState')
fix_file('lib/features/people/teachers/presentation/screens/teachers_list_screen.dart', '_TeachersListScreenState')

print("done")
