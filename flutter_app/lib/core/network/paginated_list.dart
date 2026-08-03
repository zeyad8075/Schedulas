/// Mirrors Schedulas.Application.Common.Models.PaginatedList&lt;T&gt; exactly.
/// Every list endpoint in the API returns this shape inside the
/// ApiEnvelope's `data` field — built once here rather than re-parsed
/// per feature.
class PaginatedList<T> {
  final List<T> items;
  final int totalCount;
  final int pageNumber;
  final int pageSize;

  const PaginatedList({
    required this.items,
    required this.totalCount,
    required this.pageNumber,
    required this.pageSize,
  });

  int get totalPages => (totalCount / pageSize).ceil();
  bool get hasPreviousPage => pageNumber > 1;
  bool get hasNextPage => pageNumber < totalPages;

  factory PaginatedList.fromJson(
    Map<String, dynamic> json,
    T Function(Map<String, dynamic> json) fromItem,
  ) {
    return PaginatedList<T>(
      items: (json['items'] as List<dynamic>)
          .map((e) => fromItem(e as Map<String, dynamic>))
          .toList(),
      totalCount: json['totalCount'] as int,
      pageNumber: json['pageNumber'] as int,
      pageSize: json['pageSize'] as int,
    );
  }
}
