using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SACS.Application.Common.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream stream, string fileName, string containerName, string contentType, CancellationToken cancellationToken = default);
    Task<string> GeneratePresignedUrlAsync(string blobName, string containerName, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task DeleteAsync(string blobName, string containerName, CancellationToken cancellationToken = default);
}
