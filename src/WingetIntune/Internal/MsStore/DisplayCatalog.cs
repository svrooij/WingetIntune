using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetIntune.Internal.MsStore;

public class DisplayCatalogResponse
{
    public Product[] Products { get; set; }
}

public class Product
{
    public DateTime LastModifiedDate { get; set; }
    public Localizedproperty[] LocalizedProperties { get; set; }
    public Marketproperty[] MarketProperties { get; set; }
    public string ProductASchema { get; set; }
    public string ProductBSchema { get; set; }
    public string ProductId { get; set; }
    public AppProperties Properties { get; set; }
    public Alternateid[] AlternateIds { get; set; }
    public object DomainDataVersion { get; set; }
    public string IngestionSource { get; set; }
    public bool IsMicrosoftProduct { get; set; }
    public string PreferredSkuId { get; set; }
    public string ProductType { get; set; }
    public Validationdata ValidationData { get; set; }
    public object[] MerchandizingTags { get; set; }
    public string PartD { get; set; }
    public string ProductFamily { get; set; }
    public string SchemaVersion { get; set; }
    public string ProductKind { get; set; }
    public Productpolicies ProductPolicies { get; set; }
    public Displayskuavailability[] DisplaySkuAvailabilities { get; set; }
}

public class AppProperties
{
    public AppAttribute[] Attributes { get; set; }
    public bool CanInstallToSDCard { get; set; }
    public string Category { get; set; }
    public string SubCategory { get; set; }
    public object Categories { get; set; }
    public object Extensions { get; set; }
    public bool IsAccessible { get; set; }
    public bool IsLineOfBusinessApp { get; set; }
    public bool IsPublishedToLegacyWindowsPhoneStore { get; set; }
    public bool IsPublishedToLegacyWindowsStore { get; set; }
    public bool IsSettingsApp { get; set; }
    public string PackageFamilyName { get; set; }
    public string PackageIdentityName { get; set; }
    public string PublisherCertificateName { get; set; }
    public string PublisherId { get; set; }
    public object XboxLiveTier { get; set; }
    public object XboxXPA { get; set; }
    public object XboxCrossGenSetId { get; set; }
    public object XboxConsoleGenOptimized { get; set; }
    public object XboxConsoleGenCompatible { get; set; }
    public bool XboxLiveGoldRequired { get; set; }
    public XBOX XBOX { get; set; }
    public Extendedclientmetadata ExtendedClientMetadata { get; set; }
    public object OwnershipType { get; set; }
    public string PdpBackgroundColor { get; set; }
    public bool HasAddOns { get; set; }
    public DateTime RevisionId { get; set; }
}

public class XBOX
{
}

public class Extendedclientmetadata
{
}

public class AppAttribute
{
    public string Name { get; set; }
    public object Minimum { get; set; }
    public object Maximum { get; set; }
    public object ApplicablePlatforms { get; set; }
    public object Group { get; set; }
}

public class Validationdata
{
    public bool PassedValidation { get; set; }
    public string RevisionId { get; set; }
    public string ValidationResultUri { get; set; }
}

public class Productpolicies
{
}

public class Localizedproperty
{
    public string DeveloperName { get; set; }
    public object DisplayPlatformProperties { get; set; }
    public string PublisherName { get; set; }
    public object PublisherAddress { get; set; }
    public string PublisherWebsiteUri { get; set; }
    public string SupportUri { get; set; }
    public object SupportPhone { get; set; }
    public object EligibilityProperties { get; set; }
    public object[] Franchises { get; set; }
    public Image[] Images { get; set; }
    public object[] Videos { get; set; }
    public string ProductDescription { get; set; }
    public string ProductTitle { get; set; }
    public string ShortTitle { get; set; }
    public string SortTitle { get; set; }
    public object FriendlyTitle { get; set; }
    public string ShortDescription { get; set; }
    public object[] SearchTitles { get; set; }
    public string VoiceTitle { get; set; }
    public object RenderGroupDetails { get; set; }
    public object[] ProductDisplayRanks { get; set; }
    public object InteractiveModelConfig { get; set; }
    public bool Interactive3DEnabled { get; set; }
    public string Language { get; set; }
    public string[] Markets { get; set; }
}

public class Image
{
    public string FileId { get; set; }
    public object EISListingIdentifier { get; set; }
    public string BackgroundColor { get; set; }
    public string Caption { get; set; }
    public int FileSizeInBytes { get; set; }
    public string ForegroundColor { get; set; }
    public int Height { get; set; }
    public string ImagePositionInfo { get; set; }
    public string ImagePurpose { get; set; }
    public string UnscaledImageSHA256Hash { get; set; }
    public string Uri { get; set; }
    public int Width { get; set; }
}

public class Marketproperty
{
    public DateTime OriginalReleaseDate { get; set; }
    public string OriginalReleaseDateFriendlyName { get; set; }
    public int MinimumUserAge { get; set; }
    public Contentrating[] ContentRatings { get; set; }
    public object[] RelatedProducts { get; set; }
    public Usagedata[] UsageData { get; set; }
    public object BundleConfig { get; set; }
    public string[] Markets { get; set; }
}

public class Contentrating
{
    public string RatingSystem { get; set; }
    public string RatingId { get; set; }
    public string[] RatingDescriptors { get; set; }
    public object[] RatingDisclaimers { get; set; }
    public string[] InteractiveElements { get; set; }
}

public class Usagedata
{
    public string AggregateTimeSpan { get; set; }
    public float AverageRating { get; set; }
    public int PlayCount { get; set; }
    public int RatingCount { get; set; }
    public string RentalCount { get; set; }
    public string TrialCount { get; set; }
    public string PurchaseCount { get; set; }
}

public class Alternateid
{
    public string IdType { get; set; }
    public string Value { get; set; }
}

public class Displayskuavailability
{
    public Sku Sku { get; set; }
    public Availability[] Availabilities { get; set; }
    public Historicalbestavailability[] HistoricalBestAvailabilities { get; set; }
}

public class Sku
{
    public DateTime LastModifiedDate { get; set; }
    public SkuLocalizedproperty[] LocalizedProperties { get; set; }
    public SkuMarketproperty[] MarketProperties { get; set; }
    public string ProductId { get; set; }
    public SkuProperties Properties { get; set; }
    public string SkuASchema { get; set; }
    public string SkuBSchema { get; set; }
    public string SkuId { get; set; }
    public string SkuType { get; set; }
    public object RecurrencePolicy { get; set; }
    public object SubscriptionPolicyId { get; set; }
}

public class SkuProperties
{
    public object EarlyAdopterEnrollmentUrl { get; set; }
    public Fulfillmentdata FulfillmentData { get; set; }
    public string FulfillmentType { get; set; }
    public object FulfillmentPluginId { get; set; }
    public bool HasThirdPartyIAPs { get; set; }
    public DateTime LastUpdateDate { get; set; }
    public Hardwareproperties HardwareProperties { get; set; }
    public object[] HardwareRequirements { get; set; }
    public object[] HardwareWarningList { get; set; }
    public string InstallationTerms { get; set; }
    public Package[] Packages { get; set; }
    public string VersionString { get; set; }
    public object[] VisibleToB2BServiceIds { get; set; }
    public bool XboxXPA { get; set; }
    public object[] BundledSkus { get; set; }
    public bool IsRepurchasable { get; set; }
    public int SkuDisplayRank { get; set; }
    public object DisplayPhysicalStoreInventory { get; set; }
    public object[] AdditionalIdentifiers { get; set; }
    public bool IsTrial { get; set; }
    public bool IsPreOrder { get; set; }
    public bool IsBundle { get; set; }
}

public class Fulfillmentdata
{
    public string ProductId { get; set; }
    public string WuBundleId { get; set; }
    public string WuCategoryId { get; set; }
    public string PackageFamilyName { get; set; }
    public string SkuId { get; set; }
    public object Content { get; set; }
    public object PackageFeatures { get; set; }
}

public class Hardwareproperties
{
    public object[] MinimumHardware { get; set; }
    public object[] RecommendedHardware { get; set; }
    public string MinimumProcessor { get; set; }
    public string RecommendedProcessor { get; set; }
    public string MinimumGraphics { get; set; }
    public string RecommendedGraphics { get; set; }
}

public class Package
{
    public Application[] Applications { get; set; }
    public string[] Architectures { get; set; }
    public string[] Capabilities { get; set; }
    public object[] DeviceCapabilities { get; set; }
    public object[] ExperienceIds { get; set; }
    public object[] FrameworkDependencies { get; set; }
    public object[] HardwareDependencies { get; set; }
    public object[] HardwareRequirements { get; set; }
    public string Hash { get; set; }
    public string HashAlgorithm { get; set; }
    public bool IsStreamingApp { get; set; }
    public string[] Languages { get; set; }
    public int MaxDownloadSizeInBytes { get; set; }
    public int MaxInstallSizeInBytes { get; set; }
    public string PackageFormat { get; set; }
    public string PackageFamilyName { get; set; }
    public object MainPackageFamilyNameForDlc { get; set; }
    public string PackageFullName { get; set; }
    public string PackageId { get; set; }
    public string ContentId { get; set; }
    public object KeyId { get; set; }
    public int PackageRank { get; set; }
    public string PackageUri { get; set; }
    public Platformdependency[] PlatformDependencies { get; set; }
    public string PlatformDependencyXmlBlob { get; set; }
    public string ResourceId { get; set; }
    public string Version { get; set; }
    public object PackageDownloadUris { get; set; }
    public object[] DriverDependencies { get; set; }
    public PackageFulfillmentdata FulfillmentData { get; set; }
}

public class PackageFulfillmentdata
{
    public string ProductId { get; set; }
    public string WuBundleId { get; set; }
    public string WuCategoryId { get; set; }
    public string PackageFamilyName { get; set; }
    public string SkuId { get; set; }
    public object Content { get; set; }
    public object PackageFeatures { get; set; }
}

public class Application
{
    public string ApplicationId { get; set; }
    public int DeclarationOrder { get; set; }
    public string[] Extensions { get; set; }
}

public class Platformdependency
{
    public long MaxTested { get; set; }
    public long MinVersion { get; set; }
    public string PlatformName { get; set; }
}

public class SkuLocalizedproperty
{
    public object[] Contributors { get; set; }
    public object[] Features { get; set; }
    public string MinimumNotes { get; set; }
    public string RecommendedNotes { get; set; }
    public string ReleaseNotes { get; set; }
    public object DisplayPlatformProperties { get; set; }
    public string SkuDescription { get; set; }
    public string SkuTitle { get; set; }
    public string SkuButtonTitle { get; set; }
    public object DeliveryDateOverlay { get; set; }
    public object[] SkuDisplayRank { get; set; }
    public object TextResources { get; set; }
    public object[] Images { get; set; }
    public Legaltext LegalText { get; set; }
    public string Language { get; set; }
    public string[] Markets { get; set; }
}

public class Legaltext
{
    public string AdditionalLicenseTerms { get; set; }
    public string Copyright { get; set; }
    public string CopyrightUri { get; set; }
    public string PrivacyPolicy { get; set; }
    public string PrivacyPolicyUri { get; set; }
    public string Tou { get; set; }
    public string TouUri { get; set; }
}

public class SkuMarketproperty
{
    public DateTime FirstAvailableDate { get; set; }
    public string[] SupportedLanguages { get; set; }
    public object PackageIds { get; set; }
    public object PIFilter { get; set; }
    public string[] Markets { get; set; }
}

public class Availability
{
    public string[] Actions { get; set; }
    public string AvailabilityASchema { get; set; }
    public string AvailabilityBSchema { get; set; }
    public string AvailabilityId { get; set; }
    public Conditions Conditions { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public string[] Markets { get; set; }
    public Ordermanagementdata OrderManagementData { get; set; }
    public AvailabilityProperties Properties { get; set; }
    public string SkuId { get; set; }
    public int DisplayRank { get; set; }
    public bool RemediationRequired { get; set; }
    public Licensingdata LicensingData { get; set; }
}

public class Conditions
{
    public Clientconditions ClientConditions { get; set; }
    public DateTime EndDate { get; set; }
    public string[] ResourceSetIds { get; set; }
    public DateTime StartDate { get; set; }
}

public class Clientconditions
{
    public Allowedplatform[] AllowedPlatforms { get; set; }
}

public class Allowedplatform
{
    public int MaxVersion { get; set; }
    public int MinVersion { get; set; }
    public string PlatformName { get; set; }
}

public class Ordermanagementdata
{
    public object[] GrantedEntitlementKeys { get; set; }
    public Pifilter PIFilter { get; set; }
    public Price Price { get; set; }
}

public class Pifilter
{
    public object[] ExclusionProperties { get; set; }
    public object[] InclusionProperties { get; set; }
}

public class Price
{
    public string CurrencyCode { get; set; }
    public bool IsPIRequired { get; set; }
    public double ListPrice { get; set; }
    public double MSRP { get; set; }
    public string TaxType { get; set; }
    public string WholesaleCurrencyCode { get; set; }
}

public class AvailabilityProperties
{
    public DateTime OriginalReleaseDate { get; set; }
}

public class Licensingdata
{
    public Satisfyingentitlementkey[] SatisfyingEntitlementKeys { get; set; }
}

public class Satisfyingentitlementkey
{
    public string[] EntitlementKeys { get; set; }
    public string[] LicensingKeyIds { get; set; }
}

public class Historicalbestavailability
{
    public string[] Actions { get; set; }
    public string AvailabilityASchema { get; set; }
    public string AvailabilityBSchema { get; set; }
    public string AvailabilityId { get; set; }
    public HistoricalbestavailabilityConditions Conditions { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public string[] Markets { get; set; }
    public Ordermanagementdata1 OrderManagementData { get; set; }
    public Properties3 Properties { get; set; }
    public string SkuId { get; set; }
    public int DisplayRank { get; set; }
    public string ProductASchema { get; set; }
}

public class HistoricalbestavailabilityConditions
{
    public Clientconditions1 ClientConditions { get; set; }
    public DateTime EndDate { get; set; }
    public string[] ResourceSetIds { get; set; }
    public DateTime StartDate { get; set; }
    public string[] EligibilityPredicateIds { get; set; }
    public int SupportedCatalogVersion { get; set; }
}

public class Clientconditions1
{
    public Allowedplatform1[] AllowedPlatforms { get; set; }
}

public class Allowedplatform1
{
    public int MaxVersion { get; set; }
    public int MinVersion { get; set; }
    public string PlatformName { get; set; }
}

public class Ordermanagementdata1
{
    public object[] GrantedEntitlementKeys { get; set; }
    public Pifilter1 PIFilter { get; set; }
    public Price1 Price { get; set; }
}

public class Pifilter1
{
    public object[] ExclusionProperties { get; set; }
    public object[] InclusionProperties { get; set; }
}

public class Price1
{
    public string CurrencyCode { get; set; }
    public bool IsPIRequired { get; set; }
    public double ListPrice { get; set; }
    public double MSRP { get; set; }
    public string TaxType { get; set; }
    public string WholesaleCurrencyCode { get; set; }
}

public class Properties3
{
    public DateTime OriginalReleaseDate { get; set; }
}

