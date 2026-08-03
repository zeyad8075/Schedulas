import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:intl/intl.dart';
import '../../../../../l10n/app_localizations.dart';
import '../../../../../core/widgets/error_view.dart';
import '../../../../../core/widgets/loading_view.dart';
import '../../../../../core/widgets/empty_view.dart';
import '../../../../../core/widgets/confirmation_dialog.dart';
import '../../../auth/presentation/providers/auth_providers.dart';
import '../../../academic/presentation/providers/academic_providers.dart';
import '../../../auth/domain/user_role.dart';
import '../../domain/models/activity_dto.dart';
import '../../domain/models/activity_enums.dart';
import '../providers/activity_providers.dart';
import '../widgets/activity_form.dart';
import 'activity_details_screen.dart';

class ActivitiesScreen extends ConsumerStatefulWidget {
  const ActivitiesScreen({super.key});

  @override
  ConsumerState<ActivitiesScreen> createState() => _ActivitiesScreenState();
}

class _ActivitiesScreenState extends ConsumerState<ActivitiesScreen> {
  String _searchTerm = '';
  String? _classId;
  ActivityType? _type;
  ActivityStatus? _status;

  int _pageNumber = 1;
  static const int _pageSize = 20;

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);
    final user = ref.watch(currentProfileProvider);

    if (user == null) return const Scaffold(body: LoadingView());

    final filter = ActivitiesFilter(
      institutionId: user.institutionId ?? '',
      searchTerm: _searchTerm.isEmpty ? null : _searchTerm,
      classId: _classId,
      type: _type,
      status: _status,
      pageNumber: _pageNumber,
      pageSize: _pageSize,
    );

    final activitiesAsync = ref.watch(activitiesListProvider(filter));

    final canManage = user.role == UserRole.teacher ||
        user.role == UserRole.departmentAdmin ||
        user.role == UserRole.institutionAdmin ||
        user.role == UserRole.platformAdmin;

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.activities),
        actions: [
          IconButton(
            icon: const Icon(Icons.filter_list),
            onPressed: _showFilterDialog,
          ),
        ],
      ),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: TextField(
              decoration: InputDecoration(
                labelText: loc.search,
                prefixIcon: const Icon(Icons.search),
                border: const OutlineInputBorder(),
              ),
              onChanged: (value) {
                setState(() {
                  _searchTerm = value;
                  _pageNumber = 1;
                });
              },
            ),
          ),
          Expanded(
            child: activitiesAsync.when(
              data: (paginated) {
                if (paginated.items.isEmpty) {
                  return EmptyView(message: loc.noData);
                }

                return RefreshIndicator(
                  onRefresh: () async {
                    ref.invalidate(activitiesListProvider(filter));
                  },
                  child: ListView.builder(
                    itemCount: paginated.items.length + 1,
                    itemBuilder: (context, index) {
                      if (index == paginated.items.length) {
                        return _buildPaginationControls(
                            paginated.hasNextPage, paginated.hasPreviousPage);
                      }

                      final activity = paginated.items[index];
                      return _buildActivityCard(activity, canManage, filter);
                    },
                  ),
                );
              },
              loading: () => const LoadingView(),
              error: (error, stack) => ErrorView(
                message: error.toString(),
                onRetry: () => ref.invalidate(activitiesListProvider(filter)),
              ),
            ),
          ),
        ],
      ),
      floatingActionButton: canManage
          ? FloatingActionButton(
              onPressed: () =>
                  _showActivityDialog(null, user.institutionId ?? '', filter),
              child: const Icon(Icons.add),
            )
          : null,
    );
  }

  Widget _buildPaginationControls(bool hasNext, bool hasPrevious) {
    if (!hasNext && !hasPrevious) return const SizedBox.shrink();

    final loc = AppLocalizations.of(context);
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 16.0),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          ElevatedButton(
            onPressed: hasPrevious ? () => setState(() => _pageNumber--) : null,
            child: Text(loc.previous),
          ),
          const SizedBox(width: 16),
          Text('$_pageNumber'),
          const SizedBox(width: 16),
          ElevatedButton(
            onPressed: hasNext ? () => setState(() => _pageNumber++) : null,
            child: Text(loc.next),
          ),
        ],
      ),
    );
  }

  Widget _buildActivityCard(
      ActivityDto activity, bool canManage, ActivitiesFilter filter) {
    final loc = AppLocalizations.of(context);

    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: ListTile(
        title: Text(
          activity.title,
          style: TextStyle(
            decoration: activity.status == ActivityStatus.cancelled
                ? TextDecoration.lineThrough
                : null,
          ),
        ),
        subtitle:
            Text('${activity.activityType.name} - ${activity.scheduledDate}'),
        trailing: canManage
            ? PopupMenuButton(
                itemBuilder: (context) => [
                  if (activity.status != ActivityStatus.cancelled) ...[
                    PopupMenuItem(
                      value: 'edit',
                      child: Text(loc.edit),
                    ),
                    PopupMenuItem(
                      value: 'cancel',
                      child: Text(loc.cancel),
                    ),
                  ],
                  if (activity.status == ActivityStatus.cancelled)
                    PopupMenuItem(
                      value: 'restore',
                      child: Text(loc.restore),
                    ),
                ],
                onSelected: (value) {
                  switch (value) {
                    case 'edit':
                      _showActivityDialog(
                          activity, filter.institutionId, filter);
                      break;
                    case 'cancel':
                      _confirmCancel(activity.id, filter);
                      break;
                    case 'restore':
                      _confirmRestore(activity.id, filter);
                      break;
                  }
                },
              )
            : null,
        onTap: () {
          Navigator.of(context).push(
            MaterialPageRoute(
              builder: (context) =>
                  ActivityDetailsScreen(activityId: activity.id),
            ),
          );
        },
      ),
    );
  }

  void _showFilterDialog() {
    // Basic filter dialog implementation can be added here
  }

  Future<void> _showActivityDialog(ActivityDto? activity, String institutionId,
      ActivitiesFilter filter) async {
    final isEdit = activity != null;
    final formKey = GlobalKey<FormBuilderState>();
    final loc = AppLocalizations.of(context);

    final classesResult = await ref.read(classRepositoryProvider).getClasses(
          pageSize: 1000,
        );

    final classes = classesResult.fold((l) => [], (r) => r.items);

    if (!mounted) return;

    final initialValues = isEdit
        ? {
            'title': activity.title,
            'description': activity.description,
            'classId': activity.classId,
            'activityType': activity.activityType.value,
            'scheduledDate': DateTime.parse(activity.scheduledDate),
            if (activity.scheduledTime != null)
              'scheduledTime':
                  DateFormat("HH:mm").parse(activity.scheduledTime!),
            if (activity.endTime != null)
              'endTime': DateFormat("HH:mm").parse(activity.endTime!),
            'priority': activity.priority.toString(),
          }
        : <String, dynamic>{};

    await showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(isEdit ? loc.edit : loc.create),
        content: SizedBox(
          width: double.maxFinite,
          child: ActivityForm(
            formKey: formKey,
            initialValues: initialValues,
            isEdit: isEdit,
            institutionId: institutionId,
            classes: classes,
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: Text(loc.cancel),
          ),
          ElevatedButton(
            onPressed: () async {
              if (formKey.currentState?.saveAndValidate() ?? false) {
                final values = formKey.currentState!.value;

                final payload = {
                  'title': values['title'],
                  'description': values['description'],
                  'classId': values['classId'],
                  'activityType': values['activityType'],
                  'scheduledDate': DateFormat('yyyy-MM-dd')
                      .format(values['scheduledDate'] as DateTime),
                  if (values['scheduledTime'] != null)
                    'scheduledTime': DateFormat('HH:mm:ss')
                        .format(values['scheduledTime'] as DateTime),
                  if (values['endTime'] != null)
                    'endTime': DateFormat('HH:mm:ss')
                        .format(values['endTime'] as DateTime),
                  'priority':
                      int.tryParse(values['priority']?.toString() ?? '0') ?? 0,
                  if (!isEdit) 'institutionId': institutionId,
                };

                final repository = ref.read(activityRepositoryProvider);
                final result = isEdit
                    ? await repository.updateActivity(activity.id, payload)
                    : await repository.createActivity(payload);

                if (result.isRight() && context.mounted) {
                  Navigator.of(ctx).pop();
                  ref.invalidate(activitiesListProvider(filter));
                } else if (result.isLeft() && context.mounted) {
                  result.fold(
                    (failure) => ScaffoldMessenger.of(ctx)
                        .showSnackBar(SnackBar(content: Text(failure.message))),
                    (_) {},
                  );
                }
              }
            },
            child: Text(loc.save),
          ),
        ],
      ),
    );
  }

  Future<void> _confirmCancel(
      String activityId, ActivitiesFilter filter) async {
    final loc = AppLocalizations.of(context);
    final confirm = await ConfirmationDialog.show(
      context,
      title: loc.cancel,
      content: 'Are you sure you want to cancel this activity?',
      confirmText: loc.cancel,
      cancelText: loc.back,
      isDestructive: true,
    );
    if (confirm == true) {
      final repository = ref.read(activityRepositoryProvider);
      final result = await repository.cancelActivity(activityId);
      if (result.isRight() && mounted) {
        ref.invalidate(activitiesListProvider(filter));
      } else if (result.isLeft() && mounted) {
        result.fold(
          (failure) => ScaffoldMessenger.of(context)
              .showSnackBar(SnackBar(content: Text(failure.message))),
          (_) {},
        );
      }
    }
  }

  Future<void> _confirmRestore(
      String activityId, ActivitiesFilter filter) async {
    final loc = AppLocalizations.of(context);
    final confirm = await ConfirmationDialog.show(
      context,
      title: loc.restore,
      content: 'Are you sure you want to restore this activity?',
      confirmText: loc.restore,
      cancelText: loc.cancel,
    );
    if (confirm == true) {
      final repository = ref.read(activityRepositoryProvider);
      final result = await repository.restoreActivity(activityId);
      if (result.isRight() && mounted) {
        ref.invalidate(activitiesListProvider(filter));
      } else if (result.isLeft() && mounted) {
        result.fold(
          (failure) => ScaffoldMessenger.of(context)
              .showSnackBar(SnackBar(content: Text(failure.message))),
          (_) {},
        );
      }
    }
  }
}
