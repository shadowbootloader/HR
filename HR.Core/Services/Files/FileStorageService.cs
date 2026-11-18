using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace HR.Services.Files
{
    public enum FileKind
    {
        Image,
        Document,
        Audio
    }

    public sealed record StoredFile(
        string RelativePath,
        string OriginalFileName,
        string ContentType,
        long SizeBytes,
        string Sha256);

    /// <summary>
    /// فایل‌ها زیر یک ریشه قابل تنظیم (مسیر محلی یا اشتراک شبکه) با نام GUID ذخیره می‌شوند.
    /// </summary>
    public sealed class FileStorageService
    {
        public const string EnvironmentVariable = "HR_FILE_STORAGE";
        public const long DefaultMaxImageBytes = 10 * 1024 * 1024;
        public const long DefaultMaxDocumentBytes = 20 * 1024 * 1024;

        private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp"
        };

        private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".txt", ".docx"
        };

        private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3", ".wav", ".m4a"
        };

        private readonly long _maxImageBytes;
        private readonly long _maxDocumentBytes;

        public FileStorageService(string root, long? maxImageBytes = null, long? maxDocumentBytes = null)
        {
            if (string.IsNullOrWhiteSpace(root))
                throw new ArgumentException("ریشه ذخیره فایل خالی است.", nameof(root));

            Root = Path.GetFullPath(root);
            Directory.CreateDirectory(Root);
            _maxImageBytes = maxImageBytes ?? DefaultMaxImageBytes;
            _maxDocumentBytes = maxDocumentBytes ?? DefaultMaxDocumentBytes;
        }

        public string Root { get; }

        public static FileStorageService Load()
        {
            var fromEnv = Environment.GetEnvironmentVariable(EnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(fromEnv))
                return new FileStorageService(fromEnv.Trim());

            var configured = ReadConfiguredRoot();
            if (!string.IsNullOrWhiteSpace(configured))
                return new FileStorageService(configured.Trim());

            var shared = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "HRSuite",
                "files");
            return new FileStorageService(shared);
        }

        public StoredFile Stage(string sourcePath, FileKind kind)
        {
            var stored = CopyInto(sourcePath, kind, "_staging");
            return stored;
        }

        /// <summary>فایل مرحله‌ای یا مسیر قدیمی را به پوشه مالک منتقل می‌کند.</summary>
        public StoredFile Promote(string storedOrAbsolute, string ownerFolder)
        {
            var full = ResolveExisting(storedOrAbsolute);
            if (full.Length == 0 || !File.Exists(full))
                throw new FileNotFoundException("فایل برای انتقال پیدا نشد.", storedOrAbsolute);

            var owner = ownerFolder.Replace('\\', '/').Trim('/');
            if (owner.Contains("..", StringComparison.Ordinal))
                throw new InvalidOperationException("پوشه مالک نامعتبر است.");

            var currentRelative = TryRelative(full);
            var prefix = owner + "/";
            if (currentRelative != null &&
                currentRelative.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return Describe(full, currentRelative);
            }

            var extension = Path.GetExtension(full);
            var relative = $"{owner}/{Guid.NewGuid():N}{extension}";
            var destination = CombineSafe(relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

            if (IsUnderRoot(full))
                File.Move(full, destination);
            else
                File.Copy(full, destination, overwrite: false);

            MoveMeta(full, destination);
            return Describe(destination, relative);
        }

        public string FullPath(string? stored)
        {
            if (string.IsNullOrWhiteSpace(stored)) return string.Empty;

            if (Path.IsPathRooted(stored))
                return stored;

            var combined = CombineSafe(stored);
            if (File.Exists(combined)) return combined;

            var legacy = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "HRSuite",
                "Media",
                Path.GetFileName(stored));
            return File.Exists(legacy) ? legacy : combined;
        }

        public bool TryDelete(string? stored)
        {
            var relative = TryRelative(ResolveExisting(stored ?? string.Empty));
            if (relative == null) return false;

            var full = CombineSafe(relative);
            var removed = false;
            if (File.Exists(full))
            {
                File.Delete(full);
                removed = true;
            }

            var meta = MetaPath(full);
            if (File.Exists(meta))
            {
                File.Delete(meta);
                removed = true;
            }

            return removed;
        }

        public int DeleteUnreferenced(IEnumerable<string> referencedRelativePaths)
        {
            var keep = new HashSet<string>(
                referencedRelativePaths
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Replace('\\', '/').TrimStart('/')),
                StringComparer.OrdinalIgnoreCase);

            var removed = 0;
            if (!Directory.Exists(Root)) return 0;

            foreach (var file in Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta.json", StringComparison.OrdinalIgnoreCase)) continue;

                var relative = Path.GetRelativePath(Root, file).Replace('\\', '/');
                if (keep.Contains(relative)) continue;

                File.Delete(file);
                var meta = MetaPath(file);
                if (File.Exists(meta)) File.Delete(meta);
                removed++;
            }

            return removed;
        }

        public static string ComputeHash(string fullPath)
        {
            using var stream = File.OpenRead(fullPath);
            var hash = SHA256.HashData(stream);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public static string? MatchingPath(IEnumerable<(string Sha256, string RelativePath, bool IsDeleted)> documents, string hash)
        {
            foreach (var document in documents)
            {
                if (!document.IsDeleted && string.Equals(document.Sha256, hash, StringComparison.OrdinalIgnoreCase))
                    return document.RelativePath;
            }

            return null;
        }

        private StoredFile CopyInto(string sourcePath, FileKind kind, string folder)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                throw new FileNotFoundException("فایل منبع پیدا نشد.", sourcePath);

            var extension = Path.GetExtension(sourcePath);
            if (!IsAllowed(extension, kind))
                throw new InvalidOperationException($"پسوند {extension} برای این نوع فایل مجاز نیست.");

            var length = new FileInfo(sourcePath).Length;
            var limit = kind == FileKind.Image ? _maxImageBytes : _maxDocumentBytes;
            if (length <= 0 || length > limit)
                throw new InvalidOperationException($"اندازه فایل باید بین ۱ بایت و {limit} بایت باشد.");

            var relative = $"{folder}/{Guid.NewGuid():N}{extension}";
            var destination = CombineSafe(relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(sourcePath, destination, overwrite: false);

            var stored = Describe(destination, relative, Path.GetFileName(sourcePath));
            WriteMeta(destination, stored);
            return stored;
        }

        private StoredFile Describe(string fullPath, string relative, string? originalName = null)
        {
            var meta = ReadMeta(fullPath);
            var name = originalName
                       ?? meta?.OriginalFileName
                       ?? Path.GetFileName(fullPath);
            var contentType = meta?.ContentType ?? ContentTypeFor(Path.GetExtension(fullPath));
            var hash = string.IsNullOrWhiteSpace(meta?.Sha256) ? ComputeHash(fullPath) : meta!.Sha256;
            var size = new FileInfo(fullPath).Length;
            var stored = new StoredFile(relative.Replace('\\', '/'), name, contentType, size, hash);
            if (meta == null || string.IsNullOrWhiteSpace(meta.Sha256))
                WriteMeta(fullPath, stored);
            return stored;
        }

        private string ResolveExisting(string stored)
        {
            if (string.IsNullOrWhiteSpace(stored)) return string.Empty;
            if (Path.IsPathRooted(stored) && File.Exists(stored)) return stored;
            var combined = FullPath(stored);
            return File.Exists(combined) ? combined : string.Empty;
        }

        private string? TryRelative(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath) || !IsUnderRoot(fullPath)) return null;
            return Path.GetRelativePath(Root, fullPath).Replace('\\', '/');
        }

        private bool IsUnderRoot(string fullPath)
        {
            var root = Path.GetFullPath(Root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var full = Path.GetFullPath(fullPath);
            return full.StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }

        private string CombineSafe(string relative)
        {
            var cleaned = relative.Replace('\\', '/').TrimStart('/');
            if (cleaned.Contains("..", StringComparison.Ordinal))
                throw new InvalidOperationException("مسیر فایل نامعتبر است.");

            var full = Path.GetFullPath(Path.Combine(Root, cleaned.Replace('/', Path.DirectorySeparatorChar)));
            if (!IsUnderRoot(full) && !string.Equals(Path.GetFullPath(full), Path.GetFullPath(Root), StringComparison.OrdinalIgnoreCase))
            {
                var root = Path.GetFullPath(Root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("مسیر فایل خارج از پوشه ذخیره است.");
            }

            return full;
        }

        private static bool IsAllowed(string extension, FileKind kind)
        {
            if (ImageExtensions.Contains(extension)) return true;
            if (kind == FileKind.Document && DocumentExtensions.Contains(extension)) return true;
            return kind == FileKind.Audio && AudioExtensions.Contains(extension);
        }

        public static string ContentTypeFor(string extension) => extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".bmp" => "image/bmp",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

        private static string MetaPath(string fullPath) => fullPath + ".meta.json";

        private static void WriteMeta(string fullPath, StoredFile stored)
        {
            var json = JsonSerializer.Serialize(new FileMeta
            {
                OriginalFileName = stored.OriginalFileName,
                ContentType = stored.ContentType,
                Sha256 = stored.Sha256
            });
            File.WriteAllText(MetaPath(fullPath), json);
        }

        private static FileMeta? ReadMeta(string fullPath)
        {
            var path = MetaPath(fullPath);
            if (!File.Exists(path)) return null;
            try
            {
                return JsonSerializer.Deserialize<FileMeta>(File.ReadAllText(path));
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static void MoveMeta(string sourceFull, string destinationFull)
        {
            var sourceMeta = MetaPath(sourceFull);
            var destinationMeta = MetaPath(destinationFull);
            if (!File.Exists(sourceMeta)) return;
            if (File.Exists(destinationMeta)) File.Delete(destinationMeta);
            File.Move(sourceMeta, destinationMeta);
        }

        private static string? ReadConfiguredRoot()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path)) return null;

            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                if (document.RootElement.TryGetProperty("FileStorage", out var section) &&
                    section.TryGetProperty("RootPath", out var value))
                {
                    return value.GetString();
                }
            }
            catch (JsonException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }

            return null;
        }

        private sealed class FileMeta
        {
            public string OriginalFileName { get; set; } = string.Empty;
            public string ContentType { get; set; } = string.Empty;
            public string Sha256 { get; set; } = string.Empty;
        }
    }
}
