
namespace WingetIntune.Internal.MsStore;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class MicrosoftStoreDetails
{
    public object allowedPlatforms { get; set; }
    public object skus { get; set; }
    public object platforms { get; set; }
    public bool accessible { get; set; }
    public bool isDeviceCompanionApp { get; set; }
    public MicrosoftStoreDetailsSupporturi[] supportUris { get; set; }
    public object[] features { get; set; }
    public string[] supportedLanguages { get; set; }
    public object legalUrl { get; set; }
    public object[] notes { get; set; }
    public object publisherCopyrightInformation { get; set; }
    public object additionalLicenseTerms { get; set; }
    public int approximateSizeInBytes { get; set; }
    public object appWebsiteUrl { get; set; }
    public object permissionsRequired { get; set; }
    public object requiredHardware { get; set; }
    public object recommendedHardware { get; set; }
    public object hardwareWarnings { get; set; }
    public string version { get; set; }
    public DateTime lastUpdateDateUtc { get; set; }
    public object osProductInformation { get; set; }
    public string categoryId { get; set; }
    public object subcategoryId { get; set; }
    public object addOnPriceRange { get; set; }
    public object deviceFamilyDisallowedReason { get; set; }
    public object builtFor { get; set; }
    public object trailers { get; set; }
    public object pdpBackgroundColor { get; set; }
    public bool containsDownloadPackage { get; set; }
    public MicrosoftStoreDetailsSystemrequirements systemRequirements { get; set; }
    public object installationTerms { get; set; }
    public object warningMessages { get; set; }
    public bool isMicrosoftProduct { get; set; }
    public bool hasParentBundles { get; set; }
    public bool hasAlternateEditions { get; set; }
    public int videoProductType { get; set; }
    public bool isMsixvc { get; set; }
    public object subtitle { get; set; }
    public object capabilitiesTable { get; set; }
    public bool isDownloadable { get; set; }
    public object promoMessage { get; set; }
    public object promoEndDateUtc { get; set; }
    public object packageFamilyName { get; set; }
    public object alternateId { get; set; }
    public object curatedBGColor { get; set; }
    public object curatedFGColor { get; set; }
    public object curatedImageUrl { get; set; }
    public object curatedTitle { get; set; }
    public object curatedDescription { get; set; }
    public object artistName { get; set; }
    public object artistId { get; set; }
    public object albumTitle { get; set; }
    public object albumProductId { get; set; }
    public bool isExplicit { get; set; }
    public int durationInSecond { get; set; }
    public object incompatibleReason { get; set; }
    public bool hasThirdPartyAPIs { get; set; }
    public object autosuggestSubtitle { get; set; }
    public object merchandizedProductType { get; set; }
    public string catalogSource { get; set; }
    public MicrosoftStoreDetailsScreenshot[] screenshots { get; set; }
    public object[] additionalTermLinks { get; set; }
    public object promotionDaysLeft { get; set; }
    public string[] categories { get; set; }
    public MicrosoftStoreDetailsImage[] images { get; set; }
    public string productId { get; set; }
    public object externalUri { get; set; }
    public string title { get; set; }
    public string shortTitle { get; set; }
    public object titleLayout { get; set; }
    public string description { get; set; }
    public string publisherName { get; set; }
    public string publisherId { get; set; }
    public object publisherAddress { get; set; }
    public object publisherPhoneNumber { get; set; }
    public bool isUniversal { get; set; }
    public string language { get; set; }
    public object bgColor { get; set; }
    public object fgColor { get; set; }
    public float averageRating { get; set; }
    public string ratingCount { get; set; }
    public bool hasFreeTrial { get; set; }
    public string productType { get; set; }
    public int price { get; set; }
    public object currencySymbol { get; set; }
    public object currencyCode { get; set; }
    public string displayPrice { get; set; }
    public object strikethroughPrice { get; set; }
    public string developerName { get; set; }
    public string productFamilyName { get; set; }
    public string mediaType { get; set; }
    public object contentIds { get; set; }
    public string[] packageFamilyNames { get; set; }
    public string subcategoryName { get; set; }
    public MicrosoftStoreDetailsAlternateid[] alternateIds { get; set; }
    public string collectionItemType { get; set; }
    public object numberOfSeasons { get; set; }
    public DateTime releaseDateUtc { get; set; }
    public int durationInSeconds { get; set; }
    public bool isCompatible { get; set; }
    public bool isPurchaseEnabled { get; set; }
    public object developerOptOutOfSDCardInstall { get; set; }
    public bool hasAddOns { get; set; }
    public bool hasThirdPartyIAPs { get; set; }
    public object voiceTitle { get; set; }
    public bool hideFromCollections { get; set; }
    public bool hideFromDownloadsAndUpdates { get; set; }
    public bool gamingOptionsXboxLive { get; set; }
    public string availableDevicesDisplayText { get; set; }
    public object availableDevicesNarratorText { get; set; }
    public bool isGamingAppOnly { get; set; }
    public bool isSoftBlocked { get; set; }
    public object tileLayout { get; set; }
    public object typeTag { get; set; }
    public object longDescription { get; set; }
    public object schema { get; set; }
    public object[] badges { get; set; }
    public MicrosoftStoreDetailsProductrating[] productRatings { get; set; }
    public object installer { get; set; }
    public string privacyUrl { get; set; }
    public string iconUrl { get; set; }
    public MicrosoftStoreDetailsLargepromotionimage largePromotionImage { get; set; }
    public string iconUrlBackground { get; set; }
}

public class MicrosoftStoreDetailsSystemrequirements
{
    public MicrosoftStoreDetailsMinimum minimum { get; set; }
    public object recomended { get; set; }
}

public class MicrosoftStoreDetailsMinimum
{
    public string title { get; set; }
    public MicrosoftStoreDetailsItem[] items { get; set; }
}

public class MicrosoftStoreDetailsItem
{
    public string level { get; set; }
    public string itemCode { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string validationHint { get; set; }
    public bool isValidationPassed { get; set; }
}

public class MicrosoftStoreDetailsLargepromotionimage
{
    public string imageType { get; set; }
    public string url { get; set; }
    public string caption { get; set; }
    public int height { get; set; }
    public int width { get; set; }
    public string backgroundColor { get; set; }
    public string foregroundColor { get; set; }
    public string imagePositionInfo { get; set; }
}

public class MicrosoftStoreDetailsSupporturi
{
    public object uri { get; set; }
}

public class MicrosoftStoreDetailsScreenshot
{
    public string imageType { get; set; }
    public string url { get; set; }
    public string caption { get; set; }
    public int height { get; set; }
    public int width { get; set; }
    public string backgroundColor { get; set; }
    public string foregroundColor { get; set; }
    public string imagePositionInfo { get; set; }
}

public class MicrosoftStoreDetailsImage
{
    public string imageType { get; set; }
    public string url { get; set; }
    public string caption { get; set; }
    public int height { get; set; }
    public int width { get; set; }
    public string backgroundColor { get; set; }
    public string foregroundColor { get; set; }
    public string imagePositionInfo { get; set; }
}

public class MicrosoftStoreDetailsAlternateid
{
    public object type { get; set; }
    public string alternateIdType { get; set; }
    public string alternateIdValue { get; set; }
    public string alternatedIdTypeString { get; set; }
}

public class MicrosoftStoreDetailsProductrating
{
    public string ratingSystem { get; set; }
    public string ratingSystemShortName { get; set; }
    public string ratingSystemId { get; set; }
    public string ratingSystemUrl { get; set; }
    public string ratingValue { get; set; }
    public string ratingValueLogoUrl { get; set; }
    public int ratingAge { get; set; }
    public bool restrictMetadata { get; set; }
    public bool restrictPurchase { get; set; }
    public object[] ratingDescriptors { get; set; }
    public object[] ratingDescriptorLogoUrls { get; set; }
    public object[] ratingDisclaimers { get; set; }
    public string[] interactiveElements { get; set; }
    public string longName { get; set; }
    public string shortName { get; set; }
    public string description { get; set; }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
