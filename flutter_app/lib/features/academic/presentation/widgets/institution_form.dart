import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/models/institution_dto.dart';

class InstitutionForm extends StatelessWidget {
  final GlobalKey<FormBuilderState> formKey;
  final InstitutionDto? initialData;
  final VoidCallback onSubmit;
  final VoidCallback onCancel;

  const InstitutionForm({
    super.key,
    required this.formKey,
    this.initialData,
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
          FormBuilderDropdown<InstitutionType>(
            name: 'type',
            initialValue: initialData?.type ?? InstitutionType.school,
            decoration: InputDecoration(
              labelText: loc.academicInstitutionType,
              border: const OutlineInputBorder(),
            ),
            items: InstitutionType.values.map((type) {
              String label = '';
              switch (type) {
                case InstitutionType.school:
                  label = loc.academicInstitutionTypeSchool;
                  break;
                case InstitutionType.university:
                  label = loc.academicInstitutionTypeUniversity;
                  break;
                case InstitutionType.trainingCenter:
                  label = loc.academicInstitutionTypeTrainingCenter;
                  break;
                case InstitutionType.other:
                  label = loc.academicInstitutionTypeOther;
                  break;
              }
              return DropdownMenuItem(
                value: type,
                child: Text(label),
              );
            }).toList(),
            validator: FormBuilderValidators.required(),
          ),
          const SizedBox(height: 16),
          FormBuilderTextField(
            name: 'timezone',
            initialValue: initialData?.timezone ?? 'UTC',
            decoration: InputDecoration(
              labelText: loc.academicTimezone,
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
