using Mediator;

namespace Couture.Dashboard.Features.ExportReport;

public sealed record ExportReportQuery(int Year, int Quarter, string Format) : IQuery<ExportResult>;

public sealed record ExportResult(byte[] Data, string ContentType, string FileName);
