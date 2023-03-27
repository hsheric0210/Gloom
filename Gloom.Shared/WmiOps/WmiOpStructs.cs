namespace Gloom.WmiOps
{
	[Serializable]
	public struct WmiInfoRequest
	{
		public Guid WmiOp { get; set; }
	}

	[Serializable]
	public struct WmiInfoResponse
	{
		public Guid WmiOp { get; set; }
		public byte[] Data { get; set; }
	}

	#region Process List
	[Serializable]
	public struct ProcessListResponse
	{
		public Win32Process[] List { get; set; }
	}

	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-process
	/// </summary>
	[Serializable]
	public struct Win32Process
	{
		public string CreationClassName { get; set; }
		public string Caption { get; set; }
		public string CommandLine { get; set; }
		public DateTime CreationDate { get; set; }
		public string CSCreationClassName { get; set; }
		public string CSName { get; set; }
		public string Description { get; set; }
		public string ExecutablePath { get; set; }
		public ushort ExecutionState { get; set; }
		public string Handle { get; set; }
		public uint HandleCount { get; set; }
		public DateTime InstallDate { get; set; }
		public ulong KernelModeTime { get; set; }
		public uint MaximumWorkingSetSize { get; set; }
		public uint MinimumWorkingSetSize { get; set; }
		public string Name { get; set; }
		public string OSCreationClassName { get; set; }
		public string OSName { get; set; }
		public ulong OtherOperationCount { get; set; }
		public ulong OtherTransferCount { get; set; }
		public uint PageFaults { get; set; }
		public uint PageFileUsage { get; set; }
		public uint ParentProcessId { get; set; }
		public uint PeakPageFileUsage { get; set; }
		public ulong PeakVirtualSize { get; set; }
		public uint PeakWorkingSetSize { get; set; }
		public uint Priority { get; set; }
		public ulong PrivatePageCount { get; set; }
		public uint ProcessId { get; set; }
		public uint QuotaNonPagedPoolUsage { get; set; }
		public uint QuotaPagedPoolUsage { get; set; }
		public uint QuotaPeakNonPagedPoolUsage { get; set; }
		public uint QuotaPeakPagedPoolUsage { get; set; }
		public ulong ReadOperationCount { get; set; }
		public ulong ReadTransferCount { get; set; }
		public uint SessionId { get; set; }
		public string Status { get; set; }
		public DateTime TerminationDate { get; set; }
		public uint ThreadCount { get; set; }
		public ulong UserModeTime { get; set; }
		public ulong VirtualSize { get; set; }
		public ulong WriteOperationCount { get; set; }
		public ulong WriteTransferCount { get; set; }
	}
	#endregion

	#region Service List
	[Serializable]
	public struct ServiceListResponse
	{
		public Win32Service[] List { get; set; }
	}

	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-service
	/// </summary>
	[Serializable]
	public struct Win32Service
	{
		public bool AcceptPause { get; set; }
		public bool AcceptStop { get; set; }
		public string Caption { get; set; }
		public uint CheckPoint { get; set; }
		public string CreationClassName { get; set; }
		public bool DelayedAutoStart { get; set; }
		public string Description { get; set; }
		public bool DesktopInteract { get; set; }
		public string DisplayName { get; set; }
		public string ErrorControl { get; set; }
		public uint ExitCode { get; set; }
		public DateTime InstallDate { get; set; }
		public string Name { get; set; }
		public string PathName { get; set; }
		public uint ProcessId { get; set; }
		public uint ServiceSpecificExitCode { get; set; }
		public string ServiceType { get; set; }
		public bool Started { get; set; }
		public string StartMode { get; set; }
		public string StartName { get; set; }
		public string State { get; set; }
		public string Status { get; set; }
		public string SystemCreationClassName { get; set; }
		public string SystemName { get; set; }
		public uint TagId { get; set; }
		public uint WaitHint { get; set; }
	}
	#endregion

	#region Hardware Info
	[Serializable]
	public struct HardwareInfoResponse
	{
		public Win32Bios Bios { get; set; }
		public Win32BaseBoard BaseBoard { get; set; }
		public Win32Keyboard[] Keyboards { get; set; }
		public Win32NetworkAdapter[] NetworkAdapters { get; set; }
		public Win32PhysicalMemory[] PhysicalMemories { get; set; }
		public Win32PointingDevice[] PointingDevices { get; set; }
		public Win32VideoController[] VideoControllers { get; set; }
	}

	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-bios
	/// </summary>
	[Serializable]
	public struct Win32Bios
	{
		public ushort[] BiosCharacteristics { get; set; }
		public string[] BIOSVersion { get; set; }
		public string BuildNumber { get; set; }
		public string Caption { get; set; }
		public string CodeSet { get; set; }
		public string CurrentLanguage { get; set; }
		public string Description { get; set; }
		public byte EmbeddedControllerMajorVersion { get; set; }
		public byte EmbeddedControllerMinorVersion { get; set; }
		public string IdentificationCode { get; set; }
		public ushort InstallableLanguages { get; set; }
		public DateTime InstallDate { get; set; }
		public string LanguageEdition { get; set; }
		public string[] ListOfLanguages { get; set; }
		public string Manufacturer { get; set; }
		public string Name { get; set; }
		public string OtherTargetOS { get; set; }
		public bool PrimaryBIOS { get; set; }
		public DateTime ReleaseDate { get; set; }
		public string SerialNumber { get; set; }
		public string SMBIOSBIOSVersion { get; set; }
		public ushort SMBIOSMajorVersion { get; set; }
		public ushort SMBIOSMinorVersion { get; set; }
		public bool SMBIOSPresent { get; set; }
		public string SoftwareElementID { get; set; }
		public ushort SoftwareElementState { get; set; }
		public string Status { get; set; }
		public byte SystemBiosMajorVersion { get; set; }
		public byte SystemBiosMinorVersion { get; set; }
		public ushort TargetOperatingSystem { get; set; }
		public string Version { get; set; }
	}

	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-baseboard
	/// </summary>
	[Serializable]
	public struct Win32BaseBoard
	{
		public string Caption { get; set; }
		public string[] ConfigOptions { get; set; }
		public string CreationClassName { get; set; }
		public float Depth { get; set; }
		public string Description { get; set; }
		public float Height { get; set; }
		public bool HostingBoard { get; set; }
		public bool HotSwappable { get; set; }
		public DateTime InstallDate { get; set; }
		public string Manufacturer { get; set; }
		public string Model { get; set; }
		public string Name { get; set; }
		public string OtherIdentifyingInfo { get; set; }
		public string PartNumber { get; set; }
		public bool PoweredOn { get; set; }
		public string Product { get; set; }
		public bool Removable { get; set; }
		public bool Replaceable { get; set; }
		public string RequirementsDescription { get; set; }
		public bool RequiresDaughterBoard { get; set; }
		public string SerialNumber { get; set; }
		public string SKU { get; set; }
		public string SlotLayout { get; set; }
		public bool SpecialRequirements { get; set; }
		public string Status { get; set; }
		public string Tag { get; set; }
		public string Version { get; set; }
		public float Weight { get; set; }
		public float Width { get; set; }
	}

	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-keyboard
	/// </summary>
	[Serializable]
	public struct Win32Keyboard
	{
		public ushort Availability { get; set; }
		public string Caption { get; set; }
		public uint ConfigManagerErrorCode { get; set; }
		public bool ConfigManagerUserConfig { get; set; }
		public string CreationClassName { get; set; }
		public string Description { get; set; }
		public string DeviceID { get; set; }
		public bool ErrorCleared { get; set; }
		public string ErrorDescription { get; set; }
		public DateTime InstallDate { get; set; }
		public bool IsLocked { get; set; }
		public uint LastErrorCode { get; set; }
		public string Layout { get; set; }
		public string Name { get; set; }
		public ushort NumberOfFunctionKeys { get; set; }
		public ushort Password { get; set; }
		public string PNPDeviceID { get; set; }
		public ushort[] PowerManagementCapabilities { get; set; }
		public bool PowerManagementSupported { get; set; }
		public string Status { get; set; }
		public ushort StatusInfo { get; set; }
		public string SystemCreationClassName { get; set; }
		public string SystemName { get; set; }
	}

	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-networkadapter
	/// </summary>
	[Serializable]
	public struct Win32NetworkAdapter
	{
		public string AdapterType { get; set; }
		public ushort AdapterTypeID { get; set; }
		public bool AutoSense { get; set; }
		public ushort Availability { get; set; }
		public string Caption { get; set; }
		public uint ConfigManagerErrorCode { get; set; }
		public bool ConfigManagerUserConfig { get; set; }
		public string CreationClassName { get; set; }
		public string Description { get; set; }
		public string DeviceID { get; set; }
		public bool ErrorCleared { get; set; }
		public string ErrorDescription { get; set; }
		public string GUID { get; set; }
		public uint Index { get; set; }
		public DateTime InstallDate { get; set; }
		public bool Installed { get; set; }
		public uint InterfaceIndex { get; set; }
		public uint LastErrorCode { get; set; }
		public string MACAddress { get; set; }
		public string Manufacturer { get; set; }
		public uint MaxNumberControlled { get; set; }
		public ulong MaxSpeed { get; set; }
		public string Name { get; set; }
		public string NetConnectionID { get; set; }
		public ushort NetConnectionStatus { get; set; }
		public bool NetEnabled { get; set; }
		public string[] NetworkAddresses { get; set; }
		public string PermanentAddress { get; set; }
		public bool PhysicalAdapter { get; set; }
		public string PNPDeviceID { get; set; }
		public ushort[] PowerManagementCapabilities { get; set; }
		public bool PowerManagementSupported { get; set; }
		public string ProductName { get; set; }
		public string ServiceName { get; set; }
		public ulong Speed { get; set; }
		public string Status { get; set; }
		public ushort StatusInfo { get; set; }
		public string SystemCreationClassName { get; set; }
		public string SystemName { get; set; }
		public DateTime TimeOfLastReset { get; set; }
	}


	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-physicalmemory
	/// </summary>
	[Serializable]
	public struct Win32PhysicalMemory
	{
		public uint Attributes { get; set; }
		public string BankLabel { get; set; }
		public ulong Capacity { get; set; }
		public string Caption { get; set; }
		public uint ConfiguredClockSpeed { get; set; }
		public uint ConfiguredVoltage { get; set; }
		public string CreationClassName { get; set; }
		public ushort DataWidth { get; set; }
		public string Description { get; set; }
		public string DeviceLocator { get; set; }
		public ushort FormFactor { get; set; }
		public bool HotSwappable { get; set; }
		public DateTime InstallDate { get; set; }
		public ushort InterleaveDataDepth { get; set; }
		public uint InterleavePosition { get; set; }
		public string Manufacturer { get; set; }
		public uint MaxVoltage { get; set; }
		public ushort MemoryType { get; set; }
		public uint MinVoltage { get; set; }
		public string Model { get; set; }
		public string Name { get; set; }
		public string OtherIdentifyingInfo { get; set; }
		public string PartNumber { get; set; }
		public uint PositionInRow { get; set; }
		public bool PoweredOn { get; set; }
		public bool Removable { get; set; }
		public bool Replaceable { get; set; }
		public string SerialNumber { get; set; }
		public string SKU { get; set; }
		public uint SMBIOSMemoryType { get; set; }
		public uint Speed { get; set; }
		public string Status { get; set; }
		public string Tag { get; set; }
		public ushort TotalWidth { get; set; }
		public ushort TypeDetail { get; set; }
		public string Version { get; set; }
	}

	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-pointingdevice
	/// </summary>
	[Serializable]
	public struct Win32PointingDevice
	{
		public ushort Availability { get; set; }
		public string Caption { get; set; }
		public uint ConfigManagerErrorCode { get; set; }
		public bool ConfigManagerUserConfig { get; set; }
		public string CreationClassName { get; set; }
		public string Description { get; set; }
		public string DeviceID { get; set; }
		public ushort DeviceInterface { get; set; }
		public uint DoubleSpeedThreshold { get; set; }
		public bool ErrorCleared { get; set; }
		public string ErrorDescription { get; set; }
		public ushort Handedness { get; set; }
		public string HardwareType { get; set; }
		public string InfFileName { get; set; }
		public string InfSection { get; set; }
		public DateTime InstallDate { get; set; }
		public bool IsLocked { get; set; }
		public uint LastErrorCode { get; set; }
		public string Manufacturer { get; set; }
		public string Name { get; set; }
		public byte NumberOfButtons { get; set; }
		public string PNPDeviceID { get; set; }
		public ushort PointingType { get; set; }
		public ushort[] PowerManagementCapabilities { get; set; }
		public bool PowerManagementSupported { get; set; }
		public uint QuadSpeedThreshold { get; set; }
		public uint Resolution { get; set; }
		public uint SampleRate { get; set; }
		public string Status { get; set; }
		public ushort StatusInfo { get; set; }
		public uint Synch { get; set; }
		public string SystemCreationClassName { get; set; }
		public string SystemName { get; set; }
	}

	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-videocontroller
	/// </summary>
	[Serializable]
	public struct Win32VideoController
	{
		public ushort[] AcceleratorCapabilities { get; set; }
		public string AdapterCompatibility { get; set; }
		public string AdapterDACType { get; set; }
		public uint AdapterRAM { get; set; }
		public ushort Availability { get; set; }
		public string[] CapabilityDescriptions { get; set; }
		public string Caption { get; set; }
		public uint ColorTableEntries { get; set; }
		public uint ConfigManagerErrorCode { get; set; }
		public bool ConfigManagerUserConfig { get; set; }
		public string CreationClassName { get; set; }
		public uint CurrentBitsPerPixel { get; set; }
		public uint CurrentHorizontalResolution { get; set; }
		public ulong CurrentNumberOfColors { get; set; }
		public uint CurrentNumberOfColumns { get; set; }
		public uint CurrentNumberOfRows { get; set; }
		public uint CurrentRefreshRate { get; set; }
		public ushort CurrentScanMode { get; set; }
		public uint CurrentVerticalResolution { get; set; }
		public string Description { get; set; }
		public string DeviceID { get; set; }
		public uint DeviceSpecificPens { get; set; }
		public uint DitherType { get; set; }
		public DateTime DriverDate { get; set; }
		public string DriverVersion { get; set; }
		public bool ErrorCleared { get; set; }
		public string ErrorDescription { get; set; }
		public uint ICMIntent { get; set; }
		public uint ICMMethod { get; set; }
		public string InfFilename { get; set; }
		public string InfSection { get; set; }
		public DateTime InstallDate { get; set; }
		public string InstalledDisplayDrivers { get; set; }
		public uint LastErrorCode { get; set; }
		public uint MaxMemorySupported { get; set; }
		public uint MaxNumberControlled { get; set; }
		public uint MaxRefreshRate { get; set; }
		public uint MinRefreshRate { get; set; }
		public bool Monochrome { get; set; }
		public string Name { get; set; }
		public ushort NumberOfColorPlanes { get; set; }
		public uint NumberOfVideoPages { get; set; }
		public string PNPDeviceID { get; set; }
		public ushort[] PowerManagementCapabilities { get; set; }
		public bool PowerManagementSupported { get; set; }
		public ushort ProtocolSupported { get; set; }
		public uint ReservedSystemPaletteEntries { get; set; }
		public uint SpecificationVersion { get; set; }
		public string Status { get; set; }
		public ushort StatusInfo { get; set; }
		public string SystemCreationClassName { get; set; }
		public string SystemName { get; set; }
		public uint SystemPaletteEntries { get; set; }
		public DateTime TimeOfLastReset { get; set; }
		public ushort VideoArchitecture { get; set; }
		public ushort VideoMemoryType { get; set; }
		public ushort VideoMode { get; set; }
		public string VideoModeDescription { get; set; }
		public string VideoProcessor { get; set; }
	}

	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-onboarddevice
	/// </summary>
	[Serializable]
	public struct Win32OnboardDevice
	{
		public string Caption { get; set; }
		public string CreationClassName { get; set; }
		public string Description { get; set; }
		public ushort DeviceType { get; set; }
		public bool Enabled { get; set; }
		public bool HotSwappable { get; set; }
		public DateTime InstallDate { get; set; }
		public string Manufacturer { get; set; }
		public string Model { get; set; }
		public string Name { get; set; }
		public string OtherIdentifyingInfo { get; set; }
		public string PartNumber { get; set; }
		public bool PoweredOn { get; set; }
		public bool Removable { get; set; }
		public bool Replaceable { get; set; }
		public string SerialNumber { get; set; }
		public string SKU { get; set; }
		public string Status { get; set; }
		public string Tag { get; set; }
		public string Version { get; set; }
	}
	#endregion

	#region Disk Info
	[Serializable]
	public struct DiskInfoResponse
	{
		public Win32DiskDrive[] DiskDrives { get; set; }
		public Win32DiskPartition[] DiskPartitions { get; set; }
		public Win32LogicalDisk[] LogicalDisks { get; set; }
	}

	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-diskdrive
	/// </summary>
	[Serializable]
	public struct Win32DiskDrive
	{
		public ushort Availability { get; set; }
		public uint BytesPerSector { get; set; }
		public ushort[] Capabilities { get; set; }
		public string[] CapabilityDescriptions { get; set; }
		public string Caption { get; set; }
		public string CompressionMethod { get; set; }
		public uint ConfigManagerErrorCode { get; set; }
		public bool ConfigManagerUserConfig { get; set; }
		public string CreationClassName { get; set; }
		public ulong DefaultBlockSize { get; set; }
		public string Description { get; set; }
		public string DeviceID { get; set; }
		public bool ErrorCleared { get; set; }
		public string ErrorDescription { get; set; }
		public string ErrorMethodology { get; set; }
		public string FirmwareRevision { get; set; }
		public uint Index { get; set; }
		public DateTime InstallDate { get; set; }
		public string InterfaceType { get; set; }
		public uint LastErrorCode { get; set; }
		public string Manufacturer { get; set; }
		public ulong MaxBlockSize { get; set; }
		public ulong MaxMediaSize { get; set; }
		public bool MediaLoaded { get; set; }
		public string MediaType { get; set; }
		public ulong MinBlockSize { get; set; }
		public string Model { get; set; }
		public string Name { get; set; }
		public bool NeedsCleaning { get; set; }
		public uint NumberOfMediaSupported { get; set; }
		public uint Partitions { get; set; }
		public string PNPDeviceID { get; set; }
		public ushort[] PowerManagementCapabilities { get; set; }
		public bool PowerManagementSupported { get; set; }
		public uint SCSIBus { get; set; }
		public ushort SCSILogicalUnit { get; set; }
		public ushort SCSIPort { get; set; }
		public ushort SCSITargetId { get; set; }
		public uint SectorsPerTrack { get; set; }
		public string SerialNumber { get; set; }
		public uint Signature { get; set; }
		public ulong Size { get; set; }
		public string Status { get; set; }
		public ushort StatusInfo { get; set; }
		public string SystemCreationClassName { get; set; }
		public string SystemName { get; set; }
		public ulong TotalCylinders { get; set; }
		public uint TotalHeads { get; set; }
		public ulong TotalSectors { get; set; }
		public ulong TotalTracks { get; set; }
		public uint TracksPerCylinder { get; set; }
	}

	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-diskpartition
	/// </summary>
	[Serializable]
	public struct Win32DiskPartition
	{
		public ushort AdditionalAvailability { get; set; }
		public ushort Availability { get; set; }
		public ushort[] PowerManagementCapabilities { get; set; }
		public string[] IdentifyingDescriptions { get; set; }
		public ulong MaxQuiesceTime { get; set; }
		public ulong OtherIdentifyingInfo { get; set; }
		public ushort StatusInfo { get; set; }
		public ulong PowerOnHours { get; set; }
		public ulong TotalPowerOnHours { get; set; }
		public ushort Access { get; set; }
		public ulong BlockSize { get; set; }
		public bool Bootable { get; set; }
		public bool BootPartition { get; set; }
		public string Caption { get; set; }
		public uint ConfigManagerErrorCode { get; set; }
		public bool ConfigManagerUserConfig { get; set; }
		public string CreationClassName { get; set; }
		public string Description { get; set; }
		public string DeviceID { get; set; }
		public uint DiskIndex { get; set; }
		public bool ErrorCleared { get; set; }
		public string ErrorDescription { get; set; }
		public string ErrorMethodology { get; set; }
		public uint HiddenSectors { get; set; }
		public uint Index { get; set; }
		public DateTime InstallDate { get; set; }
		public uint LastErrorCode { get; set; }
		public string Name { get; set; }
		public ulong NumberOfBlocks { get; set; }
		public string PNPDeviceID { get; set; }
		public bool PowerManagementSupported { get; set; }
		public bool PrimaryPartition { get; set; }
		public string Purpose { get; set; }
		public bool RewritePartition { get; set; }
		public ulong Size { get; set; }
		public ulong StartingOffset { get; set; }
		public string Status { get; set; }
		public string SystemCreationClassName { get; set; }
		public string SystemName { get; set; }
		public string Type { get; set; }
	}

	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-logicaldisk
	/// </summary>
	[Serializable]
	public struct Win32LogicalDisk
	{
		public ushort Access { get; set; }
		public ushort Availability { get; set; }
		public ulong BlockSize { get; set; }
		public string Caption { get; set; }
		public bool Compressed { get; set; }
		public uint ConfigManagerErrorCode { get; set; }
		public bool ConfigManagerUserConfig { get; set; }
		public string CreationClassName { get; set; }
		public string Description { get; set; }
		public string DeviceID { get; set; }
		public uint DriveType { get; set; }
		public bool ErrorCleared { get; set; }
		public string ErrorDescription { get; set; }
		public string ErrorMethodology { get; set; }
		public string FileSystem { get; set; }
		public ulong FreeSpace { get; set; }
		public DateTime InstallDate { get; set; }
		public uint LastErrorCode { get; set; }
		public uint MaximumComponentLength { get; set; }
		public uint MediaType { get; set; }
		public string Name { get; set; }
		public ulong NumberOfBlocks { get; set; }
		public string PNPDeviceID { get; set; }
		public ushort[] PowerManagementCapabilities { get; set; }
		public bool PowerManagementSupported { get; set; }
		public string ProviderName { get; set; }
		public string Purpose { get; set; }
		public bool QuotasDisabled { get; set; }
		public bool QuotasIncomplete { get; set; }
		public bool QuotasRebuilding { get; set; }
		public ulong Size { get; set; }
		public string Status { get; set; }
		public ushort StatusInfo { get; set; }
		public bool SupportsDiskQuotas { get; set; }
		public bool SupportsFileBasedCompression { get; set; }
		public string SystemCreationClassName { get; set; }
		public string SystemName { get; set; }
		public bool VolumeDirty { get; set; }
		public string VolumeName { get; set; }
		public string VolumeSerialNumber { get; set; }
	}
	#endregion
}
