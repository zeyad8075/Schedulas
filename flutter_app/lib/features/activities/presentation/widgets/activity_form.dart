import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import '../../../../../l10n/app_localizations.dart';
import '../../domain/models/activity_enums.dart';

class ActivityForm extends StatelessWidget {
  final GlobalKey<FormBuilderState> formKey;
  final Map<String, dynamic> initialValues;
  final bool isEdit;
  final String institutionId;
  final List<dynamic> classes;

  const ActivityForm({
    super.key,
    required this.formKey,
    this.initialValues = const {},
    this.isEdit = false,
    required this.institutionId,
    required this.classes,
  });

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);

    return FormBuilder(
      key: formKey,
      initialValue: initialValues,
      child: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          mainAxisSize: MainAxisSize.min,
          children: [
            FormBuilderTextField(
              name: 'title',
              decoration: InputDecoration(
                labelText: loc.title,
                border: const OutlineInputBorder(),
              ),
              validator: FormBuilderValidators.compose([
                FormBuilderValidators.required(),
                FormBuilderValidators.maxLength(200),
              ]),
            ),
            const SizedBox(height: 16),
            FormBuilderTextField(
              name: 'description',
              decoration: InputDecoration(
                labelText: loc.description,
                border: const OutlineInputBorder(),
              ),
              maxLines: 3,
              validator: FormBuilderValidators.maxLength(1000),
            ),
            const SizedBox(height: 16),
            FormBuilderDropdown<String>(
              name: 'classId',
              decoration: InputDecoration(
                labelText: loc.classLabel,
                border: const OutlineInputBorder(),
              ),
              validator: FormBuilderValidators.required(),
              items: classes.map((c) {
                return DropdownMenuItem<String>(
                  value: c.id,
                  child: Text(c.name),
                );
              }).toList(),
            ),
            const SizedBox(height: 16),
            FormBuilderDropdown<int>(
              name: 'activityType',
              decoration: InputDecoration(
                labelText: loc.type,
                border: const OutlineInputBorder(),
              ),
              validator: FormBuilderValidators.required(),
              items: ActivityType.values.map((type) {
                return DropdownMenuItem<int>(
                  value: type.value,
                  child: Text(type.name),
                );
              }).toList(),
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                Expanded(
                  child: FormBuilderDateTimePicker(
                    name: 'scheduledDate',
                    inputType: InputType.date,
                    format: null,
                    decoration: InputDecoration(
                      labelText: loc.date,
                      border: const OutlineInputBorder(),
                    ),
                    validator: FormBuilderValidators.required(),
                  ),
                ),
                const SizedBox(width: 16),
                Expanded(
                  child: FormBuilderDateTimePicker(
                    name: 'scheduledTime',
                    inputType: InputType.time,
                    decoration: InputDecoration(
                      labelText: loc.startTime,
                      border: const OutlineInputBorder(),
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                Expanded(
                  child: FormBuilderDateTimePicker(
                    name: 'endTime',
                    inputType: InputType.time,
                    decoration: InputDecoration(
                      labelText: loc.endTime,
                      border: const OutlineInputBorder(),
                    ),
                  ),
                ),
                const SizedBox(width: 16),
                Expanded(
                  child: FormBuilderTextField(
                    name: 'priority',
                    decoration: InputDecoration(
                      labelText: loc.priority,
                      border: const OutlineInputBorder(),
                    ),
                    keyboardType: TextInputType.number,
                    validator: FormBuilderValidators.compose([
                      FormBuilderValidators.integer(),
                    ]),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
          ],
        ),
      ),
    );
  }
}
