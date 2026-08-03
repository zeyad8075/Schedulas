import re
import os

def fix_file(filepath, fixes):
    if not os.path.exists(filepath): return
    with open(filepath, 'r') as f:
        lines = f.readlines()
    
    for fix in fixes:
        line_idx = fix['line'] - 1
        if fix['type'] == 'todo':
            lines[line_idx] = lines[line_idx].replace('// TODO localize', '')
        elif fix['type'] == 'if_block':
            # This is a bit tricky, if the next line is the statement, we need to wrap it
            # Typically: if (condition)\n  statement;
            # Or: if (condition) statement;
            # Since we know the exact line of the `if` from the IDE:
            match = re.match(r'^(\s*)if\s*\(.*?\)\s*(.*)', lines[line_idx])
            if match:
                indent = match.group(1)
                statement = match.group(2).strip()
                if statement and statement != '':
                    # Inline statement: if (c) return;
                    new_line = lines[line_idx].replace(statement, '{ ' + statement + ' }')
                    if new_line.endswith('}\n') == False:
                         new_line = new_line.rstrip('\n') + ' }\n'
                    lines[line_idx] = new_line
                else:
                    # Statement on next line
                    lines[line_idx] = lines[line_idx].rstrip() + ' {\n'
                    # find the next line with code
                    next_idx = line_idx + 1
                    while next_idx < len(lines) and lines[next_idx].strip() == '':
                        next_idx += 1
                    
                    if next_idx < len(lines):
                        lines[next_idx] = lines[next_idx].rstrip() + '\n' + indent + '}\n'

    with open(filepath, 'w') as f:
        f.writelines(lines)

fixes_map = {
    "lib/core/routing/app_router.dart": [
        {'line': 52, 'type': 'if_block'} # the error was on line 53, but the if is on 52 usually? Let's assume the if is on line 52 or 53.
    ]
}

# Actually, writing a precise regex for this might be buggy if line numbers changed.
# It's better to use multi_replace_file_content tool directly for safety and precision.
