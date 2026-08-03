import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import '../../../../../l10n/app_localizations.dart';
import '../../domain/models/profile_dto.dart';

class ProfileForm extends StatelessWidget {
  final GlobalKey<FormBuilderState> formKey;
  final ProfileDto? initialData;
  final VoidCallback onSubmit;
  final VoidCallback onCancel;

  const ProfileForm({
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
      initialValue: {
        if (initialData != null) 'fullName': initialData!.fullName,
        if (initialData != null) 'phoneNumber': initialData!.phoneNumber,
      },
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          FormBuilderTextField(
            name: 'fullName',
            decoration: InputDecoration(
              labelText: loc.fullName,
              border: const OutlineInputBorder(),
            ),
            validator: FormBuilderValidators.compose([
              FormBuilderValidators.required(errorText: loc.validationRequired),
              FormBuilderValidators.maxLength(255,
                  errorText: loc.validationMaxLength(255)),
            ]),
          ),
          const SizedBox(height: 16),
          FormBuilderTextField(
            name: 'phoneNumber',
            decoration: InputDecoration(
              labelText: loc.phoneNumber,
              border: const OutlineInputBorder(),
            ),
            validator: FormBuilderValidators.maxLength(20,
                errorText: loc.validationMaxLength(20)),
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
