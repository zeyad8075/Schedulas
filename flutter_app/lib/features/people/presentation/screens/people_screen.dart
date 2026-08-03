import 'package:flutter/material.dart';
import '../../../../../l10n/app_localizations.dart';
import '../../profile/presentation/screens/profiles_list_screen.dart';
import '../../students/presentation/screens/students_list_screen.dart';
import '../../teachers/presentation/screens/teachers_list_screen.dart';
import '../../parents/presentation/screens/parents_list_screen.dart';

class PeopleScreen extends StatelessWidget {
  const PeopleScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);
    return DefaultTabController(
      length: 4,
      child: Scaffold(
        appBar: AppBar(
          title: Text(loc.people),
          bottom: TabBar(
            isScrollable: true,
            tabs: [
              Tab(text: loc.users),
              Tab(text: loc.students),
              Tab(text: loc.teachers),
              Tab(text: loc.parents),
            ],
          ),
        ),
        body: const TabBarView(
          children: [
            ProfilesListScreen(),
            StudentsListScreen(),
            TeachersListScreen(),
            ParentsListScreen(),
          ],
        ),
      ),
    );
  }
}
