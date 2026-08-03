import re
import os

f = "test/features/dashboard/data/repositories/dashboard_repository_impl_test.dart"
with open(f, 'r') as file:
    content = file.read()
content = content.replace("DashboardRepositoryImpl(mockApiClient, mockUserProfile);", "DashboardRepositoryImpl(mockApiClient);")
with open(f, 'w') as file:
    file.write(content)
