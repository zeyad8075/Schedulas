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

    start_replace = idx2 + len('static const int _pageSize = 20;')
    end_replace = content.find('  Widget build(BuildContext context) {', start_replace)
    
    if end_replace != -1:
        new_content = content[:start_replace] + '\n\n  @override\n' + content[end_replace:]
        with open(file_path, 'w') as f:
            f.write(new_content)

fix_file('lib/features/people/profile/presentation/screens/profiles_list_screen.dart', '_ProfilesListScreenState')

print("done")
