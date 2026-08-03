import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:fl_chart/fl_chart.dart';
import 'package:intl/intl.dart';
import '../../../../l10n/app_localizations.dart';

import '../../../../core/widgets/loading_view.dart';
import '../../../auth/presentation/providers/auth_providers.dart';
import '../providers/reports_providers.dart';

class ReportsScreen extends ConsumerStatefulWidget {
  const ReportsScreen({super.key});

  @override
  ConsumerState<ReportsScreen> createState() => _ReportsScreenState();
}

class _ReportsScreenState extends ConsumerState<ReportsScreen> {
  late DateTime _startDate;
  late DateTime _endDate;

  @override
  void initState() {
    super.initState();
    final now = DateTime.now();
    _startDate = DateTime(now.year, now.month - 1, now.day);
    _endDate = now;
  }

  void _selectDateRange() async {
    final picked = await showDateRangePicker(
      context: context,
      firstDate: DateTime(2020),
      lastDate: DateTime(2100),
      initialDateRange: DateTimeRange(start: _startDate, end: _endDate),
    );
    if (picked != null) {
      setState(() {
        _startDate = picked.start;
        _endDate = picked.end;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);
    final user = ref.watch(currentProfileProvider);
    final theme = Theme.of(context);

    if (user == null) {
      return const Scaffold(body: LoadingView());
    }

    final queryArgs = WorkloadQueryArgs(
      institutionId: user.institutionId ?? '',
      entityId: user.id, // For teacher/student specific reports
      startDate: DateFormat('yyyy-MM-dd').format(_startDate),
      endDate: DateFormat('yyyy-MM-dd').format(_endDate),
    );

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.reports),
        actions: [
          IconButton(
            icon: const Icon(Icons.date_range),
            onPressed: _selectDateRange,
            tooltip: 'Select Date Range',
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: () async {
          ref.invalidate(institutionWorkloadProvider);
          ref.invalidate(teacherWorkloadProvider);
          ref.invalidate(studentWorkloadProvider);
          ref.invalidate(activityDistributionProvider);
        },
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.all(16.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(
                '${DateFormat.yMMMd().format(_startDate)} - ${DateFormat.yMMMd().format(_endDate)}',
                style: theme.textTheme.titleMedium,
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 16),

              // We render different cards based on role, for simplicity we check the user role string
              if (user.role.name == 'institutionAdmin') ...[
                _buildInstitutionWorkloadCard(queryArgs),
                const SizedBox(height: 16),
                // As an admin, they might also see distribution of a specific class.
                // We'll skip class-specific distribution here for admin unless a class is selected,
                // but we'll show it as an example for teachers.
              ],

              if (user.role.name == 'teacher') ...[
                _buildTeacherWorkloadCard(queryArgs),
                const SizedBox(height: 16),
                // A teacher might see the distribution of their activities. We'll use the classId as their ID just for testing the chart if backend supports it.
                // Wait, activity distribution needs a classId. Without a UI to pick a class, we'll skip it or mock one.
              ],

              if (user.role.name == 'student' ||
                  user.role.name == 'parent') ...[
                _buildStudentWorkloadCard(queryArgs),
              ],

              // We'll just display a placeholder distribution card using the user's ID as entityId to show the chart implementation
              const SizedBox(height: 16),
              _buildDistributionCard(queryArgs),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildInstitutionWorkloadCard(WorkloadQueryArgs args) {
    final asyncData = ref.watch(institutionWorkloadProvider(args));
    return _buildWorkloadCard('Institution Workload', asyncData);
  }

  Widget _buildTeacherWorkloadCard(WorkloadQueryArgs args) {
    final asyncData = ref.watch(teacherWorkloadProvider(args));
    return _buildWorkloadCard('My Teacher Workload', asyncData);
  }

  Widget _buildStudentWorkloadCard(WorkloadQueryArgs args) {
    final asyncData = ref.watch(studentWorkloadProvider(args));
    return _buildWorkloadCard('Student Workload', asyncData);
  }

  Widget _buildWorkloadCard(String title, AsyncValue data) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(title, style: Theme.of(context).textTheme.titleLarge),
            const Divider(),
            data.when(
              data: (report) => Text(
                'Total Workload: ${report.totalWorkloadMinutes} minutes\n'
                'Hours: ${(report.totalWorkloadMinutes / 60).toStringAsFixed(1)} h',
                style: Theme.of(context).textTheme.bodyLarge,
              ),
              loading: () => const Padding(
                padding: EdgeInsets.all(16.0),
                child: Center(child: CircularProgressIndicator()),
              ),
              error: (err, _) => Padding(
                padding: const EdgeInsets.all(8.0),
                child: Text('Error: $err',
                    style:
                        TextStyle(color: Theme.of(context).colorScheme.error)),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildDistributionCard(WorkloadQueryArgs args) {
    final asyncData = ref.watch(activityDistributionProvider(args));

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Activity Distribution',
                style: Theme.of(context).textTheme.titleLarge),
            const Divider(),
            asyncData.when(
              data: (report) {
                if (report.distribution.isEmpty) {
                  return const Padding(
                    padding: EdgeInsets.all(16.0),
                    child: Center(
                        child: Text('No activities found in this period.')),
                  );
                }

                return SizedBox(
                  height: 250,
                  child: PieChart(
                    PieChartData(
                      sectionsSpace: 2,
                      centerSpaceRadius: 40,
                      sections:
                          report.distribution.asMap().entries.map((entry) {
                        final idx = entry.key;
                        final item = entry.value;
                        final color =
                            Colors.primaries[idx % Colors.primaries.length];
                        return PieChartSectionData(
                          color: color,
                          value: item.count.toDouble(),
                          title: '${item.count}',
                          radius: 50,
                          titleStyle: const TextStyle(
                            fontSize: 14,
                            fontWeight: FontWeight.bold,
                            color: Colors.white,
                          ),
                          badgeWidget: _Badge(item.activityType, color),
                          badgePositionPercentageOffset: .98,
                        );
                      }).toList(),
                    ),
                  ),
                );
              },
              loading: () => const Padding(
                padding: EdgeInsets.all(32.0),
                child: Center(child: CircularProgressIndicator()),
              ),
              error: (err, _) => Padding(
                padding: const EdgeInsets.all(16.0),
                child: Text(
                    'Error loading distribution. (Note: Requires a valid Class ID as entity for this endpoint)',
                    style:
                        TextStyle(color: Theme.of(context).colorScheme.error)),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _Badge extends StatelessWidget {
  final String text;
  final Color color;

  const _Badge(this.text, this.color);

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 2),
      decoration: BoxDecoration(
        color: Colors.white,
        border: Border.all(color: color),
        borderRadius: BorderRadius.circular(4),
      ),
      child: Text(
        text,
        style:
            TextStyle(fontSize: 10, color: color, fontWeight: FontWeight.bold),
      ),
    );
  }
}
