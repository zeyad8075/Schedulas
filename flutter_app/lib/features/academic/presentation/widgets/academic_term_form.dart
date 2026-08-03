import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/models/academic_term_dto.dart';
import '../../domain/models/institution_dto.dart';

class AcademicTermForm extends StatelessWidget {
  final GlobalKey<FormBuilderState> formKey;
  final AcademicTermDto? initialData;
  final List<InstitutionDto> institutions;
  final VoidCallback onSubmit;
  final VoidCallback onCancel;

  const AcademicTermForm({
    super.key,
    required this.formKey,
    this.initialData,
    required this.institutions,
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
          FormBuilderTextField(
            name: 'name',
            initialValue: initialData?.name,
            decoration: InputDecoration(
              labelText: loc.academicName,
              border: const OutlineInputBorder(),
            ),
            validator: FormBuilderValidators.compose([
              FormBuilderValidators.required(),
              FormBuilderValidators.maxLength(200),
            ]),
          ),
          const SizedBox(height: 16),
          FormBuilderDateTimePicker(
            name: 'startDate',
            initialValue: initialData?.startDate ?? DateTime.now(),
            inputType: InputType.date,
            decoration: InputDecoration(
              labelText: loc.academicStartDate,
              border: const OutlineInputBorder(),
            ),
            validator: FormBuilderValidators.required(),
          ),
          const SizedBox(height: 16),
          FormBuilderDateTimePicker(
            name: 'endDate',
            initialValue: initialData?.endDate ??
                DateTime.now().add(const Duration(days: 90)),
            inputType: InputType.date,
            decoration: InputDecoration(
              labelText: loc.academicEndDate,
              border: const OutlineInputBorder(),
            ),
            validator: FormBuilderValidators.required(),
          ),
          const SizedBox(height: 16),
          FormBuilderSwitch(
            name: 'isActive',
            initialValue: initialData?.isActive ?? true,
            title: Text(loc.academicIsActive),
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
