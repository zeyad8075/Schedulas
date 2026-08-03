import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final connectivityProvider = StreamProvider<List<ConnectivityResult>>((ref) {
  return Connectivity().onConnectivityChanged;
});

final isOfflineProvider = Provider<bool>((ref) {
  final connectivityState = ref.watch(connectivityProvider);
  return connectivityState.when(
    data: (results) {
      // In connectivity_plus >= 6.0.0, onConnectivityChanged returns a List<ConnectivityResult>
      return results.contains(ConnectivityResult.none) && results.length == 1;
    },
    loading: () => false, // Assume online while checking
    error: (_, __) => false, // Fallback to false on error
  );
});
