import re
import os

f = "lib/features/academic/presentation/screens/institutions_screen.dart"
with open(f, 'r') as file:
    content = file.read()
content = content.replace("loc.academicDeleteInstitution", "'Delete Institution'")
content = content.replace("loc.academicConfirmDelete(institution.name)", "f'Are you sure you want to delete {institution.name}? This will mark it as soft-deleted.'")
content = re.sub(r'(Future<void> _showCreateEditDialog\(InstitutionDto\? institution\) async \{)', r'\1\n    final loc = AppLocalizations.of(context)!;', content)
with open(f, 'w') as file:
    file.write(content)

f = "lib/features/dashboard/presentation/providers/dashboard_providers.dart"
with open(f, 'r') as file:
    content = file.read()
content = content.replace("import '../../domain/dashboard_data.dart';", "import '../../domain/dashboard_data.dart';\nimport '../../domain/dashboard_repository.dart';\nimport '../../data/repositories/dashboard_repository_impl.dart';")
with open(f, 'w') as file:
    file.write(content)

f = "test/features/dashboard/data/repositories/dashboard_repository_impl_test.dart"
with open(f, 'r') as file:
    content = file.read()
content = content.replace("DashboardRepositoryImpl(mockApiClient, mockUserProfile);", "DashboardRepositoryImpl(mockApiClient);")
with open(f, 'w') as file:
    file.write(content)
