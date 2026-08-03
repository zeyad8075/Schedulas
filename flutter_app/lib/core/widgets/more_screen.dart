import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class MoreScreen extends StatelessWidget {
  const MoreScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('المزيد'),
      ),
      body: ListView(
        children: [
          ListTile(
            leading: const Icon(Icons.school_outlined),
            title: const Text('الأكاديمية'),
            onTap: () => context.push('/academic'),
          ),
          ListTile(
            leading: const Icon(Icons.people_outline),
            title: const Text('الأشخاص'),
            onTap: () => context.push('/people'),
          ),
          ListTile(
            leading: const Icon(Icons.bar_chart_outlined),
            title: const Text('التقارير'),
            onTap: () => context.push('/reports'),
          ),
          const Divider(),
          ListTile(
            leading: const Icon(Icons.settings_outlined),
            title: const Text('الإعدادات'),
            onTap: () => context.push('/settings'),
          ),
        ],
      ),
    );
  }
}
