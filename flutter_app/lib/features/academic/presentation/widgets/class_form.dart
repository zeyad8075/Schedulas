import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/models/class_dto.dart';
import '../../domain/models/course_dto.dart';
import '../../domain/models/academic_term_dto.dart';

class ClassForm extends StatelessWidget {
  final GlobalKey<FormBuilderState> formKey;
  final ClassDto? initialData;
  final List<CourseDto> courses;
  final List<AcademicTermDto> terms;
  final VoidCallback onSubmit;
  final VoidCallback onCancel;

  const ClassForm({
    super.key,
    required this.formKey,
    this.initialData,
    required this.courses,
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
            name: 'courseId',
            initialValue: initialData?.courseId ??
                (courses.isNotEmpty ? courses.first.id : null),
            decoration: InputDecoration(
              labelText: loc.academicCourses,
              border: const OutlineInputBorder(),
            ),
            items: courses.map((crs) {
              return DropdownMenuItem(
                value: crs.id,
                child: Text(crs.name),
              );
            }).toList(),
            validator: FormBuilderValidators.required(),
          ),
          const SizedBox(height: 16),
          FormBuilderDropdown<String>(
            name: 'academicTermId',
            initialValue: initialData?.academicTermId ??
                (terms.isNotEmpty ? terms.first.id : null),
            decoration: InputDecoration(
              labelText: loc.academicTerms,
              border: const OutlineInputBorder(),
            ),
            items: terms.map((trm) {
              return DropdownMenuItem(
                value: trm.id,
                child: Text(trm.name),
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
