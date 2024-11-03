namespace Repositories.Common;

public static class Constant
{
    #region JWT

    public const int AccessTokenValidityInMinutes = 5;
    public const int RefreshTokenValidityInDays = 7;

    #endregion

    #region Pagination

    // Default
    public const int DefaultMinPageSize = 10;
    public const int DefaultMaxPageSize = 50;

    #endregion

    #region Cache

    // Default
    public const int DefaultAbsoluteExpirationInMinutes = 20;
    public const int DefaultSlidingExpirationInMinutes = 5;

    #endregion
}