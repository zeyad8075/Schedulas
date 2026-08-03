import os

files_to_fix = [
    'lib/features/people/profile/domain/models/profile_dto.dart',
    'lib/features/people/profile/domain/repositories/profile_repository.dart',
    'lib/features/people/profile/data/repositories/profile_repository_impl.dart',
]

for path in files_to_fix:
    if os.path.exists(path):
        with open(path, 'r') as f:
            content = f.read()
        
        content = content.replace('Theme preferredTheme', 'model_theme.Theme preferredTheme')
        content = content.replace('Theme.values', 'model_theme.Theme.values')
        content = content.replace('Theme.system', 'model_theme.Theme.system')
        content = content.replace('ThemeExtension.', 'model_theme.ThemeExtension.')
        content = content.replace('updateTheme(Theme preferredTheme)', 'updateTheme(model_theme.Theme preferredTheme)')
        
        with open(path, 'w') as f:
            f.write(content)

print("done")
