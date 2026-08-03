import { useSearchParams } from 'react-router-dom';

export const useTablePagination = () => {
  const [searchParams, setSearchParams] = useSearchParams();

  const page = parseInt(searchParams.get('page') || '1', 10);
  const pageSize = parseInt(searchParams.get('pageSize') || '20', 10);
  const searchTerm = searchParams.get('search') || '';
  const sortBy = searchParams.get('sortBy') || '';
  const sortDescending = searchParams.get('sortDescending') === 'true';

  const setPage = (newPage: number) => {
    searchParams.set('page', newPage.toString());
    setSearchParams(searchParams);
  };

  const setPageSize = (newPageSize: number) => {
    searchParams.set('pageSize', newPageSize.toString());
    searchParams.set('page', '1'); // Reset to page 1 on page size change
    setSearchParams(searchParams);
  };

  const setSearchTerm = (term: string) => {
    if (term) {
      searchParams.set('search', term);
    } else {
      searchParams.delete('search');
    }
    searchParams.set('page', '1');
    setSearchParams(searchParams);
  };

  const setSorting = (newSortBy: string, isDescending: boolean) => {
    if (newSortBy) {
      searchParams.set('sortBy', newSortBy);
      searchParams.set('sortDescending', isDescending.toString());
    } else {
      searchParams.delete('sortBy');
      searchParams.delete('sortDescending');
    }
    setSearchParams(searchParams);
  };

  return {
    page,
    pageSize,
    searchTerm,
    sortBy,
    sortDescending,
    setPage,
    setPageSize,
    setSearchTerm,
    setSorting,
  };
};
