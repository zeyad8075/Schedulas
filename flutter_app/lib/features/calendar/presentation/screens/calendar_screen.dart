import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import '../../../../../l10n/app_localizations.dart';
import '../../../../../core/widgets/error_view.dart';
import '../../../../../core/widgets/loading_view.dart';
import '../../../auth/presentation/providers/auth_providers.dart';
import '../providers/calendar_providers.dart';
import '../../domain/models/calendar_dtos.dart';
import '../../../activities/presentation/screens/activity_details_screen.dart';

enum CalendarViewType { daily, weekly, monthly }

class CalendarScreen extends ConsumerStatefulWidget {
  const CalendarScreen({super.key});

  @override
  ConsumerState<CalendarScreen> createState() => _CalendarScreenState();
}

class _CalendarScreenState extends ConsumerState<CalendarScreen> {
  CalendarViewType _viewType = CalendarViewType.daily;
  DateTime _currentDate = DateTime.now();

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);
    final user = ref.watch(currentProfileProvider);

    if (user == null) return const Scaffold(body: LoadingView());

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.calendar),
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(60),
          child: Padding(
            padding: const EdgeInsets.all(8.0),
            child: SegmentedButton<CalendarViewType>(
              segments: [
                ButtonSegment(
                    value: CalendarViewType.daily, label: Text(loc.daily)),
                ButtonSegment(
                    value: CalendarViewType.weekly, label: Text(loc.weekly)),
                ButtonSegment(
                    value: CalendarViewType.monthly, label: Text(loc.monthly)),
              ],
              selected: {_viewType},
              onSelectionChanged: (Set<CalendarViewType> newSelection) {
                setState(() {
                  _viewType = newSelection.first;
                });
              },
            ),
          ),
        ),
      ),
      body: Column(
        children: [
          _buildDateSelector(),
          Expanded(
            child: _buildCalendarView(user.institutionId ?? ''),
          ),
        ],
      ),
    );
  }

  Widget _buildDateSelector() {
    String displayDate;

    switch (_viewType) {
      case CalendarViewType.daily:
        displayDate = DateFormat.yMMMMd().format(_currentDate);
        break;
      case CalendarViewType.weekly:
        final endOfWeek = _currentDate.add(const Duration(days: 6));
        displayDate =
            '${DateFormat.MMMd().format(_currentDate)} - ${DateFormat.MMMd().format(endOfWeek)}';
        break;
      case CalendarViewType.monthly:
        displayDate = DateFormat.yMMMM().format(_currentDate);
        break;
    }

    return Padding(
      padding: const EdgeInsets.all(8.0),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          IconButton(
            icon: const Icon(Icons.chevron_left),
            onPressed: () {
              setState(() {
                switch (_viewType) {
                  case CalendarViewType.daily:
                    _currentDate =
                        _currentDate.subtract(const Duration(days: 1));
                    break;
                  case CalendarViewType.weekly:
                    _currentDate =
                        _currentDate.subtract(const Duration(days: 7));
                    break;
                  case CalendarViewType.monthly:
                    _currentDate =
                        DateTime(_currentDate.year, _currentDate.month - 1, 1);
                    break;
                }
              });
            },
          ),
          Text(displayDate, style: Theme.of(context).textTheme.titleMedium),
          IconButton(
            icon: const Icon(Icons.chevron_right),
            onPressed: () {
              setState(() {
                switch (_viewType) {
                  case CalendarViewType.daily:
                    _currentDate = _currentDate.add(const Duration(days: 1));
                    break;
                  case CalendarViewType.weekly:
                    _currentDate = _currentDate.add(const Duration(days: 7));
                    break;
                  case CalendarViewType.monthly:
                    _currentDate =
                        DateTime(_currentDate.year, _currentDate.month + 1, 1);
                    break;
                }
              });
            },
          ),
        ],
      ),
    );
  }

  Widget _buildCalendarView(String institutionId) {
    final dateStr = DateFormat('yyyy-MM-dd').format(_currentDate);

    switch (_viewType) {
      case CalendarViewType.daily:
        final filter =
            CalendarFilter(institutionId: institutionId, date: dateStr);
        final asyncData = ref.watch(dailyCalendarProvider(filter));
        return asyncData.when(
          data: (data) => _DailyView(data: data),
          loading: () => const LoadingView(),
          error: (e, s) => ErrorView(
              message: e.toString(),
              onRetry: () => ref.invalidate(dailyCalendarProvider(filter))),
        );
      case CalendarViewType.weekly:
        final filter =
            CalendarFilter(institutionId: institutionId, date: dateStr);
        final asyncData = ref.watch(weeklyCalendarProvider(filter));
        return asyncData.when(
          data: (data) => _WeeklyView(data: data),
          loading: () => const LoadingView(),
          error: (e, s) => ErrorView(
              message: e.toString(),
              onRetry: () => ref.invalidate(weeklyCalendarProvider(filter))),
        );
      case CalendarViewType.monthly:
        final filter = MonthlyCalendarFilter(
            institutionId: institutionId,
            year: _currentDate.year,
            month: _currentDate.month);
        final asyncData = ref.watch(monthlyCalendarProvider(filter));
        return asyncData.when(
          data: (data) => _MonthlyView(data: data),
          loading: () => const LoadingView(),
          error: (e, s) => ErrorView(
              message: e.toString(),
              onRetry: () => ref.invalidate(monthlyCalendarProvider(filter))),
        );
    }
  }
}

class _DailyView extends StatelessWidget {
  final DailyCalendarDto data;
  const _DailyView({required this.data});

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);
    if (data.entries.isEmpty) {
      return Center(child: Text(loc.noData));
    }
    return ListView.builder(
      itemCount: data.entries.length,
      itemBuilder: (context, index) {
        return _CalendarEntryCard(entry: data.entries[index]);
      },
    );
  }
}

class _WeeklyView extends StatelessWidget {
  final WeeklyCalendarDto data;
  const _WeeklyView({required this.data});

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      itemCount: data.days.length,
      itemBuilder: (context, index) {
        final day = data.days[index];
        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Padding(
              padding: const EdgeInsets.all(8.0),
              child: Text(
                day.date,
                style: Theme.of(context)
                    .textTheme
                    .titleMedium
                    ?.copyWith(fontWeight: FontWeight.bold),
              ),
            ),
            if (day.entries.isEmpty)
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16.0),
                child: Text(AppLocalizations.of(context).noData),
              )
            else
              ...day.entries.map((e) => _CalendarEntryCard(entry: e)),
            const Divider(),
          ],
        );
      },
    );
  }
}

class _MonthlyView extends StatelessWidget {
  final MonthlyCalendarDto data;
  const _MonthlyView({required this.data});

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      itemCount: data.weeks.length,
      itemBuilder: (context, weekIndex) {
        final week = data.weeks[weekIndex];
        return Column(
          children: [
            ...week.days.where((day) => day.entries.isNotEmpty).map((day) {
              return Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Padding(
                    padding: const EdgeInsets.all(8.0),
                    child: Text(
                      day.date,
                      style: Theme.of(context)
                          .textTheme
                          .titleMedium
                          ?.copyWith(fontWeight: FontWeight.bold),
                    ),
                  ),
                  ...day.entries.map((e) => _CalendarEntryCard(entry: e)),
                  const Divider(),
                ],
              );
            }),
          ],
        );
      },
    );
  }
}

class _CalendarEntryCard extends StatelessWidget {
  final CalendarEntryDto entry;
  const _CalendarEntryCard({required this.entry});

  @override
  Widget build(BuildContext context) {
    Color cardColor;
    try {
      cardColor = Color(int.parse(entry.colorHex.replaceFirst('#', '0xFF')));
    } catch (_) {
      cardColor = Colors.blueGrey;
    }

    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 8.0, vertical: 4.0),
      child: ListTile(
        leading: CircleAvatar(backgroundColor: cardColor, radius: 8),
        title: Text(entry.title),
        subtitle: Text(
            '${entry.className} - ${entry.courseName}\n${entry.startTime ?? ''} - ${entry.endTime ?? ''}'),
        isThreeLine: true,
        trailing: Text(entry.activityType.name),
        onTap: () {
          Navigator.of(context).push(
            MaterialPageRoute(
              builder: (context) =>
                  ActivityDetailsScreen(activityId: entry.activityId),
            ),
          );
        },
      ),
    );
  }
}
