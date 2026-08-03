import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/models/holiday_dto.dart';
import '../../domain/models/institution_dto.dart';
import '../../domain/models/academic_term_dto.dart';

class HolidayForm extends StatelessWidget {
  final GlobalKey<FormBuilderState> formKey;
  final HolidayDto? initialData;
  final List<InstitutionDto> institutions;
  final List<AcademicTermDto> terms;
  final VoidCallback onSubmit;
  final VoidCallback onCancel;

  const HolidayForm({
    super.key,
    required this.formKey,
    this.initialData,
    required this.institutions,
    required this.terms,
    required this.onSubmit,
    required this.onCancel,
  });

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);

    return FormBuilder(
      key: formKey,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          FormBuilderDropdown<String>(
            name: 'institutionId',
            initialValue: initialData?.institutionId ??
                (institutions.isNotEmpty ? institutions.first.id : null),
            decoration: InputDecoration(
              labelText: loc.academicInstitutions,
              border: const OutlineInputBorder(),
            ),
            items: institutions.map((inst) {
              return DropdownMenuItem(
                value: inst.id,
                child: Text(inst.name),
              );
            }).toList(),
            validator: FormBuilderValidators.required(),
          ),
          const SizedBox(height: 16),
          FormBuilderDropdown<String>(
            name: 'academicTermId',
            initialValue: initialData?.academicTermId,
            decoration: InputDecoration(
              labelText: '${loc.academicTerms} (Optional)',
              border: const OutlineInputBorder(),
            ),
            items: [
              const DropdownMenuItem(
                  value: null,
                  child: Text('None (Institution-wide)')),
              ...terms.map((trm) {
                return DropdownMenuItem(
                  value: trm.id,
                  child: Text(trm.name),
                );
              }),
            ],
          ),
          const SizedBox(height: 16),
          FormBuilderTextField(
            name: 'name',
            initialValue: initialData?.name,
            decoration: InputDecoration(
              labelText: loc.academicName,
              border: const OutlineInputBorder(),
            ),
            validator: FormBuilderValidators.compose([
              FormBuilderValidators.required(),
              FormBuilderValidators.maxLength(300),
            ]),
          ),
          const SizedBox(height: 16),
          FormBuilderDateTimePicker(
            name: 'holidayDate',
            initialValue: initialData?.holidayDate ?? DateTime.now(),
            inputType: InputType.date,
            decoration: InputDecoration(
              labelText: loc.academicHolidayDate,
              border: const OutlineInputBorder(),
            ),
            validator: FormBuilderValidators.required(),
          ),
          const SizedBox(height: 24),
          Row(
            mainAxisAlignment: MainAxisAlignment.end,
            children: [
              TextButton(
                onPressed: onCancel,
                child: Text(loc.commonCancel),
              ),
              const SizedBox(width: 8),
              FilledButton(
                onPressed: onSubmit,
                child: Text(loc.commonSave),
              ),
            ],
          )
        ],
      ),
    );
  }
}
