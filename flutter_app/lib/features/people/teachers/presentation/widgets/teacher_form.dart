import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import '../../../../../l10n/app_localizations.dart';
import '../../domain/models/teacher_dto.dart';
import '../../../profile/domain/models/profile_dto.dart';
import '../../../../academic/domain/models/department_dto.dart';

class TeacherForm extends StatelessWidget {
  final GlobalKey<FormBuilderState> formKey;
  final TeacherDto? initialData;
  final List<ProfileDto>? availableProfiles;
  final List<DepartmentDto>? availableDepartments;
  final VoidCallback onSubmit;
  final VoidCallback onCancel;

  const TeacherForm({
    super.key,
    required this.formKey,
    this.initialData,
    this.availableProfiles,
    this.availableDepartments,
    required this.onSubmit,
    required this.onCancel,
  });

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);
    final isEditing = initialData != null;

    return FormBuilder(
      key: formKey,
      initialValue: {
        if (initialData != null) 'departmentId': initialData!.departmentId,
        if (initialData != null) 'profileId': initialData!.profileId,
      },
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          if (!isEditing && availableProfiles != null)
            FormBuilderDropdown<String>(
              name: 'profileId',
              decoration: InputDecoration(
                labelText: loc.profile,
                border: const OutlineInputBorder(),
              ),
              validator: FormBuilderValidators.required(
                  errorText: loc.validationRequired),
              items: availableProfiles!
                  .map((p) => DropdownMenuItem(
                        value: p.id,
                        child: Text('${p.fullName} (${p.email})'),
                      ))
                  .toList(),
            ),
          if (isEditing)
            Padding(
              padding: const EdgeInsets.only(bottom: 16),
              child: Text(
                initialData!.fullName,
                style: Theme.of(context).textTheme.titleMedium,
              ),
            ),
          if (!isEditing) const SizedBox(height: 16),
          if (availableDepartments != null)
            FormBuilderDropdown<String>(
              name: 'departmentId',
              decoration: InputDecoration(
                labelText: loc.academicDepartments,
                border: const OutlineInputBorder(),
              ),
              items: availableDepartments!
                  .map((d) => DropdownMenuItem(
                        value: d.id,
                        child: Text(d.name),
                      ))
                  .toList(),
            ),
          const SizedBox(height: 24),
          Row(
            mainAxisAlignment: MainAxisAlignment.end,
            children: [
              TextButton(
                onPressed: onCancel,
                child: Text(loc.cancel),
              ),
              const SizedBox(width: 8),
              FilledButton(
                onPressed: onSubmit,
                child: Text(loc.save),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
