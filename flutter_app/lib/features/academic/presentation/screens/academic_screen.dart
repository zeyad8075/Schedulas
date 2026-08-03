import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../../../l10n/app_localizations.dart';

class AcademicScreen extends StatelessWidget {
  const AcademicScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(
        title: Text(loc.navAcademic),
      ),
      body: ListView(
        children: [
          ListTile(
            leading: const Icon(Icons.account_balance),
            title: Text(loc.academicInstitutions),
            trailing: const Icon(Icons.chevron_right),
            onTap: () => context.push('/academic/institutions'),
          ),
          ListTile(
            leading: const Icon(Icons.domain),
            title: Text(loc.academicDepartments),
            trailing: const Icon(Icons.chevron_right),
            onTap: () => context.push('/academic/departments'),
          ),
          ListTile(
            leading: const Icon(Icons.school),
            title: Text(loc.academicPrograms),
            trailing: const Icon(Icons.chevron_right),
            onTap: () => context.push('/academic/programs'),
          ),
          ListTile(
            leading: const Icon(Icons.book),
            title: Text(loc.academicCourses),
            trailing: const Icon(Icons.chevron_right),
            onTap: () => context.push('/academic/courses'),
          ),
          ListTile(
            leading: const Icon(Icons.class_),
            title: Text(loc.academicClasses),
            trailing: const Icon(Icons.chevron_right),
            onTap: () => context.push('/academic/classes'),
          ),
          ListTile(
            leading: const Icon(Icons.calendar_month),
            title: Text(loc.academicCalendar),
            trailing: const Icon(Icons.chevron_right),
            onTap: () => context.push('/academic/calendar'),
          ),
        ],
      ),
    );
  }
}
