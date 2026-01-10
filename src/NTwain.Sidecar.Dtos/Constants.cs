namespace NTwain.Sidecar.Dtos;

/// <summary>
/// TWAIN Direct API error codes.
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// Invalid session identifier.
    /// </summary>
    public const string InvalidSessionId = "invalidSessionId";

    /// <summary>
    /// Session already exists.
    /// </summary>
    public const string SessionAlreadyExists = "sessionAlreadyExists";

    /// <summary>
    /// No session exists.
    /// </summary>
    public const string NoSession = "noSession";

    /// <summary>
    /// Invalid state for the requested operation.
    /// </summary>
    public const string InvalidState = "invalidState";

    /// <summary>
    /// Invalid command.
    /// </summary>
    public const string InvalidCommand = "invalidCommand";

    /// <summary>
    /// Invalid parameter value.
    /// </summary>
    public const string InvalidValue = "invalidValue";

    /// <summary>
    /// Resource not found.
    /// </summary>
    public const string NotFound = "notFound";

    /// <summary>
    /// Operation busy.
    /// </summary>
    public const string Busy = "busy";

    /// <summary>
    /// Device communication error.
    /// </summary>
    public const string CommunicationError = "communicationError";

    /// <summary>
    /// Device offline.
    /// </summary>
    public const string DeviceOffline = "deviceOffline";

    /// <summary>
    /// Authorization required.
    /// </summary>
    public const string AuthorizationRequired = "authorizationRequired";

    /// <summary>
    /// Invalid X-Privet-Token.
    /// </summary>
    public const string InvalidXPrivetToken = "invalidXPrivetToken";

    /// <summary>
    /// Timeout occurred.
    /// </summary>
    public const string Timeout = "timeout";

    /// <summary>
    /// Paper jam detected.
    /// </summary>
    public const string PaperJam = "paperJam";

    /// <summary>
    /// Paper empty/feeder empty.
    /// </summary>
    public const string PaperEmpty = "paperEmpty";

    /// <summary>
    /// Double feed detected.
    /// </summary>
    public const string DoubleFeed = "doubleFeed";

    /// <summary>
    /// Cover open.
    /// </summary>
    public const string CoverOpen = "coverOpen";

    /// <summary>
    /// Internal error.
    /// </summary>
    public const string InternalError = "internalError";
}

/// <summary>
/// TWAIN Direct session states as string constants.
/// </summary>
public static class SessionStates
{
    /// <summary>
    /// No session exists.
    /// </summary>
    public const string NoSession = "noSession";

    /// <summary>
    /// Session is ready for commands.
    /// </summary>
    public const string Ready = "ready";

    /// <summary>
    /// Scanner is capturing images.
    /// </summary>
    public const string Capturing = "capturing";

    /// <summary>
    /// Scanner is closed.
    /// </summary>
    public const string Closed = "closed";

    /// <summary>
    /// Scanner is draining remaining images.
    /// </summary>
    public const string Draining = "draining";
}

/// <summary>
/// TWAIN Direct detected status values.
/// </summary>
public static class DetectedStatuses
{
    /// <summary>
    /// Normal operation.
    /// </summary>
    public const string Nominal = "nominal";

    /// <summary>
    /// Operation successful with info.
    /// </summary>
    public const string Success = "success";

    /// <summary>
    /// Paper jam detected.
    /// </summary>
    public const string PaperJam = "paperJam";

    /// <summary>
    /// Paper double feed detected.
    /// </summary>
    public const string PaperDoubleFeed = "paperDoubleFeed";

    /// <summary>
    /// Automatic document feeder is empty.
    /// </summary>
    public const string AdfEmpty = "adfEmpty";

    /// <summary>
    /// Feeder is empty.
    /// </summary>
    public const string FeederEmpty = "feederEmpty";

    /// <summary>
    /// Cover is open.
    /// </summary>
    public const string CoverOpen = "coverOpen";

    /// <summary>
    /// Interlock error.
    /// </summary>
    public const string Interlock = "interlock";

    /// <summary>
    /// Paper size error.
    /// </summary>
    public const string PaperSize = "paperSize";

    /// <summary>
    /// Image file write error.
    /// </summary>
    public const string ImageFileWriteError = "imageFileWriteError";

    /// <summary>
    /// No media in flatbed.
    /// </summary>
    public const string FlatbedEmpty = "flatbedEmpty";

    /// <summary>
    /// Device problem.
    /// </summary>
    public const string DeviceProblem = "deviceProblem";

    /// <summary>
    /// Unknown or unspecified condition.
    /// </summary>
    public const string Unknown = "unknown";

    /// <summary>
    /// End of job flag indicating scan completion.
    /// </summary>
    public const string EndOfJob = "endOfJob";

    /// <summary>
    /// Next action requested.
    /// </summary>
    public const string NextAction = "nextAction";
}

/// <summary>
/// TWAIN Direct method names.
/// </summary>
public static class MethodNames
{
    /// <summary>
    /// Create a new session.
    /// </summary>
    public const string CreateSession = "createSession";

    /// <summary>
    /// Close the current session.
    /// </summary>
    public const string CloseSession = "closeSession";

    /// <summary>
    /// Get current session state.
    /// </summary>
    public const string GetSession = "getSession";

    /// <summary>
    /// Get scanner information.
    /// </summary>
    public const string InfoEx = "infoex";

    /// <summary>
    /// Send a task to the scanner.
    /// </summary>
    public const string SendTask = "sendTask";

    /// <summary>
    /// Start capturing images.
    /// </summary>
    public const string StartCapturing = "startCapturing";

    /// <summary>
    /// Stop capturing images.
    /// </summary>
    public const string StopCapturing = "stopCapturing";

    /// <summary>
    /// Read an image block.
    /// </summary>
    public const string ReadImageBlock = "readImageBlock";

    /// <summary>
    /// Read image block metadata only.
    /// </summary>
    public const string ReadImageBlockMetadata = "readImageBlockMetadata";

    /// <summary>
    /// Release processed image blocks.
    /// </summary>
    public const string ReleaseImageBlocks = "releaseImageBlocks";

    /// <summary>
    /// Wait for events.
    /// </summary>
    public const string WaitForEvents = "waitForEvents";
}

/// <summary>
/// Kind values for TWAIN Direct requests.
/// </summary>
public static class KindValues
{
    /// <summary>
    /// TWAIN Local scanner kind.
    /// </summary>
    public const string TwainLocalScanner = "twainlocalscanner";

    /// <summary>
    /// TWAIN Cloud scanner kind.
    /// </summary>
    public const string TwainCloudScanner = "twaincloudscanner";
}

/// <summary>
/// HTTP header names used in TWAIN Direct.
/// </summary>
public static class HttpHeaders
{
    /// <summary>
    /// X-Privet-Token header for authentication.
    /// </summary>
    public const string XPrivetToken = "X-Privet-Token";

    /// <summary>
    /// Content-Type header.
    /// </summary>
    public const string ContentType = "Content-Type";

    /// <summary>
    /// Accept header.
    /// </summary>
    public const string Accept = "Accept";
}

/// <summary>
/// MIME types used in TWAIN Direct.
/// </summary>
public static class MimeTypes
{
    /// <summary>
    /// JSON content type.
    /// </summary>
    public const string ApplicationJson = "application/json";

    /// <summary>
    /// PDF raster content type.
    /// </summary>
    public const string ApplicationPdfRaster = "application/pdf";

    /// <summary>
    /// TIFF content type.
    /// </summary>
    public const string ImageTiff = "image/tiff";

    /// <summary>
    /// JPEG content type.
    /// </summary>
    public const string ImageJpeg = "image/jpeg";

    /// <summary>
    /// PNG content type.
    /// </summary>
    public const string ImagePng = "image/png";

    /// <summary>
    /// Octet stream (binary data).
    /// </summary>
    public const string ApplicationOctetStream = "application/octet-stream";

    /// <summary>
    /// Multipart content type.
    /// </summary>
    public const string MultipartMixed = "multipart/mixed";
}

/// <summary>
/// Event types for TWAIN Direct sessions.
/// </summary>
public static class SessionEventTypes
{
    /// <summary>
    /// Image blocks are available for reading.
    /// </summary>
    public const string ImageBlocks = "imageBlocks";

    /// <summary>
    /// Session has been closed.
    /// </summary>
    public const string SessionClosed = "sessionClosed";

    /// <summary>
    /// Scanner status has changed.
    /// </summary>
    public const string StatusChanged = "statusChanged";

    /// <summary>
    /// Scanning has completed.
    /// </summary>
    public const string ScanDone = "scanDone";
}

/// <summary>
/// Common attribute names used in TWAIN Direct tasks.
/// </summary>
public static class TaskAttributeNames
{
    /// <summary>
    /// Compression attribute.
    /// </summary>
    public const string Compression = "compression";

    /// <summary>
    /// Resolution attribute.
    /// </summary>
    public const string Resolution = "resolution";

    /// <summary>
    /// Number of sheets attribute.
    /// </summary>
    public const string NumberOfSheets = "numberOfSheets";

    /// <summary>
    /// Cropping attribute.
    /// </summary>
    public const string Cropping = "cropping";

    /// <summary>
    /// Automatic deskew attribute.
    /// </summary>
    public const string AutomaticDeskew = "automaticDeskew";

    /// <summary>
    /// Image merge attribute.
    /// </summary>
    public const string ImageMerge = "imageMerge";

    /// <summary>
    /// Automatic color detection attribute.
    /// </summary>
    public const string AutomaticColorDetect = "automaticColorDetect";

    /// <summary>
    /// Brightness attribute.
    /// </summary>
    public const string Brightness = "brightness";

    /// <summary>
    /// Contrast attribute.
    /// </summary>
    public const string Contrast = "contrast";

    /// <summary>
    /// Threshold attribute (for B&amp;W).
    /// </summary>
    public const string Threshold = "threshold";

    /// <summary>
    /// DoubleFeed detection attribute.
    /// </summary>
    public const string DoubleFeedDetection = "doubleFeedDetection";

    /// <summary>
    /// Sheetcount attribute.
    /// </summary>
    public const string Sheetcount = "sheetcount";
}

/// <summary>
/// Common action names used in TWAIN Direct tasks.
/// </summary>
public static class TaskActionNames
{
    /// <summary>
    /// Configure action.
    /// </summary>
    public const string Configure = "configure";

    /// <summary>
    /// Encrypt PDF passwords action.
    /// </summary>
    public const string EncryptPdfPassword = "encryptPdfPassword";
}

/// <summary>
/// Common source names used in TWAIN Direct tasks.
/// </summary>
public static class TaskSourceNames
{
    /// <summary>
    /// Any available source.
    /// </summary>
    public const string Any = "any";

    /// <summary>
    /// Automatic document feeder.
    /// </summary>
    public const string Adf = "adf";

    /// <summary>
    /// Front side of duplex ADF.
    /// </summary>
    public const string AdfFront = "adfFront";

    /// <summary>
    /// Rear side of duplex ADF.
    /// </summary>
    public const string AdfRear = "adfRear";

    /// <summary>
    /// Flatbed scanner.
    /// </summary>
    public const string Flatbed = "flatbed";

    /// <summary>
    /// Feeder front.
    /// </summary>
    public const string FeederFront = "feederFront";

    /// <summary>
    /// Feeder rear.
    /// </summary>
    public const string FeederRear = "feederRear";
}

/// <summary>
/// Common pixel format names used in TWAIN Direct tasks.
/// </summary>
public static class TaskPixelFormatNames
{
    /// <summary>
    /// Black and white (1-bit).
    /// </summary>
    public const string Bw1 = "bw1";

    /// <summary>
    /// 8-bit grayscale.
    /// </summary>
    public const string Gray8 = "gray8";

    /// <summary>
    /// 16-bit grayscale.
    /// </summary>
    public const string Gray16 = "gray16";

    /// <summary>
    /// 24-bit RGB color.
    /// </summary>
    public const string Rgb24 = "rgb24";

    /// <summary>
    /// 48-bit RGB color.
    /// </summary>
    public const string Rgb48 = "rgb48";
}

/// <summary>
/// Common compression names used in TWAIN Direct tasks.
/// </summary>
public static class TaskCompressionNames
{
    /// <summary>
    /// No compression.
    /// </summary>
    public const string None = "none";

    /// <summary>
    /// CCITT Group 4 compression.
    /// </summary>
    public const string Group4 = "group4";

    /// <summary>
    /// JPEG compression.
    /// </summary>
    public const string Jpeg = "jpeg";
}
