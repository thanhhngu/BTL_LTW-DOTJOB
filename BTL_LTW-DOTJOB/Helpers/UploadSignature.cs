namespace BTL_LTW_DOTJOB.Helpers
{
    public static class UploadSignature
    {
        private static readonly Dictionary<string, byte[][]> Signatures = new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } },
            [".docx"] = new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } },
            [".doc"] = new[] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } },
            [".png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
            [".jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } }
        };

        public static async Task<bool> MatchesExtensionAsync(IFormFile file, string extension)
        {
            if (!Signatures.TryGetValue(extension, out var expectedSignatures))
            {
                return false;
            }

            var header = new byte[8];
            int bytesRead;
            await using (var stream = file.OpenReadStream())
            {
                bytesRead = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);
            }

            return expectedSignatures.Any(signature =>
                bytesRead >= signature.Length && header.AsSpan(0, signature.Length).SequenceEqual(signature));
        }
    }
}
