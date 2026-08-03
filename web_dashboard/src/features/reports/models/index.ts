export interface WorkloadReportDto {
  entityId: string;
  totalWorkloadMinutes: number;
  startDate: string;
  endDate: string;
}

export interface ActivityDistributionItemDto {
  activityType: string;
  count: number;
}

export interface ActivityDistributionReportDto {
  entityId: string;
  startDate: string;
  endDate: string;
  distribution: ActivityDistributionItemDto[];
}
