using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelListing.Api.Common.Constants;

public static class CacheConstants
{
    public const string AuthenticatedUserCachingPolicy = "AuthenticatedUserCachingPolicy";
    public const string AuthenticatedUserCachingPolicyTag = "authpolicy-";
    public const int ShortDuration = 60;//1 minute
    public const int LongDuration = 900;//15 minutes
    public const int MediumDuration = 300;//5 minutes
}
