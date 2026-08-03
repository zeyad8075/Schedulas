class InstitutionSettingsDto {
  final String id;
  final String name;
  final String? logoUrl;
  final String timezone;

  const InstitutionSettingsDto({
    required this.id,
    required this.name,
    this.logoUrl,
    required this.timezone,
  });

  factory InstitutionSettingsDto.fromJson(Map<String, dynamic> json) =>
      InstitutionSettingsDto(
        id: json['id'] as String,
        name: json['name'] as String,
        logoUrl: json['logoUrl'] as String?,
        timezone: json['timezone'] as String,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'name': name,
        'logoUrl': logoUrl,
        'timezone': timezone,
      };
}
