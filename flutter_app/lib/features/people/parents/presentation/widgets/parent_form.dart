import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import '../../../../../l10n/app_localizations.dart';
import '../../../profile/domain/models/profile_dto.dart';

class ParentForm extends StatelessWidget {
  final GlobalKey<FormBuilderState> formKey;
  final List<ProfileDto> availableProfiles;
  final VoidCallback onSubmit;
  final VoidCallback onCancel;

  const ParentForm({
    super.key,
    required this.formKey,
    required this.availableProfiles,
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
            name: 'profileId',
            decoration: InputDecoration(
              labelText: loc.profile,
              border: const OutlineInputBorder(),
            ),
            validator: FormBuilderValidators.required(
                errorText: loc.validationRequired),
            items: availableProfiles
                .map((p) => DropdownMenuItem(
                      value: p.id,
                      child: Text('${p.fullName} (${p.email})'),
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
