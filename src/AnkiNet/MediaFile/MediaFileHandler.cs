namespace AnkiNet.MediaFile;

internal static class MediaFileHandler
{
    /// <summary>
    /// Media file is not handled by Anki.NET, this writes an empty file.
    /// </summary>
    /// <param name="mediaFilePath">File to write media to.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous write operation.</returns>
    public static async Task WriteMediaFile(string mediaFilePath, AnkiCollection _)
	{
        await using var writer = new StreamWriter(
            stream: File.OpenWrite(mediaFilePath),
            leaveOpen: false);

        await writer.WriteAsync("{}").ConfigureAwait(false);
    }
}