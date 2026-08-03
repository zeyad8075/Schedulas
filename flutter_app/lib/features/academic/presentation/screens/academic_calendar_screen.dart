import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import '../../../../core/widgets/info_tile.dart';
import '../../../../core/widgets/loading_view.dart';
import '../../../../l10n/app_localizations.dart';
import '../providers/academic_providers.dart';
import '../../domain/models/academic_term_dto.dart';
import '../../domain/models/holiday_dto.dart';
import '../../domain/models/institution_dto.dart';
import '../widgets/academic_term_form.dart';
import '../widgets/holiday_form.dart';

class AcademicCalendarScreen extends ConsumerStatefulWidget {
  const AcademicCalendarScreen({super.key});

  @override
  ConsumerState<AcademicCalendarScreen> createState() =>
      _AcademicCalendarScreenState();
}

class _AcademicCalendarScreenState extends ConsumerState<AcademicCalendarScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  final int _termsPage = 1;
  final int _holidaysPage = 1;
  String? _searchTerm;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  // --- Terms ---
  Future<void> _showTermDialog(
      [AcademicTermDto? term, List<InstitutionDto>? institutions]) async {
    if (institutions == null || institutions.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('Cannot create term: No institutions available.')),
      );
      return;
    }
    final formKey = GlobalKey<FormBuilderState>();

    await showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(term == null ? 'Add Term' : 'Edit Term'),
        content: SizedBox(
          width: 400,
          child: AcademicTermForm(
            formKey: formKey,
            initialData: term,
            institutions: institutions,
            onSubmit: () async {
              if (formKey.currentState?.saveAndValidate() ?? false) {
                final values = formKey.currentState!.value;
                final repository = ref.read(academicCalendarRepositoryProvider);

                if (term == null) {
                  // Create
                  final result = await repository.createAcademicTerm(
                    values['institutionId'] as String,
                    values['name'] as String,
                    values['startDate'] as DateTime,
                    values['endDate'] as DateTime,
                  );
                  if (result.isRight() && ctx.mounted) {
                    Navigator.of(ctx).pop();
                    ref.invalidate(academicTermsListProvider);
                  }
                } else {
                  // Update
                  final result = await repository.updateAcademicTerm(
                    term.id,
                    values['name'] as String,
                    values['startDate'] as DateTime,
                    values['endDate'] as DateTime,
                    values['isActive'] as bool,
                  );
                  if (result.isRight() && ctx.mounted) {
                    Navigator.of(ctx).pop();
                    ref.invalidate(academicTermsListProvider);
                  }
                }
              }
            },
            onCancel: () => Navigator.of(ctx).pop(),
          ),
        ),
      ),
    );
  }

  // --- Holidays ---
  Future<void> _showHolidayDialog(
      [HolidayDto? holiday,
      List<InstitutionDto>? institutions,
      List<AcademicTermDto>? terms]) async {
    if (institutions == null || institutions.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('Cannot create holiday: No institutions available.')),
      );
      return;
    }
    final formKey = GlobalKey<FormBuilderState>();

    await showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(
            holiday == null ? 'Add Holiday' : 'Edit Holiday'),
        content: SizedBox(
          width: 400,
          child: HolidayForm(
            formKey: formKey,
            initialData: holiday,
            institutions: institutions,
            terms: terms ?? [],
            onSubmit: () async {
              if (formKey.currentState?.saveAndValidate() ?? false) {
                final values = formKey.currentState!.value;
                final repository = ref.read(academicCalendarRepositoryProvider);

                if (holiday == null) {
                  // Create
                  final result = await repository.createHoliday(
                    values['institutionId'] as String,
                    values['academicTermId'] as String?,
                    values['name'] as String,
                    values['holidayDate'] as DateTime,
                  );
                  if (result.isRight() && ctx.mounted) {
                    Navigator.of(ctx).pop();
                    ref.invalidate(holidaysListProvider);
                  }
                } else {
                  // Update
                  final result = await repository.updateHoliday(
                    holiday.id,
                    values['name'] as String,
                    values['holidayDate'] as DateTime,
                  );
                  if (result.isRight() && ctx.mounted) {
                    Navigator.of(ctx).pop();
                    ref.invalidate(holidaysListProvider);
                  }
                }
              }
            },
            onCancel: () => Navigator.of(ctx).pop(),
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);

    final institutionsAsync = ref
        .watch(institutionsListProvider(const PaginationParams(pageSize: 100)));
    final termsAllAsync = ref.watch(
        academicTermsListProvider(const PaginationParams(pageSize: 100)));

    final termsParams =
        PaginationParams(pageNumber: _termsPage, searchTerm: _searchTerm);
    final holidaysParams =
        PaginationParams(pageNumber: _holidaysPage, searchTerm: _searchTerm);

    final termsData = ref.watch(academicTermsListProvider(termsParams));
    final holidaysData = ref.watch(holidaysListProvider(holidaysParams));

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.academicCalendar),
        bottom: TabBar(
          controller: _tabController,
          tabs: [
            Tab(text: loc.academicTerms),
            Tab(text: loc.academicHolidays),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.add),
            onPressed: () {
              if (_tabController.index == 0) {
                institutionsAsync
                    .whenData((insts) => _showTermDialog(null, insts.items));
              } else {
                institutionsAsync.whenData((insts) => termsAllAsync.whenData(
                    (trms) =>
                        _showHolidayDialog(null, insts.items, trms.items)));
              }
            },
          )
        ],
      ),
      body: TabBarView(
        controller: _tabController,
        children: [
          // Terms Tab
          _buildTermsList(termsData, institutionsAsync),
          // Holidays Tab
          _buildHolidaysList(holidaysData, institutionsAsync, termsAllAsync),
        ],
      ),
    );
  }

  Widget _buildTermsList(AsyncValue termsData, AsyncValue institutionsAsync) {
    return termsData.when(
      loading: () => const LoadingView(),
      error: (e, st) => Center(child: Text(e.toString())),
      data: (paginatedList) {
        if (paginatedList.items.isEmpty) {
                return const Center(child: Text('No terms found'));
              }
        return ListView.builder(
          itemCount: paginatedList.items.length,
          itemBuilder: (context, index) {
            final term = paginatedList.items[index];
            return InfoTile(
              title: term.name,
              subtitle:
                  '${term.startDate.toIso8601String().split('T')[0]} - ${term.endDate.toIso8601String().split('T')[0]}',
              trailing: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  institutionsAsync.maybeWhen(
                    data: (insts) => IconButton(
                      icon: const Icon(Icons.edit),
                      onPressed: () => _showTermDialog(term, insts.items),
                    ),
                    orElse: () => const SizedBox.shrink(),
                  ),
                  IconButton(
                    icon: const Icon(Icons.delete),
                    onPressed: () async {
                      final repository =
                          ref.read(academicCalendarRepositoryProvider);
                      final result =
                          await repository.deleteAcademicTerm(term.id);
                      if (result.isRight()) {
                        ref.invalidate(academicTermsListProvider);
                      }
                    },
                  )
                ],
              ),
            );
          },
        );
      },
    );
  }

  Widget _buildHolidaysList(AsyncValue holidaysData,
      AsyncValue institutionsAsync, AsyncValue termsAllAsync) {
    return holidaysData.when(
      loading: () => const LoadingView(),
      error: (e, st) => Center(child: Text(e.toString())),
      data: (paginatedList) {
        if (paginatedList.items.isEmpty) {
                return const Center(child: Text('No holidays found'));
              }
        return ListView.builder(
          itemCount: paginatedList.items.length,
          itemBuilder: (context, index) {
            final holiday = paginatedList.items[index];
            return InfoTile(
              title: holiday.name,
              subtitle: holiday.holidayDate.toIso8601String().split('T')[0],
              trailing: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  institutionsAsync.maybeWhen(
                    data: (insts) => termsAllAsync.maybeWhen(
                      data: (trms) => IconButton(
                        icon: const Icon(Icons.edit),
                        onPressed: () => _showHolidayDialog(
                            holiday, insts.items, trms.items),
                      ),
                      orElse: () => const SizedBox.shrink(),
                    ),
                    orElse: () => const SizedBox.shrink(),
                  ),
                  IconButton(
                    icon: const Icon(Icons.delete),
                    onPressed: () async {
                      final repository =
                          ref.read(academicCalendarRepositoryProvider);
                      final result = await repository.deleteHoliday(holiday.id);
                      if (result.isRight()) {
                        ref.invalidate(holidaysListProvider);
                      }
                    },
                  )
                ],
              ),
            );
          },
        );
      },
    );
  }
}
