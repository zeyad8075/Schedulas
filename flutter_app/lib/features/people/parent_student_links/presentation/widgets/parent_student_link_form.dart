import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import '../../../../../l10n/app_localizations.dart';
import '../../../students/domain/models/student_dto.dart';

class ParentStudentLinkForm extends StatelessWidget {
  final GlobalKey<FormBuilderState> formKey;
  final List<StudentDto> availableStudents;
  final VoidCallback onSubmit;
  final VoidCallback onCancel;

  const ParentStudentLinkForm({
    super.key,
    required this.formKey,
    required this.availableStudents,
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
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          FormBuilderDropdown<String>(
            name: 'studentId',
            decoration: InputDecoration(
              labelText: loc.students,
              border: const OutlineInputBorder(),
            ),
            validator: FormBuilderValidators.required(
                errorText: loc.validationRequired),
            items: availableStudents
                .map((s) => DropdownMenuItem(
                      value: s.id,
                      child:
                          Text('${s.fullName} (${s.studentNumber ?? s.email})'),
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
