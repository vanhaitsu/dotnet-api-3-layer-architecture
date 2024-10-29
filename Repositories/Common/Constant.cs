namespace Repositories.Common;

public static class Constant
{
    #region JWT

    public const int ACCESS_TOKEN_VALIDITY_IN_MINUTES = 5;
    public const int REFRESH_TOKEN_VALIDITY_IN_DAYS = 7;

    #endregion

    #region Pagination

    // Default
    public const int DEFAULT_MIN_PAGE_SIZE = 10;
    public const int DEFAULT_MAX_PAGE_SIZE = 50;

    #endregion

    #region Cache

    // Default
    public const int DEFAULT_ABSOLUTE_EXPIRATION_IN_MINUTES = 20;
    public const int DEFAULT_SLIDING_EXPIRATION_IN_MINUTES = 5;

    #endregion
}