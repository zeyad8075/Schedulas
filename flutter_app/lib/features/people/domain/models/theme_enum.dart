enum Theme { light, dark, system }

extension ThemeExtension on Theme {
  static Theme fromInt(int value) {
    return Theme.values.firstWhere(
      (e) => e.index == value,
      orElse: () => Theme.system,
    );
  }
}
