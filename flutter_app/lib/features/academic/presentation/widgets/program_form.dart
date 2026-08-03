import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/models/program_dto.dart';
import '../../domain/models/department_dto.dart';

class ProgramForm extends StatelessWidget {
  final GlobalKey<FormBuilderState> formKey;
  final ProgramDto? initialData;
  final List<DepartmentDto> departments;
  final VoidCallback onSubmit;
  final VoidCallback onCancel;

  const ProgramForm({
    super.key,
    required this.formKey,
    this.initialData,
    required this.departments,
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
            name: 'departmentId',
            initialValue: initialData?.departmentId ??
                (departments.isNotEmpty ? departments.first.id : null),
            decoration: InputDecoration(
              labelText: loc.academicDepartments,
              border: const OutlineInputBorder(),
            ),
            items: departments.map((dep) {
              return DropdownMenuItem(
                value: dep.id,
                child: Text(dep.name),
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
              FormBuilderValidators.maxLength(300),
            ]),
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
